using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Redbox.UpdateManager.Remoting
{
    internal class KioskEngineProxy
    {
        internal static string IpcHostUrl;
        private static ClientSession _clientSession;

        internal static bool IsEngineRunning()
        {
            return KioskEngineProxy.ExecuteCommandString("engine ping").Success;
        }

        internal static void Shutdown() => KioskEngineProxy.Send("engine shutdown");

        internal static ClientCommandResult Send(string command)
        {
            if (!KioskEngineProxy.IsEngineRunning())
            {
                string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties("${engine}");
                LogHelper.Instance.Log("Engine not running. Starting engine so a message can be sent.");
                if (File.Exists(str))
                {
                    if (ShellHelper.StartProcessAsShellUser(str, "--bundle:\"Rental Application,*\"") < 0)
                        LogHelper.Instance.Log("Error impersonating shell user. Errors found above");
                    ManualResetEvent manualResetEvent = new ManualResetEvent(false);
                    manualResetEvent.WaitOne(5000);
                    manualResetEvent.Close();
                }
                if (!KioskEngineProxy.IsEngineRunning())
                {
                    LogHelper.Instance.Log("Engine did not start. Command: '{0}' not sent.", (object)command);
                    throw new ApplicationException(string.Format("Engine did not start. Command: '{0}' not sent.", (object)command));
                }
            }
            LogHelper.Instance.Log("Sending IPC command '{0}' to engine.", (object)command);
            ClientCommandResult clientCommandResult = KioskEngineProxy.ExecuteCommandString(command);
            if (!clientCommandResult.Success)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (clientCommandResult.Errors.Count > 0)
                {
                    foreach (Redbox.IPC.Framework.Error error in (List<Redbox.IPC.Framework.Error>)clientCommandResult.Errors)
                    {
                        stringBuilder.AppendLine(error.ToString());
                        stringBuilder.AppendLine(string.Format("Details: {0}\n", (object)error.Details));
                    }
                }
                LogHelper.Instance.Log(string.Format("IPC command \"{0}\" to host \"{1}\" failed.  {2}", (object)command, (object)KioskEngineProxy.IpcHostUrl, (object)stringBuilder.ToString()), LogEntryType.Error);
            }
            return clientCommandResult;
        }

        internal static ClientSession IPCClientSession
        {
            get
            {
                if (KioskEngineProxy._clientSession != null && KioskEngineProxy._clientSession.IsDisposed)
                    throw new ObjectDisposedException("Cannot use disposed Kiosk Engine client session.");
                return KioskEngineProxy._clientSession ?? (KioskEngineProxy._clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(KioskEngineProxy.IpcHostUrl), new int?(30000)));
            }
            set => KioskEngineProxy._clientSession = value;
        }

        internal static ClientCommandResult ExecuteCommandString(string command)
        {
            int num = 0;
            ClientCommandResult clientCommandResult = (ClientCommandResult)null;
            for (; num <= 3; ++num)
            {
                try
                {
                    clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(KioskEngineProxy.IPCClientSession, false, command);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to send IPC command '{0}' ({1})", (object)command, (object)ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                if (clientCommandResult.Errors.FindAll((Predicate<Redbox.IPC.Framework.Error>)(x => x.Code == "J777" || x.Code == "J999" || x.Code == "J001")).Any<Redbox.IPC.Framework.Error>())
                {
                    if (KioskEngineProxy.IPCClientSession != null)
                    {
                        KioskEngineProxy.IPCClientSession.Dispose();
                        KioskEngineProxy.IPCClientSession = (ClientSession)null;
                    }
                }
                else
                    break;
            }
            return clientCommandResult;
        }
    }
}
