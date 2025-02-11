using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.FileCache;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Redbox.UpdateManager.FileSets
{
    internal class FileSetService : IPollRequestReply, IFileSetService
    {
        private IFileCacheService _fileCacheService;
        private bool _rebootRequired;
        private Timer _timer;
        private bool _isRunning;
        private int _inDoWork;
        private string _root;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _lockTimeout = 3000;

        public static FileSetService Instance => Singleton<FileSetService>.Instance;

        public ErrorList Initialize(string rootPath)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._root = rootPath;
                if (!Directory.Exists(this._root))
                    Directory.CreateDirectory(this._root);
                LogHelper.Instance.Log("FileSet Directory: {0}", (object)rootPath);
                ChangeSetFile.CreateChangeSetDirectory();
                this._timer = new Timer((TimerCallback)(o => this.DoWork()));
                this._isRunning = false;
                ServiceLocator.Instance.AddService(typeof(IFileSetService), (object)this);
                LogHelper.Instance.Log("Initialized the fileset service", LogEntryType.Info);
                IHealthService service = ServiceLocator.Instance.GetService<IHealthService>();
                service.Add("FILESETSERVICE.CLEANUP", new TimeSpan(12, 0, 0), (Action)(() => ChangeSetFile.CleanUp()));
                service.Add("FILESETSERVICE.FILESETCLEANUP", new TimeSpan(12, 0, 0), (Action)(() => new FileSetCleanup().Run()));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS0042", "Unhandled exception occurred in FileSetService.Initialize.", ex));
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(new TimeSpan(0, 0, 35), new TimeSpan(0, 2, 0));
                this._isRunning = true;
                LogHelper.Instance.Log("Starting the fileset service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS998", "An unhandled exception occurred while starting the fileset service.", ex));
            }
            return errorList;
        }

        public ErrorList Stop()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(-1, -1);
                this._isRunning = false;
                LogHelper.Instance.Log("Stopping the fileset service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS997", "An unhandled exception occurred while stopping the fileset service.", ex));
            }
            return errorList;
        }

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            ErrorList source = new ErrorList();
            pollRequestList = new List<PollRequest>();
            try
            {
                FileSetPollRequestList fileSetPollRequestList;
                source = StateFile.GetCurrentState(out fileSetPollRequestList);
                if (source.Count<Redbox.UpdateManager.ComponentModel.Error>() > 0)
                    return source;
                if (fileSetPollRequestList.FileSetPollRequests.Any<FileSetPollRequest>())
                    pollRequestList.Add(new PollRequest()
                    {
                        PollRequestType = PollRequestType.FileSets,
                        Data = fileSetPollRequestList.ToShortFormat()
                    });
                LogHelper.Instance.Log("FileSetService: Store file poll request: " + pollRequestList.ToJson(), LogEntryType.Debug);
            }
            catch (Exception ex)
            {
                source.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.GetPollRequest", "Unhandled exception occurred.", ex));
            }
            return source;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errors = new ErrorList();
            if (!errors.ContainsError() && pollReplyList != null)
            {
                if (pollReplyList.Any<PollReply>())
                {
                    try
                    {
                        IEnumerable<ClientFileSetRevisionChangeSet> revisionChangeSets = pollReplyList.Select<PollReply, ClientFileSetRevisionChangeSet>((Func<PollReply, ClientFileSetRevisionChangeSet>)(pollReply => pollReply.Data.ToObject<ClientFileSetRevisionChangeSet>()));
                        if (revisionChangeSets == null || revisionChangeSets.Count<ClientFileSetRevisionChangeSet>() == 0)
                            return errors;
                        if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                        {
                            errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.ProcessPollReply", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                            return errors;
                        }
                        try
                        {
                            errors.AddRange(this.ProcessChangeSets(revisionChangeSets));
                        }
                        finally
                        {
                            this._lock.ExitWriteLock();
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FS0010", "An unhandled exception occurred in FileSetService.ProcessPollReply.", ex));
                    }
                    errors.ToLogHelper();
                    return errors;
                }
            }
            return errors;
        }

        public string RootPath => this._root;

        private FileSetService()
        {
        }

        private void DoWork()
        {
            try
            {
                if (!this._isRunning)
                    LogHelper.Instance.Log("The fileset service is not running.", LogEntryType.Info);
                else if (Interlocked.CompareExchange(ref this._inDoWork, 1, 0) == 1)
                {
                    LogHelper.Instance.Log("Already in FileSetService.DoWork", LogEntryType.Info);
                }
                else
                {
                    try
                    {
                        ErrorList errorList = new ErrorList();
                        if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                        {
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.DoWork", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                        }
                        else
                        {
                            LogHelper.Instance.Log("Executing FileSetService.DoWork");
                            try
                            {
                                this.ProcessChangeSetWork();
                            }
                            finally
                            {
                                this._lock.ExitWriteLock();
                            }
                            LogHelper.Instance.Log("Finished FileSetService.DoWork");
                        }
                    }
                    finally
                    {
                        this._inDoWork = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an exception in FileSetService.DoWork()", ex);
            }
        }

        private ErrorList ProcessChangeSets(
          IEnumerable<ClientFileSetRevisionChangeSet> changeSets)
        {
            ErrorList errorList = new ErrorList();
            LogHelper.Instance.Log("Executing FileSetService.ProcessChangeSets");
            try
            {
                foreach (ClientFileSetRevisionChangeSet changeSet in changeSets)
                {
                    if (changeSet.Action == FileSetAction.Delete)
                    {
                        ChangeSetFile changeSetFile = new ChangeSetFile(changeSet.FileSetId, changeSet.RevisionId);
                        errorList.AddRange(changeSetFile.Delete());
                        StateFile stateFile = new StateFile(changeSet.FileSetId);
                        errorList.AddRange(stateFile.Delete());
                    }
                    else if (changeSet.Action == FileSetAction.Update)
                        errorList.AddRange(this.FileSetUpdate(changeSet));
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.ProcessChangeSets", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        private ErrorList FileSetUpdate(ClientFileSetRevisionChangeSet set)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                ChangeSetFile changeSetFile = new ChangeSetFile();
                errorList.AddRange(changeSetFile.CreateFromChangeSet(set));
                errorList.AddRange(StateFile.CreateFromChangeSet(set));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.FileSetUpdate", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        private void ProcessChangeSetWork()
        {
            ErrorList errors = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Executing DownloadFileService.ProcessDownloads");
                List<StateFile> list1;
                errors = StateFile.GetAllInProgress(out list1);
                if (errors.Count<Redbox.UpdateManager.ComponentModel.Error>() > 0)
                    return;
                List<ChangeSetFile> chageSetFiles;
                errors = ChangeSetFile.GetAllFiles(out chageSetFiles);
                if (errors.Count<Redbox.UpdateManager.ComponentModel.Error>() > 0)
                    return;
                var bothGood = list1.Join(chageSetFiles, state => new
                {
                    FileSetId = state.FileSetId,
                    RevisionId = state.InProgressRevisionId
                }, set => new
                {
                    FileSetId = set.FileSetId,
                    RevisionId = set.RevisionId
                }, (state, set) => new { State = state, Set = set }).ToList();
                List<StateFile> stateFiles = bothGood.Select(item => item.State).ToList<StateFile>();
                List<ChangeSetFile> changeSetFiles = bothGood.Select(item => item.Set).ToList<ChangeSetFile>();
                list1.Except(stateFiles).ToList().ForEach((item => item.DeleteInProgress()));
                chageSetFiles.Except(changeSetFiles).ToList().ForEach((item => item.Delete()));
                List<IDownloader> downloads = ServiceLocator.Instance.GetService<IDownloadService>().GetDownloads();
                if (bothGood.Count() == 0 && downloads.Count() == 0)
                    return;
                ServiceLocator.Instance.GetService<IKernelService>().Execute((() =>
                {
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessChangeSet(downloads)));
                    Dictionary<long, FileSetDependencyState> dependencyStates = this.GetDependencyStates();
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessActivationDependencyCheck(dependencyStates)));
                    bothGood.ForEach(item =>
            {
                      if (!item.Set.IsInNeedsDependency())
                          return;
                      item.State.SetInProgressState(FileSetState.NeedsDependency);
                  });
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessActivationPending()));
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessActivationBeforeActions()));
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessActivating()));
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessActivationAfterActions()));
                    bothGood.ForEach(item => errors.AddRange(item.Set.ProcessActivated()));
                    bothGood.ForEach(item =>
            {
                      if (!item.Set.IsActivated())
                          return;
                      item.State.Activate();
                      item.Set.Delete();
                  });
                    bothGood.ForEach(item =>
            {
                      if (!item.Set.IsInError())
                          return;
                      item.State.SetInProgressState(FileSetState.Error);
                  });
                    if (this.RebootRequired)
                        errors.AddRange(this.Reboot());
                    downloads.ForEach((d =>
            {
                      if (d.DownloadData.DownloadState != DownloadState.PostDownload)
                          return;
                      d.Complete();
                  }));
                }));
                LogHelper.Instance.Log("Finished executing DownloadFileService.ProcessDownloads");
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.ProcessChangeSetWork", "An unhandled exception occurred", ex));
            }
        }

        private Dictionary<long, FileSetDependencyState> GetDependencyStates()
        {
            ErrorList source = new ErrorList();
            Dictionary<long, FileSetDependencyState> dependencies = new Dictionary<long, FileSetDependencyState>();
            try
            {
                List<StateFile> list;
                source = StateFile.GetAll(out list);
                if (source.Count() > 0)
                    return null;
                List<ChangeSetFile> changeSets;
                source = ChangeSetFile.GetAllFiles(out changeSets);
                if (source.Count() > 0)
                    return null;
                list.ForEach((item =>
                {
                    FileSetDependencyState setDependencyState = new FileSetDependencyState()
                    {
                        FileSetId = item.FileSetId,
                        IsInProgressStaged = false
                    };
                    if (item.HasInProgress())
                    {
                        ChangeSetFile changeSetFile = changeSets.Where((cs => cs.FileSetId == item.FileSetId)).FirstOrDefault();
                        if (changeSetFile != null)
                        {
                            ClientFileSetRevision revision = this.GetRevision(item.FileSetId, changeSetFile.RevisionId, changeSetFile.PatchRevisionId);
                            if (revision != null)
                            {
                                setDependencyState.InProgressRevisionId = changeSetFile.RevisionId;
                                setDependencyState.InProgressVersion = revision.RevisionVersion;
                                setDependencyState.IsInProgressStaged = changeSetFile.IsStaged;
                            }
                        }
                    }
                    if (item.HasActive())
                    {
                        setDependencyState.ActiveRevisionId = item.RevisionId;
                        ClientFileSetRevision revision = this.GetRevision(item.FileSetId, item.RevisionId);
                        if (revision != null)
                            setDependencyState.ActiveVersion = revision.RevisionVersion;
                    }
                    dependencies[item.FileSetId] = setDependencyState;
                }));
                return dependencies;
            }
            catch (Exception ex)
            {
                source.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.GetDependencyState", "An unhandled exception occurred", ex));
            }
            return null;
        }

        private ClientFileSetRevision GetRevision(
          long fileSetId,
          long revisionId,
          long patchRevisionId)
        {
            byte[] data;
            return this.FileCacheService.RevisionExists(fileSetId, revisionId, patchRevisionId) && this.FileCacheService.GetRevision(fileSetId, revisionId, patchRevisionId, out data) ? Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>() : (ClientFileSetRevision)null;
        }

        public ClientFileSetRevision GetRevision(long fileSetId, long revisionId)
        {
            byte[] data;
            return this.FileCacheService.GetRevision(fileSetId, revisionId, out data) ? Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>() : (ClientFileSetRevision)null;
        }

        public ClientFileSetRevision GetRevision(string path)
        {
            byte[] data;
            return this.FileCacheService.GetRevision(path, out data) ? Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>() : (ClientFileSetRevision)null;
        }

        internal FileSetFileInfo GetFileInfo(string path)
        {
            byte[] data;
            return this.FileCacheService.GetFileInfo(path, out data) ? Encoding.ASCII.GetString(data).ToObject<FileSetFileInfo>() : (FileSetFileInfo)null;
        }

        private IFileCacheService FileCacheService
        {
            get
            {
                if (this._fileCacheService == null)
                    this._fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
                return this._fileCacheService;
            }
        }

        public void MarkRebootRequired() => this._rebootRequired = true;

        public bool RebootRequired
        {
            get => this._rebootRequired;
            internal set => this._rebootRequired = value;
        }

        private ErrorList Reboot()
        {
            ErrorList errorList = new ErrorList();
            IKernelService service1 = ServiceLocator.Instance.GetService<IKernelService>();
            IKioskEngineService service2 = ServiceLocator.Instance.GetService<IKioskEngineService>();
            errorList.AddRange(service2.Shutdown(15000, 2));
            if (errorList.ContainsError())
            {
                errorList.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
                return errorList;
            }
            service1.ShutdownType = ShutdownType.Reboot;
            service1.PerformShutdown();
            return errorList;
        }
    }
}
