using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client
{
    public sealed class MachineConfiguration : JobExecutor
    {
        public MachineConfiguration(HardwareService service) : base(service)
        {
        }

        public bool IsDenseMachine { get; private set; }

        public bool LegacyCamera { get; private set; }

        public bool VMZConfigured { get; private set; }

        public bool DoorSensorsConfigured { get; private set; }

        public bool HasQuickReturn => throw new NotImplementedException("HasQuickReturn property deprecated.");

        public bool AirExchangerConfigured { get; private set; }

        public bool HasFraudDevice { get; private set; }

        public string DoorSensorStatus { get; private set; }

        public bool HasAuxRelayBoard { get; private set; }

        public bool HasABEDevice { get; private set; }

        public DeviceStatus QuickReturnStatus { get; private set; }

        protected override string JobName => "kiosk-configuration-job";

        protected override void OnJobCompleted()
        {
            foreach (var result in Results)
                switch (result.Code)
                {
                    case "AbeDeviceStatus":
                        HasABEDevice = result.Message.Equals("ATTACHED", StringComparison.CurrentCultureIgnoreCase);
                        continue;
                    case "AirExchangerConfigured":
                        AirExchangerConfigured = result.Message == "Configured";
                        continue;
                    case "AuxRelayBoard":
                        HasAuxRelayBoard = result.Message == "Configured";
                        continue;
                    case "CameraConfig":
                        LegacyCamera = result.Message == "Legacy";
                        continue;
                    case "DoorSensorStatus":
                        DoorSensorStatus = result.Message;
                        continue;
                    case "DoorSensors":
                        DoorSensorsConfigured = result.Message == "On";
                        continue;
                    case "FraudSensorEnabled":
                        HasFraudDevice = result.Message == "Enabled";
                        continue;
                    case "MachineConfig":
                        IsDenseMachine = result.Message == "Dense";
                        continue;
                    case "MerchMode":
                        VMZConfigured = result.Message == "VMZ";
                        continue;
                    case "QuickReturnStatus":
                        QuickReturnStatus = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                        continue;
                    default:
                        continue;
                }
        }
    }
}