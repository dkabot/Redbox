using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.FileCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.FileSets
{
    internal class ChangeSetFile
    {
        private List<IDownloader> _downloads = new List<IDownloader>();
        private IDownloadService _downloadService;
        private IFileCacheService _fileCacheService;
        private FileSetRevisionDownloader _revisionDownloader;
        private FileSetDownloader _fileSetDownloader;
        private static object _lock = new object();
        private RevisionChangeSet _changeSet;
        private const string FileSetChangeSetExt = ".changeSet";
        private const string FileSetChangeSetPath = "changesets";

        public ChangeSetFile()
        {
        }

        public ChangeSetFile(long fileSetId, long revisionId)
        {
            this._changeSet = new RevisionChangeSet()
            {
                FileSetId = fileSetId,
                RevisionId = revisionId
            };
        }

        public ChangeSetFile(RevisionChangeSet changeSet) => this._changeSet = changeSet;

        internal long FileSetId => this._changeSet.FileSetId;

        internal long RevisionId => this._changeSet.RevisionId;

        internal long PatchRevisionId => this._changeSet.PatchRevisionId;

        internal bool IsStaged
        {
            get
            {
                return this._changeSet.State != ChangesetState.Error && this._changeSet.State >= ChangesetState.Staged;
            }
        }

        internal bool IsActivated() => this._changeSet.State == ChangesetState.Activated;

        internal bool IsInNeedsDependency()
        {
            return this._changeSet.State == ChangesetState.ActivationDependencyCheck;
        }

        internal bool IsInError() => this._changeSet.State == ChangesetState.Error;

        internal static void CleanUp()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                DateTime dateTime = DateTime.Now.AddDays(-10.0);
                string changeSetDirectory = ChangeSetFile.GetChangeSetDirectory();
                if (!Directory.Exists(changeSetDirectory))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.CleanUp", "Error, No directory found at " + changeSetDirectory, ""));
                }
                else
                {
                    foreach (string file in Directory.GetFiles(changeSetDirectory, '*'.ToString() + ".changeSet", SearchOption.TopDirectoryOnly))
                    {
                        if (File.GetCreationTime(file) <= dateTime)
                        {
                            Shared.SafeDelete(file);
                            LogHelper.Instance.Log(string.Format("ChangeSetManager.CleanUp - Deleting file > 60 days old: {0} ", (object)file), LogEntryType.Info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.CleanUp", "An unhandled exception occurred.", ex));
            }
        }

        internal static void CreateChangeSetDirectory()
        {
            string changeSetDirectory = ChangeSetFile.GetChangeSetDirectory();
            if (Directory.Exists(changeSetDirectory))
                return;
            Directory.CreateDirectory(changeSetDirectory);
        }

        internal ErrorList Delete()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                lock (ChangeSetFile._lock)
                    Shared.SafeDelete(this.GetChangeSetPath());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.CleanUp", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal ErrorList CreateFromChangeSet(ClientFileSetRevisionChangeSet set)
        {
            ErrorList fromChangeSet = new ErrorList();
            try
            {
                this._changeSet = ChangeSetFile.ClientToRevisionChangeSet(set);
                this.Save();
            }
            catch (Exception ex)
            {
                fromChangeSet.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CreateFromChangeSet", "An unhandled exception occurred.", ex));
            }
            return fromChangeSet;
        }

        internal ErrorList Save()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                lock (ChangeSetFile._lock)
                    File.WriteAllText(this.GetChangeSetPath(), this._changeSet.ToJson());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.Save", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal static ErrorList GetAllFiles(out List<ChangeSetFile> chageSetFiles)
        {
            chageSetFiles = new List<ChangeSetFile>();
            ErrorList allFiles = new ErrorList();
            try
            {
                lock (ChangeSetFile._lock)
                {
                    ChangeSetFile.CreateChangeSetDirectory();
                    string[] files = Directory.GetFiles(ChangeSetFile.GetChangeSetDirectory(), '*'.ToString() + ".changeSet", SearchOption.TopDirectoryOnly);
                    LogHelper.Instance.Log(string.Format("ChangeSetManager.GetAllFiles json: {0} ", (object)files.ToJson()), LogEntryType.Debug);
                    foreach (string path in files)
                    {
                        try
                        {
                            RevisionChangeSet changeSet = File.ReadAllText(path).ToObject<RevisionChangeSet>();
                            chageSetFiles.Add(new ChangeSetFile(changeSet));
                        }
                        catch (Exception ex)
                        {
                            allFiles.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.GetAllFiles", string.Format("Error parsing changeset file deleting {0} ", (object)path), ex));
                            Shared.SafeDelete(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                allFiles.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.GetAllFiles", "Unhandled exception occurred.", ex));
            }
            return allFiles;
        }

        private void AddDownloads(List<IDownloader> downloads)
        {
            this._downloads.Clear();
            downloads.ForEach((Action<IDownloader>)(d =>
            {
                string[] source = d.DownloadData.Key.Split(new string[1]
          {
          ","
              }, StringSplitOptions.RemoveEmptyEntries);
                if (((IEnumerable<string>)source).Count<string>() <= 1 || Convert.ToInt64(source[1]) != this.FileSetId)
                    return;
                this._downloads.Add(d);
            }));
        }

        private IDownloadService DownloadService
        {
            get
            {
                if (this._downloadService == null)
                    this._downloadService = ServiceLocator.Instance.GetService<IDownloadService>();
                return this._downloadService;
            }
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

        internal FileSetRevisionDownloader RevisionDownloader
        {
            get
            {
                if (this._revisionDownloader == null)
                    this._revisionDownloader = new FileSetRevisionDownloader(this._downloads, this._changeSet.FileSetId, this._changeSet.RevisionId, this._changeSet.PatchRevisionId, this._changeSet.FileHash, this._changeSet.DownloadUrl, this._changeSet.Path, this._changeSet.ContentHash, this._changeSet.DownloadPriority);
                return this._revisionDownloader;
            }
        }

        internal FileSetDownloader FileSetDownloader
        {
            get
            {
                if (this._fileSetDownloader == null)
                {
                    ClientFileSetRevision revision = this.RevisionDownloader.GetRevision();
                    if (revision != null)
                        this._fileSetDownloader = new FileSetDownloader(revision, this._downloads, this._changeSet.DownloadUrl, this._changeSet.DownloadPriority);
                }
                return this._fileSetDownloader;
            }
        }

        internal ErrorList ProcessChangeSet(List<IDownloader> downloads)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this.AddDownloads(downloads);
                if (this._changeSet.State == ChangesetState.Received)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckReceived());
                if (this._changeSet.State == ChangesetState.DownloadingRevision)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckDownloadingRevision());
                if (this._changeSet.State == ChangesetState.DownloadedRevision)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckDownloadedRevision());
                if (this._changeSet.State == ChangesetState.DownloadingFileSet)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckDownloadingFileSet());
                if (this._changeSet.State == ChangesetState.DownloadedFileSet)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckDownloadedFileSet());
                if (this._changeSet.State == ChangesetState.Staging)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckStagingFileSet());
                if (this._changeSet.State == ChangesetState.Staged)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckStagedFileSet());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetManager.ProcessChangeSet", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal ErrorList ProcessActivationDependencyCheck(
          Dictionary<long, FileSetDependencyState> dependencyStates)
        {
            string code = "ChangeSetFile.ProcessActivationDependencyCheck";
            ErrorList errorList = new ErrorList();
            try
            {
                if (this._changeSet.State == ChangesetState.ActivationDependencyCheck)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckActivationDependency(dependencyStates));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList ProcessActivationPending()
        {
            string code = "ChangeSetFile.ProcessActivationPending";
            ErrorList errorList = new ErrorList();
            try
            {
                if (this._changeSet.State == ChangesetState.ActivationPending)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckActivationPending());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList ProcessActivationBeforeActions()
        {
            string code = "ChangeSetFile.ProcessActivationBeforeActions";
            ErrorList errorList = new ErrorList();
            try
            {
                if (this._changeSet.State == ChangesetState.ActivationBeforeActions)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckActivationBeforeActions());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList ProcessActivating()
        {
            string code = "ChangeSetFile.ProcessActivating";
            ErrorList errorList = new ErrorList();
            try
            {
                if (this._changeSet.State == ChangesetState.Activating)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckActivating());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList ProcessActivationAfterActions()
        {
            string code = "ChangeSetFile.ProcessActivationAfterActions";
            ErrorList errorList = new ErrorList();
            try
            {
                if (this._changeSet.State == ChangesetState.ActivationAfterActions)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckActivationAfterActions());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList ProcessActivated()
        {
            string code = "ChangeSetFile.ProcessActivated";
            ErrorList errorList = new ErrorList();
            try
            {
                int state = (int)this._changeSet.State;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(code, "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        private ErrorList CheckReceived()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.Received)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet received fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                if (this.RevisionDownloader.Exists())
                {
                    this.SetState(ChangesetState.DownloadedRevision);
                    return errorList;
                }
                if (!this.RevisionDownloader.Exists() && !this.RevisionDownloader.DownloadExists())
                {
                    IDownloader downloader = this.RevisionDownloader.AddDownload();
                    if (downloader == null)
                    {
                        this.SetError("RevisionDownloader.AddDownloader failed");
                        return errorList;
                    }
                    this._downloads.Add(downloader);
                }
                this.SetState(ChangesetState.DownloadingRevision);
            }
            catch (Exception ex)
            {
                this.SetError("Unhandled exception in CheckReceived");
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(ChangeSetFile), "An unhandled exception occurred in CheckReceived", ex));
            }
            return errorList;
        }

        private ErrorList CheckDownloadingRevision()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.DownloadingRevision)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet downloading fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                if (this.RevisionDownloader.Exists())
                {
                    this.SetState(ChangesetState.DownloadedRevision);
                    return errorList;
                }
                if (!this.RevisionDownloader.DownloadExists())
                {
                    this.SetError("CheckDownloadingRevision - Download doesn't exist when it should");
                    return errorList;
                }
                if (this.RevisionDownloader.IsDownloadError())
                {
                    this.SetError("CheckDownloadingRevision - Download is in an error state");
                    return errorList;
                }
                if (this.RevisionDownloader.IsDownloadComplete())
                {
                    if (this.RevisionDownloader.CompleteDownload())
                    {
                        this.SetState(ChangesetState.DownloadedRevision);
                        return errorList;
                    }
                    this.SetError("CheckDownloadingRevision - CompleteDownload failed");
                    return errorList;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(ChangeSetFile), "An unhandled exception occurred in CheckReceived", ex));
                this.SetError("Unhandled exception in CheckDownloadingRevision");
            }
            return errorList;
        }

        private ErrorList CheckDownloadedRevision()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.DownloadedRevision)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet downloaded fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                if (this.FileSetDownloader.IsDownloaded())
                {
                    this.SetState(ChangesetState.DownloadedFileSet);
                    return errorList;
                }
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.FileSetDownloader.DownloadFileSet());
                if (errorList.ContainsError())
                    this.SetError("An error while starting fileset downloads");
                else
                    this.SetState(ChangesetState.DownloadingFileSet);
                return errorList;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(ChangeSetFile), "An unhandled exception occurred in CheckReceived", ex));
                this.SetError("Unhandled exception in CheckDownloadedRevision");
            }
            return errorList;
        }

        private ErrorList CheckDownloadingFileSet()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.DownloadingFileSet)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet CheckDownloadingFileSet fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.FileSetDownloader.CompleteDownloads());
                if (errorList.ContainsError())
                {
                    this.SetError("Complete downloads failed");
                    return errorList;
                }
                if (this.FileSetDownloader.IsDownloading() || !this.FileSetDownloader.IsDownloaded())
                    return errorList;
                this.SetState(ChangesetState.DownloadedFileSet);
                return errorList;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(ChangeSetFile), "An unhandled exception occurred in CheckReceived", ex));
                this.SetError("Unhandled exception in CheckDownloadingFileSet");
            }
            return errorList;
        }

        private ErrorList CheckDownloadedFileSet()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.DownloadedFileSet)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet CheckDownloadedFileSet fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                this.SetState(ChangesetState.Staging);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CheckDownloadedFileSet", "An unhandled exception occurred", ex));
                this.SetError("Unhandled exception in CheckDownloadedFileSet");
            }
            return errorList;
        }

        private ErrorList CheckStagingFileSet()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.Staging)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet CheckStagingFileSet fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                FileSetTransition fileSetTransition = new FileSetTransition(this.FileSetDownloader.Revision);
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)fileSetTransition.Stage());
                if (errorList.ContainsError())
                {
                    this.SetError("Staging revision failed");
                    return errorList;
                }
                this.SetState(ChangesetState.Staged);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CheckStagingFileSet", "An unhandled exception occurred", ex));
                this.SetError("Unhandled exception in CheckStagingFileSet");
            }
            return errorList;
        }

        private ErrorList CheckStagedFileSet()
        {
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.Staged)
                return errorList;
            LogHelper.Instance.Log(string.Format("ChangeSet CheckStagedFileSet fileset {0} revision {1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId));
            try
            {
                this.SetState(ChangesetState.ActivationDependencyCheck);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile.CheckStagedFileSet", "An unhandled exception occurred", ex));
                this.SetError("Unhandled exception in CheckStagedFileSet");
            }
            return errorList;
        }

        private ErrorList CheckActivationDependency(
          Dictionary<long, FileSetDependencyState> dependencyStates)
        {
            string str = "ChangeSetFile.CheckActivationDependencyCheck";
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.ActivationDependencyCheck)
                return errorList;
            this.Log(str, "Started");
            try
            {
                IEnumerable<ClientFileSetRevisionDependency> dependencies = this.FileSetDownloader.Revision.Dependencies;
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                FileSetTransition fileSetTransition = new FileSetTransition(this.FileSetDownloader.Revision);
                bool meetsDependencies;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)fileSetTransition.CheckMeetsDependency(dependencyStates, out meetsDependencies));
                if (errorList.ContainsError())
                {
                    this.SetError("Activating revision failed");
                    return errorList;
                }
                if (meetsDependencies)
                {
                    this.SetState(ChangesetState.ActivationPending);
                    return errorList;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(str, "An unhandled exception occurred", ex));
                this.SetError(string.Format("Unhandled exception in {0}", (object)str));
            }
            return errorList;
        }

        private ErrorList CheckActivationPending()
        {
            string str = "ChangeSetFile.CheckActivationPending";
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.ActivationPending)
                return errorList;
            this.Log(str, "Started");
            try
            {
                if (this._changeSet.ActiveOn > DateTime.Now)
                {
                    this.Log("ChangeSetFile.CheckActivationPending", string.Format("ActiveOn date ({0}) is in the future.", (object)this._changeSet.ActiveOn));
                    return errorList;
                }
                if (!this.IsValidTime())
                {
                    this.Log("ChangeSetFile.CheckActivationPending", string.Format("ActivationStartTime and ActivationEndTime are not valid yet ActivateStartTime {0} ActivateEndTime {1}.", (object)this._changeSet.ActivateStartTime, (object)this._changeSet.ActivateEndTime));
                    return errorList;
                }
                this.SetState(ChangesetState.ActivationBeforeActions);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(str, "An unhandled exception occurred", ex));
                this.SetError(string.Format("Unhandled exception in {0}", (object)str));
            }
            return errorList;
        }

        private ErrorList CheckActivationBeforeActions()
        {
            string str = "ChangeSetFile.CheckActivationBeforeActions";
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.ActivationBeforeActions)
                return errorList;
            this.Log(str, "Started");
            try
            {
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                FileSetTransition fileSetTransition = new FileSetTransition(this.FileSetDownloader.Revision);
                bool success;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)fileSetTransition.BeforeActivate(out success));
                if (errorList.ContainsError())
                {
                    this.SetError(string.Format("Unhandled exception in {0}", (object)str));
                    return errorList;
                }
                if (success)
                    this.SetState(ChangesetState.Activating);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(str, "An unhandled exception occurred", ex));
                this.SetError(string.Format("Unhandled exception in {0}", (object)str));
            }
            return errorList;
        }

        private ErrorList CheckActivating()
        {
            string str = "ChangeSetFile.CheckActivating";
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.Activating)
                return errorList;
            this.Log(str, "Started");
            try
            {
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                FileSetTransition fileSetTransition = new FileSetTransition(this.FileSetDownloader.Revision);
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)fileSetTransition.Activate());
                if (errorList.ContainsError())
                {
                    this.SetError("Activating revision failed");
                    return errorList;
                }
                this.SetState(ChangesetState.ActivationAfterActions);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(str, "An unhandled exception occurred", ex));
                this.SetError(string.Format("Unhandled exception in {0}", (object)str));
            }
            return errorList;
        }

        private ErrorList CheckActivationAfterActions()
        {
            string str = "ChangeSetFile.CheckActivationAfterActions";
            ErrorList errorList = new ErrorList();
            if (this._changeSet.State != ChangesetState.ActivationAfterActions)
                return errorList;
            this.Log(str, "Started");
            try
            {
                if (this.FileSetDownloader == null)
                {
                    this.SetError("Revision does not exist");
                    return errorList;
                }
                FileSetTransition fileSetTransition = new FileSetTransition(this.FileSetDownloader.Revision);
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)fileSetTransition.AfterActivate());
                errorList.ContainsError();
                this.SetState(ChangesetState.Activated);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(str, "An unhandled exception occurred", ex));
                this.SetState(ChangesetState.Activated);
            }
            return errorList;
        }

        private void Log(string methodName, string message)
        {
            LogHelper.Instance.Log(string.Format("{0} FileSet {1} Revision {2} Message: {3}", (object)methodName, (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId, (object)message));
        }

        private void SetState(ChangesetState state)
        {
            this._changeSet.State = state;
            this.Save();
        }

        internal bool IsValidTime()
        {
            TimeSpan result1;
            TimeSpan result2;
            if (string.IsNullOrEmpty(this._changeSet.ActivateStartTime) || string.IsNullOrEmpty(this._changeSet.ActivateEndTime) || !TimeSpan.TryParse(this._changeSet.ActivateStartTime, out result1) || !TimeSpan.TryParse(this._changeSet.ActivateEndTime, out result2) || result1 == result2)
                return true;
            DateTime now = DateTime.Now;
            return result2 < result1 ? now.TimeOfDay <= result2 || now.TimeOfDay >= result1 : now.TimeOfDay >= result1 && now.TimeOfDay <= result2;
        }

        private void SetError(string message)
        {
            LogHelper.Instance.Log("Setting ChangeSet Error for FileSet: {0} Revision: {1} ({2})", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId, (object)message);
            this._changeSet.Message = message;
            ++this._changeSet.RetryCount;
            if (this._changeSet.RetryCount > 2)
                this.SetState(ChangesetState.Error);
            else
                this.SetState(ChangesetState.Received);
        }

        private static string GetChangeSetDirectory()
        {
            return Path.Combine(FileSetService.Instance.RootPath, "changesets");
        }

        private string GetChangeSetPath()
        {
            return Path.ChangeExtension(Path.Combine(ChangeSetFile.GetChangeSetDirectory(), string.Format("{0}-{1}", (object)this._changeSet.FileSetId, (object)this._changeSet.RevisionId)), ".changeSet");
        }

        internal static RevisionChangeSet ClientToRevisionChangeSet(ClientFileSetRevisionChangeSet cs)
        {
            return new RevisionChangeSet()
            {
                FileSetId = cs.FileSetId,
                RevisionId = cs.RevisionId,
                Received = new DateTime?(DateTime.Now),
                Downloaded = new DateTime?(),
                Staged = new DateTime?(),
                Activated = new DateTime?(),
                State = ChangesetState.Received,
                Message = string.Empty,
                RetryCount = 0,
                Path = cs.Path,
                CompressionType = cs.CompressionType,
                ContentHash = cs.ContentHash,
                FileHash = cs.FileHash,
                PatchRevisionId = cs.PatchRevisionId,
                DownloadOn = cs.DownloadOn.ToLocalTime(),
                DownloadPriority = cs.DownloadPriority,
                ActiveOn = cs.ActiveOn.ToLocalTime(),
                ActivateStartTime = cs.ActivateStartTime,
                ActivateEndTime = cs.ActivateEndTime,
                DownloadUrl = cs.DownloadUrl
            };
        }
    }
}
