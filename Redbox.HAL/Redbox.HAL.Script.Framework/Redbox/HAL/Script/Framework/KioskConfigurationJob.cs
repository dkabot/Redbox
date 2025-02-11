using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "kiosk-configuration-job", Operand = "KIOSK-CONFIGURATION-JOB")]
    internal sealed class KioskConfigurationJob : NativeJobAdapter
    {
        internal KioskConfigurationJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
            var service1 = ServiceLocator.Instance.GetService<IDoorSensorService>();
            Context.CreateInfoResult("MachineConfig", DecksService.First.NumberOfSlots == 90 ? "Dense" : "Sparse");
            Context.CreateInfoResult("DeckCount", DecksService.DeckCount.ToString());
            var deck = DecksService.Last;
            if (deck != null)
            {
                if (deck.IsQlm)
                    deck = DecksService.GetByNumber(deck.Number - 1);
                Context.CreateResult("MaxDeckInfo", "This contains the last deck and slot.", deck.Number,
                    deck.NumberOfSlots, null, new DateTime?(), null);
            }

            Context.CreateInfoResult("CameraConfig", configuredDevice.Service.ToString());
            Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
            Context.CreateInfoResult("MerchMode", ControllerConfiguration.Instance.IsVMZMachine ? "VMZ" : "QLM");
            Context.CreateInfoResult("DoorSensors", service1.SensorsEnabled ? "On" : "Off");
            if (service1.SensorsEnabled)
                Context.CreateInfoResult("DoorSensorStatus", service1.Query().ToString().ToUpper());
            Context.CreateInfoResult("QuickReturn", "Not configured");
            var service2 = ServiceLocator.Instance.GetService<IAirExchangerService>();
            Context.CreateInfoResult("AirExchangerConfigured", service2.Configured ? "Configured" : "Not configured");
            Context.CreateInfoResult("AirExchangerStatus", service2.CheckStatus().ToString());
            Context.CreateInfoResult("AirExchangerFanStatus", service2.FanStatus.ToString());
            var service3 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
            Context.CreateInfoResult("FraudSensorEnabled",
                ControllerConfiguration.Instance.EnableSecureDiskValidator ? "Enabled" : "Not Enabled");
            Context.CreateInfoResult("AbeDeviceStatus",
                new AbeDeviceDescriptor(service3, true).Locate() ? "ATTACHED" : "NOT ATTACHED");
            Context.CreateInfoResult("QuickReturnStatus", DeviceStatus.None.ToString());
            Context.CreateInfoResult("RouterPowerRelay",
                ControllerConfiguration.Instance.RouterPowerCyclePause > 0 ? "Configured" : "Not Configured");
            Context.CreateInfoResult("SupportsArcusReset",
                !ControllerConfiguration.Instance.RestartControllerDuringUserJobs ||
                !ControllerConfiguration.Instance.TrackHardwareCorrections
                    ? "Not Configured"
                    : "Configured");
            Context.CreateInfoResult("SupportsFraudScan",
                ServiceLocator.Instance.GetService<IFraudService>().IsConfigured ? "Configured" : "Not Configured");
            var service4 = ServiceLocator.Instance.GetService<IScannerDeviceService>();
            var context1 = Context;
            var irHardwareInstall = service4.IRHardwareInstall;
            string message1;
            if (!irHardwareInstall.HasValue)
            {
                message1 = "NONE";
            }
            else
            {
                irHardwareInstall = service4.IRHardwareInstall;
                message1 = irHardwareInstall.ToString();
            }

            context1.CreateInfoResult("IRCameraHardwareInstall", message1);
            Context.CreateInfoResult("CurrentCameraGeneration", service4.CurrentCameraGeneration.ToString());
            var configuredReader = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader();
            Context.CreateInfoResult("BarcodeDecoder",
                configuredReader.IsLicensed ? configuredReader.Service.ToString() : BarcodeServices.None.ToString());
            var excludedSlots = InventoryService.GetExcludedSlots();
            if (excludedSlots.Count > 0)
                excludedSlots.ForEach(x => Context.CreateResult("ExcludedSlot", "The location is excluded.", x.Deck,
                    x.Slot, null, new DateTime?(), null));
            else
                Context.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
            Context.CreateInfoResult("BufferSlot", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
            Context.CreateInfoResult("DisableKFCCheckDrivers",
                ControllerConfiguration.Instance.DisableKFCCheckDrivers.ToString());
            Context.CreateInfoResult("DisableKFCDecodeTest",
                ControllerConfiguration.Instance.DisableKFCDecodeTest.ToString());
            Context.CreateInfoResult("DisableKFCTestVendDoor",
                ControllerConfiguration.Instance.DisableKFCTestVendDoor.ToString());
            Context.CreateInfoResult("DisableKFCInit", ControllerConfiguration.Instance.DisableKFCInit.ToString());
            var context2 = Context;
            var flag = ControllerConfiguration.Instance.DisableKFCVerticalSlotTest;
            var message2 = flag.ToString();
            context2.CreateInfoResult("DisableKFCVerticalSlotTest", message2);
            var context3 = Context;
            flag = ControllerConfiguration.Instance.DisableKFCUnknownCount;
            var message3 = flag.ToString();
            context3.CreateInfoResult("DisableKFCUnknownCount", message3);
        }
    }
}