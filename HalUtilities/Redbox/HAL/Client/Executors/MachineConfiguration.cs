using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors
{
  public sealed class MachineConfiguration : JobExecutor
  {
    public bool IsDenseMachine { get; private set; }

    public ScannerServices ConfiguredCamera { get; private set; }

    public CameraGeneration CurrentCameraGeneration { get; private set; }

    public BarcodeServices BarcodeDecoder { get; private set; }

    public bool LegacyCamera { get; private set; }

    public bool VMZConfigured { get; private set; }

    public bool DoorSensorsConfigured { get; private set; }

    public bool HasQuickReturn
    {
      get => throw new NotImplementedException("HasQuickReturn property deprecated.");
    }

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

    public bool HasIRHardware => this.IRHardwareInstall.HasValue;

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

    protected override void OnJobCompleted()
    {
      this.ExcludedSlots = (IList<Location>) new List<Location>();
      this.Results.ForEach((Action<ProgramResult>) (result =>
      {
        string code = result.Code;
        if (code == null)
          return;
        switch (code.Length)
        {
          case 9:
            if (!(code == "MerchMode"))
              break;
            this.VMZConfigured = result.Message == "VMZ";
            break;
          case 10:
            if (!(code == "BufferSlot"))
              break;
            int result1 = 1;
            this.BufferSlot = int.TryParse(result.Message, out result1) ? result1 : 1;
            break;
          case 11:
            switch (code[0])
            {
              case 'D':
                if (!(code == "DoorSensors"))
                  return;
                this.DoorSensorsConfigured = result.Message == "On";
                return;
              case 'M':
                if (!(code == "MaxDeckInfo"))
                  return;
                this.MaxDeck = result.Deck;
                this.MaxSlot = result.Slot;
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
                this.LegacyCamera = result.Message == "Legacy";
                this.ConfiguredCamera = Enum<ScannerServices>.ParseIgnoringCase(result.Message, ScannerServices.Legacy);
                return;
              case 'E':
                if (!(code == "ExcludedSlot"))
                  return;
                this.ExcludedSlots.Add(new Location()
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
                this.HasAuxRelayBoard = result.Message == "Configured";
                return;
              case 'M':
                if (!(code == "MachineConfig"))
                  return;
                this.IsDenseMachine = result.Message == "Dense";
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
                this.BarcodeDecoder = Enum<BarcodeServices>.ParseIgnoringCase(result.Message, BarcodeServices.None);
                return;
              case 'D':
                if (!(code == "DisableKFCInit"))
                  return;
                bool result2 = false;
                this.KFCDisableInit = bool.TryParse(result.Message, out result2) && result2;
                return;
              default:
                return;
            }
          case 15:
            if (!(code == "AbeDeviceStatus"))
              break;
            this.HasABEDevice = result.Message.Equals("ATTACHED", StringComparison.CurrentCultureIgnoreCase);
            break;
          case 16:
            switch (code[0])
            {
              case 'D':
                if (!(code == "DoorSensorStatus"))
                  return;
                this.DoorSensorStatus = result.Message;
                return;
              case 'R':
                if (!(code == "RouterPowerRelay"))
                  return;
                this.HasRouterPowerRelay = result.Message == "Configured";
                return;
              default:
                return;
            }
          case 17:
            if (!(code == "SupportsFraudScan"))
              break;
            this.SupportsFraudScan = result.Message == "Configured";
            break;
          case 18:
            switch (code[0])
            {
              case 'A':
                if (!(code == "AirExchangerStatus"))
                  return;
                this.AirExchangerStatus = Enum<AirExchangerStatus>.ParseIgnoringCase(result.Message, AirExchangerStatus.NotConfigured);
                return;
              case 'F':
                if (!(code == "FraudSensorEnabled"))
                  return;
                this.HasFraudDevice = result.Message == "Enabled";
                return;
              case 'S':
                if (!(code == "SupportsArcusReset"))
                  return;
                this.ArcusResetConfigured = result.Message == "Configured";
                return;
              default:
                return;
            }
          case 20:
            if (!(code == "DisableKFCDecodeTest"))
              break;
            bool result3 = false;
            this.KFCDisableDecodeTest = bool.TryParse(result.Message, out result3) && result3;
            break;
          case 21:
            if (!(code == "AirExchangerFanStatus"))
              break;
            this.AirExchangerFanStatus = Enum<ExchangerFanStatus>.ParseIgnoringCase(result.Message, ExchangerFanStatus.On);
            break;
          case 22:
            switch (code[10])
            {
              case 'C':
                if (!(code == "DisableKFCCheckDrivers"))
                  return;
                bool result4 = false;
                this.KFCDisableCheckDrivers = bool.TryParse(result.Message, out result4) && result4;
                return;
              case 'T':
                if (!(code == "DisableKFCTestVendDoor"))
                  return;
                bool result5 = false;
                this.KFCDisableVendDoorTest = bool.TryParse(result.Message, out result5) && result5;
                return;
              case 'U':
                if (!(code == "DisableKFCUnknownCount"))
                  return;
                bool result6 = false;
                this.KFCDisableUnknownCount = bool.TryParse(result.Message, out result6) && result6;
                return;
              case 'e':
                if (!(code == "AirExchangerConfigured"))
                  return;
                this.AirExchangerConfigured = result.Message == "Configured";
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
                this.CurrentCameraGeneration = Enum<CameraGeneration>.ParseIgnoringCase(result.Message, CameraGeneration.Unknown);
                return;
              case 'I':
                if (!(code == "IRCameraHardwareInstall"))
                  return;
                if (!(result.Message != "NONE"))
                  return;
                try
                {
                  this.IRHardwareInstall = new DateTime?(DateTime.Parse(result.Message));
                  return;
                }
                catch
                {
                  this.IRHardwareInstall = new DateTime?();
                  return;
                }
              default:
                return;
            }
          case 26:
            if (!(code == "DisableKFCVerticalSlotTest"))
              break;
            bool result7 = false;
            this.KFCDisableVerticalSlotTest = bool.TryParse(result.Message, out result7) && result7;
            break;
        }
      }));
    }

    protected override string JobName => "kiosk-configuration-job";

        public MachineConfiguration(HardwareService service) : base(service)
        {
        }
    }
}
