namespace Redbox.Lua
{
    public class CallStackEntry
    {
        internal CallStackEntry(LuaDebugInfo luaDebugInfo)
        {
            DebugInfo = luaDebugInfo;
        }

        public LuaDebugInfo DebugInfo { get; }

        public string FunctionName => DebugInfo.Name;

        public string FileName => DebugInfo.Source;

        public int Line => DebugInfo.CurrentLine;
    }
}