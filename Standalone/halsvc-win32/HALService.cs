using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using log4net;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;
using Redbox.HAL.Script.Framework;
using Redbox.HAL.Service.Win32.Commands;
using Redbox.HAL.Service.Win32.Properties;
using KioskConfiguration = Redbox.HAL.Controller.Framework.KioskConfiguration;

namespace Redbox.HAL.Service.Win32
{
    public class HALService : ServiceBase, ILogger
    {
        private const string StopMessage = "Stop Hardware Abstraction Layer (HAL) Service.";
        private const string StartMessage = "Start Hardware Abstraction Layer (HAL) Service";
        private static readonly ILog m_errorLog = LogManager.GetLogger("ErrorLog");
        private static readonly ILog m_log = LogManager.GetLogger("ServiceLog");
        private readonly object PowerStateLock = new object();
        private IContainer components;
        private Thread ListenerThread;
        private IPCProxy Proxy;
        private PowerState State;

        public HALService()
        {
            InitializeComponent();
        }

        public void Log(string message, Exception e)
        {
            m_log.Error(message, e);
        }

        public void Log(string message, LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Info:
                    m_log.Info(message);
                    break;
                case LogEntryType.Debug:
                    m_log.Debug(message);
                    break;
                case LogEntryType.Error:
                    LogServiceError(message);
                    m_log.Error(message);
                    break;
                case LogEntryType.Fatal:
                    m_log.Fatal(message);
                    break;
            }
        }

