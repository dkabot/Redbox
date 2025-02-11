namespace Redbox.Lua
{
    internal class CallStackEntry
    {
        public LuaDebugInfo DebugInfo { get; private set; }

        public string FunctionName => this.DebugInfo.Name;

        public string FileName => this.DebugInfo.Source;

        public int Line => this.DebugInfo.CurrentLine;

        internal CallStackEntry(LuaDebugInfo luaDebugInfo) => this.DebugInfo = luaDebugInfo;
    }
}
