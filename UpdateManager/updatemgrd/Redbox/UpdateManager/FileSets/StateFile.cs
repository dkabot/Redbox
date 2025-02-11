using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.FileSets
{
    internal class StateFile
    {
        private static object _lock = new object();
        private long _fileSetId;
        private long _revisionId;
        private long _inProgressRevisionId;
        private FileSetState _fileSetState;
        private FileSetState _inProgressFileSetState;
        private const string FileSetStateActiveExt = ".active";
        private const string FileSetStateInProgressExt = ".inprogress";

        internal StateFile(long fileSetId) => this._fileSetId = fileSetId;

        internal long FileSetId => this._fileSetId;

        internal long RevisionId
        {
            get => this._revisionId;
            set => this._revisionId = value;
        }

        internal long InProgressRevisionId
        {
            get => this._inProgressRevisionId;
            set => this._inProgressRevisionId = value;
        }

        internal FileSetState FileSetState
        {
            get => this._fileSetState;
            set => this._fileSetState = value;
        }

        internal FileSetState InProgressFileSetState => this._inProgressFileSetState;

        internal bool HasInProgress() => this._inProgressRevisionId > 0L;

        internal bool HasActive() => this._revisionId > 0L;

        internal ErrorList Activate()
        {
            this._revisionId = this.InProgressRevisionId;
            this._inProgressRevisionId = 0L;
            this.FileSetState = FileSetState.Active;
            this.DeleteInProgress();
            return this.Save();
        }

        internal void SetInProgressState(FileSetState state)
        {
            ClientFileSetState clientFileSetState = new ClientFileSetState()
            {
                FileSetId = this._fileSetId,
                RevisionId = this._revisionId,
                FileSetState = state
            };
            if (state == FileSetState.Active)
            {
                this._revisionId = this._inProgressRevisionId;
                this._inProgressRevisionId = 0L;
                this._fileSetState = state;
            }
            else
                this._inProgressFileSetState = state;
            this.Save();
        }

        internal ErrorList Load()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                lock (StateFile._lock)
                {
                    string activeStatePath = this.GetActiveStatePath();
                    string progressStatePath = this.GetInProgressStatePath();
                    this._revisionId = 0L;
                    this._inProgressRevisionId = 0L;
                    this._fileSetState = FileSetState.Active;
                    this._inProgressFileSetState = FileSetState.InProgress;
                    if (File.Exists(activeStatePath))
                    {
                        try
                        {
                            this._revisionId = File.ReadAllText(activeStatePath).ToObject<ClientFileSetState>().RevisionId;
                        }
                        catch (Exception ex)
                        {
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateFile.Load", string.Format("Error parsing state file deleting {0} ", (object)activeStatePath), ex));
                            Shared.SafeDelete(activeStatePath);
                        }
                    }
                    if (File.Exists(progressStatePath))
                    {
                        try
                        {
                            ClientFileSetState clientFileSetState = File.ReadAllText(progressStatePath).ToObject<ClientFileSetState>();
                            this._inProgressRevisionId = clientFileSetState.RevisionId;
                            this._inProgressFileSetState = clientFileSetState.FileSetState;
                        }
                        catch (Exception ex)
                        {
                            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateFile.Load", string.Format("Error parsing state file deleting {0} ", (object)progressStatePath), ex));
                            Shared.SafeDelete(progressStatePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.Load", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal ErrorList Delete()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                lock (StateFile._lock)
                {
                    Shared.SafeDelete(this.GetActiveStatePath());
                    Shared.SafeDelete(this.GetInProgressStatePath());
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.Delete", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal ErrorList DeleteInProgress()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                lock (StateFile._lock)
                    Shared.SafeDelete(this.GetInProgressStatePath());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.DeleteInProgress", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal ErrorList Save()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                lock (StateFile._lock)
                {
                    string activeStatePath = this.GetActiveStatePath();
                    string progressStatePath = this.GetInProgressStatePath();
                    if (this._revisionId == 0L)
                        Shared.SafeDelete(activeStatePath);
                    else
                        File.WriteAllText(activeStatePath, new ClientFileSetState()
                        {
                            FileSetId = this._fileSetId,
                            RevisionId = this._revisionId,
                            FileSetState = this._fileSetState
                        }.ToJson());
                    if (this._inProgressRevisionId == 0L)
                        Shared.SafeDelete(progressStatePath);
                    else
                        File.WriteAllText(progressStatePath, new ClientFileSetState()
                        {
                            FileSetId = this._fileSetId,
                            RevisionId = this._inProgressRevisionId,
                            FileSetState = this._inProgressFileSetState
                        }.ToJson());
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.Save", "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        internal static ErrorList CreateFromChangeSet(ClientFileSetRevisionChangeSet set)
        {
            ErrorList fromChangeSet = new ErrorList();
            try
            {
                StateFile stateFile = new StateFile(set.FileSetId);
                stateFile.Load();
                stateFile.InProgressRevisionId = set.RevisionId;
                stateFile.SetInProgressState(FileSetState.InProgress);
                stateFile.Save();
            }
            catch (Exception ex)
            {
                fromChangeSet.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.CreateFromState", "An unhandled exception occurred.", ex));
            }
            return fromChangeSet;
        }

        internal static ErrorList GetCurrentState(out FileSetPollRequestList fileSetPollRequestList)
        {
            ErrorList currentState = new ErrorList();
            fileSetPollRequestList = new FileSetPollRequestList();
            try
            {
                lock (StateFile._lock)
                {
                    List<ClientFileSetState> stateFiles;
                    currentState.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StateFile.GetAllFiles(out stateFiles));
                    if (currentState.ContainsError())
                        return currentState;
                    Dictionary<long, FileSetPollRequest> list = new Dictionary<long, FileSetPollRequest>();
                    stateFiles.Where<ClientFileSetState>((Func<ClientFileSetState, bool>)(item => item.FileSetState == FileSetState.Active)).ForEach<ClientFileSetState>((Action<ClientFileSetState>)(item =>
                    {
                        FileSetPollRequest fileSetPollRequest = new FileSetPollRequest()
                        {
                            FileSetId = item.FileSetId,
                            FileSetRevisionId = item.RevisionId,
                            FileSetState = item.FileSetState
                        };
                        list[item.FileSetId] = fileSetPollRequest;
                    }));
                    stateFiles.Where<ClientFileSetState>((Func<ClientFileSetState, bool>)(item => item.FileSetState != 0)).ForEach<ClientFileSetState>((Action<ClientFileSetState>)(item =>
                    {
                        FileSetPollRequest fileSetPollRequest = new FileSetPollRequest()
                        {
                            FileSetId = item.FileSetId,
                            FileSetRevisionId = item.RevisionId,
                            FileSetState = item.FileSetState
                        };
                        list[item.FileSetId] = fileSetPollRequest;
                    }));
                    FileSetPollRequestList requestList = new FileSetPollRequestList();
                    list.ForEach<KeyValuePair<long, FileSetPollRequest>>((Action<KeyValuePair<long, FileSetPollRequest>>)(item => requestList.FileSetPollRequests.Add(item.Value)));
                    fileSetPollRequestList = requestList;
                }
            }
            catch (Exception ex)
            {
                currentState.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetCurrentState", "Unhandled exception occurred", ex));
            }
            return currentState;
        }

        internal static ErrorList GetAllInProgress(out List<StateFile> list)
        {
            ErrorList allInProgress = new ErrorList();
            list = new List<StateFile>();
            try
            {
                List<StateFile> list1;
                allInProgress.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StateFile.GetAll(out list1));
                if (allInProgress.ContainsError())
                    return allInProgress;
                list = list1.Where<StateFile>((Func<StateFile, bool>)(item => item.HasInProgress())).ToList<StateFile>();
            }
            catch (Exception ex)
            {
                allInProgress.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetCurrentState", "Unhandled exception occurred", ex));
            }
            return allInProgress;
        }

        internal static ErrorList GetAll(out List<StateFile> list)
        {
            ErrorList all = new ErrorList();
            list = new List<StateFile>();
            try
            {
                lock (StateFile._lock)
                {
                    List<ClientFileSetState> stateFiles = (List<ClientFileSetState>)null;
                    all.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StateFile.GetAllFiles(out stateFiles));
                    if (all.ContainsError())
                        return all;
                    List<long> list1 = stateFiles.Select<ClientFileSetState, long>((Func<ClientFileSetState, long>)(a => a.FileSetId)).Distinct<long>().ToList<long>();
                    List<StateFile> tempList = list;
                    Action<long> action = (Action<long>)(fileSetId =>
                    {
                        StateFile stateFile = new StateFile(fileSetId);
                        if (stateFile.Load().ContainsError())
                        {
                            Shared.SafeDelete(StateFile.GetActiveStatePath(fileSetId));
                            Shared.SafeDelete(StateFile.GetInProgressStatePath(fileSetId));
                        }
                        else
                            tempList.Add(stateFile);
                    });
                    list1.ForEach(action);
                }
            }
            catch (Exception ex)
            {
                all.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAll", "Unhandled exception occurred", ex));
            }
            return all;
        }

        private static ErrorList GetAllFiles(out List<ClientFileSetState> stateFiles)
        {
            stateFiles = new List<ClientFileSetState>();
            ErrorList allFiles = new ErrorList();
            try
            {
                if (!Directory.Exists(FileSetService.Instance.RootPath))
                {
                    allFiles.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAllFiles", "No directory found at " + FileSetService.Instance.RootPath, "No fileset state will be sent to the server"));
                    return allFiles;
                }
                string[] files1 = Directory.GetFiles(FileSetService.Instance.RootPath, '*'.ToString() + ".active", SearchOption.TopDirectoryOnly);
                string[] files2 = Directory.GetFiles(FileSetService.Instance.RootPath, '*'.ToString() + ".inprogress", SearchOption.TopDirectoryOnly);
                List<string> instance = new List<string>();
                ((IEnumerable<string>)files1).ForEach<string>(new Action<string>(instance.Add));
                Action<string> action = new Action<string>(instance.Add);
                ((IEnumerable<string>)files2).ForEach<string>(action);
                LogHelper.Instance.Log(string.Format("StateManager.GetAllFiles json: {0} ", (object)instance.ToJson()), LogEntryType.Debug);
                foreach (string path in instance)
                {
                    try
                    {
                        ClientFileSetState clientFileSetState = File.ReadAllText(path).ToObject<ClientFileSetState>();
                        stateFiles.Add(clientFileSetState);
                    }
                    catch (Exception ex)
                    {
                        allFiles.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAllFiles", string.Format("Error parsing state file deleting {0} ", (object)path), ex));
                        Shared.SafeDelete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                allFiles.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAllFiles", "Unhandled exception occurred.", ex));
            }
            return allFiles;
        }

        private string GetActiveStatePath() => StateFile.GetActiveStatePath(this._fileSetId);

        private string GetInProgressStatePath() => StateFile.GetInProgressStatePath(this._fileSetId);

        private static string GetActiveStatePath(long fileSetId)
        {
            return Path.ChangeExtension(Path.Combine(FileSetService.Instance.RootPath, fileSetId.ToString()), ".active");
        }

        private static string GetInProgressStatePath(long fileSetId)
        {
            return Path.ChangeExtension(Path.Combine(FileSetService.Instance.RootPath, fileSetId.ToString()), ".inprogress");
        }
    }
}
