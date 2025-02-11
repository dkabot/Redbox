using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Redbox.Compression;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateService.Model;
using Error = Redbox.IPC.Framework.Error;

namespace Redbox.UpdateService.Client
{
    public class UpdateService : IDisposable
    {
        private const bool TransientSessionFlag = false;

        private static readonly Dictionary<Type, string> m_typeToCommandForm = new Dictionary<Type, string>
        {
            {
                typeof(Repository),
                "repository"
            },
            {
                typeof(Store),
                "store"
            },
            {
                typeof(StoreGroup),
                "store-group"
            },
            {
                typeof(RepositoryEntry),
                "file"
            },
            {
                typeof(RevLog),
                "revlog"
            },
            {
                typeof(Script),
                "update-script"
            },
            {
                typeof(Patch),
                "patch"
            },
            {
                typeof(Label),
                "label"
            },
            {
                typeof(TransferStatistic),
                "transfer-statistic"
            },
            {
                typeof(Release),
                "release"
            }
        };

        private ClientSession m_clientSession;

        internal UpdateService(string url)
        {
            Url = url;
        }

        public string Url { get; }

        public int Timeout { get; private set; }

        internal ClientSession Session
        {
            get
            {
                if (m_clientSession != null && m_clientSession.IsDisposed)
                    throw new ObjectDisposedException("Cannot use disposed Update Service client session.");
                if (m_clientSession == null)
                {
                    m_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(Url), new int?());
                    m_clientSession.Timeout = Timeout;
                }

                return m_clientSession;
            }
            set => m_clientSession = value;
        }

        public void Dispose()
        {
            if (Session == null)
                return;
            if (Session.IsDisposed)
                return;
            try
            {
                Session.Dispose();
            }
            catch
            {
            }
        }

        public static UpdateService GetService(string url)
        {
            return new UpdateService(url) { Timeout = 30000 };
        }

        public static UpdateService GetService(string url, int timeout)
        {
            return new UpdateService(url) { Timeout = timeout };
        }

