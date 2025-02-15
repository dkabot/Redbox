using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Executors;

public sealed class MachineConfiguration(HardwareService service) : JobExecutor(service)
{
    public bool IsDenseMachine { get; private set; }

    public ScannerServices ConfiguredCamera { get; private set; }

    public CameraGeneration CurrentCameraGeneration { get; private set; }

    public BarcodeServices BarcodeDecoder { get; private set; }

    public bool LegacyCamera { get; private set; }

    public bool VMZConfigured { get; private set; }

    public bool DoorSensorsConfigured { get; private set; }

    public bool HasQuickReturn => throw new NotImplementedException("HasQuickReturn property deprecated.");

    public AirExchangerStatus AirExchangerStatus { get; private set; }

    public ExchangerFanStatus AirExchangerFanStatus { get; private set; }

    public bool AirExchangerConfigured { get; private set; }

    public bool HasFraudDevice { get; private set; }

    public string DoorSensorStatus { get; private set; }

    public bool HasAuxRelayBoard { get; private set; }

    public bool HasABEDevice { get; private set; }

    public DeviceStatus QuickReturnStatus { get; private set; }

    public bool HasRouterPowerRelay { get; private set; }

    public bool ArcusResetConfigured { get; private set; }

    public DateTime? IRHardwareInstall { get; private set; }

    public bool SupportsFraudScan { get; private set; }

    public bool HasIRHardware => IRHardwareInstall.HasValue;

    public IList<Location> ExcludedSlots { get; private set; }

    public int MaxDeck { get; private set; }

    public int MaxSlot { get; set; }

    public int BufferSlot { get; set; }

    public bool KFCDisableCheckDrivers { get; set; }

    public bool KFCDisableDecodeTest { get; set; }

    public bool KFCDisableVendDoorTest { get; set; }

    public bool KFCDisableInit { get; set; }

    public bool KFCDisableVerticalSlotTest { get; set; }

    public bool KFCDisableUnknownCount { get; set; }

    protected override string JobName => "kiosk-configuration-job";

