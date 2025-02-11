using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.FileCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Redbox.UpdateManager.FileSets
{
    internal class FileSetTransition
    {
        private const string ResultTableName = "Results";
        private ClientFileSetRevision _revision;
        private IFileCacheService _fileCacheService;
        private const string FileSetChangeSetPath = "staging";

        internal FileSetTransition(ClientFileSetRevision revision) => this._revision = revision;

        internal ErrorList Stage()
        {
            ErrorList errors = new ErrorList();
            try
            {
                string fileSetRevisionPath = this.GetStagingFileSetRevisionPath();
                if (!Directory.Exists(fileSetRevisionPath))
                    Directory.CreateDirectory(fileSetRevisionPath);
                List<FileData> fileData = this.GetFileData();
                fileData.ForEach((Action<FileData>)(f =>
                {
                    if (this.FileCacheService.FileExists(this._revision.FileSetId, f.FileId, f.FileRevisionId))
                        return;
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Stage", string.Format("FileId {0} Revision {1} doesn't exist", (object)f.FileId, (object)f.FileRevisionId)));
                }));
                if (errors.ContainsError())
                    return errors;
                fileData.ForEach((Action<FileData>)(f =>
                {
                    if (this.FileCacheService.CopyFileToPath(this._revision.FileSetId, f.FileId, f.FileRevisionId, f.StagePath))
                        return;
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Stage", string.Format("FileId {0} Revision {1} couldn't be copied to it's staging directory", (object)f.FileId, (object)f.FileRevisionId)));
                }));
                if (errors.ContainsError())
                    return errors;
                fileData.ForEach((Action<FileData>)(f =>
                {
                    if (this.CheckHash(f.StagePath, f.Hash))
                        return;
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Stage", string.Format("FileId {0} Revision {1} hash check failed", (object)f.FileId, (object)f.FileRevisionId)));
                }));
                return errors;
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.Stage", "An unhandled exception occurred", ex));
            }
            return errors;
        }

        internal ErrorList CheckMeetsDependency(
          Dictionary<long, FileSetDependencyState> dependencyStates,
          out bool meetsDependencies)
        {
            ErrorList errorList = new ErrorList();
            meetsDependencies = false;
            try
            {
                IEnumerable<ClientFileSetRevisionDependency> dependencies = this._revision.Dependencies;
                if (dependencies.Count<ClientFileSetRevisionDependency>() == 0)
                {
                    meetsDependencies = true;
                    return errorList;
                }
                bool meetsAll = true;
                dependencies.ForEach<ClientFileSetRevisionDependency>((Action<ClientFileSetRevisionDependency>)(dep =>
                {
                    if (!dependencyStates.ContainsKey(dep.DependsOnFileSetId))
                    {
                        LogHelper.Instance.Log(string.Format("FileSetTransition.CheckMeetsDependency FileSet {0} Revision {1} Message: Depends on FileSet {2} not found.", (object)this._revision.FileSetId, (object)this._revision.RevisionId, (object)dep.DependsOnFileSetId));
                        meetsAll = false;
                    }
                    else
                    {
                        if (dependencyStates[dep.DependsOnFileSetId].IsDependencyMet(dep.DependencyType, dep.MinimumVersion, dep.MaximumVersion))
                            return;
                        meetsAll = false;
                    }
                }));
                meetsDependencies = meetsAll;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.CheckMeetsDependency", "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList BeforeActivate(out bool success)
        {
            success = true;
            ErrorList errorList = new ErrorList();
            try
            {
                FileData fileData = this.GetFileData().Where<FileData>((Func<FileData, bool>)(item => Path.GetFileName(item.FileDestination).ToLower() == "beforeactivate.lua")).FirstOrDefault<FileData>();
                if (fileData != null)
                {
                    Dictionary<string, object> result;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.RunScript(fileData.StagePath, out result));
                    bool flag = false;
                    if (result.ContainsKey("Failed"))
                        flag = Convert.ToBoolean(result["Failed"]);
                    string empty = string.Empty;
                    if (result.ContainsKey("Message"))
                        empty = Convert.ToString(result["Message"]);
                    if (result.ContainsKey("Success"))
                        success = Convert.ToBoolean(result["Success"]);
                    if (flag)
                    {
                        success = false;
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.BeforeActivate", "BeforeActivate script returned failed == true", empty));
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.BeforeActivate", "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList AfterActivate()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                FileData fileData = this.GetFileData().Where<FileData>((Func<FileData, bool>)(item => Path.GetFileName(item.FileDestination).ToLower() == "afteractivate.lua")).FirstOrDefault<FileData>();
                if (fileData != null)
                {
                    Dictionary<string, object> result;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.RunScript(fileData.FileDestination, out result));
                    bool flag = false;
                    if (result.ContainsKey("Failed"))
                        flag = Convert.ToBoolean(result["Failed"]);
                    string empty = string.Empty;
                    if (result.ContainsKey("Message"))
                        empty = Convert.ToString(result["Message"]);
                    if (flag)
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.AfterActivate", "BeforeActivate script returned failed == true", empty));
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.AfterActivate", "An unhandled exception occurred", ex));
            }
            return errorList;
        }

        internal ErrorList Activate()
        {
            ErrorList errors = new ErrorList();
            try
            {
                List<FileData> fileData = this.GetFileData();
                fileData.ForEach((Action<FileData>)(f =>
                {
                    if (File.Exists(f.StagePath))
                        return;
                    errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Activate", string.Format("Staged FileId {0} Revision {1} doesn't exist", (object)f.FileId, (object)f.FileRevisionId)));
                }));
                if (errors.ContainsError())
                    return errors;
                fileData.ForEach((Action<FileData>)(f =>
                {
                    string directoryName = Path.GetDirectoryName(f.FileDestination);
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                    try
                    {
                        if (File.Exists(f.FileDestination))
                            File.Delete(f.FileDestination);
                        File.Move(f.StagePath, f.FileDestination);
                    }
                    catch (Exception ex1)
                    {
                        try
                        {
                            LogHelper.Instance.Log("{0} is locked by anther process attempting to retry.", (object)f.FileDestination);
                            LogHelper.Instance.Log("Error from activate copy - retrying.", ex1);
                            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
                            manualResetEvent.WaitOne(TimeSpan.FromMilliseconds(100.0));
                            manualResetEvent.Close();
                            if (File.Exists(f.FileDestination))
                                File.Delete(f.FileDestination);
                            File.Move(f.StagePath, f.FileDestination);
                        }
                        catch (Exception ex2)
                        {
                            LogHelper.Instance.Log("{0} is locked by anther process doing a deferred move.", (object)f.FileDestination);
                            LogHelper.Instance.Log("Error from activate copy.", ex2);
                            string tempFileName = Path.GetTempFileName();
                            if (File.Exists(tempFileName))
                                File.Delete(tempFileName);
                            File.Move(f.StagePath, tempFileName);
                            FileSetTransition.MoveLockedFileSystemEntry(tempFileName, f.FileDestination);
                            FileSetService.Instance.MarkRebootRequired();
                        }
                    }
                }));
                errors.ContainsError();
                return errors;
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.Activate", "An unhandled exception occurred", ex));
            }
            return errors;
        }

        private static void MoveLockedFileSystemEntry(string source, string destination)
        {
            FileSetTransition.MoveFileFlags dwFlags = FileSetTransition.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
            if (!Directory.Exists(source) && !Directory.Exists(destination))
                dwFlags |= FileSetTransition.MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
            FileSetTransition.MoveFileEx(source, destination, dwFlags);
        }

        [DllImport("Kernel32.dll")]
        private static extern bool MoveFileEx(
          string lpExistingFileName,
          string lpNewFileName,
          FileSetTransition.MoveFileFlags dwFlags);

        private List<FileData> GetFileData()
        {
            List<FileData> result = new List<FileData>();
            string path = this.GetStagingFileSetRevisionPath();
            this._revision.Files.ForEach<ClientFileSetFile>((Action<ClientFileSetFile>)(f => result.Add(new FileData()
            {
                FileId = f.FileId,
                FileRevisionId = f.FileRevisionId,
                Size = f.ContentSize,
                Hash = f.ContentHash,
                StagePath = Path.Combine(path, this.FileCacheService.GetFileName(f.FileId, f.FileRevisionId)),
                FileDestination = f.FileDestination
            })));
            return result;
        }

        private ErrorList RunScript(string script, out Dictionary<string, object> result)
        {
            ErrorList errorList = new ErrorList();
            bool scriptCompleted = true;
            result = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(script))
                return errorList;
            IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
            LogHelper.Instance.Log("FileSetTransition.RunScript for FileSet: ", (object)this._revision.FileSetName);
            try
            {
                Dictionary<object, object> result1;
                service.ExecuteChunkNoLock(script, true, "Results", out result1, out scriptCompleted);
                if (result1 != null)
                {
                    Dictionary<string, object> list = new Dictionary<string, object>();
                    result1.ForEach<KeyValuePair<object, object>>((Action<KeyValuePair<object, object>>)(item => list[(string)item.Key] = item.Value));
                    result = list;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.RunScript", string.Format("FileSetTransition.RunScript: unhandled exception in script. Script for FileSet: Name {1}", (object)this._revision.FileSetName), ex));
            }
            return errorList;
        }

        internal IFileCacheService FileCacheService
        {
            get
            {
                if (this._fileCacheService == null)
                    this._fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
                return this._fileCacheService;
            }
        }

        private string GetStagingDirectory()
        {
            return Path.Combine(FileSetService.Instance.RootPath, "staging");
        }

        private string GetStagingFileSetPath()
        {
            return Path.Combine(this.GetStagingDirectory(), this._revision.FileSetId.ToString());
        }

        private string GetStagingFileSetRevisionPath()
        {
            return Path.Combine(this.GetStagingFileSetPath(), this._revision.RevisionId.ToString());
        }

        private bool CheckHash(string filePath, string hash)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                using (FileStream inputStream = File.OpenRead(filePath))
                    return inputStream.ToASCIISHA1Hash() == hash;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(FileSetTransition), "An unhandled exception occurred in CheckHash.", ex));
            }
            return false;
        }

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
