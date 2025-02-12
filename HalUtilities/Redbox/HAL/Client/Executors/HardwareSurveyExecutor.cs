using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;

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

    protected override void OnJobCompleted()
    {
      this.Timestamp = DateTime.Now;
      foreach (ProgramResult result in this.Results)
      {
        string code = result.Code;
        if (code != null)
        {
          switch (code.Length)
          {
            case 7:
              switch (code[0])
              {
                case 'K':
                  if (code == "KioskId")
                  {
                    this.Kiosk = result.Message;
                    continue;
                  }
                  continue;
                case 'P':
                  if (code == "PCModel")
                  {
                    this.PcModel = result.Message;
                    continue;
                  }
                  continue;
                case 'U':
                  if (code == "UpsInfo")
                  {
                    this.UpsModel = result.Message;
                    continue;
                  }
                  continue;
                default:
                  continue;
              }
            case 10:
              if (code == "CameraInfo")
              {
                this.CameraVersion = result.Message;
                continue;
              }
              continue;
            case 11:
              if (code == "MonitorInfo")
              {
                this.Monitor = result.Message;
                continue;
              }
              continue;
            case 12:
              if (code == "AirExchanger")
              {
                this.AirExchanger = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                continue;
              }
              continue;
            case 13:
              switch (code[0])
              {
                case 'A':
                  if (code == "AuxRelayBoard")
                  {
                    this.HasAuxRelayBoard = result.Message == "Configured";
                    continue;
                  }
                  continue;
                case 'F':
                  if (code == "FreeDiskSpace")
                  {
                    this.FreeDiskSpace = long.Parse(result.Message);
                    continue;
                  }
                  continue;
                default:
                  continue;
              }
            case 14:
              if (code == "PCManufacturer")
              {
                this.PcManufacturer = result.Message;
                continue;
              }
              continue;
            case 15:
              switch (code[0])
              {
                case 'A':
                  if (code == "AbeDeviceStatus")
                  {
                    this.ABEDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                    continue;
                  }
                  continue;
                case 'I':
                  if (code == "InstalledMemory")
                  {
                    this.Memory = long.Parse(result.Message);
                    continue;
                  }
                  continue;
                default:
                  continue;
              }
            case 17:
              switch (code[0])
              {
                case 'Q':
                  if (code == "QuickReturnStatus")
                  {
                    this.QuickReturn = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                    continue;
                  }
                  continue;
                case 'T':
                  if (code == "TouchscreenDevice")
                  {
                    this.Touchscreen = result.Message;
                    continue;
                  }
                  continue;
                default:
                  continue;
              }
            case 18:
              switch (code[0])
              {
                case 'C':
                  if (code == "ControllerFirmware")
                  {
                    this.SerialControllerVersion = result.Message;
                    continue;
                  }
                  continue;
                case 'F':
                  if (code == "FraudSensorEnabled")
                  {
                    this.FraudDevice = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
                    continue;
                  }
                  continue;
                default:
                  continue;
              }
            case 19:
              if (code == "TouchscreenFirmware")
              {
                this.TouchscreenFirmware = result.Message;
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
    }

    protected override string JobName => "hardware-survey-job";

        public HardwareSurveyExecutor(HardwareService service) : base(service)
        {
        }
    }
}
