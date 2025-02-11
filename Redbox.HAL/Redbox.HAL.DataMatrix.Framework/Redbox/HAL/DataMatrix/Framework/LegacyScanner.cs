using System;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class LegacyScanner :
        ScannerAdapter,
        IScannerDevice,
        IDisposable,
        IConfigurationObserver
    {
        private CameraGeneration _currentCameraGeneration;
        private ICameraPlugin Plugin;

        public bool SupportsRestart => Plugin.SupportsReset;

        public void NotifyConfigurationLoaded()
        {
            OnLoadPlugin();
        }

        public void NotifyConfigurationChangeStart()
        {
        }

        public void NotifyConfigurationChangeEnd()
        {
            OnLoadPlugin();
        }

        public bool Start()
        {
            return Start(false);
        }

        public bool Start(bool test)
        {
            if (IsConnected)
                return true;
            try
            {
                InError = false;
                IsConnected = Plugin.Start();
                if (!test)
                    return IsConnected;
                using (var snapResult = Snap())
                {
                    return snapResult.SnapOk;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("CameraService.Start() caught an exception.", ex);
                IsConnected = false;
                return false;
            }
        }

        public ISnapResult Snap()
        {
            var snapFileName = GenerateSnapFileName();
            Plugin.Snap(snapFileName);
            var snapResult = new SnapResult(snapFileName);
            if (snapResult.SnapOk)
                return snapResult;
            InError = true;
            return snapResult;
        }

        public bool ValidateSettings()
        {
            return ValidateSettings(null);
        }

        public bool ValidateSettings(IMessageSink imessageSink_0)
        {
            return false;
        }

        public bool Stop()
        {
            if (!IsConnected)
                return false;
            IsConnected = false;
            try
            {
                return Plugin.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("LegacyCameraService.Stop() caught an exception.", ex);
                return false;
            }
        }

        public bool Restart()
        {
            if (!Plugin.SupportsReset)
                return false;
            Stop();
            ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(500);
            LogHelper.Instance.Log("[Legacy Camera Service] Cycle camera driver.");
            var service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
            var activeCamera = service.FindActiveCamera(false);
            if (activeCamera != null)
            {
                LogHelper.Instance.Log("Found camera {0}", activeCamera.Friendlyname);
                var cameraGeneration = From(activeCamera);
                if (CameraGeneration.Third == cameraGeneration || CameraGeneration.Fourth == cameraGeneration)
                    LogHelper.Instance.Log("The running driver for the camera {0}",
                        activeCamera.MatchDriver() ? "is correct" : (object)"is not correct.");
                var deviceStatus = service.FindDeviceStatus(activeCamera);
                if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None &&
                    (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
                    LogHelper.Instance.Log(" RESET camera returned {0}",
                        activeCamera.ResetDriver() ? "OK" : (object)"FAILURE");
            }

            return Start();
        }

        public IScanResult Scan()
        {
            var sr = Snap();
            if (sr.SnapOk)
                return ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader().Scan(sr);
            return new ScanResult
            {
                DeviceError = true
            };
        }

        public bool RequiresExternalLighting => true;

        public bool IsConnected { get; private set; }

        public bool InError { get; private set; }

        public bool IsLegacy => true;

        public bool SupportsSecureReads
        {
            get
            {
                if (!(ServiceLocator.Instance.GetService<IScannerDeviceService>().IRHardwareInstall.HasValue &
                      (ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader().Service ==
                       BarcodeServices.Cortex)))
                    return false;
                return CurrentCameraGeneration == CameraGeneration.Fifth ||
                       CurrentCameraGeneration == CameraGeneration.Fourth;
            }
        }

        public ScannerServices Service => ScannerServices.Legacy;

        public CameraGeneration CurrentCameraGeneration
        {
            get
            {
                if (_currentCameraGeneration == CameraGeneration.Unknown)
                {
                    LogHelper.Instance.Log("[Legacy Camera Service] locate camera generation.");
                    var activeCamera = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(false);
                    if (activeCamera != null)
                    {
                        LogHelper.Instance.Log("Found camera {0}", activeCamera.Friendlyname);
                        _currentCameraGeneration = From(activeCamera);
                    }
                }

                return _currentCameraGeneration;
            }
        }

        public bool TestConfiguration(IMessageSink sink)
        {
            return true;
        }

        protected override void DisposeInner()
        {
            Plugin.Stop();
        }

        private void OnLoadPlugin()
        {
            if (BarcodeConfiguration.Instance.ScannerService != Service || Plugin != null)
                return;
            LoadPlugin(BarcodeConfiguration.Instance.CameraPlugin);
        }

        private void LoadPlugin(string driver)
        {
            ICameraPlugin plugin;
            var pluginDescriptor = PluginLoader.Instance.LocatePlugin(driver, out plugin);
            if (pluginDescriptor == null)
            {
                LogHelper.Instance.Log(string.Format("Unable to locate plugin {0}", driver), LogEntryType.Error);
                Plugin = new NilCameraPlugin();
            }
            else
            {
                LogHelper.Instance.Log(string.Format("Successfully loaded camera plugin {0}", driver));
                Plugin = plugin;
                if (pluginDescriptor.DescriptorPath != null)
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(pluginDescriptor.DescriptorPath);
                    if (xmlDocument.DocumentElement != null)
                    {
                        var xmlNode = xmlDocument.SelectSingleNode("Plugin/Properties");
                        if (xmlNode != null)
                            foreach (XmlNode childNode in xmlNode.ChildNodes)
                                if (childNode.Name.Equals("property"))
                                {
                                    var key = childNode.Attributes["name"].Value;
                                    var str = childNode.Attributes["value"].Value;
                                    var attribute = childNode.Attributes["type"];
                                    if (attribute == null)
                                        BarcodeConfiguration.Instance.Properties[key] = str;
                                    else
                                        try
                                        {
                                            var type = Type.GetType(attribute.Value);
                                            BarcodeConfiguration.Instance.Properties[key] =
                                                Convert.ChangeType(str, type);
                                        }
                                        catch
                                        {
                                            BarcodeConfiguration.Instance.Properties[key] = str;
                                        }
                                }
                    }
                }
            }

            Plugin.InitWithProperties(BarcodeConfiguration.Instance.Properties);
        }

        private CameraGeneration From(IDeviceDescriptor descriptor)
        {
            if (descriptor == null)
                return CameraGeneration.Unknown;
            if (descriptor.Vendor.Equals("1871") &&
                (descriptor.Product.Equals("0d01", StringComparison.CurrentCultureIgnoreCase) ||
                 descriptor.Product.Equals("0f01", StringComparison.CurrentCultureIgnoreCase)))
                return CameraGeneration.Fourth;
            return descriptor.Vendor.Equals("0c45", StringComparison.CurrentCultureIgnoreCase) &&
                   descriptor.Product.Equals("627b", StringComparison.CurrentCultureIgnoreCase)
                ? CameraGeneration.Third
                : CameraGeneration.Unknown;
        }
    }
}