using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "hardware-survey-job")]
    internal sealed class HardwareSurveyJob : NativeJobAdapter
    {
        private readonly Dictionary<string, string> Monitors;

        internal HardwareSurveyJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
            Monitors = new Dictionary<string, string>();
            Monitors["PMT025D".ToLower()] = "Promate 17 inch";
            Monitors["KST0011".ToLower()] = "Kristel 17 inch";
            Monitors["KST0015".ToLower()] = "Kristel 15 inch";
            Monitors["MST1500".ToLower()] = "Promate 15 inch";
        }

        protected override void ExecuteInner()
        {
            Context.CreateInfoResult("KioskId", ServiceLocator.Instance.GetService<IRuntimeService>().KioskId);
            var service1 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
            Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
            Context.CreateInfoResult("QuickReturnStatus", DeviceStatus.None.ToString());
            var service2 = ServiceLocator.Instance.GetService<IAirExchangerService>();
            var deviceStatus1 = DeviceStatus.None;
            if (service2.Configured)
                deviceStatus1 |= DeviceStatus.Configured;
            Context.CreateInfoResult("AirExchanger", deviceStatus1.ToString());
            Context.CreateInfoResult("FraudSensorEnabled", DeviceStatus.None.ToString());
            var computerinfo = new Computerinfo();
            Context.CreateInfoResult("PCManufacturer", computerinfo.Manufacturer);
            Context.CreateInfoResult("PCModel", computerinfo.Model);
            Context.CreateInfoResult("InstalledMemory", computerinfo.InstalledMemory.ToString());
            Context.CreateInfoResult("FreeDiskSpace", computerinfo.DiskFreeSpace.ToString());
            var activeCamera = service1.FindActiveCamera(false);
            Context.CreateInfoResult("CameraInfo", activeCamera == null ? "NONE" : activeCamera.Friendlyname);
            var deviceStatus2 = DeviceStatus.None;
            if (new AbeDeviceDescriptor(service1).Locate())
                deviceStatus2 = DeviceStatus.Found;
            Context.CreateInfoResult("AbeDeviceStatus", deviceStatus2.ToString());
            var touchScreen = service1.FindTouchScreen();
            Context.CreateInfoResult("TouchscreenDevice", touchScreen == null ? "UNKNOWN" : touchScreen.Friendlyname);
            Context.CreateInfoResult("TouchscreenFirmware",
                touchScreen == null ? "UNKNOWN" : touchScreen.ReadFirmware());
            Context.CreateInfoResult("ControllerFirmware",
                ServiceLocator.Instance.GetService<IControlSystem>().GetBoardVersion(ControlBoards.Serial).Version);
            var devices1 = service1.FindDevices(DeviceClass.Monitor);
            var message1 = "UNKNOWN";
            foreach (var deviceDescriptor in devices1)
            {
                var lower = deviceDescriptor.Product.ToLower();
                if (Monitors.ContainsKey(lower))
                {
                    message1 = Monitors[lower];
                    break;
                }
            }

            Context.CreateInfoResult("MonitorInfo", message1);
            var devices2 = service1.FindDevices(DeviceClass.Battery);
            var message2 = "NONE";
            foreach (var deviceDescriptor in devices2)
            {
                if (deviceDescriptor.Vendor.Equals("051d", StringComparison.CurrentCultureIgnoreCase))
                {
                    message2 = "APC";
                    break;
                }

                if (deviceDescriptor.Vendor.Equals("09ae", StringComparison.CurrentCultureIgnoreCase))
                {
                    message2 = "Tripplite";
                    break;
                }
            }

            Context.CreateInfoResult("UpsInfo", message2);
        }
    }
}