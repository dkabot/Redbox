using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Client.Executors
{
    public sealed class HardwareSurveyExecutor : JobExecutor
    {
        public string Kiosk { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string CameraVersion { get; private set; }

        public DeviceStatus QuickReturn { get; private set; }

        public DeviceStatus AirExchanger { get; private set; }

        public DeviceStatus FraudDevice { get; private set; }

        public bool HasAuxRelayBoard { get; private set; }

        public DeviceStatus ABEDevice { get; private set; }

        public long Memory { get; private set; }

        public long FreeDiskSpace { get; private set; }

        public string PcModel { get; private set; }

        public string PcManufacturer { get; private set; }

        public string Touchscreen { get; private set; }

        public string TouchscreenFirmware { get; private set; }

        public string UpsModel { get; private set; }

        public string SerialControllerVersion { get; private set; }

        public string Monitor { get; private set; }

        protected override string JobName => "hardware-survey-job";

        public HardwareSurveyExecutor(HardwareService service) : base(service)
        {
        }

        protected override void OnJobCompleted()
        {
            Timestamp = DateTime.Now;
            foreach (var result in Results)
                switch (result.Code)
                {
                    case "AbeDeviceStatus":
                        ABEDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                        continue;
                    case "AirExchanger":
                        AirExchanger = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                        continue;
                    case "AuxRelayBoard":
                        HasAuxRelayBoard = result.Message == "Configured";
                        continue;
                    case "CameraInfo":
                        CameraVersion = result.Message;
                        continue;
                    case "ControllerFirmware":
                        SerialControllerVersion = result.Message;
                        continue;
                    case "FraudSensorEnabled":
                        FraudDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                        continue;
                    case "FreeDiskSpace":
                        FreeDiskSpace = long.Parse(result.Message);
                        continue;
                    case "InstalledMemory":
                        Memory = long.Parse(result.Message);
                        continue;
                    case "KioskId":
                        Kiosk = result.Message;
                        continue;
                    case "MonitorInfo":
                        Monitor = result.Message;
                        continue;
                    case "PCManufacturer":
                        PcManufacturer = result.Message;
                        continue;
                    case "PCModel":
                        PcModel = result.Message;
                        continue;
                    case "QuickReturnStatus":
                        QuickReturn = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                        continue;
                    case "TouchscreenDevice":
                        Touchscreen = result.Message;
                        continue;
                    case "TouchscreenFirmware":
                        TouchscreenFirmware = result.Message;
                        continue;
                    case "UpsInfo":
                        UpsModel = result.Message;
                        continue;
                    default:
                        continue;
                }
        }
    }
}