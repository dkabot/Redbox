using System;

namespace Redbox.Core
{
    [Flags]
    internal enum ShutdownFlags : uint
    {
        LogOff = 0,
        ShutDown = 1,
        Reboot = 2,
        Force = 4,
        PowerOff = 8,
        ForceIfHung = 16, // 0x00000010
    }
}
