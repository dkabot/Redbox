using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class RepositoryServiceProxy : IRepositoryService, IDisposable
    {
        private string m_url;

        public static RepositoryServiceProxy Instance => Singleton<RepositoryServiceProxy>.Instance;

        public void Initialize(string url)
        {
            this.m_url = url;
            ServiceLocator.Instance.AddService(typeof(IRepositoryService), (object)this);
        }

        public void Dispose()
        {
        }

        public void TrimRepository(string name)
        {
            string command = string.Format("repository trim-repository name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ActivateTo(
          string name,
          string hash,
          out bool deferredMove)
        {
            string command = string.Format("repository activate-to name: '{0}' hash: '{1}'", (object)name, (object)hash);
            deferredMove = false;
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (clientCommandResult.Success)
                    deferredMove = clientCommandResult.CommandMessages[0].ToObject<bool>();
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
            }
        }

        public string FormatStagedFileName(string file)
        {
            string command = string.Format("repository format-stage-file-name file: '{0}'", (object)file.Escaped());
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<string>();
            }
        }

        public void UnpackChangeSet(string name, Guid id, string archive)
        {
            string command = string.Format("repository unpack-change-set name: '{2}' id: '{0}' archive: '{1}'", (object)id, (object)archive.Escaped(), (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public bool ContainsRepository(string name)
        {
            string command = string.Format("repository contains-repository name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.Success && clientCommandResult.CommandMessages[0].ToObject<bool>();
            }
        }

        public void AddRepository(string name)
        {
            string command = string.Format("repository add-repository name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void AddRevisions(string name, List<string> revisions)
        {
            string command = string.Format("repository add-revisions name: '{0}' revisions: '{1}'", (object)name, (object)revisions.ToJson().EscapedJson());
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void SetLabel(string name, string revision, string label)
        {
            if (string.IsNullOrEmpty(label))
                return;
            string command = string.Format("repository set-label name: '{0}' revision: '{1}' label: '{2}'", (object)name, (object)revision, (object)label);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void RemoveRevisions(string name, List<string> revisions)
        {
            string command = string.Format("repository remove-revisions name: '{0}' revisions: '{1}'", (object)name, (object)revisions.ToJson().EscapedJson());
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void AddDelta(string name, List<DeltaItem> deltaItemList)
        {
            throw new ApplicationException("RepositoryServiceProxy.AddDelta is not implemented.");
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
            string command = string.Format("repository add-delta name: '{0}' revision: '{1}' file: '{2}' isSeed: {3} isPlaceHolder: {4} contentHash: '{5}' versionHash: '{6}'", (object)name, (object)revision, (object)file.Escaped(), (object)isSeed, (object)isPlaceHolder, (object)contentHash, (object)versionHash);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public string GetLabel(string name, string revision)
        {
            string command = string.Format("repository get-label name: '{0}' revision: '{1}'", (object)name, (object)revision);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<string>();
            }
        }

        public List<string> GetAllRepositories()
        {
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString("repository list-repositories");
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<List<string>>();
            }
        }

        public List<IRevLog> GetUnfinishedChanges(string name)
        {
            string command = string.Format("repository list-unfinished-changes name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                List<IRevLog> unfinishedChanges = new List<IRevLog>();
                clientCommandResult.CommandMessages[0].ToObject<List<RevLog>>().ForEach(new Action<RevLog>(unfinishedChanges.Add));
                return unfinishedChanges;
            }
        }

        public List<IRevLog> GetPendingChanges(string name)
        {
            string command = string.Format("repository list-pending-changes name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                List<IRevLog> pendingChanges = new List<IRevLog>();
                clientCommandResult.CommandMessages[0].ToObject<List<RevLog>>().ForEach(new Action<RevLog>(pendingChanges.Add));
                return pendingChanges;
            }
        }

        public List<IRevLog> GetPendingActivations(string name)
        {
            string command = string.Format("repository list-pending-activations name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                List<IRevLog> pendingActivations = new List<IRevLog>();
                clientCommandResult.CommandMessages[0].ToObject<List<RevLog>>().ForEach(new Action<RevLog>(pendingActivations.Add));
                return pendingActivations;
            }
        }

        public List<IRevLog> GetAllChanges(string name)
        {
            string command = string.Format("repository list-all-changes name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                List<IRevLog> allChanges = new List<IRevLog>();
                clientCommandResult.CommandMessages[0].ToObject<List<RevLog>>().ForEach(new Action<RevLog>(allChanges.Add));
                return allChanges;
            }
        }

        public List<IRevLog> GetActivatedChanges(string name)
        {
            string command = string.Format("repository list-activated-changes name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                List<IRevLog> activatedChanges = new List<IRevLog>();
                clientCommandResult.CommandMessages[0].ToObject<List<RevLog>>().ForEach(new Action<RevLog>(activatedChanges.Add));
                return activatedChanges;
            }
        }

        public string GetHeadRevision(string name)
        {
            string command = string.Format("repository get-head-hash name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<string>();
            }
        }

        public string GetActiveRevision(string name)
        {
            string command = string.Format("repository get-active-revision name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<string>();
            }
        }

        public string GetStagedRevision(string name)
        {
            string command = string.Format("repository get-staged-revision name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<string>();
            }
        }

        public string GetActiveLabel(string name)
        {
            string command = string.Format("repository get-active-label name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<string>();
            }
        }

        public List<IRevLog> GetAppliedChanges(string name)
        {
            string command = string.Format("repository list-applied-changes name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages[0].ToObject<List<IRevLog>>();
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList UpdateTo(string name, string targetHash)
        {
            string command = string.Format("repository update-to name: '{0}' hash: '{1}'", (object)name, (object)targetHash);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)service.ExecuteCommandString(command).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RebuildTo(string name, string targetHash)
        {
            string command = string.Format("repository rebuild-to name: '{0}' hash: '{1}'", (object)name, (object)targetHash);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)service.ExecuteCommandString(command).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RebuildToHead(string name)
        {
            string command = string.Format("repository rebuild-to-head name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)service.ExecuteCommandString(command).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ActivateToHead(
          string name,
          out bool deferredMove)
        {
            deferredMove = false;
            string command = string.Format("repository activate-to-head name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (clientCommandResult.Success)
                    deferredMove = clientCommandResult.CommandMessages[0].ToObject<bool>();
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
            }
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList UpdateToHead(string name)
        {
            string command = string.Format("repository update-to-head name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)service.ExecuteCommandString(command).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Verify(string name)
        {
            string command = string.Format("repository verify name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)service.ExecuteCommandString(command).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Repair(string name, out bool deferredMove)
        {
            deferredMove = false;
            string command = string.Format("repository repair name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (clientCommandResult.Success)
                    deferredMove = clientCommandResult.CommandMessages[0].ToObject<bool>();
                return RepositoryServiceProxy.ConvertToComponentErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
            }
        }

        public void Reset(string name, out bool defferedDelete)
        {
            defferedDelete = false;
            string command = string.Format("repository reset name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                defferedDelete = clientCommandResult.CommandMessages[0].ToObject<bool>();
            }
        }

        public void Reset(out bool defferedDelete)
        {
            defferedDelete = false;
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString("repository reset-all");
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                defferedDelete = clientCommandResult.CommandMessages[0].ToObject<bool>();
            }
        }

        public bool Subscribed(string name)
        {
            string command = string.Format("repository subscribed name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.Success && clientCommandResult.CommandMessages[0].ToObject<bool>();
            }
        }

        public void EndableSubscription(string name)
        {
            string command = string.Format("repository enable-subscription name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void DisableSubscription(string name)
        {
            string command = string.Format("repository disable-subscription name: '{0}'", (object)name);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        private RepositoryServiceProxy()
        {
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertToComponentErrors(
          List<Redbox.IPC.Framework.Error> ipcErrors)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ipcErrors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }
    }
}
