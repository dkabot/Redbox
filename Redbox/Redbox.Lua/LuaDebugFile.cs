namespace Redbox.Lua
{
    public class LuaDebugFile
    {
        public LuaDebugFile(LuaDebugger debugger, string fileName)
        {
            Debugger = debugger;
            FileName = fileName;
        }

        public string FileName { get; private set; }

        public LuaDebugger Debugger { get; }

        public LuaDebugBreakpointContainer Breakpoints { get; } = new LuaDebugBreakpointContainer();

        public LuaDebugBreakpoint AddBreakpoint(int line)
        {
            var breakpoint = GetBreakpoint(line);
            if (breakpoint != null)
            {
                breakpoint.Enabled = true;
            }
            else
            {
                breakpoint = Debugger.BreakpointFactory.CreateBreakpoint(this, line);
                Breakpoints.Add(breakpoint);
            }

            return breakpoint;
        }

        public void RemoveBreakpoint(int line)
        {
            RemoveBreakpoint(GetBreakpoint(line));
        }

        public void RemoveBreakpoint(LuaDebugBreakpoint breakpoint)
        {
            Breakpoints.Remove(breakpoint);
        }

        public LuaDebugBreakpoint GetBreakpoint(int line)
        {
            foreach (var breakpoint in Breakpoints)
                if (breakpoint.Line == line)
                    return breakpoint;
            return null;
        }

        public LuaDebugBreakpoint ToggleBreakpoint(int line)
        {
            var breakpoint = GetBreakpoint(line);
            if (breakpoint == null)
                return AddBreakpoint(line);
            RemoveBreakpoint(breakpoint);
            return null;
        }
    }
}