using System;

namespace Redbox.Lua
{
    internal class DebugHookEventArgs : EventArgs
    {
        public DebugHookEventArgs(LuaDebugInfo luaDebugInfo) => this.DebugInfo = luaDebugInfo;

        public LuaDebugInfo DebugInfo { get; private set; }
    }
}
