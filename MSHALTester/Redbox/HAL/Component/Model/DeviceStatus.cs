using System;

namespace Redbox.HAL.Component.Model;

[Flags]
public enum DeviceStatus
{
    None = 0,
    Unknown = 1,
    Found = 2,
    Enabled = 4,
    Disabled = 8,
    DriverMatched = 16, // 0x00000010
    Configured = 32, // 0x00000020
    NotStarted = 64 // 0x00000040
}