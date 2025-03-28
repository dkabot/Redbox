using DeviceService.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  internal class DeviceServiceShutDownInfo : IDeviceServiceShutDownInfo
  {
    public DateTime ShutDownTime { get; set; }

    public ShutDownReason ShutDownReason { get; set; }
  }
}
