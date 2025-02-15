using System;

namespace Redbox.Lua
{
    public class DebugHookEventArgs : EventArgs
    {
        public DebugHookEventArgs(LuaDebugInfo luaDebugInfo)
        {
            DebugInfo = luaDebugInfo;
        }

        public LuaDebugInfo DebugInfo { get; private set; }
    }
}