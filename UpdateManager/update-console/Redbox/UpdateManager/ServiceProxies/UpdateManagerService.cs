using Redbox.Core;
using Redbox.IPC.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Redbox.UpdateManager.ServiceProxies
{
    internal class UpdateManagerService : IDisposable
    {
        private const bool TransientSessionFlag = false;
        private ClientSession m_clientSession;
        private static readonly Guid m_applicationGuid = new Guid("{FB5E48FF-4677-4cfa-8ED1-265114D7564C}");

        public static UpdateManagerService GetService(string url)
        {
            return new UpdateManagerService(url)
            {
                Timeout = new int?(600000)
            };
        }

        public static UpdateManagerService GetService(string url, int timeout)
        {
            return new UpdateManagerService(url)
            {
                Timeout = new int?(timeout)
            };
        }

        public ClientCommandResult GetStoreNumber(out string number)
        {
            number = string.Empty;
            ClientCommandResult storeNumber = this.ExecuteCommandString("data get id: 'config-store-number'");
            if (storeNumber.Success)
                number = storeNumber.CommandMessages[0].ToObject<string>();
            return storeNumber;
        }

        public ClientCommandResult IsDeveloperUI(out bool isDeveloperUI)
        {
            isDeveloperUI = false;
            ClientCommandResult clientCommandResult = this.ExecuteCommandString("data get id: 'config-developer-mode-flag'");
            if (clientCommandResult.Success)
                isDeveloperUI = clientCommandResult.CommandMessages[0].ToObject<bool>();
            return clientCommandResult;
        }

        public ClientCommandResult GetInitialSubscriptionState(out bool initialSubscriptionState)
        {
            initialSubscriptionState = false;
            ClientCommandResult subscriptionState = this.ExecuteCommandString("data get id: 'config-initial-subscription-state-flag'");
            if (subscriptionState.Success)
                initialSubscriptionState = subscriptionState.CommandMessages[0].ToObject<bool>();
            return subscriptionState;
        }

        public ClientCommandResult ExportQueue(string path)
        {
            return this.ExecuteCommandString(string.Format("engine export-queue path: '{0}'", (object)path));
        }

        public ClientCommandResult ExportSessions(string path)
        {
            return this.ExecuteCommandString(string.Format("engine export-sessions path: '{0}'", (object)path));
        }

        public ClientCommandResult DoWork(string filter)
        {
            return this.ExecuteCommandString(!string.IsNullOrEmpty(filter) ? string.Format("client dowork filter:{0}", (object)filter) : string.Format("client dowork"));
        }

        public ClientCommandResult Poll() => this.ExecuteCommandString(string.Format("client poll"));

        public ClientCommandResult ShutdownEngine(int timeout, int tries)
        {
            ClientCommandResult clientCommandResult = this.ExecuteCommandString("engine shutdown");
            if (clientCommandResult.Success)
            {
                int num = 0;
                ManualResetEvent manualResetEvent1 = new ManualResetEvent(false);
                manualResetEvent1.WaitOne(timeout);
                manualResetEvent1.Close();
                for (; this.IsEngineRunning() && num < tries; ++num)
                {
                    ManualResetEvent manualResetEvent2 = new ManualResetEvent(false);
                    manualResetEvent2.WaitOne(timeout);
                    manualResetEvent2.Close();
                }
                if (this.IsEngineRunning())
                {
                    clientCommandResult.Success = false;
                    clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("T999", "The engine failed to shutdown in the time specified.", "Try to shutdown the engine again."));
                }
            }
            return clientCommandResult;
        }

        public ClientCommandResult ShutdownEngine() => this.ExecuteCommandString("engine shutdown");

        public ClientCommandResult LoadBundle(string bundle)
        {
            return this.ExecuteCommandString(string.Format("engine load-bundle bundle: '{0}'", (object)bundle));
        }

        public ClientCommandResult ResetBundle() => this.ExecuteCommandString("engine reset-bundle");

        public ClientCommandResult ShowOfflineScreen()
        {
            return this.ExecuteCommandString("engine show-offline-screen");
        }

        public ClientCommandResult BringToFront() => this.ExecuteCommandString("engine bring-to-front");

        public ClientCommandResult Start() => this.ExecuteCommandString("engine start");

        public ClientCommandResult Start(string bundleName)
        {
            return this.ExecuteCommandString(string.Format("engine start bundle: '{0}'", (object)bundleName));
        }

        public ClientCommandResult ExecuteScript(string path)
        {
            return File.Exists(path) ? this.ExecuteCommandString(string.Format("engine execute-script name: '{0}'", (object)path)) : throw new FileNotFoundException(path);
        }

        public void Dispose()
        {
            if (this.Session == null || this.Session.IsDisposed)
                return;
            this.Session.Dispose();
        }

        public ClientCommandResult ExecuteCommandString(string command)
        {
            return ClientCommand<ClientCommandResult>.ExecuteCommand(this.Session, false, command);
        }

        public bool IsEngineRunning()
        {
            bool createdNew;
            using (new Mutex(false, string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Local\\{0}", (object)UpdateManagerService.m_applicationGuid), out createdNew))
                return !createdNew || Process.GetProcessesByName("kioskengine").Length != 0;
        }

        public string Url { get; set; }

        public int? Timeout { get; private set; }

        internal UpdateManagerService(string url) => this.Url = url;

        internal ClientSession Session
        {
            get
            {
                if (this.m_clientSession != null && this.m_clientSession.IsDisposed)
                    throw new ObjectDisposedException("Cannot use disposed Update Service client session.");
                if (this.m_clientSession == null)
                    this.m_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(this.Url), new int?());
                this.m_clientSession.Timeout = this.Timeout.HasValue ? this.Timeout.Value : 30000;
                return this.m_clientSession;
            }
            set => this.m_clientSession = value;
        }
    }
}
