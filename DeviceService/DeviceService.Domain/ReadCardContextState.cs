using System;

namespace DeviceService.Domain
{
    [Flags]
    public enum ReadCardContextState
    {
        VasContinue = 1,
        PayContinue = 2,
        Continue = PayContinue | VasContinue, // 0x00000003
        Stop = 4,
        Timeout = Stop | VasContinue, // 0x00000005
        Canceled = Stop | PayContinue, // 0x00000006
        Tampered = Canceled | VasContinue, // 0x00000007
        Blocked = 8
    }
}