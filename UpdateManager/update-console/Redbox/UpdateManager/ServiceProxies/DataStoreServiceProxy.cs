using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class DataStoreServiceProxy : IDataStoreService
    {
        private string m_url;

        public static DataStoreServiceProxy Instance => Singleton<DataStoreServiceProxy>.Instance;

        public void Initialize(string updateManagerUrl)
        {
            this.m_url = updateManagerUrl;
            ServiceLocator.Instance.AddService(typeof(IDataStoreService), (object)this);
        }

        public void Set(Guid id, object o) => this.SetRaw(id.ToString(), o.ToJson().EscapedJson());

        public void Set(string id, object o) => this.SetRaw(id, o.ToJson().EscapedJson());

        public void SetRaw(Guid id, string o) => this.SetRaw(id.ToString(), o);

        public void SetRaw(string id, string o)
        {
            string command = string.Format("data set id: '{0}' o: '{1}'", (object)id, (object)o);
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

        public T Get<T>(Guid id) => this.Get<T>(id.ToString());

        public T Get<T>(string id)
        {
            string raw = this.GetRaw(id);
            return raw == null ? default(T) : raw.ToObject<T>();
        }

        public string GetRaw(Guid id) => this.GetRaw(id.ToString());

        public string GetRaw(string id)
        {
            string command = string.Format("data get id: '{0}'", (object)id);
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

        public void Delete(Guid id) => this.Delete(id.ToString());

        public void Delete(string id)
        {
            string command = string.Format("data delete id: '{0}'", (object)id);
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

        public void Reset()
        {
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString("data reset");
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void CleanUp()
        {
            using (UpdateManagerService service = UpdateManagerService.GetService(this.m_url))
            {
                ClientCommandResult clientCommandResult = service.ExecuteCommandString("data cleanup");
                if (!clientCommandResult.Success)
                {
                    string errorString = string.Empty;
                    clientCommandResult.Errors.ForEach((Action<Redbox.IPC.Framework.Error>)(e => errorString += string.Format("{0} Details: {1}\r\n", (object)e, (object)e.Details)));
                    throw new ApplicationException(errorString);
                }
            }
        }

        public void CleanUp(string extension)
        {
            string command = string.Format("data cleanup extension: '{0}'", (object)extension);
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

        private DataStoreServiceProxy()
        {
        }
    }
}
