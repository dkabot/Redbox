using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class UpdateServiceProxy : IUpdateService
    {
        private string m_url;
        private TimeSpan m_timeout = TimeSpan.FromSeconds(120.0);

        public static UpdateServiceProxy Instance => Singleton<UpdateServiceProxy>.Instance;

        public void Initialize(string url)
        {
            this.m_url = url;
            ServiceLocator.Instance.AddService(typeof(IUpdateService), (object)this);
        }

        public void Initialize(string url, TimeSpan timeout)
        {
            this.m_url = url;
            this.m_timeout = timeout;
            ServiceLocator.Instance.AddService(typeof(IUpdateService), (object)this);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList UploadFile(string path)
        {
            return this.NotImplemented(nameof(UploadFile));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList StartDownloads()
        {
            return this.NotImplemented(nameof(StartDownloads));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownloads()
        {
            return this.NotImplemented(nameof(FinishDownloads));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownload(Guid id)
        {
            return this.NotImplemented(nameof(FinishDownload));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ServerPoll()
        {
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString("client serverpoll").Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList WCFServerPoll()
        {
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString("client wcfserverpoll").Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Poll()
        {
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString("client poll").Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList DoWork(string filter)
        {
            string command = "client dowork";
            if (!string.IsNullOrEmpty(filter))
                command = string.Format("client dowork filter: '{0}'", (object)filter);
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(command).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList DoWork() => this.DoWork(string.Empty);

        public Redbox.UpdateManager.ComponentModel.ErrorList ClearWorkQueue()
        {
            return this.NotImplemented(nameof(ClearWorkQueue));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList DeleteFromWorkQueue(string name)
        {
            return this.NotImplemented(nameof(DeleteFromWorkQueue));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddStoreToRepository(
          string number,
          string name)
        {
            return this.NotImplemented(nameof(AddStoreToRepository));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RemoveStoreFromRepository(
          string number,
          string name)
        {
            return this.NotImplemented(nameof(RemoveStoreFromRepository));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddGroupToRepository(
          string group,
          string name)
        {
            return this.NotImplemented(nameof(AddGroupToRepository));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList RemoveGroupFromRepository(
          string group,
          string name)
        {
            return this.NotImplemented(nameof(RemoveGroupFromRepository));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ForcePublish(string name)
        {
            return this.NotImplemented(nameof(ForcePublish));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList GetSubscriptionState(
          string name,
          out DateTime lastRun,
          out SubscriptionState state)
        {
            lastRun = DateTime.MinValue;
            state = SubscriptionState.Idle;
            return this.NotImplemented(nameof(GetSubscriptionState));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList StartInstaller(
          string repositoryHash,
          string frontEndVersion,
          out Dictionary<string, string> response)
        {
            response = (Dictionary<string, string>)null;
            return this.NotImplemented(nameof(StartInstaller));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(
          string guid,
          Dictionary<string, string> data)
        {
            return this.NotImplemented(nameof(FinishInstaller));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(
          string guid,
          string name,
          string value)
        {
            return this.NotImplemented(nameof(FinishInstaller));
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(
          string path,
          out string result)
        {
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("client execute-scriptfile file:'{0}'", (object)path));
            result = "";
            if (clientCommandResult.Success)
                result = clientCommandResult.CommandMessages[0];
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(
          string path,
          out string result,
          bool reset)
        {
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("client execute-scriptfile file:'{0}' reset:'{1}'", (object)path, (object)reset));
            result = "";
            if (clientCommandResult.Success)
                result = clientCommandResult.CommandMessages[0];
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(
          string script,
          out string result)
        {
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("client execute-script script:'{0}'", (object)script));
            result = "";
            if (clientCommandResult.Success)
                result = clientCommandResult.CommandMessages[0];
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(
          string script,
          out string result,
          bool reset)
        {
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("client execute-script script:'{0}' reset:'{1}'", (object)script, (object)reset));
            result = "";
            if (clientCommandResult.Success)
                result = clientCommandResult.CommandMessages[0];
            return UpdateServiceProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList DoInCompleteWork(IEnumerable<Guid> ids)
        {
            return this.NotImplemented(nameof(DoInCompleteWork));
        }

        public string StoreNumber() => string.Empty;

        private ClientCommandResult ExecuteCommandString(string command)
        {
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url, (int)this.m_timeout.TotalMilliseconds))
                return service.ExecuteCommandString(command);
        }

        private Redbox.UpdateManager.ComponentModel.ErrorList NotImplemented(string method)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("USP999", string.Format("UpdateServiceProxy.{0} not implemented.", (object)method), ""));
            return errorList;
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromIPCErrors(
          List<Redbox.IPC.Framework.Error> ipc)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ipc.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }

        private UpdateServiceProxy()
        {
        }
    }
}
