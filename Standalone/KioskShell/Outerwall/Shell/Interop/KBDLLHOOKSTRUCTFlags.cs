using System;

namespace Outerwall.Shell.Interop
{
    [Flags]
    internal enum KBDLLHOOKSTRUCTFlags : uint
    {
        LLKHF_EXTENDED = 1,
        LLKHF_INJECTED = 16, // 0x00000010
        LLKHF_ALTDOWN = 32, // 0x00000020
        LLKHF_UP = 128 // 0x00000080
    }
}