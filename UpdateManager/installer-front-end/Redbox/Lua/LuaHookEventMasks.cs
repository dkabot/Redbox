using System;

namespace Redbox.Lua
{
    [Flags]
    internal enum LuaHookEventMasks
    {
        LuaMaskCall = 1,
        LuaMaskRet = 2,
        LuaMaskLine = 4,
        LuaMaskCount = 8,
        LuaMaskAll = 2147483647, // 0x7FFFFFFF
    }
}
