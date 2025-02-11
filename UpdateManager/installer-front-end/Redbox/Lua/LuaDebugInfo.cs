using System.Runtime.InteropServices;

namespace Redbox.Lua
{
    internal class LuaDebugInfo
    {
        public string Name { get; internal set; }

        public string What { get; internal set; }

        public string Source { get; internal set; }

        public string NameWhat { get; internal set; }

        public int CurrentLine { get; internal set; }

        public int LineDefined { get; internal set; }

        public string ShortSource { get; internal set; }

        public int ActiveFunction { get; internal set; }

        public LuaDebug LuaDebug { get; internal set; }

        public int NumberUpValues { get; internal set; }

        public int LastLineDefined { get; internal set; }

        public LuaHookEventCodes EventCode { get; internal set; }

        internal LuaDebugInfo(LuaDebug luaDebug)
        {
            this.LuaDebug = luaDebug;
            this.EventCode = luaDebug.eventCode;
            this.NumberUpValues = luaDebug.nups;
            this.LastLineDefined = luaDebug.lastlinedefined;
            this.CurrentLine = this.LuaDebug.currentline;
            this.LineDefined = this.LuaDebug.linedefined;
            this.ShortSource = this.LuaDebug.shortsrc;
            this.ActiveFunction = this.LuaDebug.i_ci;
            this.Name = Marshal.PtrToStringAnsi(this.LuaDebug.name);
            this.What = Marshal.PtrToStringAnsi(this.LuaDebug.what);
            this.Source = Marshal.PtrToStringAnsi(this.LuaDebug.source);
            this.NameWhat = Marshal.PtrToStringAnsi(this.LuaDebug.namewhat);
        }
    }
}
