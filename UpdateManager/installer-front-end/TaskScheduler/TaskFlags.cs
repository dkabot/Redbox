using System;

namespace TaskScheduler
{
    [Flags]
    internal enum TaskFlags
    {
        Interactive = 1,
        DeleteWhenDone = 2,
        Disabled = 4,
        StartOnlyIfIdle = 16, // 0x00000010
        KillOnIdleEnd = 32, // 0x00000020
        DontStartIfOnBatteries = 64, // 0x00000040
        KillIfGoingOnBatteries = 128, // 0x00000080
        RunOnlyIfDocked = 256, // 0x00000100
        Hidden = 512, // 0x00000200
        RunIfConnectedToInternet = 1024, // 0x00000400
        RestartOnIdleResume = 2048, // 0x00000800
        SystemRequired = 4096, // 0x00001000
        RunOnlyIfLoggedOn = 8192, // 0x00002000
    }
}
