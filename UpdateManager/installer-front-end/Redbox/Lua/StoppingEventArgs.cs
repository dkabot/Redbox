using System;

namespace Redbox.Lua
{
    internal class StoppingEventArgs : EventArgs
    {
        public StoppingEventArgs(
          string fileName,
          int line,
          DebuggerActions action,
          LuaDebugBreakpoint breakpoint)
        {
            this.Line = line;
            this.Action = action;
            this.FileName = fileName;
            this.Breakpoint = breakpoint;
        }

        public int Line { get; private set; }

        public string FileName { get; private set; }

        public DebuggerActions Action { get; private set; }

        public LuaDebugBreakpoint Breakpoint { get; private set; }
    }
}
