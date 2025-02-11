using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class TransferJobProxy : ITransferJob
    {
        private string m_url;

        public TransferJobType JobType { get; set; }

        public string Name { get; set; }

        public Guid ID { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        public DateTime? FinishTime { get; set; }

        public string Owner { get; set; }

        public ulong TotalBytesTransfered { get; set; }

        public ulong TotalBytes { get; set; }

        public TransferStatus Status { get; set; }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddItem(string url, string file)
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer add-item id: '{0}' url: '{1}' file: '{2}'", (object)this.ID, (object)url.Escaped(), (object)file.Escaped())).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Complete()
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer complete id: '{0}'", (object)this.ID)).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Cancel()
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer cancel id: '{0}'", (object)this.ID)).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Suspend()
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer suspend id: '{0}'", (object)this.ID)).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList Resume()
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer resume id: '{0}'", (object)this.ID)).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList SetPriority(TransferJobPriority priority)
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer set-priority id: '{0}' priority: {1}", (object)this.ID, (object)priority.ToString("G"))).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList SetCallback(
          ITransferCallbackParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList TakeOwnership() => new Redbox.UpdateManager.ComponentModel.ErrorList();

        public Redbox.UpdateManager.ComponentModel.ErrorList SetNoProgressTimeout(uint period)
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer set-no-progress-timeout id: '{0}' timeout: {1}", (object)this.ID, (object)period)).Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList SetMinimumRetryDelay(uint seconds)
        {
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)this.ExecuteCommandString(string.Format("transfer set-minimum-retry-delay id: '{0}' seconds: {1}", (object)this.ID, (object)seconds)).Errors);
        }

        public void GetErrors(out Redbox.UpdateManager.ComponentModel.ErrorList errors)
        {
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("transfer get-errors id: '{0}'", (object)this.ID));
            errors = TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList GetItems(out List<ITransferItem> items)
        {
            items = new List<ITransferItem>();
            ClientCommandResult clientCommandResult = this.ExecuteCommandString(string.Format("transfer get-items id: '{0}'", (object)this.ID));
            if (clientCommandResult.Success)
                items = clientCommandResult.CommandMessages[0].ToObject<List<TransferItem>>().ConvertAll<ITransferItem>((Converter<TransferItem, ITransferItem>)(t => (ITransferItem)t));
            return TransferJobProxy.ConvertFromIPCErrors((List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors);
        }

        public TransferJobProxy()
        {
        }

        internal void SetUrl(string url) => this.m_url = url;

        internal TransferJobProxy(Guid id, string url)
        {
            this.ID = id;
            this.m_url = url;
        }

        private ClientCommandResult ExecuteCommandString(string command)
        {
            using (UpdateManagerService updateManagerService = new UpdateManagerService(this.m_url))
                return updateManagerService.ExecuteCommandString(command);
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromIPCErrors(
          List<Redbox.IPC.Framework.Error> ipc)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            ipc.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }
    }
}
