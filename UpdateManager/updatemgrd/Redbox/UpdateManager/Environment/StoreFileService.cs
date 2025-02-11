using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.Environment
{
    internal class StoreFileService : IPollRequestReply
    {
        private string m_root;
        private const string StagedStoreFileExt = ".staged";
        private const string StoreFileStateExt = ".state";

        public static StoreFileService Instance => Singleton<StoreFileService>.Instance;

        public ErrorList Initialize(string rootPath)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this.m_root = rootPath;
                if (!Directory.Exists(this.m_root))
                    Directory.CreateDirectory(this.m_root);
                if (!Directory.Exists(this.m_root))
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0041", "The root directory is not set up properly", "Please check initialization values"));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0042", "Unhandled exception occurred in StoreFileService.Initialize.", ex));
            }
            return errorList;
        }

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            ErrorList pollRequest = new ErrorList();
            pollRequestList = new List<PollRequest>();
            try
            {
                StoreFilePollRequestList storeFilePollRequestList;
                pollRequest = this.GetCurrentState(out storeFilePollRequestList);
                if (storeFilePollRequestList.StoreFilePollRequests.Any<StoreFilePollRequest>())
                    pollRequestList.Add(new PollRequest()
                    {
                        PollRequestType = PollRequestType.StoreFiles,
                        Data = storeFilePollRequestList.ToShortFormat()
                    });
                LogHelper.Instance.Log("StoreFileService: Store file poll request: " + pollRequestList.ToJson(), LogEntryType.Debug);
            }
            catch (Exception ex)
            {
                pollRequest.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0042", "Unhandled exception occurred in StoreFileService.Initialize.", ex));
            }
            return pollRequest;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errors = new ErrorList();
            try
            {
                foreach (PollReply instance in pollReplyList.Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.StoreFileChangeSet)))
                {
                    if (string.IsNullOrEmpty(instance.Data))
                    {
                        errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0092", "Error, A poll reply contained a type of store file changeset without the changeset.", instance.ToJson()));
                    }
                    else
                    {
                        StoreFileChangeSetData changeSet = instance.Data.ToObject<StoreFileChangeSetData>();
                        errors.AddRange(this.ProcessChangeSet(changeSet));
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0091", "Unhandled exception occurred in StoreFileService.ProcessPollReply.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        private StoreFileService()
        {
        }

        private IEnumerable<Redbox.UpdateManager.ComponentModel.Error> Activate(long id)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                string path1 = Path.Combine(this.m_root, id.ToString());
                string path2 = Path.ChangeExtension(path1, ".state");
                string str = Path.ChangeExtension(path1, ".staged");
                if (!File.Exists(path2))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0311", "No state file for ID: " + (object)id, "Check the id you are trying to activate"));
                    return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
                }
                if (!File.Exists(str))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0312", "No store file for ID: " + (object)id, "Check the id you are trying to activate"));
                    return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
                }
                StoreFileState instance = File.ReadAllText(path2).ToObject<StoreFileState>();
                string directoryName = Path.GetDirectoryName(instance.Destination);
                if (Directory.Exists(instance.Destination))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0316", "This store file's destination is a directory and not a path: " + (object)id, "Specify a full path, not just a directory for store files"));
                    instance.State = FileState.NoDestinationPath;
                }
                else if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0315", "No directory found at: " + directoryName, string.Empty));
                    instance.State = FileState.NoDestinationDirectory;
                }
                else
                {
                    FileState fileState;
                    bool flag;
                    switch (instance.State)
                    {
                        case FileState.Staged:
                            fileState = FileState.ActivationFailedOnce;
                            flag = true;
                            break;
                        case FileState.ActivationFailedOnce:
                            fileState = FileState.ActivationFailedTwice;
                            flag = true;
                            break;
                        case FileState.ActivationFailedTwice:
                            fileState = FileState.ActivationFailedNoRetry;
                            flag = true;
                            break;
                        default:
                            fileState = FileState.Unknown;
                            flag = false;
                            break;
                    }
                    if (flag)
                    {
                        try
                        {
                            File.Copy(str, instance.Destination, true);
                            instance.State = FileState.Activated;
                            LogHelper.Instance.Log("StoreFileService: file activated for ID: " + (object)id, LogEntryType.Debug);
                        }
                        catch (Exception ex)
                        {
                            instance.State = fileState;
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0313", "File copy failed for store file ID: " + (object)id, ex));
                        }
                    }
                }
                instance.Timestamp = DateTime.Now;
                File.WriteAllText(path2, instance.ToJson());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0032", "Unhandled exception occurred in StoreFileService.Activate.", ex));
            }
            return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
        }

        private ErrorList GetCurrentState(
          out StoreFilePollRequestList storeFilePollRequestList)
        {
            ErrorList currentState = new ErrorList();
            storeFilePollRequestList = new StoreFilePollRequestList();
            try
            {
                if (!Directory.Exists(this.m_root))
                {
                    currentState.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("GCS02", "No directory found at " + this.m_root, "No storefile state will be sent to the server"));
                    return currentState;
                }
                string[] files = Directory.GetFiles(this.m_root, '*'.ToString() + ".state", SearchOption.TopDirectoryOnly);
                LogHelper.Instance.Log("StoreFileService: GetCurrentState files: " + files.ToJson(), LogEntryType.Debug);
                foreach (string path in files)
                {
                    try
                    {
                        StoreFileState storeFileState = File.ReadAllText(path).ToObject<StoreFileState>();
                        if (storeFileState.State == FileState.Staged || storeFileState.State == FileState.ActivationFailedOnce || storeFileState.State == FileState.ActivationFailedTwice)
                            currentState.AddRange(this.Activate(storeFileState.StoreFilePollRequest.StoreFile));
                        storeFilePollRequestList.StoreFilePollRequests.Add(storeFileState.StoreFilePollRequest);
                        LogHelper.Instance.Log("StoreFileService: StoreFilePollRequest added: " + storeFileState.StoreFilePollRequest.ToJson(), LogEntryType.Debug);
                    }
                    catch (Exception ex)
                    {
                        currentState.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("GCS01", "Error parsing storefile state file at " + path, ex));
                        LogHelper.Instance.Log("Error parsing storefile state file at " + path, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                currentState.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("GCS02", "Unhandled exception occurred in GetCurrentState()", ex));
            }
            return currentState;
        }

        private IEnumerable<Redbox.UpdateManager.ComponentModel.Error> ProcessChangeSet(
          StoreFileChangeSetData changeSet)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                if (changeSet == null)
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("PCS01", "ChangeSet is null", "Make sure that there is a changeset to process"));
                    return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
                }
                string path = Path.Combine(this.m_root, changeSet.StoreFile.ToString());
                string str1 = Path.ChangeExtension(path, ".state");
                string str2 = Path.ChangeExtension(path, ".staged");
                switch (changeSet.Action)
                {
                    case StoreFileAction.Update:
                        errorList.AddRange(this.UpdateStoreFile(changeSet, str1, str2));
                        break;
                    case StoreFileAction.Delete:
                        string destinationPath = string.Empty;
                        try
                        {
                            destinationPath = File.ReadAllText(str1).ToObject<StoreFileState>().Destination;
                            break;
                        }
                        catch (Exception ex)
                        {
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("PCS02", "An exception occurred in parsing the state file at: " + str1, ex));
                            break;
                        }
                        finally
                        {
                            errorList.AddRange(StoreFileService.DeleteStoreFile(destinationPath, str1, str2));
                        }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0092", "Unhandled exception occurred in StoreFileService.ProcessChangeSet.", ex));
            }
            return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
        }

        private static IEnumerable<Redbox.UpdateManager.ComponentModel.Error> DeleteStoreFile(
          string destinationPath,
          string statePath,
          string filePath)
        {
            ErrorList errorList = new ErrorList();
            errorList.AddRange(StoreFileService.Delete(destinationPath));
            errorList.AddRange(StoreFileService.Delete(filePath));
            errorList.AddRange(StoreFileService.Delete(statePath));
            return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
        }

        private static IEnumerable<Redbox.UpdateManager.ComponentModel.Error> Delete(string path)
        {
            List<Redbox.UpdateManager.ComponentModel.Error> errorList = new List<Redbox.UpdateManager.ComponentModel.Error>();
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SFS0099", "Error trying to delete path: " + path, ex));
            }
            return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
        }

        private IEnumerable<Redbox.UpdateManager.ComponentModel.Error> UpdateStoreFile(
          StoreFileChangeSetData changeSet,
          string statePath,
          string stagedFilePath)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                byte[] numArray = Convert.FromBase64String(changeSet.Base64Data);
                if (!string.Equals(numArray.ToASCIISHA1Hash(), changeSet.DataHash, StringComparison.Ordinal))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("USF03", "Hash check failed for storefile changeset " + changeSet.Name, "Aborting storefile update"));
                    return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
                }
                StoreFileState instance = new StoreFileState()
                {
                    Destination = changeSet.Path,
                    State = FileState.Staged,
                    Timestamp = DateTime.Now,
                    StoreFilePollRequest = new StoreFilePollRequest()
                    {
                        StoreFile = changeSet.StoreFile,
                        StoreFileData = changeSet.StoreFileData,
                        SyncId = changeSet.SyncId
                    }
                };
                File.WriteAllBytes(stagedFilePath, numArray);
                File.WriteAllText(statePath, instance.ToJson());
                errorList.AddRange(this.Activate(changeSet.StoreFile));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("USF01", "Unknown error in Updating StoreFile", ex));
            }
            return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList;
        }
    }
}
