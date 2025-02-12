using System;
using System.Linq;
using Redbox.Core;
using Redbox.IPC.Framework;

namespace Redbox.Shell.Remoting
{
    internal class KioskEngineProxy
    {
        internal static string IpcHostUrl;
        private static ClientSession _clientSession;

        internal static ClientSession IPCClientSession
        {
            get
            {
                if (_clientSession != null && _clientSession.IsDisposed)
                    throw new ObjectDisposedException("Cannot use disposed Kiosk Engine client session.");
                return _clientSession ??
                       (_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(IpcHostUrl), 30000));
            }
            set => _clientSession = value;
        }

        internal static bool IsEngineRunning()
        {
            var clientCommandResult = ExecuteCommandString("engine ping");
            return clientCommandResult != null && clientCommandResult.Success;
        }

        internal static ClientCommandResult ActiviateControlPanel()
        {
            return ExecuteCommandString("engine load-control-panel");
        }

        private static ClientCommandResult ExecuteCommandString(string command)
        {
            var num = 0;
            var clientCommandResult = (ClientCommandResult)null;
            for (; num <= 3; ++num)
            {
                try
                {
                    clientCommandResult =
                        ClientCommand<ClientCommandResult>.ExecuteCommand(IPCClientSession, false, command);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Exception in KioskEngineProxy.ExecuteCommandString()!", ex);
                }

                if (clientCommandResult != null && clientCommandResult.Errors
                        .FindAll(x => x.Code == "J777" || x.Code == "J999" || x.Code == "J001").Any())
                {
                    if (IPCClientSession != null)
                    {
                        IPCClientSession.Dispose();
                        IPCClientSession = null;
                    }
                }
                else
                {
                    break;
                }
            }

            return clientCommandResult;
        }
    }
}