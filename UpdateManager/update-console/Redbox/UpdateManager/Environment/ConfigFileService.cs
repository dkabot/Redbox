using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Redbox.UpdateManager.Environment
{
    internal class ConfigFileService : IConfigFileService, IPollRequestReply
    {
        private int _syncID;
        private string _clientConfigFileHash;
        private const string ConfigFile = ".config";
        private const string ConfigFileStaged = ".configstaged";
        private const string StagedDate = ".staged";
        private const string ActiveDate = ".active";
        private const string ArchiveDirectory = "archive";

        public static ConfigFileService Instance => Singleton<ConfigFileService>.Instance;

        public void Initialize(string rootPath)
        {
            this._root = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(rootPath);
            if (!Path.IsPathRooted(this._root))
                this._root = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), rootPath);
            this.MakePathExist();
            if (ServiceLocator.Instance.GetService<IConfigFileService>() != null)
                return;
            ServiceLocator.Instance.AddService(typeof(IConfigFileService), (object)this);
        }

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            ErrorList errors = new ErrorList();
            pollRequestList = new List<PollRequest>();
            try
            {
                List<CCFDTO> clientConfigFileDataList;
                ErrorList ccfdtoList = this.GetCCFDTOList(out clientConfigFileDataList);
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)ccfdtoList);
                string json = clientConfigFileDataList.ToJson();
                string asciishA1Hash = Encoding.ASCII.GetBytes(json).ToASCIISHA1Hash();
                if (!string.IsNullOrEmpty(this._clientConfigFileHash) && this._clientConfigFileHash != asciishA1Hash)
                    this._syncID = 0;
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SetHash());
                if (this._syncID == 0)
                    pollRequestList.Add(new PollRequest()
                    {
                        PollRequestType = PollRequestType.ConfigFiles,
                        SyncId = 0,
                        Data = json
                    });
                else
                    pollRequestList.Add(new PollRequest()
                    {
                        PollRequestType = PollRequestType.ConfigFiles,
                        SyncId = this._syncID
                    });
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", "Unhandled exception occurred in ConfigFileService.GetPollRequest.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errors = new ErrorList();
            try
            {
                if (pollReplyList.ToList<PollReply>().Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.UpdateConfigFiles)).FirstOrDefault<PollReply>() != null)
                {
                    this._syncID = 0;
                    this._clientConfigFileHash = string.Empty;
                    LogHelper.Instance.Log("Config File server is clearing the sync id as requested by the server.");
                    return new ErrorList();
                }
                PollReply instance = pollReplyList.ToList<PollReply>().Where<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.ConfigFileChangeSet)).FirstOrDefault<PollReply>();
                if (instance != null)
                {
                    if (string.IsNullOrEmpty(instance.Data))
                    {
                        errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0092", "Error, A poll reply contained a type of config file changeset without the changeset.", instance.ToJson()));
                    }
                    else
                    {
                        foreach (ConfigFileChangeSetData changeSet in instance.Data.ToObject<List<ConfigFileChangeSetData>>())
                        {
                            ErrorList collection = this.ProcessChangeSet(changeSet);
                            errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection);
                            if (!collection.ContainsError())
                                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.StageChangeSet(changeSet));
                        }
                    }
                    this._syncID = instance.SyncId;
                }
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", "Unhandled exception occurred in ConfigFileService.ProcessPollReply.", ex));
            }
            errors.ToLogHelper();
            return errors;
        }

        public List<long> GetAllConfigFiles()
        {
            List<long> allConfigFiles = new List<long>();
            try
            {
                this.MakePathExist();
                foreach (string directory in Directory.GetDirectories(this._root))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    try
                    {
                        long int64 = Convert.ToInt64(directoryInfo.Name);
                        allConfigFiles.Add(int64);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("(CFS9988) Invalid name in .configfiles directory.  Remove ({0})", (object)directoryInfo.Name), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("(CFS0092) Unhandled exception occurred in ConfigFileService.GetAllConfigFiles.", ex);
            }
            return allConfigFiles;
        }

        public ErrorList ActivateConfigFile(long configFileId)
        {
            ConfigFileChangeSetData changeSet;
            ErrorList fileChangeSetData = this.GetConfigFileChangeSetData(configFileId, out changeSet, out ConfigFileService.ConfigFileStateLabel _, out ConfigFileService.ConfigFileStateLabel _);
            return fileChangeSetData.ContainsError() ? fileChangeSetData : this.ActivateChangeSet(changeSet);
        }

        private ErrorList SetHash()
        {
            List<CCFDTO> clientConfigFileDataList;
            ErrorList ccfdtoList = this.GetCCFDTOList(out clientConfigFileDataList);
            this._clientConfigFileHash = Encoding.ASCII.GetBytes(clientConfigFileDataList.ToJson()).ToASCIISHA1Hash();
            return ccfdtoList;
        }

        private ErrorList GetCCFDTOList(out List<CCFDTO> clientConfigFileDataList)
        {
            ErrorList ccfdtoList = new ErrorList();
            clientConfigFileDataList = new List<CCFDTO>();
            foreach (long allConfigFile in this.GetAllConfigFiles())
            {
                ConfigFileChangeSetData changeSet;
                ConfigFileService.ConfigFileStateLabel stagedState;
                ConfigFileService.ConfigFileStateLabel activeState;
                ErrorList fileChangeSetData = this.GetConfigFileChangeSetData(allConfigFile, out changeSet, out stagedState, out activeState);
                ccfdtoList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)fileChangeSetData);
                if (!fileChangeSetData.ContainsError())
                {
                    CCFDTO ccfdto = new CCFDTO();
                    ccfdto.Id = allConfigFile;
                    ccfdto.Name = changeSet.Name;
                    if (stagedState != null)
                    {
                        ccfdto.SGId = stagedState.GenerationId;
                        ccfdto.SGD = stagedState.Created;
                    }
                    if (activeState != null)
                    {
                        ccfdto.AGId = activeState.GenerationId;
                        ccfdto.AGD = activeState.Created;
                    }
                    clientConfigFileDataList.Add(ccfdto);
                }
            }
            return ccfdtoList;
        }

        private ErrorList GetConfigFileChangeSetData(
          long configFileId,
          out ConfigFileChangeSetData changeSet,
          out ConfigFileService.ConfigFileStateLabel stagedState,
          out ConfigFileService.ConfigFileStateLabel activeState)
        {
            changeSet = (ConfigFileChangeSetData)null;
            stagedState = (ConfigFileService.ConfigFileStateLabel)null;
            activeState = (ConfigFileService.ConfigFileStateLabel)null;
            ErrorList fileChangeSetData = new ErrorList();
            try
            {
                string configFilePath = this.GetConfigFilePath(configFileId);
                string stagedStatePath = this.GetStagedStatePath(configFileId);
                string activeStatePath = this.GetActiveStatePath(configFileId);
                if (!File.Exists(configFilePath))
                    fileChangeSetData.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", string.Format("Configuration file number {0} does not exist.", (object)configFileId), ""));
                if (!File.Exists(stagedStatePath))
                    fileChangeSetData.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", string.Format("Configuration staged date file number {0} does not exist.", (object)configFileId), ""));
                string json1 = File.ReadAllText(configFilePath);
                changeSet = json1.ToObject<ConfigFileChangeSetData>();
                string json2 = File.ReadAllText(stagedStatePath);
                if (string.IsNullOrEmpty(json2))
                    fileChangeSetData.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", string.Format("Configuration staged date file number {0} contents should not be empty.", (object)configFileId), ""));
                else
                    stagedState = json2.ToObject<ConfigFileService.ConfigFileStateLabel>();
                if (File.Exists(activeStatePath))
                {
                    string json3 = File.ReadAllText(activeStatePath);
                    if (!string.IsNullOrEmpty(json3))
                        activeState = json3.ToObject<ConfigFileService.ConfigFileStateLabel>();
                }
            }
            catch (Exception ex)
            {
                fileChangeSetData.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", string.Format("Unhandled exception occurred in ConfigFileService.GetConfigFilechangeSetData for config file {0}.", (object)configFileId), ex));
            }
            if (fileChangeSetData.ContainsError())
                this.ClearConfigFile(configFileId);
            return fileChangeSetData;
        }

        private ErrorList ProcessChangeSet(ConfigFileChangeSetData changeSet)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this.WriteConfigFile(changeSet);
                DateTime utcNow = DateTime.UtcNow;
                this.WriteStagedState(changeSet.OID, changeSet.GenerationOID, new DateTime?(utcNow));
                ConfigFileChangeSetData changeSet1 = changeSet;
                this.WriteConfigFileArchive(changeSet1, changeSet1.GenerationOID);
                this.WriteStagedStateArchive(changeSet.OID, changeSet.GenerationOID, new DateTime?(utcNow));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0089", string.Format("Unhandled exception occurred in ConfigFileService.ProcessChangeSet for config file {0}.", (object)changeSet.OID), ex));
            }
            return errorList;
        }

        private ErrorList ActivateChangeSet(ConfigFileChangeSetData changeSet)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                string activateFilePath = this.GetDestinationActivateFilePath(changeSet);
                if (File.Exists(activateFilePath))
                {
                    DateTime utcNow = DateTime.UtcNow;
                    this.WriteActiveState(changeSet.OID, changeSet.GenerationOID, new DateTime?(utcNow));
                    this.WriteActiveStateArchive(changeSet.OID, changeSet.GenerationOID, new DateTime?(utcNow));
                    File.Delete(activateFilePath);
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0088", string.Format("Unhandled exception occurred in ConfigFileService.ActivateChangeSet for config file {0}.", (object)changeSet.OID), ex));
            }
            return errorList;
        }

        private string GetDestinationFilePath(ConfigFileChangeSetData changeSet)
        {
            string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(changeSet.Path);
            if (!Path.IsPathRooted(str))
                str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), str);
            return Path.ChangeExtension(Path.Combine(str, changeSet.Name), ".config");
        }

        private string GetDestinationActivateFilePath(ConfigFileChangeSetData changeSet)
        {
            string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(changeSet.Path);
            if (!Path.IsPathRooted(str))
                str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), str);
            return Path.ChangeExtension(Path.Combine(str, changeSet.Name), ".configstaged");
        }

        private ErrorList StageChangeSet(ConfigFileChangeSetData changeSet)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                string destinationFilePath = this.GetDestinationFilePath(changeSet);
                string activateFilePath = this.GetDestinationActivateFilePath(changeSet);
                string json = changeSet.ToJson();
                File.WriteAllText(destinationFilePath, json);
                File.WriteAllText(activateFilePath, "This file is a placeholder to indicate that config file activation is needed.");
                this.WriteStagedStateArchive(changeSet.OID, changeSet.GenerationOID, new DateTime?(DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0087", string.Format("Unhandled exception occurred in ConfigFileService.StageChangeSet for config file {0}.", (object)changeSet.OID), ex));
            }
            return errorList;
        }

        private void ClearConfigFile(long configFileId)
        {
            try
            {
                this._syncID = 0;
                LogHelper.Instance.Log("Clearing configFile {0}", (object)configFileId);
                string configFilePath = this.GetConfigFilePath(configFileId);
                string stagedStatePath = this.GetStagedStatePath(configFileId);
                string activeStatePath = this.GetActiveStatePath(configFileId);
                if (File.Exists(configFilePath))
                    File.Delete(configFilePath);
                if (File.Exists(stagedStatePath))
                    File.Delete(stagedStatePath);
                if (File.Exists(activeStatePath))
                    File.Delete(activeStatePath);
                string configDirectory = this.GetConfigDirectory(configFileId);
                if (!Directory.Exists(configDirectory))
                    return;
                Directory.Delete(configDirectory, true);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("(CFS0092) Unhandled exception occurred in ConfigFileService.ClearConfigFile for config file {0}.", ex, (object)configFileId);
            }
        }

        private void WriteConfigFile(ConfigFileChangeSetData changeSet)
        {
            File.WriteAllText(this.GetConfigFilePath(changeSet.OID), changeSet.ToJson());
        }

        private void WriteActiveState(long configFileId, long generationId, DateTime? date)
        {
            ConfigFileService.ConfigFileStateLabel instance = new ConfigFileService.ConfigFileStateLabel()
            {
                GenerationId = generationId,
                Created = date
            };
            File.WriteAllText(this.GetActiveStatePath(configFileId), instance.ToJson());
        }

        private void WriteStagedState(long configFileId, long generationId, DateTime? date)
        {
            ConfigFileService.ConfigFileStateLabel instance = new ConfigFileService.ConfigFileStateLabel()
            {
                GenerationId = generationId,
                Created = date
            };
            File.WriteAllText(this.GetStagedStatePath(configFileId), instance.ToJson());
        }

        private string GetConfigDirectory(long configFileId)
        {
            string path = Path.Combine(this._root, configFileId.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetConfigFilePath(long configFileId)
        {
            return Path.Combine(this.GetConfigDirectory(configFileId), ".config");
        }

        private string GetActiveStatePath(long configFileId)
        {
            return Path.Combine(this.GetConfigDirectory(configFileId), ".active");
        }

        private string GetStagedStatePath(long configFileId)
        {
            return Path.Combine(this.GetConfigDirectory(configFileId), ".staged");
        }

        private void WriteConfigFileArchive(ConfigFileChangeSetData changeSet, long generationId)
        {
            File.WriteAllText(this.GetConfigFilePathArchive(changeSet.OID, generationId), changeSet.ToJson());
        }

        private void WriteActiveStateArchive(long configFileId, long generationId, DateTime? date)
        {
            ConfigFileService.ConfigFileStateLabel instance = new ConfigFileService.ConfigFileStateLabel()
            {
                GenerationId = generationId,
                Created = date
            };
            File.WriteAllText(this.GetActiveStatePathArchive(configFileId, generationId), instance.ToJson());
        }

        private void WriteStagedStateArchive(long configFileId, long generationId, DateTime? date)
        {
            ConfigFileService.ConfigFileStateLabel instance = new ConfigFileService.ConfigFileStateLabel()
            {
                GenerationId = generationId,
                Created = date
            };
            File.WriteAllText(this.GetStagedStatePathArchive(configFileId, generationId), instance.ToJson());
        }

        private string GetConfigDirectoryArchive(long configFileId)
        {
            string path = Path.Combine(this.GetConfigDirectory(configFileId), "archive");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string GetConfigFilePathArchive(long configFileId, long generationId)
        {
            return Path.Combine(this.GetConfigDirectoryArchive(configFileId), generationId.ToString() + ".config");
        }

        private string GetActiveStatePathArchive(long configFileId, long generationId)
        {
            return Path.Combine(this.GetConfigDirectoryArchive(configFileId), generationId.ToString() + ".active");
        }

        private string GetStagedStatePathArchive(long configFileId, long generationId)
        {
            return Path.Combine(this.GetConfigDirectoryArchive(configFileId), generationId.ToString() + ".staged");
        }

        private ConfigFileService()
        {
        }

        private void MakePathExist()
        {
            if (Directory.Exists(this._root))
                return;
            Directory.CreateDirectory(this._root);
        }

        private string _root { get; set; }

        private class ConfigFileStateLabel
        {
            public long GenerationId { get; set; }

            public DateTime? Created { get; set; }
        }
    }
}
