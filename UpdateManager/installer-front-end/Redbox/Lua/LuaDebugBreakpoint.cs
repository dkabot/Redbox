namespace Redbox.Lua
{
    internal class LuaDebugBreakpoint
    {
        public LuaDebugBreakpoint(LuaDebugFile file, int line)
        {
            this.File = file;
            this.Line = line;
            this.Enabled = true;
        }

        public int Line { get; set; }

        public bool Enabled { get; set; }

        public LuaDebugFile File { get; private set; }
    }
}
