using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.UpdateManager.BITS;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.Kernel;
using Redbox.UpdateManager.Remoting;
using Redbox.UpdateManager.TaskScheduler;

namespace Redbox.UpdateManager.Domain
{
    class UpdateManagerService : IDisposable
    {
        public static UpdateManagerService Instance
        {
            get
            {
                return Singleton<UpdateManagerService>.Instance;
            }
        }

        public void Intialize(string kioskEnginePath, string scriptLocation, string updateServiceUrl, string wcfUpdateServiceUrl, string kioskEngineUrl, bool developerMode, bool initialSubscriptionState, string storeNumberFromConfigFile, int trimWorkingSetInterval, TimeSpan updateServiceTimeout, string shutdownScript, int minimumRetryDelay)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            ErrorList errorList = new ErrorList();
            string text;
            if (string.IsNullOrEmpty(storeNumberFromConfigFile))
            {
                text = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "ID", "N\\A") as string;
                if (text == null || text == "N\\A")
                {
                    if (!Debugger.IsAttached)
                    {
                        throw new ArgumentException("This store does not have a store id. The engine can not run.");
                    }
                    text = "7045";
                }
            }
            else
            {
                text = storeNumberFromConfigFile;
            }
            this._storeNumber = text;
            string directoryName = Path.GetDirectoryName(typeof(UpdateManagerService).Assembly.Location);
            service["StoreNumber"] = text;
            service["REDS"] = "C:\\Program Files\\Redbox\\REDS";
            service["RedboxProgramFiles"] = "C:\\Program Files\\Redbox";
            service["ProgramFiles"] = "C:\\Program Files";
            this.ScriptLocation = scriptLocation;
            this.KioskEnginePath = kioskEnginePath;
            if (!Path.IsPathRooted(this.ScriptLocation))
            {
                this.ScriptLocation = Path.Combine(directoryName, this.ScriptLocation);
            }
            if (!Path.IsPathRooted(kioskEnginePath))
            {
                this.KioskEnginePath = Path.Combine(directoryName, this.KioskEnginePath);
            }
            LogHelper.Instance.Log("Working Directory: {0}", new object[] { directoryName });
            service["WorkingDirectory"] = directoryName;
            service["engine"] = this.KioskEnginePath;
            service["Scripts"] = this.ScriptLocation;
            LogHelper.Instance.Log("Scripts = {0}", new object[] { service.ExpandProperties("${Scripts}") });
            LogHelper.Instance.Log("engine = {0}", new object[] { service.ExpandProperties("${engine}") });
            KioskEngineService.Instance.Initialize(this.KioskEnginePath, kioskEngineUrl);
            LogHelper.Instance.Log("Kiosk Engine URL: {0}", new object[] { kioskEngineUrl });
            string text2 = service.ExpandProperties("${RunningPath}\\..\\.store\\.data");
            QueueService.Instance.Initialize(text2);
            DataStoreService.Instance.Initialize(text2);
            LogHelper.Instance.Log("Data Store Directory: {0}", new object[] { text2 });
            KernelService.Instance.Initialize();
            Scheduler.Instance.Initialize();
            WindowsTaskScheduler.Instance.Initialize();
            TransferService.Instance.Initialize();
            string text3 = service.ExpandProperties("${RunningPath}\\..\\.store\\.repository");
            RepositoryService.Instance.Initialize(text3);
            LogHelper.Instance.Log("Repository Directory: {0}", new object[] { text3 });
            string text4 = service.ExpandProperties("${RunningPath}\\..\\.store\\.configfiles");
            ConfigFileService.Instance.Initialize(text4);
            LogHelper.Instance.Log("Config File Directory: {0}", new object[] { text4 });
            string text5 = service.ExpandProperties("${RunningPath}\\..\\.store\\.storefiles");
            StoreFileService.Instance.Initialize(text5);
            LogHelper.Instance.Log("StoreFile Directory: {0}", new object[] { text5 });
            IPCPoll.Instance.Initialize(text, updateServiceUrl, updateServiceTimeout, 2);
            LogHelper.Instance.Log("IPCPoll URL: {0}, Store Number: {1}", new object[] { updateServiceUrl, text });
            PollService.Instance.Initialize(IPCPoll.Instance);
            LogHelper.Instance.Log("Poll Service default poll type: {0}", new object[] { IPCPoll.Instance.GetType() });
            Remoting.UpdateService.Instance.Initialize(updateServiceUrl, updateServiceTimeout, text, minimumRetryDelay);
            LogHelper.Instance.Log("IPC Update Service URL: {0}, Store Number: {1}", new object[] { updateServiceUrl, text });
            IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
            service2.Set("config-developer-mode-flag", developerMode);
            service2.Set("config-initial-subscription-state-flag", initialSubscriptionState);
            service2.Set("config-store-number", text);
            if (errorList.ContainsError())
            {
                LogHelper.Instance.Log("Errors from UpdateManagerServiceInitialize to follow:", Array.Empty<object>());
                errorList.ForEach(delegate (Redbox.UpdateManager.ComponentModel.Error e)
                {
                    LogHelper.Instance.Log("{0} details: {1}", new object[] { e, e.Details });
                });
            }
            this.m_shutdownScript = shutdownScript;
        }

        public string KioskEnginePath
        {
            get
            {
                return ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(this.m_kioskEnginePath);
            }
            set
            {
                this.m_kioskEnginePath = value;
            }
        }

        public string ScriptLocation
        {
            get
            {
                return ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(this.m_scriptLocation);
            }
            set
            {
                this.m_scriptLocation = value;
            }
        }

        public void StopScheduler()
        {
            ErrorList errorList = Scheduler.Instance.Stop();
            if (errorList.ContainsError())
            {
                LogHelper.Instance.Log("Errors from TaskScheduler.Stop to follow:", Array.Empty<object>());
                errorList.ForEach(delegate (Redbox.UpdateManager.ComponentModel.Error e)
                {
                    LogHelper.Instance.Log("{0} details: {1}", new object[] { e, e.Details });
                });
            }
        }

        public void StartScheduler()
        {
            ErrorList errorList = Scheduler.Instance.Start();
            if (errorList.ContainsError())
            {
                LogHelper.Instance.Log("Errors from TaskScheduler.Start to follow:", Array.Empty<object>());
                errorList.ForEach(delegate (Redbox.UpdateManager.ComponentModel.Error e)
                {
                    LogHelper.Instance.Log("{0} details: {1}", new object[] { e, e.Details });
                });
            }
        }

        public void RunShutdownScript()
        {
            string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(this.m_scriptLocation);
            if (!File.Exists(text))
            {
                return;
            }
            string text2 = File.ReadAllText(text);
            try
            {
                KernelService.Instance.ExecuteChunk(text2);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Shutdown script failed.", ex);
            }
        }

        public string StoreNumber
        {
            get
            {
                return this._storeNumber;
            }
        }

        public void Dispose()
        {
            ServiceLocator.Instance.GetService<IRepositoryService>().Dispose();
        }

        UpdateManagerService()
        {
        }

        string m_shutdownScript = string.Empty;

        string m_scriptLocation = string.Empty;

        string m_kioskEnginePath = string.Empty;

        string _storeNumber = string.Empty;

        const int Version = 2;
    }
}
