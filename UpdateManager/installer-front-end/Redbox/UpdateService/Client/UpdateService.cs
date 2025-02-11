using Microsoft.Win32;
using Redbox.Compression;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.UpdateService.Client
{
    internal class UpdateService : IDisposable
    {
        private const bool TransientSessionFlag = false;
        private ClientSession m_clientSession;
        private static readonly Dictionary<Type, string> m_typeToCommandForm = new Dictionary<Type, string>()
    {
      {
        typeof (Repository),
        "repository"
      },
      {
        typeof (Store),
        "store"
      },
      {
        typeof (StoreGroup),
        "store-group"
      },
      {
        typeof (RepositoryEntry),
        "file"
      },
      {
        typeof (RevLog),
        "revlog"
      },
      {
        typeof (Script),
        "update-script"
      },
      {
        typeof (Patch),
        "patch"
      },
      {
        typeof (Label),
        "label"
      },
      {
        typeof (TransferStatistic),
        "transfer-statistic"
      },
      {
        typeof (Release),
        "release"
      }
    };

        public static Redbox.UpdateService.Client.UpdateService GetService(string url)
        {
            return new Redbox.UpdateService.Client.UpdateService(url) { Timeout = 30000 };
        }

        public static Redbox.UpdateService.Client.UpdateService GetService(string url, int timeout)
        {
            return new Redbox.UpdateService.Client.UpdateService(url) { Timeout = timeout };
        }

        public void Dispose()
        {
            if (this.Session == null)
                return;
            if (this.Session.IsDisposed)
                return;
            try
            {
                this.Session.Dispose();
            }
            catch
            {
            }
        }

        public ClientCommandResult List<T>(out System.Collections.Generic.List<Identifier> list)
        {
            list = new System.Collections.Generic.List<Identifier>();
            if (!Redbox.UpdateService.Client.UpdateService.m_typeToCommandForm.ContainsKey(typeof(T)))
            {
                ClientCommandResult clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("U334", string.Format("The type '{0}' does not have a command form mapping.", (object)typeof(T).FullName), "Specify a valid type."));
                return clientCommandResult;
            }
            ClientCommandResult clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("{0} list", (object)Redbox.UpdateService.Client.UpdateService.m_typeToCommandForm[typeof(T)]));
            if (clientCommandResult1.Success)
                list = clientCommandResult1.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return clientCommandResult1;
        }

        public ClientCommandResult Get<T>(Identifier id, out T t)
        {
            t = default(T);
            if (!Redbox.UpdateService.Client.UpdateService.m_typeToCommandForm.ContainsKey(typeof(T)))
            {
                ClientCommandResult clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("U334", string.Format("The type '{0}' does not have a command form mapping.", (object)typeof(T).FullName), "Specify a valid type."));
                return clientCommandResult;
            }
            ClientCommandResult clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("{0} get id: {1}", (object)Redbox.UpdateService.Client.UpdateService.m_typeToCommandForm[typeof(T)], (object)id.ID));
            if (clientCommandResult1.Success)
                t = clientCommandResult1.CommandMessages[0].ToObject<T>();
            return clientCommandResult1;
        }

        public ClientCommandResult GetSubscriptionStatus(string name, out SubscriptionStatus state)
        {
            state = new SubscriptionStatus();
            ClientCommandResult subscriptionStatus = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("subscription status name: '{0}'", (object)name));
            if (subscriptionStatus.Success)
                state = subscriptionStatus.CommandMessages[0].ToObject<SubscriptionStatus>();
            return subscriptionStatus;
        }

        public ClientCommandResult ListTransferStatisticByStoreAndRepository(
          string number,
          string name,
          out System.Collections.Generic.List<Identifier> ids)
        {
            ids = new System.Collections.Generic.List<Identifier>();
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("transfer-statistic get-store-repository-report name: '{0}' number: '{1}'", (object)name, (object)number));
            if (clientCommandResult.Success)
                ids = clientCommandResult.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return clientCommandResult;
        }

        public ClientCommandResult CheckStoreExists(string number, out bool exists)
        {
            exists = false;
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store get name: '{0}'", (object)number));
            if (!clientCommandResult.Success)
                return clientCommandResult;
            exists = true;
            return clientCommandResult;
        }

        public ClientCommandResult DownloadWorkResult(Identifier id, string targetFile)
        {
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("work-result download id: {0}", (object)id.ID));
            if (clientCommandResult.Success)
                File.WriteAllText(targetFile, clientCommandResult.CommandMessages[0]);
            return clientCommandResult;
        }

        public ClientCommandResult EfAddStoreToRelease(string name, string number, string user)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store ef-add-to-release number:'{0}' name:'{1}' user:'{2}'", (object)number, (object)name, (object)user));
        }

        public ClientCommandResult AddStoreToRelease(string name, string number)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store add-to-release name: '{0}' number: '{1}'", (object)name, (object)number));
        }

        public ClientCommandResult StartStoreInstaller(
          string kioskId,
          string repositoryHash,
          string frontEndVersion)
        {
            string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(kioskId);
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("installer start hash: '{0}' version: '{1}' h: '{2}'", (object)repositoryHash, (object)frontEndVersion, (object)credentialHash));
        }

        public ClientCommandResult InstallerLog(
          string kioskId,
          string guid,
          string name,
          string value)
        {
            string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(kioskId);
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("installer log guid:'{0}' name:'{1}' value:'{2}' h: '{2}'", (object)guid, (object)name, (object)value, (object)credentialHash));
        }

        public ClientCommandResult CreateNewSetting(string name, string value)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("setting set name: '{0}' value: '{1}'", (object)name, (object)value));
        }

        public ClientCommandResult CreateNewSetting(string name, string value, string filter)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("setting set name: '{0}' value: '{1}' filter: '{2}'", (object)name, (object)value, (object)filter));
        }

        public ClientCommandResult ListStores(int maxItems, long? startId, out System.Collections.Generic.List<Identifier> stores)
        {
            stores = new System.Collections.Generic.List<Identifier>();
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store list maxitems: {0}{1}", (object)maxItems, startId.HasValue ? (object)string.Format("startid: {0}", (object)startId.Value) : (object)string.Empty));
            if (clientCommandResult.Success)
                stores = clientCommandResult.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return clientCommandResult;
        }

        public ClientCommandResult GetLastCheckIn(string number, out DateTime lastCheckIn)
        {
            lastCheckIn = DateTime.UtcNow;
            ClientCommandResult lastCheckIn1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store last-check-in number: '{0}'", (object)number));
            if (lastCheckIn1.Success)
                lastCheckIn = lastCheckIn1.CommandMessages[0].ToObject<DateTime>();
            return lastCheckIn1;
        }

        public ClientCommandResult CreateSubscription(
          long generatorId,
          string name,
          string repository,
          string fileName,
          string path,
          string createdBy,
          string system,
          string displayName)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("SUBSCRIPTION create generatorId: {0} name: '{1}' repositoryName: '{2}' fileName: '{3}' path: '{4}' createdBy: '{5}' system: '{6}' displayName: '{7}'", (object)generatorId, (object)name, (object)repository, (object)fileName, (object)path, (object)createdBy, (object)system, (object)displayName));
        }

        public ClientCommandResult CreateFileGenerator(string type, string createdBy, string system)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("FILE-GENERATOR create type: '{0}' createdBy: '{1}' system: '{2}'", (object)type, (object)createdBy, (object)system));
        }

        public ClientCommandResult DeleteFile(
          string repositoryName,
          string fullPath,
          string user,
          string system,
          out RepositoryEntry entry)
        {
            System.Collections.Generic.List<Identifier> files;
            ClientCommandResult filesForRepository = this.GetFilesForRepository(repositoryName, out files);
            if (!filesForRepository.Success)
            {
                entry = new RepositoryEntry();
                return filesForRepository;
            }
            System.Collections.Generic.List<RepositoryEntry> repositoryEntryList = new System.Collections.Generic.List<RepositoryEntry>();
            foreach (Identifier id in files)
            {
                RepositoryEntry t;
                filesForRepository = this.Get<RepositoryEntry>(id, out t);
                if (filesForRepository.Success)
                    repositoryEntryList.Add(t);
            }
            RepositoryEntry repositoryEntry = repositoryEntryList.Find((Predicate<RepositoryEntry>)(f => Path.Combine(f.Path, f.Name) == fullPath));
            if (repositoryEntry != null)
                return this.DeleteFile(repositoryEntry.ID, user, system, out entry);
            filesForRepository.Success = false;
            filesForRepository.Errors.Add(Redbox.IPC.Framework.Error.NewError("M100", string.Format("No file found with path: {0} in repository {1}", (object)fullPath, (object)repositoryName), "Supply a valid file id and retry your command."));
            entry = new RepositoryEntry();
            return filesForRepository;
        }

        public ClientCommandResult DeleteFile(
          long id,
          string user,
          string system,
          out RepositoryEntry entry)
        {
            entry = new RepositoryEntry();
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("file delete id: {0} modifiedby: '{1}' system: '{2}'", (object)id, (object)user, (object)system));
            if (clientCommandResult.Success)
                entry = clientCommandResult.CommandMessages[0].ToObject<RepositoryEntry>();
            return clientCommandResult;
        }

        public ClientCommandResult Commit(
          string name,
          LocalChangeSet set,
          string user,
          string system,
          string comment,
          out RevLog log)
        {
            log = new RevLog();
            System.Collections.Generic.List<Dictionary<string, string>> instance = new System.Collections.Generic.List<Dictionary<string, string>>();
            foreach (KeyValuePair<string, Dictionary<string, string>> newFile in set.m_newFiles)
                instance.Add(newFile.Value);
            foreach (KeyValuePair<long, Dictionary<string, string>> update in set.m_updates)
            {
                update.Value.Add("ID", update.Key.ToString());
                instance.Add(update.Value);
            }
            foreach (Dictionary<string, string> dictionary in instance)
            {
                ClientCommandResult clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, "data-upload begin-upload");
                if (!clientCommandResult1.Success)
                    return clientCommandResult1;
                CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
                long int64 = Convert.ToInt64(clientCommandResult1.CommandMessages[0]);
                using (FileStream file = File.OpenRead(dictionary["Data"]))
                {
                    byte[] numArray1 = new byte[10485760];
                    for (int length = Redbox.UpdateService.Client.UpdateService.ReadChunk((Stream)file, numArray1); length > 0; length = Redbox.UpdateService.Client.UpdateService.ReadChunk((Stream)file, numArray1))
                    {
                        byte[] numArray2 = new byte[length];
                        Array.Copy((Array)numArray1, (Array)numArray2, numArray2.Length);
                        ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("data-upload append id: {0} data: '{1}'", (object)int64, (object)algorithm.Compress(numArray2).ToBase64()));
                        if (!clientCommandResult2.Success)
                            return clientCommandResult2;
                    }
                    dictionary["Data"] = int64.ToString();
                }
            }
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository commit name: '{0}' comment: '{1}' modifiedBy: '{2}' system: '{3}' data: '{4}'", (object)name, (object)comment, (object)user, (object)system, (object)instance.ToJson().EscapedJson()));
            if (clientCommandResult.Success)
                log = clientCommandResult.CommandMessages[0].ToObject<RevLog>();
            return clientCommandResult;
        }

        public ClientCommandResult UnlinkRepositoryToStore(
          string number,
          string name,
          string modifiedBy,
          string system,
          out Store store)
        {
            store = new Store();
            ClientCommandResult store1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository remove-link-to-store name: {0} number: {1} modifiedby: '{2}' system: '{3}'", (object)name, (object)number, (object)modifiedBy, (object)system));
            if (store1.Success)
                store = store1.CommandMessages[0].ToObject<Store>();
            return store1;
        }

        public ClientCommandResult LinkRepositoryToStore(
          string number,
          string name,
          string modifiedBy,
          string system,
          out Store store)
        {
            store = new Store();
            ClientCommandResult store1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository link-to-store name: {0} number: {1} modifiedby: '{2}' system: '{3}'", (object)name, (object)number, (object)modifiedBy, (object)system));
            if (store1.Success)
                store = store1.CommandMessages[0].ToObject<Store>();
            return store1;
        }

        public ClientCommandResult UnlinkRepositoryToGroup(
          string groupName,
          string name,
          string modifiedBy,
          string system,
          out StoreGroup group)
        {
            group = new StoreGroup();
            ClientCommandResult group1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository remove-link-to-group name: {0} groupName: {1} modifiedby: '{2}' system: '{3}'", (object)name, (object)groupName, (object)modifiedBy, (object)system));
            if (group1.Success)
                group = group1.CommandMessages[0].ToObject<StoreGroup>();
            return group1;
        }

        public ClientCommandResult LinkRepositoryToGroup(
          string groupName,
          string name,
          string modifiedBy,
          string system,
          out StoreGroup group)
        {
            group = new StoreGroup();
            ClientCommandResult group1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository link-to-group name: {0} groupName: {1} modifiedby: '{2}' system: '{3}'", (object)name, (object)groupName, (object)modifiedBy, (object)system));
            if (group1.Success)
                group = group1.CommandMessages[0].ToObject<StoreGroup>();
            return group1;
        }

        public ClientCommandResult LinkStoreToGroup(
          string number,
          string name,
          string modifiedBy,
          string system,
          out Store store)
        {
            store = new Store();
            ClientCommandResult group = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store link-to-group name: {0} number: {1} modifiedby: '{2}' system: '{3}'", (object)name, (object)number, (object)modifiedBy, (object)system));
            if (group.Success)
                store = group.CommandMessages[0].ToObject<Store>();
            return group;
        }

        public ClientCommandResult CreateScript(
          string name,
          string scriptLocation,
          string createBy,
          string system,
          out Script script)
        {
            script = (Script)null;
            if (!File.Exists(scriptLocation))
            {
                ClientCommandResult script1 = new ClientCommandResult();
                script1.Errors.Add(Redbox.IPC.Framework.Error.NewError("U335", string.Format("The script file does not exist: {0}.", (object)scriptLocation), "Specify a path to a valid script file."));
                return script1;
            }
            script = new Script();
            ClientCommandResult script2 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("update-script create text: '{0}' createdBy: '{1}' system: '{2}' name: '{3}'", (object)Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation)).ToBase64().Replace("'", "\\'"), (object)createBy, (object)system, (object)name));
            if (script2.Success)
                script = script2.CommandMessages[0].ToObject<Script>();
            return script2;
        }

        public ClientCommandResult UpdateScript(
          long id,
          string scriptLocation,
          string modifiedBy,
          string system,
          out Script script)
        {
            script = (Script)null;
            if (!File.Exists(scriptLocation))
            {
                ClientCommandResult clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("U335", string.Format("The script file does not exist: {0}.", (object)scriptLocation), "Specify a path to a valid script file."));
                return clientCommandResult;
            }
            script = new Script();
            ClientCommandResult clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("update-script update id: {3} text: '{0}' modifiedby: '{1}' system: '{2}'", (object)Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation)).ToBase64().Replace("'", "\\'"), (object)modifiedBy, (object)system, (object)id));
            if (clientCommandResult1.Success)
                script = clientCommandResult1.CommandMessages[0].ToObject<Script>();
            return clientCommandResult1;
        }

        public ClientCommandResult GetHash(long fileId, out string hash)
        {
            hash = string.Empty;
            ClientCommandResult hash1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("file hash id: {0}", (object)fileId));
            if (hash1.Success)
                hash = hash1.CommandMessages[0];
            return hash1;
        }

        public ClientCommandResult CreateStore(
          string storeNumber,
          string groupName,
          string createdBy,
          string system)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store create storeNumber: '{0}' groupName: '{1}' createdBy: '{2}' system: '{3}'", (object)storeNumber, (object)groupName, (object)createdBy, (object)system));
        }

        public ClientCommandResult EnableStore(string storeNumber)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store enable number:'{0}'", (object)storeNumber));
        }

        public ClientCommandResult DeleteStore(string storeNumber)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store delete number:'{0}'", (object)storeNumber));
        }

        public ClientCommandResult CreateRepository(
          string name,
          System.Collections.Generic.List<string> storeNumbers,
          System.Collections.Generic.List<string> groupNames,
          string createdBy,
          string system,
          out Repository repository)
        {
            repository = new Repository();
            ClientCommandResult repository1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository create name: '{0}' storeNumbers: '{1}' storeGroups: '{2}' createdBy: '{3}' system: '{4}'", (object)name, (object)storeNumbers.ToJson().EscapedJson(), (object)groupNames.ToJson().EscapedJson(), (object)createdBy, (object)system));
            if (repository1.Success)
                repository = repository1.CommandMessages[0].ToObject<Repository>();
            return repository1;
        }

        public ClientCommandResult GetFile(long fileId, out RepositoryEntry repositoryEntry)
        {
            repositoryEntry = new RepositoryEntry();
            ClientCommandResult file = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("file get id: {0}", (object)fileId));
            if (file.Success)
                repositoryEntry = file.CommandMessages[0].ToObject<RepositoryEntry>();
            return file;
        }

        public ClientCommandResult GetFileWithData(
          long fileId,
          out RepositoryEntry repositoryEntry,
          out byte[] data)
        {
            repositoryEntry = new RepositoryEntry();
            data = new byte[0];
            ClientCommandResult fileWithData = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("file get-data id: {0}", (object)fileId));
            if (fileWithData.Success)
            {
                repositoryEntry = fileWithData.CommandMessages[0].ToObject<RepositoryEntry>();
                CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
                data = algorithm.Decompress(Convert.FromBase64String(fileWithData.CommandMessages[1]));
            }
            return fileWithData;
        }

        public ClientCommandResult UpdateStoreGroup(
          string name,
          Dictionary<string, int> properties,
          out StoreGroup group)
        {
            group = new StoreGroup();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("STOREGROUP update name: '{0}'", (object)name);
            foreach (KeyValuePair<string, int> property in properties)
                stringBuilder.AppendFormat(" {0}: {1}", (object)property.Key, (object)property.Value);
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, stringBuilder.ToString());
            if (clientCommandResult.Success)
                group = clientCommandResult.CommandMessages[0].ToObject<StoreGroup>();
            return clientCommandResult;
        }

        public ClientCommandResult UpdateStore(
          string name,
          Dictionary<string, int> properties,
          out StoreGroup group)
        {
            group = new StoreGroup();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("STORE update number: '{0}'", (object)name);
            foreach (KeyValuePair<string, int> property in properties)
                stringBuilder.AppendFormat(" {0}: {1}", (object)property.Key, (object)property.Value);
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, stringBuilder.ToString());
            if (clientCommandResult.Success)
                group = clientCommandResult.CommandMessages[0].ToObject<StoreGroup>();
            return clientCommandResult;
        }

        public ClientCommandResult CreateStoreGroup(
          string name,
          string createdBy,
          string system,
          uint maxKPSInSchedule,
          uint maxKPSOutOfSchedule,
          byte scheduleStart,
          byte scheduleEnd,
          int maxBandwidthInMB,
          out StoreGroup group)
        {
            group = new StoreGroup();
            ClientCommandResult storeGroup = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("STORE-GROUP create name: '{0}' createdBy: '{1}' system: '{2}' scheduleStart: {3} scheduleEnd: {4} maxKPSInSchedule: {5} maxKPSOutOfSchedule: {6} maxBandwidthInMB: {7}", (object)name, (object)createdBy, (object)system, (object)scheduleStart, (object)scheduleEnd, (object)maxKPSInSchedule, (object)maxKPSOutOfSchedule, (object)maxBandwidthInMB));
            if (storeGroup.Success)
                group = storeGroup.CommandMessages[0].ToObject<StoreGroup>();
            return storeGroup;
        }

        public ClientCommandResult DeleteStoreGroup(string groupName)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("store-group delete name:'{0}'", (object)groupName));
        }

        public ClientCommandResult CreateStore(
          string number,
          string groupName,
          string createdBy,
          string system,
          uint maxKPSInSchedule,
          uint maxKPSOutOfSchedule,
          byte scheduleStart,
          byte scheduleEnd,
          int maxBandwidthInMB,
          out Store store)
        {
            store = new Store();
            ClientCommandResult store1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("STORE create name: '{0}' groupName: '{8}' createdBy: '{1}' system: '{2}' scheduleStart: {3} scheduleEnd: {4} maxKPSInSchedule: {5} maxKPSOutOfSchedule: {6} maxBandwidthInMB: {7}", (object)number, (object)createdBy, (object)system, (object)scheduleStart, (object)scheduleEnd, (object)maxKPSInSchedule, (object)maxKPSOutOfSchedule, (object)maxBandwidthInMB, (object)groupName));
            if (store1.Success)
                store = store1.CommandMessages[0].ToObject<Store>();
            return store1;
        }

        public ClientCommandResult GetStoreGroups(out System.Collections.Generic.List<StoreGroup> groups)
        {
            groups = new System.Collections.Generic.List<StoreGroup>();
            ClientCommandResult storeGroups = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, "STOREGROUP list");
            if (storeGroups.Success)
                groups = storeGroups.CommandMessages[0].ToObject<System.Collections.Generic.List<StoreGroup>>();
            return storeGroups;
        }

        public ClientCommandResult GetChangeSet(
          string storeNumber,
          Dictionary<string, string> currentFiles,
          out Store store,
          out System.Collections.Generic.List<ChangeSet> jobs)
        {
            store = new Store();
            jobs = new System.Collections.Generic.List<ChangeSet>();
            string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(storeNumber);
            ClientCommandResult changeSet = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("u p n: '{0}' c: '{1}' h:'{2}'", (object)storeNumber, (object)currentFiles.ToJson().EscapedJson(), (object)credentialHash));
            if (changeSet.Success)
            {
                jobs = changeSet.CommandMessages[0].ToObject<System.Collections.Generic.List<ChangeSet>>();
                if (jobs.Count > 0)
                    store = changeSet.CommandMessages[1].ToObject<Store>();
            }
            return changeSet;
        }

        public ClientCommandResult Poll(
          string storeNumber,
          int version,
          System.Collections.Generic.List<PollRequest> pollRequestList,
          out System.Collections.Generic.List<PollReply> pollReplyList)
        {
            System.Collections.Generic.List<PollRequestDTO> pollReplyDtoList = PollRequestDTO.GetPollReplyDTOList(pollRequestList);
            string credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(storeNumber);
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("u pr n:'{0}' v:'{1}' c:'{2}' h:'{3}'", (object)storeNumber, (object)version, (object)pollReplyDtoList.ToJson().EscapedJson(), (object)credentialHash));
            System.Collections.Generic.List<PollReplyDTO> list = new System.Collections.Generic.List<PollReplyDTO>();
            foreach (string commandMessage in clientCommandResult.CommandMessages)
            {
                try
                {
                    PollReplyDTO pollReplyDto = commandMessage.ToObject<PollReplyDTO>();
                    list.Add(pollReplyDto);
                }
                catch (Exception ex)
                {
                    clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("POLL", "Error parsing poll reply: " + commandMessage, ex));
                }
            }
            pollReplyList = PollReplyDTO.GetPollReplyList(list);
            return clientCommandResult;
        }

        public ClientCommandResult GetFilesForRepository(string name, out System.Collections.Generic.List<Identifier> files)
        {
            files = new System.Collections.Generic.List<Identifier>();
            ClientCommandResult filesForRepository = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("file list repositoryName: '{0}' maxitems:9999", (object)name));
            if (filesForRepository.Success)
                files = filesForRepository.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return filesForRepository;
        }

        public ClientCommandResult GetRepositoriesForGroup(
          string name,
          out System.Collections.Generic.List<Identifier> repositories)
        {
            repositories = new System.Collections.Generic.List<Identifier>();
            ClientCommandResult repositoriesForGroup = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, "repository list ascendingOrder: true");
            if (repositoriesForGroup.Success)
                repositories = repositoriesForGroup.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return repositoriesForGroup;
        }

        public ClientCommandResult GetRepositoriesForStore(
          string number,
          out System.Collections.Generic.List<Identifier> repositories)
        {
            repositories = new System.Collections.Generic.List<Identifier>();
            ClientCommandResult repositoriesForStore = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("repository list number: '{0}'", (object)number));
            if (repositoriesForStore.Success)
                repositories = repositoriesForStore.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return repositoriesForStore;
        }

        public ClientCommandResult GetRevisionsForRepository(string name, out System.Collections.Generic.List<Identifier> versions)
        {
            versions = new System.Collections.Generic.List<Identifier>();
            ClientCommandResult revisionsForRepository = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("revlog list name: '{0}'", (object)name));
            if (revisionsForRepository.Success)
                versions = revisionsForRepository.CommandMessages[0].ToObject<System.Collections.Generic.List<Identifier>>();
            return revisionsForRepository;
        }

        public ClientCommandResult ForcePublishRun(string name)
        {
            object obj = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager", "AllowForceDataFiles", (object)false.ToString());
            bool flag = false;
            bool result;
            if (obj != null && obj is string && bool.TryParse(obj as string, out result))
                flag = result;
            return flag ? ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("subscription publish name: '{0}'", (object)name)) : new ClientCommandResult();
        }

        public ClientCommandResult ExecuteCommandString(string command)
        {
            this.Session.Timeout = this.Timeout;
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, command);
        }

        public ClientCommandResult Ping() => this.ExecuteCommandString("INTERSERVER ping");

        public ClientCommandResult QuickPost(string data)
        {
            return this.ExecuteCommandString(string.Format("interserver quickpost data: '{0}'", (object)data));
        }

        public ClientCommandResult BatchPost(string data)
        {
            return this.ExecuteCommandString(string.Format("interserver batchpost data: '{0}'", (object)data));
        }

        public ClientCommandResult ConfigDefaultCreate(
          long configSetting,
          string configdefaulttype,
          long configdefaulttypekey,
          string configdefaulttypevalue,
          string value,
          string path,
          DateTime effectiveTime,
          DateTime? expireTime)
        {
            string format = "configdefault create configsetting: '{0}' configdefaulttype: '{1}' configdefaulttypekey: '{2}' configdefaulttypevalue: '{3}' effectiveTime: '{4}'";
            string str1 = "";
            if (!string.IsNullOrEmpty(value))
                str1 = value;
            else if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                {
                    ClientCommandResult clientCommandResult = new ClientCommandResult();
                    clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("U563", string.Format("The file does not exist: {0}.", (object)path), "Specify a path to a valid file."));
                    return clientCommandResult;
                }
                str1 = File.ReadAllBytes(path).ToBase64();
            }
            string str2 = string.Format(format, (object)configSetting, (object)configdefaulttype, (object)configdefaulttypekey, (object)configdefaulttypevalue, (object)effectiveTime);
            if (expireTime.HasValue)
                str2 += string.Format(" expireTime: '{0}'", (object)expireTime.Value);
            string command = str2 + string.Format(" value: '{0}'", (object)str1);
            Console.WriteLine(command);
            return this.ExecuteCommandString(command);
        }

        public ClientCommandResult ConfigDefaultUpdate(long id, string path)
        {
            string format = "configdefault update id: '{0}' value: '{1}' ";
            ClientCommandResult clientCommandResult = new ClientCommandResult();
            if (string.IsNullOrEmpty(path))
            {
                clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("US1000", "Invalid Path", "Specify a path to a valid file."));
                return clientCommandResult;
            }
            if (!File.Exists(path))
            {
                clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("US1010", string.Format("The file does not exist: {0}.", (object)path), "Specify a path to a valid file."));
                return clientCommandResult;
            }
            string base64 = File.ReadAllBytes(path).ToBase64();
            string command = string.Format(format, (object)id, (object)base64);
            Console.WriteLine(command);
            return this.ExecuteCommandString(command);
        }

        public ClientCommandResult DownloadFileSetFile(
          long? id,
          string name,
          string filename,
          string destinationPath,
          string path)
        {
            ErrorList collection = new ErrorList();
            if (!id.HasValue && string.IsNullOrEmpty(name))
                collection.Add(Redbox.IPC.Framework.Error.NewError("US0500", "DownloadFileSetFile requires either an id or name in order to set a file.", "Supply a valid value and retry your command."));
            if (string.IsNullOrEmpty(filename))
                collection.Add(Redbox.IPC.Framework.Error.NewError("US0501", "DownloadFileSetFile requires a filename in order to set a file.", "Supply a valid value and retry your command."));
            if (string.IsNullOrEmpty(destinationPath))
                collection.Add(Redbox.IPC.Framework.Error.NewError("US0502", "DownloadFileSetFile requires a destinationPath in order to set a file.", "Supply a valid value and retry your command."));
            if (!File.Exists(path))
                collection.Add(Redbox.IPC.Framework.Error.NewError("US0503", string.Format("The file does not exist: {0}.", (object)path), "Specify a path to a valid file."));
            if (collection.ContainsError())
            {
                ClientCommandResult clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)collection);
                return clientCommandResult;
            }
            int length1 = File.ReadAllBytes(path).Length;
            int num1 = length1 == 0 ? 0 : length1 / 1048576;
            int num2 = 0;
            Redbox.UpdateService.Client.UpdateService.Log("Uploading file: {0} - {1} mb", (object)path, (object)num1);
            using (FileStream fileStream = File.OpenRead(path))
            {
                string asciishA1Hash = fileStream.ToASCIISHA1Hash();
                fileStream.Seek(0L, SeekOrigin.Begin);
                ClientCommandResult clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, "downloadfile set-file " + (id.HasValue ? string.Format("id:{0} ", (object)id.Value) : string.Format("name:'{0}' ", (object)name)) + string.Format("filename:'{0}' destinationpath:'{1}' filekey:'{2}'", (object)filename, (object)destinationPath.EscapedJson(), (object)asciishA1Hash));
                if (!clientCommandResult1.Success)
                    return clientCommandResult1;
                byte[] numArray1 = new byte[1048576];
                for (int length2 = Redbox.UpdateService.Client.UpdateService.ReadChunk((Stream)fileStream, numArray1); length2 > 0; length2 = Redbox.UpdateService.Client.UpdateService.ReadChunk((Stream)fileStream, numArray1))
                {
                    byte[] numArray2 = new byte[length2];
                    Array.Copy((Array)numArray1, (Array)numArray2, numArray2.Length);
                    ClientCommandResult clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, "downloadfile upload-file " + (id.HasValue ? string.Format("id:{0} ", (object)id.Value) : string.Format("name:'{0}' ", (object)name)) + string.Format("data:'{0}'", (object)numArray2.ToBase64()));
                    if (!clientCommandResult2.Success)
                        return clientCommandResult2;
                    num2 += numArray2.Length;
                    Redbox.UpdateService.Client.UpdateService.Log("Uploaded {0} mb / {1} mb", (object)(num2 == 0 ? 0 : num2 / 1048576), (object)num1);
                }
                ClientCommandResult clientCommandResult3 = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, "downloadfile upload-file " + (id.HasValue ? string.Format("id:{0}", (object)id.Value) : string.Format("name:'{0}'", (object)name)) + " finished:true");
                if (clientCommandResult3.Success)
                    Redbox.UpdateService.Client.UpdateService.Log("Upload complete");
                return clientCommandResult3;
            }
        }

        public ClientCommandResult DownloadFileSetScript(long? id, string name, string path)
        {
            ErrorList collection = new ErrorList();
            if (!id.HasValue && string.IsNullOrEmpty(name))
                collection.Add(Redbox.IPC.Framework.Error.NewError("US0505", "DownloadFileSetScript requires either an id or name in order to set a script.", "Supply a valid value and retry your command."));
            if (!File.Exists(path))
                collection.Add(Redbox.IPC.Framework.Error.NewError("US0506", string.Format("The file does not exist: {0}.", (object)path), "Specify a path to a valid file."));
            if (collection.ContainsError())
            {
                ClientCommandResult clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.AddRange((IEnumerable<Redbox.IPC.Framework.Error>)collection);
                return clientCommandResult;
            }
            string command = string.Format("downloadfile set-script " + (id.HasValue ? string.Format(" id:{0} ", (object)id.Value) : string.Format(" name:'{0}' ", (object)name)) + " text:'{0}'", (object)File.ReadAllBytes(path).ToBase64());
            Console.WriteLine(command);
            return this.ExecuteCommandString(command);
        }

        public ClientCommandResult StoreCacheGet(
          string filterKey,
          string filterValue,
          string kioskStatus,
          string enabled,
          out System.Collections.Generic.List<StoreCacheData> storeCacheData)
        {
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, string.Format("storecache list key:'{0}' value:'{1}' kioskStatus:'{2}' enabled:'{3}'", (object)filterKey, (object)filterValue, (object)kioskStatus, (object)enabled));
            storeCacheData = clientCommandResult.Success ? clientCommandResult.CommandMessages[0].ToObject<System.Collections.Generic.List<StoreCacheData>>() : new System.Collections.Generic.List<StoreCacheData>();
            return clientCommandResult;
        }

        public ClientCommandResult StatusMessageGet(
          string key,
          string subKey,
          string type,
          int? numResults,
          out System.Collections.Generic.List<StatusMessage> updates)
        {
            string command = string.Format("STATUSMESSAGE list", (object)key);
            if (!string.IsNullOrEmpty(key))
                command += string.Format(" key:{0}", (object)key);
            if (!string.IsNullOrEmpty(subKey))
                command += string.Format(" subkey:{0}", (object)subKey);
            if (!string.IsNullOrEmpty(type))
                command += string.Format(" type:{0}", (object)type);
            if (numResults.HasValue)
                command += string.Format(" numresults:{0}", (object)numResults.Value);
            ClientCommandResult clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, command);
            updates = clientCommandResult.Success ? clientCommandResult.CommandMessages[0].ToObject<System.Collections.Generic.List<StatusMessage>>() : new System.Collections.Generic.List<StatusMessage>();
            return clientCommandResult;
        }

        public string Url { get; private set; }

        public int Timeout { get; private set; }

        internal UpdateService(string url) => this.Url = url;

        internal ClientSession Session
        {
            get
            {
                if (this.m_clientSession != null && this.m_clientSession.IsDisposed)
                    throw new ObjectDisposedException("Cannot use disposed Update Service client session.");
                if (this.m_clientSession == null)
                {
                    this.m_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(this.Url), new int?());
                    this.m_clientSession.Timeout = this.Timeout;
                }
                return this.m_clientSession;
            }
            set => this.m_clientSession = value;
        }

        private static int ReadChunk(Stream file, byte[] buffer)
        {
            int offset;
            int num;
            for (offset = 0; offset < buffer.Length; offset += num)
            {
                num = file.Read(buffer, offset, buffer.Length - offset);
                if (num == 0)
                    break;
            }
            return offset;
        }

        private static void Log(string text, params object[] parms)
        {
            Console.WriteLine(string.Format(text, parms));
        }
    }
}
