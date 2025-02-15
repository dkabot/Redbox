using System;

namespace Outerwall.Shell.Interop
{
    [Flags]
    internal enum VirtualKeyCodes : uint
    {
        VK_SHIFT = 16, // 0x00000010
        VK_CONTROL = 17, // 0x00000011
        VK_MENU = 18, // 0x00000012
        VK_LSHIFT = 160, // 0x000000A0
        VK_RSHIFT = 161 // 0x000000A1
    }
}