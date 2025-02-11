namespace Redbox.Lua
{
    internal class LuaDebugFile
    {
        private readonly LuaDebugBreakpointContainer m_breakpoints = new LuaDebugBreakpointContainer();

        public LuaDebugFile(LuaDebugger debugger, string fileName)
        {
            this.Debugger = debugger;
            this.FileName = fileName;
        }

        public LuaDebugBreakpoint AddBreakpoint(int line)
        {
            LuaDebugBreakpoint breakpoint = this.GetBreakpoint(line);
            if (breakpoint != null)
            {
                breakpoint.Enabled = true;
            }
            else
            {
                breakpoint = this.Debugger.BreakpointFactory.CreateBreakpoint(this, line);
                this.m_breakpoints.Add(breakpoint);
            }
            return breakpoint;
        }

        public void RemoveBreakpoint(int line) => this.RemoveBreakpoint(this.GetBreakpoint(line));

        public void RemoveBreakpoint(LuaDebugBreakpoint breakpoint)
        {
            this.m_breakpoints.Remove(breakpoint);
        }

        public LuaDebugBreakpoint GetBreakpoint(int line)
        {
            foreach (LuaDebugBreakpoint breakpoint in this.m_breakpoints)
            {
                if (breakpoint.Line == line)
                    return breakpoint;
            }
            return (LuaDebugBreakpoint)null;
        }

        public LuaDebugBreakpoint ToggleBreakpoint(int line)
        {
            LuaDebugBreakpoint breakpoint = this.GetBreakpoint(line);
            if (breakpoint == null)
                return this.AddBreakpoint(line);
            this.RemoveBreakpoint(breakpoint);
            return (LuaDebugBreakpoint)null;
        }

        public string FileName { get; private set; }

        public LuaDebugger Debugger { get; private set; }

        public LuaDebugBreakpointContainer Breakpoints => this.m_breakpoints;
    }
}