    protected override void OnJobCompleted()
    {
        ExcludedSlots = new List<Location>();
        Results.ForEach(result =>
        {
            var code = result.Code;
            if (code == null)
                return;
            switch (code.Length)
            {
                case 9:
                    if (!(code == "MerchMode"))
                        break;
                    VMZConfigured = result.Message == "VMZ";
                    break;
                case 10:
                    if (!(code == "BufferSlot"))
                        break;
                    var result1 = 1;
                    BufferSlot = int.TryParse(result.Message, out result1) ? result1 : 1;
                    break;
                case 11:
                    switch (code[0])
                    {
                        case 'D':
                            if (!(code == "DoorSensors"))
                                return;
                            DoorSensorsConfigured = result.Message == "On";
                            return;
                        case 'M':
                            if (!(code == "MaxDeckInfo"))
                                return;
                            MaxDeck = result.Deck;
                            MaxSlot = result.Slot;
                            return;
                        default:
                            return;
                    }
                case 12:
                    switch (code[0])
                    {
                        case 'C':
                            if (!(code == "CameraConfig"))
                                return;
                            LegacyCamera = result.Message == "Legacy";
                            ConfiguredCamera =
                                Enum<ScannerServices>.ParseIgnoringCase(result.Message, ScannerServices.Legacy);
                            return;
                        case 'E':
                            if (!(code == "ExcludedSlot"))
                                return;
                            ExcludedSlots.Add(new Location
                            {
                                Deck = result.Deck,
                                Slot = result.Slot
                            });
                            return;
                        default:
                            return;
                    }
                case 13:
                    switch (code[0])
                    {
                        case 'A':
                            if (!(code == "AuxRelayBoard"))
                                return;
                            HasAuxRelayBoard = result.Message == "Configured";
                            return;
                        case 'M':
                            if (!(code == "MachineConfig"))
                                return;
                            IsDenseMachine = result.Message == "Dense";
                            return;
                        default:
                            return;
                    }
                case 14:
                    switch (code[0])
                    {
                        case 'B':
                            if (!(code == "BarcodeDecoder"))
                                return;
                            BarcodeDecoder =
                                Enum<BarcodeServices>.ParseIgnoringCase(result.Message, BarcodeServices.None);
                            return;
                        case 'D':
                            if (!(code == "DisableKFCInit"))
                                return;
                            var result2 = false;
                            KFCDisableInit = bool.TryParse(result.Message, out result2) && result2;
                            return;
                        default:
                            return;
                    }
                case 15:
                    if (!(code == "AbeDeviceStatus"))
                        break;
                    HasABEDevice = result.Message.Equals("ATTACHED", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case 16:
                    switch (code[0])
                    {
                        case 'D':
                            if (!(code == "DoorSensorStatus"))
                                return;
                            DoorSensorStatus = result.Message;
                            return;
                        case 'R':
                            if (!(code == "RouterPowerRelay"))
                                return;
                            HasRouterPowerRelay = result.Message == "Configured";
                            return;
                        default:
                            return;
                    }
                case 17:
                    if (!(code == "SupportsFraudScan"))
                        break;
                    SupportsFraudScan = result.Message == "Configured";
                    break;
                case 18:
                    switch (code[0])
                    {
                        case 'A':
                            if (!(code == "AirExchangerStatus"))
                                return;
                            AirExchangerStatus =
                                Enum<AirExchangerStatus>.ParseIgnoringCase(result.Message,
                                    AirExchangerStatus.NotConfigured);
                            return;
                        case 'F':
                            if (!(code == "FraudSensorEnabled"))
                                return;
                            HasFraudDevice = result.Message == "Enabled";
                            return;
                        case 'S':
                            if (!(code == "SupportsArcusReset"))
                                return;
                            ArcusResetConfigured = result.Message == "Configured";
                            return;
                        default:
                            return;
                    }
                case 20:
                    if (!(code == "DisableKFCDecodeTest"))
                        break;
                    var result3 = false;
                    KFCDisableDecodeTest = bool.TryParse(result.Message, out result3) && result3;
                    break;
                case 21:
                    if (!(code == "AirExchangerFanStatus"))
                        break;
                    AirExchangerFanStatus =
                        Enum<ExchangerFanStatus>.ParseIgnoringCase(result.Message, ExchangerFanStatus.On);
                    break;
                case 22:
                    switch (code[10])
                    {
                        case 'C':
                            if (!(code == "DisableKFCCheckDrivers"))
                                return;
                            var result4 = false;
                            KFCDisableCheckDrivers = bool.TryParse(result.Message, out result4) && result4;
                            return;
                        case 'T':
                            if (!(code == "DisableKFCTestVendDoor"))
                                return;
                            var result5 = false;
                            KFCDisableVendDoorTest = bool.TryParse(result.Message, out result5) && result5;
                            return;
                        case 'U':
                            if (!(code == "DisableKFCUnknownCount"))
                                return;
                            var result6 = false;
                            KFCDisableUnknownCount = bool.TryParse(result.Message, out result6) && result6;
                            return;
                        case 'e':
                            if (!(code == "AirExchangerConfigured"))
                                return;
                            AirExchangerConfigured = result.Message == "Configured";
                            return;
                        default:
                            return;
                    }
                case 23:
                    switch (code[0])
                    {
                        case 'C':
                            if (!(code == "CurrentCameraGeneration"))
                                return;
                            CurrentCameraGeneration =
                                Enum<CameraGeneration>.ParseIgnoringCase(result.Message, CameraGeneration.Unknown);
                            return;
                        case 'I':
                            if (!(code == "IRCameraHardwareInstall"))
                                return;
                            if (!(result.Message != "NONE"))
                                return;
                            try
                            {
                                IRHardwareInstall = DateTime.Parse(result.Message);
                                return;
                            }
                            catch
                            {
                                IRHardwareInstall = new DateTime?();
                                return;
                            }
                        default:
                            return;
                    }
                case 26:
                    if (!(code == "DisableKFCVerticalSlotTest"))
                        break;
                    var result7 = false;
                    KFCDisableVerticalSlotTest = bool.TryParse(result.Message, out result7) && result7;
                    break;
            }
        });
    }
}