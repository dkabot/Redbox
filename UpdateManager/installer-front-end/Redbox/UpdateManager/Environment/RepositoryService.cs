using Redbox.Compression;
using Redbox.Core;
using Redbox.DeltaCompression.Binary;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Redbox.UpdateManager.Environment
{
    internal class RepositoryService : IRepositoryService, IDisposable
    {
        private const int LockTimeout = 30;
        private readonly object m_dictLock = new object();
        private readonly Dictionary<string, ReaderWriterLockSlim> m_locks = new Dictionary<string, ReaderWriterLockSlim>();
        private readonly Dictionary<string, PersistentList<RevLog>> m_revLogs = new Dictionary<string, PersistentList<RevLog>>();

        public static RepositoryService Instance => Singleton<RepositoryService>.Instance;

        public Redbox.UpdateManager.ComponentModel.ErrorList Repair(string name, out bool deferredMove)
        {
            deferredMove = false;
            if (!this.ContainsRepository(name))
                throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
                return errorList;
            }
            try
            {
                this.WriteCurrentRevisionInternal(name, "0000000000000000000000000000000000000000");
                this.WriteActiveRevisionInternal(name, "0000000000000000000000000000000000000000");
                string revisionInternal = this.GetHeadRevisionInternal(name);
                if (revisionInternal == "0000000000000000000000000000000000000000")
                    return errorList;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.ActivateToInternal(name, revisionInternal, out deferredMove));
                return errorList;
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
        }

        public string GetStagedRevision(string name)
        {
            return this.ContainsRepository(name) ? this.ReadCurrentRevisionInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
        }

        public string GetActiveRevision(string name)
        {
            return this.ContainsRepository(name) ? this.ReadActiveRevisionInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
        }

        public string GetActiveLabel(string name)
        {
            return this.ContainsRepository(name) ? this.ReadActiveLabelInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Verify(string name)
        {
            if (!this.ContainsRepository(name))
                throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            List<RevLog> revLogList = this.GetRevLogList(name);
            for (int index = 0; index < revLogList.Count; ++index)
            {
                foreach (ChangeItem change in revLogList[index].Changes)
                {
                    if (!change.Composite)
                    {
                        string str = Path.Combine(this.FormatRepositoryLocationInternal(name), revLogList[index].HashFileName(change.FormatFileName()));
                        if (new FileInfo(str).Length >= 1L)
                        {
                            using (FileStream inputStream = File.OpenRead(str))
                            {
                                string asciishA1Hash = inputStream.ToASCIISHA1Hash();
                                if (change.IsSeed && change.VersionHash != asciishA1Hash)
                                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", string.Format("{0} should have hash {1} but has {2}", (object)str, (object)change.VersionHash, (object)asciishA1Hash), "This store's repository is corrupted."));
                                else if (!change.IsSeed)
                                {
                                    if (change.ContentHash != asciishA1Hash)
                                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", string.Format("{0} should have hash {1} but has {2}", (object)str, (object)change.ContentHash, (object)asciishA1Hash), "This store's repository is corrupted."));
                                }
                            }
                        }
                    }
                }
            }
            HashSet<string> stringSet = new HashSet<string>();
            string current = this.ReadCurrentRevisionInternal(name);
            if (current != "0000000000000000000000000000000000000000")
            {
                for (int index = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == current)); index > -1; --index)
                {
                    foreach (ChangeItem change in revLogList[index].Changes)
                    {
                        string file = Path.Combine(change.TargetPath, change.TargetName);
                        if (!stringSet.Contains(file))
                        {
                            stringSet.Add(file);
                            string path = this.FormatStagedFileName(file);
                            using (FileStream inputStream = File.OpenRead(path))
                            {
                                string asciishA1Hash = inputStream.ToASCIISHA1Hash();
                                if (change.VersionHash != asciishA1Hash)
                                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", string.Format("Staged file: {0} should have hash {1} but has {2}", (object)path, (object)change.VersionHash, (object)asciishA1Hash), "This store's repository is corrupted."));
                            }
                        }
                    }
                }
                stringSet.Clear();
            }
            if (this.ReadActiveRevisionInternal(name) != "0000000000000000000000000000000000000000")
            {
                for (int index = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == current)); index > -1; --index)
                {
                    foreach (ChangeItem change in revLogList[index].Changes)
                    {
                        string path = Path.Combine(change.TargetPath, change.TargetName);
                        if (!stringSet.Contains(path))
                        {
                            stringSet.Add(path);
                            try
                            {
                                using (FileStream inputStream = File.OpenRead(path))
                                {
                                    string asciishA1Hash = inputStream.ToASCIISHA1Hash();
                                    if (change.VersionHash != asciishA1Hash)
                                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("V999", string.Format("Active file: {0} should have hash {1} but has {2}", (object)path, (object)change.VersionHash, (object)asciishA1Hash), "This store's repository is corrupted."));
                                }
                            }
                            catch (Exception ex)
                            {
                                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", string.Format("Error hashing {0}.", (object)path), ex));
                            }
                        }
                    }
                }
            }
            return errorList;
        }

        public List<IRevLog> GetUnfinishedChanges(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                List<IRevLog> unfinishedChanges = new List<IRevLog>();
                ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
                IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
                List<ITransferJob> transferJobList;
                Redbox.UpdateManager.ComponentModel.ErrorList jobs = service1.GetJobs(out transferJobList, false);
                if (jobs.ContainsError())
                {
                    jobs.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
                    return unfinishedChanges;
                }
                foreach (ITransferJob transferJob in transferJobList)
                {
                    ChangeSet changeSet = service2.Get<ChangeSet>(transferJob.ID);
                    if (changeSet != null && !(changeSet.Name != name))
                    {
                        Dictionary<string, IRevLog> dictionary = new Dictionary<string, IRevLog>();
                        foreach (string revision in changeSet.Revisions)
                        {
                            RevLog revLog = new RevLog()
                            {
                                Changes = new List<ChangeItem>(),
                                Hash = revision
                            };
                            dictionary.Add(revision, (IRevLog)revLog);
                            unfinishedChanges.Add((IRevLog)revLog);
                        }
                        foreach (ChangeSetItem changeSetItem in changeSet.Items)
                            dictionary[changeSetItem.Revision].Changes.Add(new ChangeItem()
                            {
                                IsSeed = changeSetItem.IsFirst,
                                Composite = changeSetItem.Composite,
                                ContentHash = changeSetItem.ContentHash,
                                TargetName = Path.GetFileName(changeSetItem.TargetName),
                                TargetPath = Path.GetDirectoryName(changeSetItem.TargetPath),
                                VersionHash = changeSetItem.VersionHash
                            });
                    }
                }
                return unfinishedChanges;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public List<string> GetAllRepositories()
        {
            if (!Directory.Exists(this.Root))
                Directory.CreateDirectory(this.Root);
            string[] directories = Directory.GetDirectories(this.Root);
            List<string> allRepositories = new List<string>();
            DirectoryInfo directoryInfo1 = new DirectoryInfo(this.StagingDirectory);
            if (!Directory.Exists(this.StagingDirectory))
                Directory.CreateDirectory(this.StagingDirectory);
            foreach (string path in directories)
            {
                DirectoryInfo directoryInfo2 = new DirectoryInfo(path);
                if (directoryInfo2.FullName != directoryInfo1.FullName)
                    allRepositories.Add(directoryInfo2.Name);
            }
            return allRepositories;
        }

        public string FormatStagedFileName(string file)
        {
            string fileName = Path.GetFileName(file);
            return Path.Combine(Path.GetDirectoryName(file), string.Format(".staged_\\{0}", (object)fileName));
        }

        public void RemoveRevision(string name, string hash)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                List<int> indexList = new List<int>();
                PersistentList<RevLog> revLog = this.GetRevLog(name);
                List<RevLog> revLogList = revLog.AsList();
                while (true)
                {
                    int index = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == hash));
                    if (index != -1)
                        indexList.Add(index);
                    else
                        break;
                }
                revLog.RemoveAllAt((IEnumerable<int>)indexList);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void TrimRepository(string name)
        {
            LogHelper.Instance.Log("Trimming Repository: {0}.", (object)name);
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                List<RevLog> revLogList = this.GetRevLogList(name);
                if (revLogList.Count < 1)
                {
                    LogHelper.Instance.Log("No repository revisions were found. No need to trim.");
                }
                else
                {
                    string current = this.ReadCurrentRevisionInternal(name);
                    if (current == "0000000000000000000000000000000000000000")
                    {
                        LogHelper.Instance.Log("Current revision is not set. No need to trim.");
                    }
                    else
                    {
                        int index1 = revLogList.FindIndex((Predicate<RevLog>)(r => r.Hash == current));
                        if (index1 == -1)
                        {
                            LogHelper.Instance.Log("Current revision index was not found. No need to trim.");
                        }
                        else
                        {
                            List<int> indexList = new List<int>();
                            for (int index2 = 1; index2 < index1; ++index2)
                            {
                                RevLog revLog = revLogList[index2];
                                bool keep = false;
                                revLog.Changes.ForEach((Action<ChangeItem>)(ci =>
                                {
                                    if (!ci.IsSeed)
                                        return;
                                    keep = true;
                                }));
                                if (!keep)
                                    indexList.Add(index2);
                            }
                            LogHelper.Instance.Log("Current Hash: {0} Index:{1}", (object)current, (object)index1);
                            if (indexList.Count <= 0)
                                return;
                            indexList.ForEach((Action<int>)(item => LogHelper.Instance.Log("Deleting index: {0} ", (object)item)));
                            this.GetRevLog(name).RemoveAllAt((IEnumerable<int>)indexList);
                        }
                    }
                }
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public string GetHeadRevision(string name)
        {
            if (this.ContainsRepository(name))
                return this.GetHeadRevisionInternal(name);
            LogHelper.Instance.Log(string.Format("repository {0} does not exist.", (object)name));
            return string.Empty;
        }

        public void UnpackChangeSet(string name, Guid id, string archive)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                LogHelper.Instance.Log("Unpacking change set.");
                ChangeSet changeSet = ServiceLocator.Instance.GetService<IDataStoreService>().Get<ChangeSet>(id);
                if ((byte)changeSet.CompressionType != (byte)0)
                {
                    using (ExecutionTimer executionTimer = new ExecutionTimer())
                    {
                        CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm((CompressionType)(byte)changeSet.CompressionType);
                        string tempFileName = Path.GetTempFileName();
                        try
                        {
                            using (FileStream source = File.OpenRead(archive))
                            {
                                using (FileStream target = File.OpenWrite(tempFileName))
                                    algorithm.Decompress((Stream)source, (Stream)target);
                            }
                            File.Delete(archive);
                            File.Move(tempFileName, archive);
                            LogHelper.Instance.Log("Decompressing change set for {0} took {1} with {2}", (object)changeSet.Name, (object)executionTimer.Elapsed, (object)changeSet.CompressionType.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            File.Delete(tempFileName);
                            throw;
                        }
                    }
                }
                if (!changeSet.IsArchive)
                {
                    string asciishA1Hash = Encoding.ASCII.GetBytes(Path.Combine(changeSet.Items[changeSet.Items.Count - 1].TargetPath, changeSet.Items[changeSet.Items.Count - 1].TargetName)).ToASCIISHA1Hash();
                    string str = Path.Combine(this.StagingDirectory, string.Format("{0}-{1}.patch", (object)changeSet.Revisions.Last<string>(), (object)asciishA1Hash));
                    File.Delete(str);
                    File.Move(archive, str);
                    LogHelper.Instance.Log("Unpacked file: {0}", (object)str);
                }
                else
                    TarHelper.Unpack(archive, this.StagingDirectory);
                LogHelper.Instance.Log("Finished unpacking change set.");
            }
            catch (Exception ex)
            {
                File.Delete(archive);
                throw;
            }
            finally
            {
                File.Delete(archive);
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public List<IRevLog> GetPendingChanges(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                string current = this.ReadCurrentRevisionInternal(name);
                List<RevLog> revLogList = this.GetRevLogList(name);
                int index1 = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == current));
                List<IRevLog> pendingChanges = new List<IRevLog>();
                for (int index2 = index1 + 1; index2 < revLogList.Count; ++index2)
                    pendingChanges.Add((IRevLog)revLogList[index2]);
                return pendingChanges;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public List<IRevLog> GetPendingActivations(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                return this.GetPendingActivationsInternal(name);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public List<IRevLog> GetAllChanges(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                List<IRevLog> allChanges = new List<IRevLog>();
                this.GetRevLogList(name).ForEach(new Action<RevLog>(allChanges.Add));
                return allChanges;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public List<IRevLog> GetAppliedChanges(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                string current = this.ReadCurrentRevisionInternal(name);
                List<RevLog> revLogList = this.GetRevLogList(name);
                int index1 = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == current));
                List<IRevLog> appliedChanges = new List<IRevLog>();
                for (int index2 = 0; index2 <= index1; ++index2)
                    appliedChanges.Add((IRevLog)revLogList[index2]);
                return appliedChanges;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public List<IRevLog> GetActivatedChanges(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                string active = this.ReadActiveRevisionInternal(name);
                List<RevLog> revLogList = this.GetRevLogList(name);
                int index1 = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == active));
                List<IRevLog> activatedChanges = new List<IRevLog>();
                for (int index2 = 0; index2 <= index1; ++index2)
                    activatedChanges.Add((IRevLog)revLogList[index2]);
                return activatedChanges;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RebuildTo(string name, string targetHash)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", string.Format("repository {0} does not exists.", (object)name), "Pass in a valid direcotry"));
                return errorList;
            }
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.RebuildToInternal(name, targetHash));
                return errorList;
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ActivateTo(
          string name,
          string targetHash,
          out bool deferredMove)
        {
            deferredMove = false;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", string.Format("repository {0} does not exists.", (object)name), "Pass in a valid direcotry"));
                return errorList;
            }
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                return this.ActivateToInternal(name, targetHash, out deferredMove);
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList UpdateTo(string name, string targetHash)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", string.Format("repository {0} does not exists.", (object)name), "Pass in a valid direcotry"));
                return errorList;
            }
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire write lock.");
            try
            {
                return this.UpdateToInternal(name, targetHash, false);
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
        }

        public bool ContainsRepository(string name)
        {
            return Directory.Exists(this.FormatRepositoryLocationInternal(name));
        }

        public void AddRepository(string name)
        {
            string path = this.FormatRepositoryLocationInternal(name);
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire write lock.");
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                this.GetRevLog(name);
                this.WriteCurrentRevisionInternal(name, "0000000000000000000000000000000000000000");
                this.WriteActiveRevisionInternal(name, "0000000000000000000000000000000000000000");
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
        }

        public void RemoveRevisions(string name, List<string> revisions)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire write lock.");
            try
            {
                List<int> toRemove = new List<int>();
                PersistentList<RevLog> revLog = this.GetRevLog(name);
                List<RevLog> revLogList = revLog.AsList();
                int i = 0;
                Action<RevLog> action = (Action<RevLog>)(item =>
                {
                    if (revisions.Contains(item.Hash))
                        toRemove.Add(i);
                    ++i;
                });
                revLogList.ForEach(action);
                revLog.RemoveAllAt((IEnumerable<int>)toRemove);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void AddRevisions(string name, List<string> revisions)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire write lock.");
            try
            {
                PersistentList<RevLog> revLog = this.GetRevLog(name);
                foreach (string revision in revisions)
                    revLog.Add(new RevLog()
                    {
                        Changes = new List<ChangeItem>(),
                        Hash = revision
                    });
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public string GetLabel(string name, string revision)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterReadLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                return (this.GetRevLogList(name).Find((Predicate<RevLog>)(l => l.Hash == revision)) ?? throw new ApplicationException(string.Format("No revsion with hash: {0} found in repository name: {1}", (object)revision, (object)name))).Label;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void SetLabel(string name, string revision, string label)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                PersistentList<RevLog> revLog1 = this.GetRevLog(name);
                List<RevLog> revLogList = revLog1.AsList();
                int index = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == revision));
                RevLog revLog2 = index != -1 ? revLogList[index] : throw new ApplicationException(string.Format("No revsion with hash: {0} found in repository name: {1}", (object)revision, (object)name));
                if (string.IsNullOrEmpty(label))
                    return;
                revLog2.Label = label;
                revLog1[index] = revLog2;
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void AddDelta(string name, List<DeltaItem> deltaItemList)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to acquire write lock.");
            try
            {
                PersistentList<RevLog> revLog1 = this.GetRevLog(name);
                List<RevLog> revLogList = revLog1.AsList();
                foreach (DeltaItem deltaItem in deltaItemList)
                {
                    DeltaItem item = deltaItem;
                    int index = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == item.Revision));
                    RevLog revLog2 = index != -1 ? revLogList[index] : throw new ApplicationException(string.Format("No revision with hash: {0} found in repository name: {1}", (object)deltaItem.Revision, (object)name));
                    ChangeItem changeItem = new ChangeItem()
                    {
                        IsSeed = deltaItem.IsSeed,
                        Composite = deltaItem.IsPlaceHolder,
                        ContentHash = deltaItem.ContentHash,
                        TargetName = deltaItem.TargetName,
                        TargetPath = deltaItem.TargetPath,
                        VersionHash = deltaItem.VersionHash
                    };
                    revLog2.Changes.Add(changeItem);
                    revLog1[index] = revLog2;
                    string str1 = Path.Combine(this.FormatRepositoryLocationInternal(name), revLog2.HashFileName(changeItem.FormatFileName()));
                    LogHelper.Instance.Log("Add patch from Revision: {0} with the following properties.\r\nComposite: {1}\r\n ContentHash: {2}\r\n TargetName: {3}\r\n TargetPath: {4}\r\n VersionHash: {5}\r\n IsSeed: {6}\r\n", (object)deltaItem.Revision, (object)changeItem.Composite, (object)changeItem.ContentHash, (object)changeItem.TargetName, (object)changeItem.TargetPath, (object)changeItem.VersionHash, (object)changeItem.IsSeed);
                    if (!deltaItem.IsPlaceHolder)
                    {
                        string asciishA1Hash = Encoding.ASCII.GetBytes(deltaItem.FormatFileName()).ToASCIISHA1Hash();
                        if (!Directory.Exists(this.StagingDirectory))
                            Directory.CreateDirectory(this.StagingDirectory);
                        string str2 = Path.Combine(this.StagingDirectory, string.Format("{0}-{1}.patch", (object)deltaItem.Revision, (object)asciishA1Hash));
                        if (!File.Exists(str2))
                            throw new FileNotFoundException(string.Format("Could not find staged file: {0}", (object)str2));
                        if (File.Exists(str1))
                            File.Delete(str1);
                        File.Move(str2, str1);
                    }
                }
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void AddDelta(
          string name,
          string revision,
          string file,
          bool isSeed,
          bool isPlaceHolder,
          string contentHash,
          string versionHash)
        {
            DeltaItem deltaItem = new DeltaItem()
            {
                Revision = revision,
                IsSeed = isSeed,
                IsPlaceHolder = isPlaceHolder,
                ContentHash = contentHash,
                TargetName = Path.GetFileName(file),
                TargetPath = Path.GetDirectoryName(file),
                VersionHash = versionHash
            };
            this.AddDelta(name, new List<DeltaItem>()
      {
        deltaItem
      });
        }

        public void Initialize(string rootPath)
        {
            this.Root = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(rootPath);
            if (!Path.IsPathRooted(this.Root))
                this.Root = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), rootPath);
            this.StagingDirectory = Path.Combine(this.Root, "____");
            if (!Directory.Exists(this.Root))
                Directory.CreateDirectory(this.Root);
            if (!Directory.Exists(this.StagingDirectory))
                Directory.CreateDirectory(this.StagingDirectory);
            if (ServiceLocator.Instance.GetService<IRepositoryService>() != null)
                return;
            ServiceLocator.Instance.AddService(typeof(IRepositoryService), (object)this);
        }

        public void Dispose()
        {
            foreach (ReaderWriterLockSlim readerWriterLockSlim in this.m_locks.Values)
            {
                try
                {
                    readerWriterLockSlim.Dispose();
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Exception while disposeing read write lock.", ex);
                }
            }
        }

        public void Reset(string name, out bool defferedDelete)
        {
            defferedDelete = false;
            List<RevLog> revLogList = this.GetRevLogList(name);
            HashSet<string> stringSet = new HashSet<string>();
            string current = this.ReadCurrentRevisionInternal(name);
            int index1 = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == current));
            if (index1 > -1)
            {
                for (int index2 = index1; index2 < revLogList.Count; ++index2)
                {
                    foreach (ChangeItem change in revLogList[index2].Changes)
                    {
                        string file = Path.Combine(change.TargetPath, change.TargetName);
                        if (!stringSet.Contains(file))
                        {
                            stringSet.Add(file);
                            string str = this.FormatStagedFileName(file);
                            try
                            {
                                File.Delete(str);
                            }
                            catch (Exception ex)
                            {
                                this.DeleteLockedFileSystemEntry(str);
                                defferedDelete = true;
                            }
                        }
                    }
                }
            }
            string path = this.FormatRepositoryLocationInternal(name);
            if (!Directory.Exists(path))
                return;
            Directory.Delete(path, true);
        }

        public void Reset(out bool defferedDelete)
        {
            defferedDelete = false;
            foreach (string allRepository in this.GetAllRepositories())
                this.Reset(allRepository, out defferedDelete);
            if (Directory.Exists(this.Root))
                Directory.Delete(this.Root, true);
            this.Initialize(this.Root);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RebuildToHead(string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList head = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                head.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", string.Format("repository {0} does not exists.", (object)name), "Pass in a valid direcotry"));
                return head;
            }
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
            {
                head.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
                return head;
            }
            try
            {
                string revisionInternal = this.GetHeadRevisionInternal(name);
                if (revisionInternal == "0000000000000000000000000000000000000000")
                    return new Redbox.UpdateManager.ComponentModel.ErrorList();
                head.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.RebuildToInternal(name, revisionInternal));
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
            return head;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ActivateToHead(
          string name,
          out bool deferredMove)
        {
            deferredMove = false;
            Redbox.UpdateManager.ComponentModel.ErrorList head = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                head.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", string.Format("repository {0} does not exists.", (object)name), "Pass in a valid direcotry"));
                return head;
            }
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
            {
                head.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
                return head;
            }
            try
            {
                string revisionInternal = this.GetHeadRevisionInternal(name);
                if (revisionInternal == "0000000000000000000000000000000000000000")
                    return new Redbox.UpdateManager.ComponentModel.ErrorList();
                head.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.ActivateToInternal(name, revisionInternal, out deferredMove));
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
            return head;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList UpdateToHead(string name)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList head = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                head.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M999", string.Format("repository {0} does not exists.", (object)name), "Pass in a valid direcotry"));
                return head;
            }
            ReaderWriterLockSlim repositoryInternal = this.GetLockForRepositoryInternal(name);
            if (!repositoryInternal.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
            {
                head.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("L999", "Unable to aquire lock.", "Try again when the service is not so busy."));
                return head;
            }
            try
            {
                string revisionInternal = this.GetHeadRevisionInternal(name);
                if (revisionInternal == "0000000000000000000000000000000000000000")
                    return new Redbox.UpdateManager.ComponentModel.ErrorList();
                head.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.UpdateToInternal(name, revisionInternal, false));
            }
            finally
            {
                repositoryInternal.ExitWriteLock();
            }
            return head;
        }

        public bool Subscribed(string name)
        {
            return !File.Exists(Path.Combine(this.FormatRepositoryLocationInternal(name), ".subscription"));
        }

        public void DisableSubscription(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                File.WriteAllText(Path.Combine(this.FormatRepositoryLocationInternal(name), ".subscription"), "0");
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void EndableSubscription(string name)
        {
            ReaderWriterLockSlim readerWriterLockSlim = this.ContainsRepository(name) ? this.GetLockForRepositoryInternal(name) : throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (!readerWriterLockSlim.TryEnterWriteLock(TimeSpan.FromSeconds(30.0)))
                throw new TimeoutException("Unable to aquire read lock.");
            try
            {
                File.Delete(Path.Combine(this.FormatRepositoryLocationInternal(name), ".subscription"));
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        private RepositoryService()
        {
        }

        [DllImport("Kernel32.dll")]
        private static extern bool MoveFileEx(
          string lpExistingFileName,
          string lpNewFileName,
          RepositoryService.MoveFileFlags dwFlags);

        public Redbox.UpdateManager.ComponentModel.ErrorList RebuildToInternal(
          string name,
          string targetHash)
        {
            this.Reset(name, out bool _);
            Directory.CreateDirectory(this.FormatRepositoryLocationInternal(name));
            return ServiceLocator.Instance.GetService<IUpdateService>().Poll();
        }

        private Redbox.UpdateManager.ComponentModel.ErrorList ActivateToInternal(
          string name,
          string targetHash,
          out bool deferredMove)
        {
            deferredMove = false;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
                throw new ArgumentException(string.Format("repository {0} does not exists.", (object)name), nameof(name));
            if (targetHash == "0000000000000000000000000000000000000000")
            {
                LogHelper.Instance.Log("TargetHash = {0} for name = {1} exiting.", (object)targetHash, (object)name);
                return errorList;
            }
            LogHelper.Instance.Log("Activating Repository: {0} to '{1}'", (object)name, (object)targetHash);
            string str1 = this.ReadCurrentRevisionInternal(name);
            if (str1 != targetHash)
            {
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.UpdateToInternal(name, targetHash, false));
                if (errorList.ContainsError())
                {
                    LogHelper.Instance.Log("{3} errors occurred updating {0} from {1} to {2} in activate.", (object)name, (object)str1, (object)targetHash, (object)errorList.Count);
                    errorList.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
                    return errorList;
                }
                str1 = this.ReadCurrentRevisionInternal(name);
            }
            try
            {
                if (this.ReadActiveRevisionInternal(name) == str1)
                    return errorList;
                List<IRevLog> activationsInternal = this.GetPendingActivationsInternal(name);
                HashSet<string> stringSet = new HashSet<string>();
                string label = string.Empty;
                foreach (IRevLog revLog in activationsInternal)
                {
                    if (revLog.Changes.Count > 0)
                        LogHelper.Instance.Log("Activating revision {0}{1}{1}", (object)revLog.Hash, (object)System.Environment.NewLine);
                    foreach (ChangeItem change in revLog.Changes)
                    {
                        if (!change.Composite)
                        {
                            string str2 = change.FormatFileName();
                            if (!stringSet.Contains(str2))
                            {
                                LogHelper.Instance.Log("Activating file {0} to {1}{2}{2}", (object)this.FormatStagedFileName(str2), (object)str2, (object)System.Environment.NewLine);
                                stringSet.Add(str2);
                                string path = this.FormatStagedFileName(str2);
                                if (!File.Exists(path))
                                {
                                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U998", string.Format("Error Activating staged files for repository: {0} to revision: {1}", (object)name, (object)targetHash), string.Format("Staged file does not exist.File:{0}", (object)path)));
                                    this.Reset(name, out bool _);
                                    return errorList;
                                }
                                try
                                {
                                    File.Copy(this.FormatStagedFileName(str2), str2, true);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Instance.Log("{0} is locked by anther process doing a deferred move.", (object)str2);
                                    LogHelper.Instance.Log("Error from activate copy.", ex);
                                    string tempFileName = Path.GetTempFileName();
                                    File.Copy(this.FormatStagedFileName(str2), tempFileName, true);
                                    RepositoryService.MoveLockedFileSystemEntry(tempFileName, str2);
                                    deferredMove = true;
                                }
                            }
                        }
                    }
                    label = revLog.Label;
                    if (revLog.Hash == targetHash)
                        break;
                }
                this.WriteActiveLabelInternal(name, label);
                this.WriteActiveRevisionInternal(name, targetHash);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U999", string.Format("Error Activating staged files for repository: {0} to revision: {1}", (object)name, (object)targetHash), ex));
            }
            return errorList;
        }

        private Redbox.UpdateManager.ComponentModel.ErrorList UpdateToInternal(
          string name,
          string targetHash,
          bool rebuilding)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            if (!this.ContainsRepository(name))
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("A999", string.Format("repository {0} does not exists.", (object)name), "Supply a valid repository"));
                return errorList;
            }
            if (targetHash == "0000000000000000000000000000000000000000")
                return errorList;
            List<RevLog> revLogList = this.GetRevLogList(name);
            if (revLogList.Count < 1)
                return errorList;
            int index1 = revLogList.FindIndex((Predicate<RevLog>)(r => r.Hash == targetHash));
            if (index1 == -1)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("A999", string.Format("No revision found for hash: {0}", (object)targetHash), "Supply a revision repository"));
                return errorList;
            }
            string current = this.ReadCurrentRevisionInternal(name);
            if (current == targetHash)
                return errorList;
            int index2 = revLogList.FindIndex((Predicate<RevLog>)(r => r.Hash == current));
            if (index2 == -1)
                LogHelper.Instance.Log("Moving to target log index {0}.", (object)index1);
            else
                LogHelper.Instance.Log("Moving from current log index {0} to target log index {1}.", (object)index2, (object)index1);
            if (index1 < index2)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("A999", "Moving to earlier version is not supported.", "Supply a revision repository"));
                return errorList;
            }
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.MoveUpInternal(name, index2, index1, (IList<RevLog>)revLogList));
            if (errorList.ContainsError() && !rebuilding)
            {
                LogHelper.Instance.Log("Error Updating {0} from {1} to {2} attempting to rebuild.", (object)name, (object)current, (object)targetHash);
                if (name.Equals("dvd-data", StringComparison.CurrentCultureIgnoreCase))
                {
                    LogHelper.Instance.Log("Skipped resetting dvd-data");
                    return errorList;
                }
                Redbox.UpdateManager.ComponentModel.ErrorList collection = this.RebuildToInternal(name, targetHash);
                if (collection.ContainsError())
                {
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection);
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("F999", "Unable to rebuild tree after update error. Tree is in an inconsistent state.", "Manual intervention is required."));
                    return errorList;
                }
                errorList.Clear();
            }
            if (!errorList.ContainsError())
                this.WriteCurrentRevisionInternal(name, targetHash);
            return errorList;
        }

        private Redbox.UpdateManager.ComponentModel.ErrorList MoveUpInternal(
          string name,
          int current,
          int target,
          IList<RevLog> logList)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
            try
            {
                HashSet<string> stringSet = new HashSet<string>();
                Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
                for (int index1 = current + 1; index1 <= target; ++index1)
                {
                    foreach (ChangeItem change in logList[index1].Changes)
                    {
                        if (change.Composite)
                        {
                            if (index1 == target)
                                throw new InvalidOperationException("The tree can not be upated to a version that has a placeholder patch.");
                        }
                        else
                        {
                            string str = Path.Combine(change.TargetPath, string.Format(".staged_\\{0}", (object)change.TargetName));
                            string targetPath = change.TargetPath;
                            string targetName = change.TargetName;
                            if (!dictionary2.ContainsKey(str))
                            {
                                stringSet.Add(str);
                                if (change.IsSeed)
                                    dictionary2[str] = index1;
                                else if (!File.Exists(str))
                                {
                                    dictionary2[str] = 0;
                                }
                                else
                                {
                                    string stagedHash;
                                    using (FileStream inputStream = new FileStream(str, FileMode.Open, FileAccess.ReadWrite))
                                        stagedHash = inputStream.ToASCIISHA1Hash();
                                    for (int index2 = 0; index2 < target; ++index2)
                                    {
                                        if (logList[index2].Changes.FindIndex((Predicate<ChangeItem>)(c => !c.Composite && c.VersionHash == stagedHash && c.TargetPath == targetPath && c.TargetName == targetName)) > -1)
                                        {
                                            dictionary2[str] = index2 + 1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (string key in stringSet)
                {
                    if (!dictionary2.ContainsKey(key))
                    {
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("P999", string.Format("{0} can not be patched becuase a source version was not found in the revlog.", (object)key), "This repository will now be redownloaded."));
                        return errorList;
                    }
                }
                foreach (KeyValuePair<string, int> keyValuePair in dictionary2)
                {
                    for (int index = keyValuePair.Value; index <= target; ++index)
                    {
                        foreach (ChangeItem change in logList[index].Changes)
                        {
                            if (change.Composite)
                            {
                                if (index == target)
                                    throw new InvalidOperationException("The tree can not be upated to a version that has a placeholder patch.");
                            }
                            else
                            {
                                string str1 = Path.Combine(change.TargetPath, string.Format(".staged_\\{0}", (object)change.TargetName));
                                if (!(str1 != keyValuePair.Key))
                                {
                                    string str2 = Path.Combine(this.FormatRepositoryLocationInternal(name), logList[index].HashFileName(change.FormatFileName()));
                                    string path = Path.Combine(change.TargetPath, ".staged_\\");
                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);
                                    if (change.IsSeed)
                                    {
                                        if (!dictionary1.ContainsKey(str1))
                                            dictionary1[str1] = Path.GetTempFileName();
                                        LogHelper.Instance.Log("Moving seed from {0} to {1}", (object)str2, (object)str1);
                                        File.Copy(str2, dictionary1[str1], true);
                                    }
                                    else
                                    {
                                        if (!dictionary1.ContainsKey(str1))
                                        {
                                            string tempFileName = Path.GetTempFileName();
                                            dictionary1[str1] = tempFileName;
                                            File.Copy(str1, tempFileName, true);
                                        }
                                        LogHelper.Instance.Log("Patching file {0} with {1}", (object)str1, (object)str2);
                                        string tempFileName1 = Path.GetTempFileName();
                                        File.Delete(tempFileName1);
                                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)RepositoryService.ConvertFromXDeltaErrors((List<Redbox.DeltaCompression.Error>)XDeltaHelper.Apply(dictionary1[str1], str2, tempFileName1)));
                                        if (errorList.ContainsError())
                                            return errorList;
                                        File.Delete(dictionary1[str1]);
                                        File.Move(tempFileName1, dictionary1[str1]);
                                    }
                                    string asciishA1Hash;
                                    using (FileStream inputStream = new FileStream(dictionary1[str1], FileMode.Open, FileAccess.ReadWrite))
                                        asciishA1Hash = inputStream.ToASCIISHA1Hash();
                                    if (asciishA1Hash != change.VersionHash)
                                    {
                                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("H999", string.Format("{0} should have SHA1 hash of {1} but it has {2}.", (object)str1, (object)change.VersionHash, (object)asciishA1Hash), "Reset the repository and rebuild it."));
                                        return errorList;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("U999", "An unhandled exception occurred while updating the tree.", ex));
            }
            finally
            {
                foreach (KeyValuePair<string, string> keyValuePair in dictionary1)
                {
                    if (errorList.ContainsError())
                    {
                        File.Delete(keyValuePair.Value);
                    }
                    else
                    {
                        File.Delete(keyValuePair.Key);
                        File.Move(keyValuePair.Value, keyValuePair.Key);
                    }
                }
            }
            return errorList;
        }

        private List<RevLog> GetRevLogList(string name) => this.GetRevLog(name).AsList();

        private PersistentList<RevLog> GetRevLog(string name)
        {
            if (!this.m_revLogs.ContainsKey(name))
            {
                string path = Path.Combine(this.FormatRepositoryLocationInternal(name), ".revlog");
                this.m_revLogs[name] = new PersistentList<RevLog>(path);
            }
            return this.m_revLogs[name];
        }

        private string ReadCurrentRevisionInternal(string name)
        {
            string path = Path.Combine(this.FormatRepositoryLocationInternal(name), ".current");
            if (!File.Exists(path))
                return "0000000000000000000000000000000000000000";
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)fileStream))
                        return streamReader.ReadToEnd().ToObject<string>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadCurrentRevisionInternal.", ex);
                return "0000000000000000000000000000000000000000";
            }
        }

        private string ReadActiveRevisionInternal(string name)
        {
            string path = Path.Combine(this.FormatRepositoryLocationInternal(name), ".active");
            if (!File.Exists(path))
                return "0000000000000000000000000000000000000000";
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)fileStream))
                        return streamReader.ReadToEnd().ToObject<string>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadActiveRevisionInternal.", ex);
                return "0000000000000000000000000000000000000000";
            }
        }

        private void WriteCurrentRevisionInternal(string name, string revision)
        {
            File.WriteAllText(Path.Combine(this.FormatRepositoryLocationInternal(name), ".current"), revision.ToJson());
        }

        private void WriteActiveRevisionInternal(string name, string revision)
        {
            File.WriteAllText(Path.Combine(this.FormatRepositoryLocationInternal(name), ".active"), revision.ToJson());
        }

        private string ReadActiveLabelInternal(string name)
        {
            string path = Path.Combine(this.FormatRepositoryLocationInternal(name), ".label");
            if (!File.Exists(path))
                return string.Empty;
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)fileStream))
                        return streamReader.ReadToEnd().ToObject<string>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in RepositoryService.ReadActiveLabelInternal.", ex);
                return string.Empty;
            }
        }

        private void WriteActiveLabelInternal(string name, string label)
        {
            File.WriteAllText(Path.Combine(this.FormatRepositoryLocationInternal(name), ".label"), label.ToJson());
        }

        private string FormatRepositoryLocationInternal(string name)
        {
            return string.Format("{0}\\{1}", (object)this.Root, (object)name);
        }

        private ReaderWriterLockSlim GetLockForRepositoryInternal(string name)
        {
            lock (this.m_dictLock)
            {
                if (this.m_locks.ContainsKey(name))
                    return this.m_locks[name];
                ReaderWriterLockSlim repositoryInternal = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
                this.m_locks.Add(name, repositoryInternal);
                return repositoryInternal;
            }
        }

        private string GetHeadRevisionInternal(string name)
        {
            List<RevLog> revLogList = this.GetRevLogList(name);
            return revLogList == null || revLogList.Count < 1 ? "0000000000000000000000000000000000000000" : revLogList[revLogList.Count - 1].Hash;
        }

        private List<IRevLog> GetPendingActivationsInternal(string name)
        {
            string activeRevision = this.ReadActiveRevisionInternal(name);
            List<RevLog> revLogList = this.GetRevLogList(name);
            int index1 = revLogList.FindIndex((Predicate<RevLog>)(l => l.Hash == activeRevision));
            List<IRevLog> activationsInternal = new List<IRevLog>();
            for (int index2 = index1 + 1; index2 < revLogList.Count; ++index2)
                activationsInternal.Add((IRevLog)revLogList[index2]);
            return activationsInternal;
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromXDeltaErrors(
          List<Redbox.DeltaCompression.Error> xDeltaErrors)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            xDeltaErrors.ForEach((Action<Redbox.DeltaCompression.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }

        private static void MoveLockedFileSystemEntry(string source, string destination)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            string str1 = service.ExpandProperties(source);
            string str2 = service.ExpandProperties(destination);
            RepositoryService.MoveFileFlags dwFlags = RepositoryService.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
            if (!Directory.Exists(str1) && !Directory.Exists(str2))
                dwFlags |= RepositoryService.MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
            RepositoryService.MoveFileEx(str1, str2, dwFlags);
        }

        public void DeleteLockedFileSystemEntry(string target)
        {
            RepositoryService.MoveFileEx(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(target), (string)null, RepositoryService.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
        }

        private string Root { get; set; }

        private string StagingDirectory { get; set; }

        [Flags]
        private enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8,
        }
    }
}
