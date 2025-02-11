using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class QueueServiceProxy : IQueueService
    {
        private string m_url;

        public static QueueServiceProxy Instance => Singleton<QueueServiceProxy>.Instance;

        public void Initialize(string updateManagerUrl)
        {
            this.m_url = updateManagerUrl;
            ServiceLocator.Instance.AddService(typeof(IQueueService), (object)this);
        }

        public void Enqueue(string label, object entry)
        {
            string command = string.Format("queue enqueue label: '{0}' entry: '{1}'", (object)label, (object)entry.ToJson().EscapedJson());
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

        public void EnqueueRaw(string label, string entry)
        {
            string command = string.Format("queue enqueue label: '{0}' entry: '{1}'", (object)label, (object)entry.EscapedJson());
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

        public void Dequeue(string label)
        {
            string command = string.Format("queue dequeue label: '{0}'", (object)label);
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

        public T Peek<T>(string label)
        {
            string command = string.Format("queue peek label: '{0}'", (object)label);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages.Count < 1 ? default(T) : clientCommandResult.CommandMessages[0].ToObject<T>();
            }
        }

        public string PeekRaw(string label)
        {
            string command = string.Format("queue peek label: '{0}'", (object)label);
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString(command);
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
                return clientCommandResult.CommandMessages.Count < 1 ? (string)null : clientCommandResult.CommandMessages[0];
            }
        }

        private QueueServiceProxy()
        {
        }
    }
}