        public void Log(string message, Exception e, LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Info:
                    m_log.Info(message, e);
                    break;
                case LogEntryType.Debug:
                    m_log.Debug(message, e);
                    break;
                case LogEntryType.Error:
                    LogServiceError(string.Format("{0}: {1} ({2})", message, e.Message, e.StackTrace));
                    m_log.Error(message, e);
                    break;
                case LogEntryType.Fatal:
                    m_log.Fatal(message, e);
                    break;
            }
        }

        public bool IsLevelEnabled(LogEntryType entryLogLevel)
        {
            switch (entryLogLevel)
            {
                case LogEntryType.Info:
                    return m_log.IsInfoEnabled;
                case LogEntryType.Debug:
                    return m_log.IsDebugEnabled;
                case LogEntryType.Error:
                    return m_log.IsErrorEnabled;
                case LogEntryType.Fatal:
                    return m_log.IsFatalEnabled;
                default:
                    return false;
            }
        }

        public void Log(string msg)
        {
            Log(msg, LogEntryType.Info);
        }

        protected override void OnStart(string[] args)
        {
            StartService();
        }

        protected override void OnStop()
        {
            Log("Stop service request from OnStop.");
            StopService();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            LogHelper.Instance.Log("OnPowerEvent powerStatus = {0} State = {1}.", powerStatus.ToString(),
                State.ToString());
            var service1 = ServiceLocator.Instance.GetService<IExecutionService>();
            var service2 = ServiceLocator.Instance.GetService<IControllerService>();
            var service3 = ServiceLocator.Instance.GetService<IScannerDeviceService>();
            lock (PowerStateLock)
            {
                if (PowerBroadcastStatus.Suspend == powerStatus)
                {
                    if (State == PowerState.Normal)
                    {
                        State = PowerState.Suspended;
                        service1.Suspend();
                        service2.Shutdown();
                        service3.Shutdown();
                    }
                }
                else if (PowerBroadcastStatus.ResumeSuspend == powerStatus)
                {
                    if (PowerState.Suspended == State)
                    {
                        service1.Restart();
                        State = PowerState.Normal;
                    }
                }
            }

            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnShutdown()
        {
            Log("Stop service request from OnShutdown.");
            StopService();
            base.OnShutdown();
        }

        internal void StopService()
        {
            ServiceLocator.Instance.GetService<IExecutionService>().Shutdown();
            ServiceLocator.Instance.GetService<IControllerService>().Shutdown();
            ServiceLocator.Instance.GetService<IScannerDeviceService>().Shutdown();
            Log("Stop Hardware Abstraction Layer (HAL) Service.", LogEntryType.Info);
            WriteEventLogEntry("Stop Hardware Abstraction Layer (HAL) Service.", EventLogEntryType.Information);
            Proxy.Dispose();
        }

        internal void StartService()
        {
            ListenerThread = new Thread(ExecuteListener);
            ListenerThread.TrySetApartmentState(ApartmentState.MTA);
            ListenerThread.Start();
        }

        internal void ExecuteListener()
        {
            var runtimeService = (IRuntimeService)new RuntimeService();
            var message = string.Format("{0} version {1} (.NET version {2}; OS = {3})",
                "Start Hardware Abstraction Layer (HAL) Service", typeof(HALService).Assembly.GetName().Version,
                Environment.Version.ToString(), runtimeService.GetPlatform().ToString());
            var errors = new ErrorList();
            var initProperties = new Dictionary<string, object>();
            try
            {
                Log(message, LogEntryType.Info);
                WriteEventLogEntry("Start Hardware Abstraction Layer (HAL) Service", EventLogEntryType.Information);
                ServiceLocator.Instance.AddService<ITableTypeFactory>(new TableTypeFactory());
                ServiceLocator.Instance.AddService<IFormattedLogFactoryService>(new FormattedLogFactory());
                ServiceLocator.Instance.AddService<ILogger>(this);
                ServiceLocator.Instance.AddService<IEncryptionService>(new TripleDesEncryptionService());
                ServiceLocator.Instance.AddService<IRuntimeService>(runtimeService);
                ServiceLocator.Instance.AddService<IPortManagerService>(new PortManager());
                ServiceLocator.Instance.AddService<IZipFileService>(new ZipFileService(runtimeService));
                ServiceLocator.Instance.AddService<IDeviceSetupClassFactory>(new DeviceSetupClassFactory());
                ServiceLocator.Instance.AddService<IUsbDeviceService>(
                    new UsbDeviceService(Settings.Default.UsbServiceDebug));
                ServiceLocator.Instance.AddService<IDataTableService>(
                    new DataTableService(Settings.Default.CountersDBExclusiveMode));
                ServiceLocator.Instance.AddService<IPersistentMapService>(new PersistentMapService());
                var instance1 = ConfigurationService.Make(runtimeService.RuntimePath("HAL.xml"));
                ServiceLocator.Instance.AddService<IConfigurationService>(instance1);
                KioskConfiguration.Instance.Initialize(errors);
                var instance2 = new ControllerService();
                ServiceLocator.Instance.AddService<IControllerService>(instance2);
                instance2.Initialize(errors, initProperties);
                LogHelper.Instance.Log("Loading Barcode Configuration Instance.");
                BarcodeConfiguration.NewInstance();
                LogHelper.Instance.Log("Loading BarcodeReaderFactory.");
                var instance3 = new BarcodeReaderFactory();
                ServiceLocator.Instance.AddService<IBarcodeReaderFactory>(instance3);
                LogHelper.Instance.Log("Initializing barcode reader factory.");
                instance3.Initialize(errors);
                var scriptsPath = runtimeService.InstallPath("Scripts");
                LogHelper.Instance.Log("Loading ExecutionEngine.");
                var instance4 = ExecutionEngine.NewEngine(scriptsPath);
                ServiceLocator.Instance.AddService<IExecutionService>(instance4);
                instance4.Initialize(errors);
                instance1.Load(errors);
                if (errors.ContainsError())
                {
                    errors.ForEach(initError => Log(string.Format("Error = {0}", initError), LogEntryType.Error));
                    Log("The HAL service encountered startup problems, and is stopping.", LogEntryType.Error);
                    Log("Failed to load configuration.", LogEntryType.Error);
                    WriteEventLogEntry("HAL Service encountered startup problems, and is stopping.",
                        EventLogEntryType.Error);
                    Stop();
                }
                else
                {
                    IPCProxy.InitializeServices(Enum<IpcHostVersion>.Parse(Settings.Default.ServiceHostGeneration,
                        IpcHostVersion.Legacy));
                    Proxy = IPCProxy.Parse(Settings.Default.ServiceProtocol);
                    if (!Proxy.ProtocolValid)
                    {
                        var str = "ProtocolURI is malformed; please fix. HAL service is exiting.";
                        Log(str, LogEntryType.Fatal);
                        WriteEventLogEntry(str, EventLogEntryType.Error);
                    }
                    else
                    {
                        instance4.Start();
                        Proxy.StartListener();
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Top level exception caught", ex);
            }
        }

        private void LogServiceError(string msg)
        {
            try
            {
                m_errorLog.Error(msg);
            }
            catch (Exception ex)
            {
                Log("Unable to log error message to diagnostic log.", ex);
            }
        }

        private bool WriteEventLogEntry(string msg, EventLogEntryType type)
        {
            try
            {
                EventLog.WriteEntry(msg, type);
                return true;
            }
            catch (Exception ex)
            {
                Log("Unable to write to the event log.", ex);
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            CanHandlePowerEvent = true;
            CanShutdown = true;
            ServiceName = "HalSvc$Default";
        }

        private enum PowerState
        {
            Normal,
            Suspended
        }
    }
}