using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Redbox.UpdateManager.Remoting
{
    internal class KioskEngineService : IKioskEngineService
    {
        private string _kioskEnginePath;

        public static KioskEngineService Instance => Singleton<KioskEngineService>.Instance;

        public void Initialize(string kioskEnginePath, string kioskEngineUrl)
        {
            string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(kioskEnginePath);
            if (!Path.IsPathRooted(str))
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), str);
            this._kioskEnginePath = kioskEnginePath;
            ServiceLocator.Instance.AddService(typeof(IKioskEngineService), (object)this);
            KioskEngineProxy.IpcHostUrl = kioskEngineUrl;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList ExportSessions(string path)
        {
            ErrorList errorList = new ErrorList();
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(ExportSessions)), "Windows Messaging communication no longer supported."));
            return errorList;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList ExportQueue(string path)
        {
            ErrorList errorList = new ErrorList();
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(ExportQueue)), "Windows Messaging communication no longer supported."));
            return errorList;
        }

        public ErrorList IsEngineRunning(out bool isRunning)
        {
            isRunning = false;
            ErrorList errorList = new ErrorList();
            try
            {
                isRunning = KioskEngineProxy.IsEngineRunning();
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.IsEngineRunning.", ex));
            }
            return errorList;
        }

        public ErrorList StartEngine()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Starting engine at location: {0}", (object)this._kioskEnginePath);
                if (ShellHelper.StartProcessAsShellUser(this._kioskEnginePath) < 0)
                    LogHelper.Instance.Log("Error impersonating shell user. Errors found above");
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.StartEngine.", ex));
            }
            return errorList;
        }

        public ErrorList StartEngine(string bundleName)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Starting engine at location: {0}", (object)this._kioskEnginePath);
                if (ShellHelper.StartProcessAsShellUser(this._kioskEnginePath, string.Format("--bundle:\"{0}\"", (object)bundleName)) < 0)
                    LogHelper.Instance.Log("Error impersonating shell user. Errors found above");
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.StartEngine.", ex));
            }
            return errorList;
        }

        public ErrorList ReloadBundle()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                KioskEngineProxy.Send("engine reset-bundle");
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.ReloadBundle.", ex));
            }
            return errorList;
        }

        public ErrorList Shutdown()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                if (!KioskEngineProxy.IsEngineRunning())
                    return errorList;
                KioskEngineProxy.Send("engine shutdown");
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.Shutdown.", ex));
            }
            return errorList;
        }

        public ErrorList Shutdown(int timeout, int tries)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                if (!KioskEngineProxy.IsEngineRunning())
                    return errorList;
                KioskEngineProxy.Send("engine shutdown");
                int num = 0;
                ManualResetEvent manualResetEvent1 = new ManualResetEvent(false);
                manualResetEvent1.WaitOne(timeout);
                manualResetEvent1.Close();
                bool isRunning;
                for (errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.IsEngineRunning(out isRunning)); isRunning && num < tries; ++num)
                {
                    ManualResetEvent manualResetEvent2 = new ManualResetEvent(false);
                    manualResetEvent2.WaitOne(timeout);
                    manualResetEvent2.Close();
                }
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.IsEngineRunning(out isRunning));
                if (isRunning)
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("T999", "The engine failed to shutdown in the time specified.", "Try to shutdown the engine again."));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.Shutdown.", ex));
            }
            return errorList;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList ChangeBundle(string name)
        {
            ErrorList errorList = new ErrorList();
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(ChangeBundle)), "Windows Messaging communication no longer supported."));
            return errorList;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList BringToFront()
        {
            ErrorList front = new ErrorList();
            front.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(BringToFront)), "Windows Messaging communication no longer supported."));
            return front;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList ExecuteScript(string path)
        {
            ErrorList errorList = new ErrorList();
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(ExecuteScript)), "Windows Messaging communication no longer supported."));
            return errorList;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList ShowOfflineScreen()
        {
            ErrorList errorList = new ErrorList();
            errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(ShowOfflineScreen)), "Windows Messaging communication no longer supported."));
            return errorList;
        }

        [Obsolete("Windows Messaging communication no longer supported")]
        public ErrorList GetMemoryUsage(out long total)
        {
            ErrorList memoryUsage = new ErrorList();
            memoryUsage.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("W999", string.Format("Service method '{0}' not implemented.", (object)nameof(GetMemoryUsage)), "Windows Messaging communication no longer supported."));
            total = -1L;
            return memoryUsage;
        }

        private KioskEngineService()
        {
        }
    }
}
