using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using KioskConfiguration = Redbox.HAL.Controller.Framework.KioskConfiguration;

namespace Redbox.HAL.DataMatrix.Framework
{
    public sealed class ScannerDeviceFactory : IScannerDeviceService, IConfigurationObserver
    {
        private readonly List<IConfigurationObserver> ConfigurableScanners = new List<IConfigurationObserver>();
        private readonly CortexService CortexDevice = new CortexService();
        private readonly LegacyScanner LegacyDevice = new LegacyScanner();
        private readonly NilScanner m_nilInstance = new NilScanner();
        private IScannerDevice m_configuredDevice;
        private ScannerServices m_configuredService;

        public ScannerDeviceFactory(ScannerServices service)
        {
            ConfiguredService = service;
        }

        internal ScannerDeviceFactory()
        {
            m_configuredDevice = m_nilInstance;
            ConfigurableScanners.Add(CortexDevice);
            ConfigurableScanners.Add(LegacyDevice);
        }

        private ScannerServices ConfiguredService
        {
            get => m_configuredService;
            set
            {
                m_configuredService = value;
                switch (m_configuredService)
                {
                    case ScannerServices.Legacy:
                        m_configuredDevice = LegacyDevice;
                        break;
                    case ScannerServices.Cortex:
                        m_configuredDevice = CortexDevice;
                        break;
                    default:
                        m_configuredDevice = m_nilInstance;
                        break;
                }
            }
        }

        public void NotifyConfigurationLoaded()
        {
            LogHelper.Instance.Log("[ScannerFactory] Configuration load.");
            ConfiguredService = BarcodeConfiguration.Instance.ScannerService;
            ConfigurableScanners.ForEach(each => each.NotifyConfigurationLoaded());
        }

        public void NotifyConfigurationChangeStart()
        {
            LogHelper.Instance.Log("[ScannerFactory] Configuration change start.");
            m_configuredDevice.Stop();
            m_configuredDevice = m_nilInstance;
            ConfigurableScanners.ForEach(each => each.NotifyConfigurationChangeStart());
        }

        public void NotifyConfigurationChangeEnd()
        {
            LogHelper.Instance.Log("[ScannerFactory] Configuration change end.");
            ConfiguredService = BarcodeConfiguration.Instance.ScannerService;
            ConfigurableScanners.ForEach(each => each.NotifyConfigurationChangeEnd());
            m_configuredDevice.Start();
        }

        public IScannerDevice GetConfiguredDevice()
        {
            return m_configuredDevice;
        }

        public void Shutdown()
        {
            LogHelper.Instance.Log("[ScannerFactory] Shutdown");
            if (m_configuredDevice == null)
                return;
            m_configuredDevice.Stop();
        }

        public DateTime? IRHardwareInstall => KioskConfiguration.Instance.IRHardwareInstallDate;

        public CameraGeneration CurrentCameraGeneration => m_configuredDevice != null
            ? m_configuredDevice.CurrentCameraGeneration
            : CameraGeneration.Unknown;
    }
}