        public ClientCommandResult List<T>(out List<Identifier> list)
        {
            list = new List<Identifier>();
            if (!m_typeToCommandForm.ContainsKey(typeof(T)))
            {
                var clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.Add(Error.NewError("U334",
                    string.Format("The type '{0}' does not have a command form mapping.", typeof(T).FullName),
                    "Specify a valid type."));
                return clientCommandResult;
            }

            var clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("{0} list", m_typeToCommandForm[typeof(T)]));
            if (clientCommandResult1.Success)
                list = clientCommandResult1.CommandMessages[0].ToObject<List<Identifier>>();
            return clientCommandResult1;
        }

        public ClientCommandResult Get<T>(Identifier id, out T t)
        {
            t = default;
            if (!m_typeToCommandForm.ContainsKey(typeof(T)))
            {
                var clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.Add(Error.NewError("U334",
                    string.Format("The type '{0}' does not have a command form mapping.", typeof(T).FullName),
                    "Specify a valid type."));
                return clientCommandResult;
            }

            var clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("{0} get id: {1}", m_typeToCommandForm[typeof(T)], id.ID));
            if (clientCommandResult1.Success)
                t = clientCommandResult1.CommandMessages[0].ToObject<T>();
            return clientCommandResult1;
        }

        public ClientCommandResult GetSubscriptionStatus(string name, out SubscriptionStatus state)
        {
            state = new SubscriptionStatus();
            var subscriptionStatus =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("subscription status name: '{0}'", name));
            if (subscriptionStatus.Success)
                state = subscriptionStatus.CommandMessages[0].ToObject<SubscriptionStatus>();
            return subscriptionStatus;
        }

        public ClientCommandResult ListTransferStatisticByStoreAndRepository(
            string number,
            string name,
            out List<Identifier> ids)
        {
            ids = new List<Identifier>();
            var clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("transfer-statistic get-store-repository-report name: '{0}' number: '{1}'", name,
                    number));
            if (clientCommandResult.Success)
                ids = clientCommandResult.CommandMessages[0].ToObject<List<Identifier>>();
            return clientCommandResult;
        }

        public ClientCommandResult CheckStoreExists(string number, out bool exists)
        {
            exists = false;
            var clientCommandResult =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("store get name: '{0}'", number));
            if (!clientCommandResult.Success)
                return clientCommandResult;
            exists = true;
            return clientCommandResult;
        }

        public ClientCommandResult DownloadWorkResult(Identifier id, string targetFile)
        {
            var clientCommandResult =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("work-result download id: {0}", id.ID));
            if (clientCommandResult.Success)
                File.WriteAllText(targetFile, clientCommandResult.CommandMessages[0]);
            return clientCommandResult;
        }

        public ClientCommandResult EfAddStoreToRelease(string name, string number, string user)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store ef-add-to-release number:'{0}' name:'{1}' user:'{2}'", number, name, user));
        }

        public ClientCommandResult AddStoreToRelease(string name, string number)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store add-to-release name: '{0}' number: '{1}'", name, number));
        }

        public ClientCommandResult StartStoreInstaller(
            string kioskId,
            string repositoryHash,
            string frontEndVersion)
        {
            var credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(kioskId);
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("installer start hash: '{0}' version: '{1}' h: '{2}'", repositoryHash, frontEndVersion,
                    credentialHash));
        }

        public ClientCommandResult InstallerLog(
            string kioskId,
            string guid,
            string name,
            string value)
        {
            var credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(kioskId);
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("installer log guid:'{0}' name:'{1}' value:'{2}' h: '{2}'", guid, name, value,
                    credentialHash));
        }

        public ClientCommandResult CreateNewSetting(string name, string value)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("setting set name: '{0}' value: '{1}'", name, value));
        }

        public ClientCommandResult CreateNewSetting(string name, string value, string filter)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("setting set name: '{0}' value: '{1}' filter: '{2}'", name, value, filter));
        }

        public ClientCommandResult ListStores(int maxItems, long? startId, out List<Identifier> stores)
        {
            stores = new List<Identifier>();
            var clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store list maxitems: {0}{1}", maxItems,
                    startId.HasValue ? string.Format("startid: {0}", startId.Value) : (object)string.Empty));
            if (clientCommandResult.Success)
                stores = clientCommandResult.CommandMessages[0].ToObject<List<Identifier>>();
            return clientCommandResult;
        }

        public ClientCommandResult GetLastCheckIn(string number, out DateTime lastCheckIn)
        {
            lastCheckIn = DateTime.UtcNow;
            var lastCheckIn1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store last-check-in number: '{0}'", number));
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
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format(
                    "SUBSCRIPTION create generatorId: {0} name: '{1}' repositoryName: '{2}' fileName: '{3}' path: '{4}' createdBy: '{5}' system: '{6}' displayName: '{7}'",
                    generatorId, name, repository, fileName, path, createdBy, system, displayName));
        }

        public ClientCommandResult CreateFileGenerator(string type, string createdBy, string system)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("FILE-GENERATOR create type: '{0}' createdBy: '{1}' system: '{2}'", type, createdBy,
                    system));
        }

        public ClientCommandResult DeleteFile(
            string repositoryName,
            string fullPath,
            string user,
            string system,
            out RepositoryEntry entry)
        {
            List<Identifier> files;
            var filesForRepository = GetFilesForRepository(repositoryName, out files);
            if (!filesForRepository.Success)
            {
                entry = new RepositoryEntry();
                return filesForRepository;
            }

            var repositoryEntryList = new List<RepositoryEntry>();
            foreach (var id in files)
            {
                RepositoryEntry t;
                filesForRepository = Get(id, out t);
                if (filesForRepository.Success)
                    repositoryEntryList.Add(t);
            }

            var repositoryEntry = repositoryEntryList.Find(f => Path.Combine(f.Path, f.Name) == fullPath);
            if (repositoryEntry != null)
                return DeleteFile(repositoryEntry.ID, user, system, out entry);
            filesForRepository.Success = false;
            filesForRepository.Errors.Add(Error.NewError("M100",
                string.Format("No file found with path: {0} in repository {1}", fullPath, repositoryName),
                "Supply a valid file id and retry your command."));
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
            var clientCommandResult =
                ExecuteCommandString(string.Format("file delete id: {0} modifiedby: '{1}' system: '{2}'", id, user,
                    system));
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
            var dictionaryList = new List<Dictionary<string, string>>();
            foreach (var newFile in set.m_newFiles)
                dictionaryList.Add(newFile.Value);
            foreach (var update in set.m_updates)
            {
                update.Value.Add("ID", update.Key.ToString());
                dictionaryList.Add(update.Value);
            }

            foreach (var dictionary in dictionaryList)
            {
                var clientCommandResult1 =
                    ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false, "data-upload begin-upload");
                if (!clientCommandResult1.Success)
                    return clientCommandResult1;
                var algorithm = CompressionAlgorithm.GetAlgorithm((CompressionType)1);
                var int64 = Convert.ToInt64(clientCommandResult1.CommandMessages[0]);
                using (var file = File.OpenRead(dictionary["Data"]))
                {
                    var numArray = new byte[10485760];
                    for (var length = ReadChunk(file, numArray); length > 0; length = ReadChunk(file, numArray))
                    {
                        var destinationArray = new byte[length];
                        Array.Copy(numArray, destinationArray, destinationArray.Length);
                        var clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(
                            Session, false,
                            string.Format("data-upload append id: {0} data: '{1}'", int64,
                                algorithm.Compress(destinationArray).ToBase64()));
                        if (!clientCommandResult2.Success)
                            return clientCommandResult2;
                    }

                    dictionary["Data"] = int64.ToString();
                }
            }

            var clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format(
                    "repository commit name: '{0}' comment: '{1}' modifiedBy: '{2}' system: '{3}' data: '{4}'", name,
                    comment, user, system,
                    dictionaryList.ToJson().EscapedJson()));
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
            var store1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("repository remove-link-to-store name: {0} number: {1} modifiedby: '{2}' system: '{3}'",
                    name, number, modifiedBy, system));
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
            var store1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("repository link-to-store name: {0} number: {1} modifiedby: '{2}' system: '{3}'", name,
                    number, modifiedBy, system));
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
            var group1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format(
                    "repository remove-link-to-group name: {0} groupName: {1} modifiedby: '{2}' system: '{3}'", name,
                    groupName, modifiedBy, system));
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
            var group1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("repository link-to-group name: {0} groupName: {1} modifiedby: '{2}' system: '{3}'", name,
                    groupName, modifiedBy, system));
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
            var group = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store link-to-group name: {0} number: {1} modifiedby: '{2}' system: '{3}'", name, number,
                    modifiedBy, system));
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
            script = null;
            if (!File.Exists(scriptLocation))
            {
                var script1 = new ClientCommandResult();
                script1.Errors.Add(Error.NewError("U335",
                    string.Format("The script file does not exist: {0}.", scriptLocation),
                    "Specify a path to a valid script file."));
                return script1;
            }

            script = new Script();
            var script2 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("update-script create text: '{0}' createdBy: '{1}' system: '{2}' name: '{3}'",
                    Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation)).ToBase64()
                        .Replace("'", "\\'"), createBy, system, name));
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
            script = null;
            if (!File.Exists(scriptLocation))
            {
                var clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.Add(Error.NewError("U335",
                    string.Format("The script file does not exist: {0}.", scriptLocation),
                    "Specify a path to a valid script file."));
                return clientCommandResult;
            }

            script = new Script();
            var clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("update-script update id: {3} text: '{0}' modifiedby: '{1}' system: '{2}'",
                    Encoding.Unicode.GetBytes(File.ReadAllText(scriptLocation)).ToBase64()
                        .Replace("'", "\\'"), modifiedBy, system, id));
            if (clientCommandResult1.Success)
                script = clientCommandResult1.CommandMessages[0].ToObject<Script>();
            return clientCommandResult1;
        }

        public ClientCommandResult GetHash(long fileId, out string hash)
        {
            hash = string.Empty;
            var hash1 =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("file hash id: {0}", fileId));
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
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store create storeNumber: '{0}' groupName: '{1}' createdBy: '{2}' system: '{3}'",
                    storeNumber, groupName, createdBy, system));
        }

        public ClientCommandResult EnableStore(string storeNumber)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store enable number:'{0}'", storeNumber));
        }

        public ClientCommandResult DeleteStore(string storeNumber)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store delete number:'{0}'", storeNumber));
        }

        public ClientCommandResult CreateRepository(
            string name,
            List<string> storeNumbers,
            List<string> groupNames,
            string createdBy,
            string system,
            out Repository repository)
        {
            repository = new Repository();
            var repository1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format(
                    "repository create name: '{0}' storeNumbers: '{1}' storeGroups: '{2}' createdBy: '{3}' system: '{4}'",
                    name, storeNumbers.ToJson().EscapedJson(),
                    groupNames.ToJson().EscapedJson(), createdBy,
                    system));
            if (repository1.Success)
                repository = repository1.CommandMessages[0].ToObject<Repository>();
            return repository1;
        }

        public ClientCommandResult GetFile(long fileId, out RepositoryEntry repositoryEntry)
        {
            repositoryEntry = new RepositoryEntry();
            var file =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("file get id: {0}", fileId));
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
            var fileWithData =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("file get-data id: {0}", fileId));
            if (fileWithData.Success)
            {
                repositoryEntry = fileWithData.CommandMessages[0].ToObject<RepositoryEntry>();
                var algorithm = CompressionAlgorithm.GetAlgorithm((CompressionType)1);
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
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("STOREGROUP update name: '{0}'", name);
            foreach (var property in properties)
                stringBuilder.AppendFormat(" {0}: {1}", property.Key, property.Value);
            var clientCommandResult =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false, stringBuilder.ToString());
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
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("STORE update number: '{0}'", name);
            foreach (var property in properties)
                stringBuilder.AppendFormat(" {0}: {1}", property.Key, property.Value);
            var clientCommandResult =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false, stringBuilder.ToString());
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
            var storeGroup = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format(
                    "STORE-GROUP create name: '{0}' createdBy: '{1}' system: '{2}' scheduleStart: {3} scheduleEnd: {4} maxKPSInSchedule: {5} maxKPSOutOfSchedule: {6} maxBandwidthInMB: {7}",
                    name, createdBy, system, scheduleStart, scheduleEnd, maxKPSInSchedule, maxKPSOutOfSchedule,
                    maxBandwidthInMB));
            if (storeGroup.Success)
                group = storeGroup.CommandMessages[0].ToObject<StoreGroup>();
            return storeGroup;
        }

        public ClientCommandResult DeleteStoreGroup(string groupName)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("store-group delete name:'{0}'", groupName));
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
            var store1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format(
                    "STORE create name: '{0}' groupName: '{8}' createdBy: '{1}' system: '{2}' scheduleStart: {3} scheduleEnd: {4} maxKPSInSchedule: {5} maxKPSOutOfSchedule: {6} maxBandwidthInMB: {7}",
                    number, createdBy, system, scheduleStart, scheduleEnd, maxKPSInSchedule, maxKPSOutOfSchedule,
                    maxBandwidthInMB, groupName));
            if (store1.Success)
                store = store1.CommandMessages[0].ToObject<Store>();
            return store1;
        }

        public ClientCommandResult GetStoreGroups(out List<StoreGroup> groups)
        {
            groups = new List<StoreGroup>();
            var storeGroups =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false, "STOREGROUP list");
            if (storeGroups.Success)
                groups = storeGroups.CommandMessages[0].ToObject<List<StoreGroup>>();
            return storeGroups;
        }

        public ClientCommandResult GetChangeSet(
            string storeNumber,
            Dictionary<string, string> currentFiles,
            out Store store,
            out List<ChangeSet> jobs)
        {
            store = new Store();
            jobs = new List<ChangeSet>();
            var credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(storeNumber);
            var changeSet = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("u p n: '{0}' c: '{1}' h:'{2}'", storeNumber,
                    currentFiles.ToJson().EscapedJson(),
                    credentialHash));
            if (changeSet.Success)
            {
                jobs = changeSet.CommandMessages[0].ToObject<List<ChangeSet>>();
                if (jobs.Count > 0)
                    store = changeSet.CommandMessages[1].ToObject<Store>();
            }

            return changeSet;
        }

        public ClientCommandResult Poll(
            string storeNumber,
            int version,
            List<PollRequest> pollRequestList,
            out List<PollReply> pollReplyList)
        {
            var pollReplyDtoList = PollRequestDTO.GetPollReplyDTOList(pollRequestList);
            var credentialHash = UpdateServiceCredentials.Instance.GetCredentialHash(storeNumber);
            var clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("u pr n:'{0}' v:'{1}' c:'{2}' h:'{3}'", storeNumber, version,
                    pollReplyDtoList.ToJson().EscapedJson(),
                    credentialHash));
            var list = new List<PollReplyDTO>();
            foreach (var commandMessage in clientCommandResult.CommandMessages)
                try
                {
                    var pollReplyDto = commandMessage.ToObject<PollReplyDTO>();
                    list.Add(pollReplyDto);
                }
                catch (Exception ex)
                {
                    clientCommandResult.Errors.Add(Error.NewError("POLL",
                        "Error parsing poll reply: " + commandMessage, ex));
                }

            pollReplyList = PollReplyDTO.GetPollReplyList(list);
            return clientCommandResult;
        }

        public ClientCommandResult GetFilesForRepository(string name, out List<Identifier> files)
        {
            files = new List<Identifier>();
            var filesForRepository = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("file list repositoryName: '{0}' maxitems:9999", name));
            if (filesForRepository.Success)
                files = filesForRepository.CommandMessages[0].ToObject<List<Identifier>>();
            return filesForRepository;
        }

        public ClientCommandResult GetRepositoriesForGroup(
            string name,
            out List<Identifier> repositories)
        {
            repositories = new List<Identifier>();
            var repositoriesForGroup =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    "repository list ascendingOrder: true");
            if (repositoriesForGroup.Success)
                repositories = repositoriesForGroup.CommandMessages[0].ToObject<List<Identifier>>();
            return repositoriesForGroup;
        }

        public ClientCommandResult GetRepositoriesForStore(
            string number,
            out List<Identifier> repositories)
        {
            repositories = new List<Identifier>();
            var repositoriesForStore =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("repository list number: '{0}'", number));
            if (repositoriesForStore.Success)
                repositories = repositoriesForStore.CommandMessages[0].ToObject<List<Identifier>>();
            return repositoriesForStore;
        }

        public ClientCommandResult GetRevisionsForRepository(string name, out List<Identifier> versions)
        {
            versions = new List<Identifier>();
            var revisionsForRepository =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("revlog list name: '{0}'", name));
            if (revisionsForRepository.Success)
                versions = revisionsForRepository.CommandMessages[0].ToObject<List<Identifier>>();
            return revisionsForRepository;
        }

        public ClientCommandResult ForcePublishRun(string name)
        {
            var obj = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager",
                "AllowForceDataFiles", false.ToString());
            var flag = false;
            bool result;
            if (obj != null && obj is string && bool.TryParse(obj as string, out result))
                flag = result;
            return flag
                ? ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                    string.Format("subscription publish name: '{0}'", name))
                : new ClientCommandResult();
        }

        public ClientCommandResult ExecuteCommandString(string command)
        {
            Session.Timeout = Timeout;
            return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false, command);
        }

        public ClientCommandResult Ping()
        {
            return ExecuteCommandString("INTERSERVER ping");
        }

        public ClientCommandResult QuickPost(string data)
        {
            return ExecuteCommandString(string.Format("interserver quickpost data: '{0}'", data));
        }

        public ClientCommandResult BatchPost(string data)
        {
            return ExecuteCommandString(string.Format("interserver batchpost data: '{0}'", data));
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
            var format =
                "configdefault create configsetting: '{0}' configdefaulttype: '{1}' configdefaulttypekey: '{2}' configdefaulttypevalue: '{3}' effectiveTime: '{4}'";
            var str1 = "";
            if (!string.IsNullOrEmpty(value))
            {
                str1 = value;
            }
            else if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                {
                    var clientCommandResult = new ClientCommandResult();
                    clientCommandResult.Errors.Add(Error.NewError("U563",
                        string.Format("The file does not exist: {0}.", path), "Specify a path to a valid file."));
                    return clientCommandResult;
                }

                str1 = File.ReadAllBytes(path).ToBase64();
            }

            var str2 = string.Format(format, configSetting, configdefaulttype, configdefaulttypekey,
                configdefaulttypevalue, effectiveTime);
            if (expireTime.HasValue)
                str2 += string.Format(" expireTime: '{0}'", expireTime.Value);
            var command = str2 + string.Format(" value: '{0}'", str1);
            Console.WriteLine(command);
            return ExecuteCommandString(command);
        }

        public ClientCommandResult ConfigDefaultUpdate(long id, string path)
        {
            var format = "configdefault update id: '{0}' value: '{1}' ";
            var clientCommandResult = new ClientCommandResult();
            if (string.IsNullOrEmpty(path))
            {
                clientCommandResult.Errors.Add(Error.NewError("US1000", "Invalid Path",
                    "Specify a path to a valid file."));
                return clientCommandResult;
            }

            if (!File.Exists(path))
            {
                clientCommandResult.Errors.Add(Error.NewError("US1010",
                    string.Format("The file does not exist: {0}.", path), "Specify a path to a valid file."));
                return clientCommandResult;
            }

            var base64 = File.ReadAllBytes(path).ToBase64();
            var command = string.Format(format, id, base64);
            Console.WriteLine(command);
            return ExecuteCommandString(command);
        }

        public ClientCommandResult DownloadFileSetFile(
            long? id,
            string name,
            string filename,
            string destinationPath,
            string path)
        {
            var collection = new ErrorList();
            if (!id.HasValue && string.IsNullOrEmpty(name))
                collection.Add(Error.NewError("US0500",
                    "DownloadFileSetFile requires either an id or name in order to set a file.",
                    "Supply a valid value and retry your command."));
            if (string.IsNullOrEmpty(filename))
                collection.Add(Error.NewError("US0501",
                    "DownloadFileSetFile requires a filename in order to set a file.",
                    "Supply a valid value and retry your command."));
            if (string.IsNullOrEmpty(destinationPath))
                collection.Add(Error.NewError("US0502",
                    "DownloadFileSetFile requires a destinationPath in order to set a file.",
                    "Supply a valid value and retry your command."));
            if (!File.Exists(path))
                collection.Add(Error.NewError("US0503",
                    string.Format("The file does not exist: {0}.", path), "Specify a path to a valid file."));
            if (collection.ContainsError())
            {
                var clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.AddRange(collection);
                return clientCommandResult;
            }

            var length1 = File.ReadAllBytes(path).Length;
            var num1 = length1 == 0 ? 0 : length1 / 1048576;
            var num2 = 0;
            Log("Uploading file: {0} - {1} mb", path, num1);
            using (var file = File.OpenRead(path))
            {
                var asciishA1Hash = file.ToASCIISHA1Hash();
                file.Seek(0L, SeekOrigin.Begin);
                var clientCommandResult1 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session,
                    false,
                    "downloadfile set-file " +
                    (id.HasValue ? string.Format("id:{0} ", id.Value) : string.Format("name:'{0}' ", name)) +
                    string.Format("filename:'{0}' destinationpath:'{1}' filekey:'{2}'", filename,
                        destinationPath.EscapedJson(), asciishA1Hash));
                if (!clientCommandResult1.Success)
                    return clientCommandResult1;
                var numArray = new byte[1048576];
                for (var length2 = ReadChunk(file, numArray); length2 > 0; length2 = ReadChunk(file, numArray))
                {
                    var destinationArray = new byte[length2];
                    Array.Copy(numArray, destinationArray, destinationArray.Length);
                    var clientCommandResult2 = ClientCommand<ClientCommandResult>.ExecuteCommand(
                        Session, false,
                        "downloadfile upload-file " +
                        (id.HasValue ? string.Format("id:{0} ", id.Value) : string.Format("name:'{0}' ", name)) +
                        string.Format("data:'{0}'", destinationArray.ToBase64()));
                    if (!clientCommandResult2.Success)
                        return clientCommandResult2;
                    num2 += destinationArray.Length;
                    Log("Uploaded {0} mb / {1} mb", num2 == 0 ? 0 : num2 / 1048576, num1);
                }

                var clientCommandResult3 = ClientCommand<ClientCommandResult>.ExecuteCommand(Session,
                    false,
                    "downloadfile upload-file " +
                    (id.HasValue ? string.Format("id:{0}", id.Value) : string.Format("name:'{0}'", name)) +
                    " finished:true");
                if (clientCommandResult3.Success)
                    Log("Upload complete");
                return clientCommandResult3;
            }
        }

        public ClientCommandResult DownloadFileSetScript(long? id, string name, string path)
        {
            var collection = new ErrorList();
            if (!id.HasValue && string.IsNullOrEmpty(name))
                collection.Add(Error.NewError("US0505",
                    "DownloadFileSetScript requires either an id or name in order to set a script.",
                    "Supply a valid value and retry your command."));
            if (!File.Exists(path))
                collection.Add(Error.NewError("US0506",
                    string.Format("The file does not exist: {0}.", path), "Specify a path to a valid file."));
            if (collection.ContainsError())
            {
                var clientCommandResult = new ClientCommandResult();
                clientCommandResult.Errors.AddRange(collection);
                return clientCommandResult;
            }

            var command =
                string.Format(
                    "downloadfile set-script " +
                    (id.HasValue ? string.Format(" id:{0} ", id.Value) : string.Format(" name:'{0}' ", name)) +
                    " text:'{0}'", File.ReadAllBytes(path).ToBase64());
            Console.WriteLine(command);
            return ExecuteCommandString(command);
        }

        public ClientCommandResult StoreCacheGet(
            string filterKey,
            string filterValue,
            string kioskStatus,
            string enabled,
            out List<StoreCacheData> storeCacheData)
        {
            var clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false,
                string.Format("storecache list key:'{0}' value:'{1}' kioskStatus:'{2}' enabled:'{3}'", filterKey,
                    filterValue, kioskStatus, enabled));
            storeCacheData = clientCommandResult.Success
                ? clientCommandResult.CommandMessages[0].ToObject<List<StoreCacheData>>()
                : new List<StoreCacheData>();
            return clientCommandResult;
        }

        public ClientCommandResult StatusMessageGet(
            string key,
            string subKey,
            string type,
            int? numResults,
            out List<StatusMessage> updates)
        {
            var str = string.Format("STATUSMESSAGE list", key);
            if (!string.IsNullOrEmpty(key))
                str += string.Format(" key:{0}", key);
            if (!string.IsNullOrEmpty(subKey))
                str += string.Format(" subkey:{0}", subKey);
            if (!string.IsNullOrEmpty(type))
                str += string.Format(" type:{0}", type);
            if (numResults.HasValue)
                str += string.Format(" numresults:{0}", numResults.Value);
            var clientCommandResult =
                ClientCommand<ClientCommandResult>.ExecuteCommand(Session, false, str);
            updates = clientCommandResult.Success
                ? clientCommandResult.CommandMessages[0].ToObject<List<StatusMessage>>()
                : new List<StatusMessage>();
            return clientCommandResult;
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
            Console.WriteLine(text, parms);
        }
    }
}