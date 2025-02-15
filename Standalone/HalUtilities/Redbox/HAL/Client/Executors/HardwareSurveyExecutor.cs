using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Executors
{
    public sealed class HardwareSurveyExecutor : JobExecutor
    {
        public HardwareSurveyExecutor(HardwareService service) : base(service)
        {
        }

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

        protected override void OnJobCompleted()
        {
            Timestamp = DateTime.Now;
            foreach (var result in Results)
            {
                var code = result.Code;
                if (code != null)
                    switch (code.Length)
                    {
                        case 7:
                            switch (code[0])
                            {
                                case 'K':
                                    if (code == "KioskId") Kiosk = result.Message;
                                    continue;
                                case 'P':
                                    if (code == "PCModel") PcModel = result.Message;
                                    continue;
                                case 'U':
                                    if (code == "UpsInfo") UpsModel = result.Message;
                                    continue;
                                default:
                                    continue;
                            }
                        case 10:
                            if (code == "CameraInfo") CameraVersion = result.Message;
                            continue;
                        case 11:
                            if (code == "MonitorInfo") Monitor = result.Message;
                            continue;
                        case 12:
                            if (code == "AirExchanger")
                                AirExchanger =
                                    Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                            continue;
                        case 13:
                            switch (code[0])
                            {
                                case 'A':
                                    if (code == "AuxRelayBoard") HasAuxRelayBoard = result.Message == "Configured";
                                    continue;
                                case 'F':
                                    if (code == "FreeDiskSpace") FreeDiskSpace = long.Parse(result.Message);
                                    continue;
                                default:
                                    continue;
                            }
                        case 14:
                            if (code == "PCManufacturer") PcManufacturer = result.Message;
                            continue;
                        case 15:
                            switch (code[0])
                            {
                                case 'A':
                                    if (code == "AbeDeviceStatus")
                                        ABEDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message,
                                            DeviceStatus.Unknown);
                                    continue;
                                case 'I':
                                    if (code == "InstalledMemory") Memory = long.Parse(result.Message);
                                    continue;
                                default:
                                    continue;
                            }
                        case 17:
                            switch (code[0])
                            {
                                case 'Q':
                                    if (code == "QuickReturnStatus")
                                        QuickReturn =
                                            Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                                    continue;
                                case 'T':
                                    if (code == "TouchscreenDevice") Touchscreen = result.Message;
                                    continue;
                                default:
                                    continue;
                            }
                        case 18:
                            switch (code[0])
                            {
                                case 'C':
                                    if (code == "ControllerFirmware") SerialControllerVersion = result.Message;
                                    continue;
                                case 'F':
                                    if (code == "FraudSensorEnabled")
                                        FraudDevice =
                                            Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                                    continue;
                                default:
                                    continue;
                            }
                        case 19:
                            if (code == "TouchscreenFirmware") TouchscreenFirmware = result.Message;
                            continue;
                        default:
                            continue;
                    }
            }
        }
    }
}