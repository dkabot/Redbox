using Microsoft.Win32;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.Command;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Daemon.Properties;
using Redbox.UpdateManager.DownloadFile;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.FileCache;
using Redbox.UpdateManager.FileSets;
using Redbox.UpdateManager.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace Redbox.UpdateManager.Daemon
{
    public class UpdateManagerService : ServiceBase
    {
        internal const string ServiceMoniker = "updatemgrd$default";
        internal const string StopMessage = "Stop Update Manager Service.";
        internal const string StartMessage = "Start Update Manager Service.";
        public DateTime? ScriptLockedSince;
        public bool ScriptLockedNeedsReboot;
        private Thread m_thread;
        private IPCServiceHost m_listener;
        private const string ServiceApplicationId = "{C423B0E3-95B3-4191-B326-AB3EA516F825}";
        private static NamedLock m_instanceLock;
        private IContainer components;

        public UpdateManagerService()
        {
            LogHelper.Instance.Log("UpdateService constructor");
            this.InitializeComponent();
        }

        protected override void OnStart(string[] args) => this.StartService();

        protected override void OnStop() => this.StopService();

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.QuerySuspend:
                    this.StopService();
                    return true;
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.ResumeAutomatic:
                    this.StartService();
                    return true;
                default:
                    return base.OnPowerEvent(powerStatus);
            }
        }

        protected override void OnShutdown()
        {
            this.StopService();
            base.OnShutdown();
        }

        internal void StartService()
        {
            LogHelper.Instance.Log("Aquiring namedlock");
            UpdateManagerService.m_instanceLock = new NamedLock("{C423B0E3-95B3-4191-B326-AB3EA516F825}", LockScope.Local);
            LogHelper.Instance.Log("Finished Aquiring namedlock");
            LogHelper.Instance.Log("Starting main thread");
            this.m_thread = new Thread(new ThreadStart(this.ExecuteListener));
            this.m_thread.TrySetApartmentState(ApartmentState.MTA);
            this.m_thread.Start();
        }

        internal void StopService()
        {
            LogHelper.Instance.Log("Stopping service");
            Redbox.UpdateManager.Domain.UpdateManagerService.Instance.RunShutdownScript();
            try
            {
                LogHelper.Instance.Log("Stopping Certificate service");
                CertificateService.Instance.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error stopping Certificate service.", ex);
            }
            try
            {
                LogHelper.Instance.Log("Stopping fileset service");
                FileSetService.Instance.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error stopping fileset service.", ex);
            }
            try
            {
                LogHelper.Instance.Log("Stopping Download service");
                Redbox.UpdateManager.DownloadService.DownloadService.Instance.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error stopping Download service.", ex);
            }
            try
            {
                LogHelper.Instance.Log("Stopping downloadfile service");
                DownloadFileService.Instance.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error stopping downloadfile service.", ex);
            }
            try
            {
                LogHelper.Instance.Log("Stopping health service");
                HealthService.Instance.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error stopping health service.", ex);
            }
            try
            {
                LogHelper.Instance.Log("Stopping work service");
                WorkService.Instance.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error stopping work service.", ex);
            }
            try
            {
                LogHelper.Instance.Log("Stop Update Manager Service.", LogEntryType.Info);
                this.EventLog.WriteEntry("Stop Update Manager Service.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error writing stop message to event log.", ex);
            }
            try
            {
                Redbox.UpdateManager.Domain.UpdateManagerService.Instance.StopScheduler();
                Redbox.UpdateManager.Domain.UpdateManagerService.Instance.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception while stopping scheduler or disposing Update Manager Domain.", ex);
            }
            try
            {
                if (this.m_listener != null)
                    this.m_listener.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in StopService.", ex);
            }
            try
            {
                if (UpdateManagerService.m_instanceLock == null)
                    return;
                UpdateManagerService.m_instanceLock.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in releasing instance lock.", ex);
            }
        }

        internal void ExecuteListener()
        {
            try
            {
                LogHelper.Instance.Log("Start Update Manager Service.", LogEntryType.Info);
                this.EventLog.WriteEntry("Start Update Manager Service.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Error writing start message to event log.", ex);
            }
            try
            {
                MacroService instance = new MacroService();
                ServiceLocator.Instance.AddService(typeof(IMacroService), (object)instance);
                string directoryName = Path.GetDirectoryName(typeof(UpdateManagerService).Assembly.Location);
                instance["RunningPath"] = directoryName;
                LogHelper.Instance.Log("Start the daemon thread.", LogEntryType.Info);
                LogHelper.Instance.Log("Initializing status message service");
                StatusMessageService.Instance.Initialize();
                LogHelper.Instance.Log("Initializing health service");
                HealthService.Instance.Initialize();
                LogHelper.Instance.Log("Starting health service");
                HealthService.Instance.Start();
                this.RegisterHealthItems();
                CommandRepository.AssemblyPatterns = new string[3]
                {
          "updatemgrd.exe",
          "Redbox.IPC.Framework.dll",
          "Redbox.UpdateManager.Command.dll"
                };
                LogHelper.Instance.Log("Initialize the Update Manager Service domain.", LogEntryType.Info);
                Redbox.UpdateManager.Domain.UpdateManagerService.Instance.Intialize(Settings.Default.KioskEnginePath, Settings.Default.ScriptLocation, Settings.Default.UpdateServiceUrl, Settings.Default.WcfUpdateServiceUrl, Settings.Default.KioskEngineUrl, Settings.Default.DeveloperMode, Settings.Default.InitialSubscriptionState, Settings.Default.StoreNumber, Settings.Default.TrimWorkingSetInterval, Settings.Default.UpdateServiceTimeout, Settings.Default.ShutdownScriptPath, Settings.Default.MinimumRetryDelay);
                LogHelper.Instance.Log("Execute start-up scripts.", LogEntryType.Info);
                BatchCommandRunner.ExecuteStartupFiles();
                LogHelper.Instance.Log("Start Update Manager Service scheduler.", LogEntryType.Info);
                Redbox.UpdateManager.Domain.UpdateManagerService.Instance.StartScheduler();
                this.m_listener = IPCServiceHost.Create(IPCProtocol.Parse(Settings.Default.Url), AssemblyInfoHelper.GetProductName(typeof(EngineCommand).Assembly), AssemblyInfoHelper.GetVersion(typeof(EngineCommand).Assembly), AssemblyInfoHelper.GetCopyright(typeof(EngineCommand).Assembly), Settings.Default.MaximumWorkerThreads, Settings.Default.MaximumWorkerThreads, 8192, 64, new int?(5));
                LogHelper.Instance.Log("Traffic is non-encrypted; no valid certificate specified or found.", LogEntryType.Info);
                LogHelper.Instance.Log(string.Format("Start Service Listener for interface '{0}' on port '{1}'.", (object)this.m_listener.Protocol.Host, (object)this.m_listener.Protocol.Port), LogEntryType.Info);
                this.m_listener.Start();
                LogHelper.Instance.Log("Initializing work service");
                WorkService.Instance.Initialize();
                LogHelper.Instance.Log("Starting work service");
                WorkService.Instance.Start();
                LogHelper.Instance.Log("Downloadfile service starting");
                DownloadFileService.Instance.Initialize();
                LogHelper.Instance.Log("Starting downloadfile service");
                DownloadFileService.Instance.Start();
                LogHelper.Instance.Log("Download service starting");
                Redbox.UpdateManager.DownloadService.DownloadService.Instance.Initialize(instance.ExpandProperties("${RunningPath}\\..\\.store\\.downloads"));
                Redbox.UpdateManager.DownloadService.DownloadService.Instance.Start();
                LogHelper.Instance.Log("FileCacheService Initialization");
                FileCacheService.Instance.Initialize(instance.ExpandProperties("${RunningPath}\\..\\.store\\.filecache"));
                LogHelper.Instance.Log("FileSet service starting");
                FileSetService.Instance.Initialize(instance.ExpandProperties("${RunningPath}\\..\\.store\\.filesets"));
                FileSetService.Instance.Start();
                LogHelper.Instance.Log("Certificate service starting");
                CertificateService.Instance.Initialize();
                CertificateService.Instance.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Top level exception caught", ex);
            }
        }

        internal void RegisterHealthItems()
        {
            try
            {
                HealthService.Instance.Add("UPDATE_SERVICE_SCRIPT_LOCKED", TimeSpan.FromSeconds(60.0), (Action)(() =>
                {
                    try
                    {
                        bool flag1 = true;
                        object obj1 = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "HealthServiceScriptCheck", (object)"true");
                        bool result1;
                        if (obj1 != null && obj1 is string && bool.TryParse(obj1 as string, out result1))
                            flag1 = result1;
                        if (!flag1)
                            return;
                        bool locked;
                        ServiceLocator.Instance.GetService<IKernelService>().ExecuteChunkLockCheck("kernel.log(\"Heath Check Script - locked check\")", out locked);
                        if (locked)
                        {
                            LogHelper.Instance.Log("Health CheckScript - execution is locked");
                            if (!this.ScriptLockedSince.HasValue)
                                this.ScriptLockedSince = new DateTime?(DateTime.Now);
                            if (SystemFunctions.ShouldReboot())
                            {
                                this.ScriptLockedNeedsReboot = true;
                                LogHelper.Instance.Log("Health Check Script - execution is locked and needs reboot is true");
                            }
                            TimeSpan timeSpan = DateTime.Now - this.ScriptLockedSince.Value;
                            LogHelper.Instance.Log("Health Check Script - has been locked for {0} minutes", (object)Math.Round(timeSpan.TotalMinutes));
                            if (!(timeSpan > TimeSpan.FromHours(1.0)))
                                return;
                            LogHelper.Instance.Log("Health Check Script - execution has been locked for more than an hour");
                            if (this.ScriptLockedNeedsReboot)
                            {
                                LogHelper.Instance.Log("Health Check Script - attempting reboot");
                                object obj2 = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "RunInitialSync", (object)"false");
                                bool flag2 = false;
                                bool result2;
                                if (obj2 != null && obj2 is string && bool.TryParse(obj2 as string, out result2))
                                    flag2 = result2;
                                if (flag2)
                                {
                                    LogHelper.Instance.Log("Health Check Script - Service Watchdog: shouldreboot is true; initial_sync_flag is true; exiting");
                                }
                                else
                                {
                                    Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IKioskEngineService>().Shutdown(15000, 2);
                                    if (errorList.ContainsError())
                                    {
                                        errorList.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
                                        ProcessFunctions.KillProcessByName("kioskengine");
                                    }
                                    LogHelper.Instance.Log("Health Check Script - Setting kiosk to reboot");
                                    KernelService.Instance.ShutdownType = ShutdownType.Reboot;
                                    KernelService.Instance.PerformShutdown();
                                }
                            }
                            else
                                LogHelper.Instance.Log("Health Check Script - reboot has not passed yet");
                        }
                        else
                        {
                            this.ScriptLockedSince = new DateTime?();
                            this.ScriptLockedNeedsReboot = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("(UMS995) Health Check Script - Error running", ex);
                    }
                }));
                HealthService.Instance.Add("UPDATE_SERVICE_SERVERPOLL", TimeSpan.FromMinutes(17.0), (Action)(() =>
                {
                    try
                    {
                        LogHelper.Instance.Log("Running Health Entry for serverpoll.");
                        object obj = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager\\Health Service", "RunServerPoll", (object)"true");
                        bool flag = true;
                        bool result;
                        if (obj != null && obj is string && bool.TryParse(obj as string, out result))
                            flag = result;
                        LogHelper.Instance.Log("Running Health Entry for ServerPoll: {0}.", (object)flag);
                        if (!flag)
                            return;
                        IPollService service = ServiceLocator.Instance.GetService<IPollService>();
                        if (service == null)
                            return;
                        foreach (Redbox.UpdateManager.ComponentModel.Error error in (List<Redbox.UpdateManager.ComponentModel.Error>)service.ServerPoll(true))
                            LogHelper.Instance.Log(error.ToString(), error.IsWarning ? LogEntryType.Info : LogEntryType.Error);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("(UMS990) Error running ServerPoll health item", ex);
                    }
                }));
                HealthService.Instance.Add("UPDATE_SERVICE_WATCHDOG", new TimeSpan(0, 5, 0), (Action)(() =>
                {
                    try
                    {
                        LogHelper.Instance.Log("Running Health Entry for Watchdog.");
                        object obj = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager\\Health Service", "RunWatchdog", (object)"true");
                        bool flag = true;
                        bool result;
                        if (obj != null && obj is string && bool.TryParse(obj as string, out result))
                            flag = result;
                        LogHelper.Instance.Log("Running Health Entry for watchdog: {0}.", (object)flag);
                        if (!flag)
                            return;
                        string path = Path.Combine(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties("${Scripts}"), "watchdog.lua");
                        if (!File.Exists(path))
                        {
                            LogHelper.Instance.Log("Health Entry for watchdog.  Script: {0} not found.", (object)path);
                        }
                        else
                        {
                            IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
                            string chunk = File.ReadAllText(path);
                            LogHelper.Instance.Log("Health Entry for watchdog. Executing lua script at {0}", (object)path);
                            service.ExecuteChunk(chunk);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("(UMS991) Error running watchdog health item", ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("(UMS994) Error registering health items.", ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent() => this.ServiceName = "updatemgrd$default";
    }
}
