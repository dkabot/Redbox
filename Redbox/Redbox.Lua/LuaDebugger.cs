using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.Lua
{
    public class LuaDebugger : ILuaDebugFileFactory, ILuaDebugBreakpointFactory
    {
        private DebuggerActions m_action;
        private bool m_inDebugHook;
        private int m_stackLevel;
        private LuaDebuggerState m_state;
        private int m_stepStackLevel;

        public LuaDebugger(LuaVirtualMachine lua)
        {
            Lua = lua;
            FullTrace = false;
            FileFactory = this;
            BreakpointFactory = this;
        }

        public bool Enabled
        {
            get => m_state != 0;
            set => State = value ? LuaDebuggerState.Running : LuaDebuggerState.Disabled;
        }

        public bool FullTrace { get; set; }

        public LuaDebugFileContainer Files { get; } = new LuaDebugFileContainer();

        public LuaVirtualMachine Lua { get; }

        public ILuaDebugFileFactory FileFactory { get; set; }

        public ILuaDebugBreakpointFactory BreakpointFactory { get; set; }

        public LuaDebuggerState State
        {
            get => m_state;
            set
            {
                switch (value)
                {
                    case LuaDebuggerState.Disabled:
                        m_stackLevel = 0;
                        if (m_state == LuaDebuggerState.Stopped)
                            Run();
                        if (m_state != LuaDebuggerState.Disabled)
                        {
                            Lua.RemoveDebugHook();
                            Lua.DebugHook -= OnDebugHook;
                        }

                        m_state = LuaDebuggerState.Disabled;
                        break;
                    case LuaDebuggerState.Running:
                        switch (m_state)
                        {
                            case LuaDebuggerState.Disabled:
                                Lua.DebugHook += OnDebugHook;
                                Lua.SetDebugHook(LuaHookEventMasks.LuaMaskAll, 0);
                                m_state = LuaDebuggerState.Running;
                                return;
                            case LuaDebuggerState.Stopped:
                                Run();
                                return;
                            default:
                                return;
                        }
                    case LuaDebuggerState.Stopped:
                        throw new InvalidOperationException("Cant set state to Stoped");
                    default:
                        throw new ArgumentException("Unknown value for state");
                }
            }
        }

        LuaDebugBreakpoint ILuaDebugBreakpointFactory.CreateBreakpoint(LuaDebugFile file, int line)
        {
            return new LuaDebugBreakpoint(file, line);
        }

        LuaDebugFile ILuaDebugFileFactory.CreateFile(LuaDebugger debugger, string fileName)
        {
            return new LuaDebugFile(debugger, fileName);
        }

        public void Run()
        {
            if (m_state != LuaDebuggerState.Stopped)
                return;
            m_action = DebuggerActions.Run;
            m_state = LuaDebuggerState.Running;
        }

        public void Stop()
        {
            m_action = DebuggerActions.Stop;
        }

        public void StepInto()
        {
            if (m_state != LuaDebuggerState.Stopped)
                return;
            m_action = DebuggerActions.StepInto;
            m_state = LuaDebuggerState.Running;
        }

        public void StepOver()
        {
            if (m_state != LuaDebuggerState.Stopped)
                return;
            m_action = DebuggerActions.StepOver;
            m_stepStackLevel = m_stackLevel;
            m_state = LuaDebuggerState.Running;
        }

        public void StepOut()
        {
            if (m_state != LuaDebuggerState.Stopped)
                return;
            m_action = DebuggerActions.StepOut;
            m_stepStackLevel = m_stackLevel - 1;
            m_state = LuaDebuggerState.Running;
        }

        public LuaVar[] GetGlobalVars()
        {
            var luaVarList = new List<LuaVar>();
            var luaTable = (LuaTable)Lua["_G"];
            var num = 1;
            foreach (var key in luaTable.Keys)
                if (!(key.ToString() == "_G"))
                    luaVarList.Add(new LuaVar(num++, key.ToString(), luaTable[key]));
            return luaVarList.ToArray();
        }

        public CallStackEntry[] GetCallStack()
        {
            var callStackEntryList = new List<CallStackEntry>();
            LuaDebug luaDebug;
            for (var level = 0; Lua.GetStack(level, out luaDebug); ++level)
            {
                Lua.GetInfo("nSl", ref luaDebug);
                callStackEntryList.Add(new CallStackEntry(new LuaDebugInfo(luaDebug)));
            }

            return callStackEntryList.ToArray();
        }

        public LuaVar[] GetLocalVars()
        {
            LuaDebug luaDebug;
            if (!Lua.GetStack(0, out luaDebug))
                return new List<LuaVar>().ToArray();
            Lua.GetInfo("nSl", ref luaDebug);
            return GetLocalVars(luaDebug);
        }

        public LuaVar[] GetLocalVars(LuaDebug luaDebugInfo)
        {
            var luaVarList = new List<LuaVar>();
            var n = 1;
            for (var local = Lua.GetLocal(luaDebugInfo, n); local != null; local = Lua.GetLocal(luaDebugInfo, n))
            {
                var obj = Lua.Stack.Pop();
                luaVarList.Add(new LuaVar(n++, local, obj));
            }

            return luaVarList.ToArray();
        }

        public void SetLocalVar(LuaDebug luaDebugInfo, ref LuaVar var, object newValue)
        {
            if (m_state != LuaDebuggerState.Stopped)
                return;
            var.Value = newValue;
            Lua.Stack.Push(newValue);
            Lua.SetLocal(luaDebugInfo, var.Index);
        }

        public bool SetLocalVar(LuaDebug luaDebugInfo, string varName, object newValue)
        {
            if (m_state != LuaDebuggerState.Stopped)
                return false;
            var localVars = GetLocalVars(luaDebugInfo);
            for (var i = 0; i < localVars.Count(); i++)
                if (string.Compare(varName, localVars[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    SetLocalVar(luaDebugInfo, ref localVars[i], newValue);
                    return true;
                }

            return false;
        }

        public LuaVar[] GetUpValues(int functionIndex)
        {
            var luaVarList = new List<LuaVar>();
            if (m_state != LuaDebuggerState.Stopped)
                return luaVarList.ToArray();
            var num = 1;
            for (var upValue = Lua.GetUpValue(functionIndex, num);
                 upValue != null;
                 upValue = Lua.GetUpValue(functionIndex, num))
            {
                var obj = Lua.Stack.Pop();
                luaVarList.Add(new LuaVar(num, upValue, obj));
                ++num;
            }

            return luaVarList.ToArray();
        }

        public void SetUpValue(int functionIndex, ref LuaVar var, object newValue)
        {
            if (m_state != LuaDebuggerState.Stopped)
                return;
            var.Value = newValue;
            Lua.Stack.Push(newValue);
            Lua.SetUpValue(functionIndex, var.Index);
        }

        public bool SetUpValue(int functionIndex, string varName, object newValue)
        {
            if (m_state != LuaDebuggerState.Stopped)
                return false;

            var upValues = GetUpValues(functionIndex);
            for (var i = 0; i < upValues.Count(); i++)
            {
                var upValue = upValues[i];
                if (string.Compare(varName, upValue.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    SetUpValue(functionIndex, ref upValues[i], newValue);
                    return true;
                }
            }

            return false;
        }

        public LuaDebugFile AddFile(string fileName)
        {
            var file = GetFile(fileName);
            if (file == null)
            {
                file = FileFactory.CreateFile(this, fileName);
                Files.Add(file);
            }

            return file;
        }

        public LuaDebugFile GetFile(string fileName)
        {
            foreach (var file in Files)
                if (string.Compare(fileName, file.FileName, StringComparison.OrdinalIgnoreCase) == 0)
                    return file;
            return null;
        }

        public void RemoveBreakpoint(string fileName, int line)
        {
            GetFile(fileName)?.RemoveBreakpoint(line);
        }

        public void ClearAllBreakpoints()
        {
            Files.Clear();
        }

        public LuaDebugFileContainer GetFiles()
        {
            return Files;
        }

        public LuaDebugBreakpoint GetBreakpoint(string fileName, int line)
        {
            return GetFile(fileName)?.GetBreakpoint(line);
        }

        public LuaDebugBreakpoint AddBreakpoint(string fileName, int line)
        {
            var luaDebugFile = GetFile(fileName) ?? AddFile(fileName);
            return luaDebugFile.GetBreakpoint(line) ?? luaDebugFile.AddBreakpoint(line);
        }

        public LuaDebugBreakpointContainer GetBreakpoints(string fileName)
        {
            var file = GetFile(fileName);
            return file == null ? new LuaDebugBreakpointContainer() : file.Breakpoints;
        }

        public event EventHandler<StoppingEventArgs> Stopping;

        public event EventHandler<EventArgs> WaitingForAction;

        public event EventHandler<DebugHookEventArgs> FullTraceData;

        [DllImport("User32.dll")]
        private static extern bool PeekMessage(
            out NativeMessage msg,
            IntPtr hWnd,
            uint messageFilterMin,
            uint messageFilterMax,
            uint flags);

        private void OnDebugHook(object sender, DebugHookEventArgs e)
        {
            if (m_inDebugHook)
                return;
            m_inDebugHook = true;
            try
            {
                if (FullTrace)
                    OnFullTraceData(e);
                if (m_state == LuaDebuggerState.Disabled)
                    return;
                if (e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookCall)
                    ++m_stackLevel;
                else if (e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookRet ||
                         e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookTailRet)
                    --m_stackLevel;
                if (e.DebugInfo.EventCode != LuaHookEventCodes.LuaHookCall &&
                    e.DebugInfo.EventCode != LuaHookEventCodes.LuaHookLine)
                    return;
                var luaDebug = e.DebugInfo.LuaDebug;
                Lua.GetInfo("nS", ref luaDebug);
                var luaDebugInfo = new LuaDebugInfo(luaDebug);
                var line = luaDebugInfo.EventCode == LuaHookEventCodes.LuaHookCall
                    ? luaDebugInfo.LineDefined
                    : luaDebugInfo.CurrentLine;
                if (luaDebugInfo.Source.Length <= 0)
                    return;
                switch (m_action)
                {
                    case DebuggerActions.Run:
                        var breakpoint1 = GetBreakpoint(luaDebugInfo.Source, line);
                        if (breakpoint1 == null || !breakpoint1.Enabled)
                            break;
                        StopExecution(luaDebugInfo, m_action, breakpoint1);
                        break;
                    case DebuggerActions.Stop:
                    case DebuggerActions.StepInto:
                        StopExecution(luaDebugInfo, m_action, null);
                        break;
                    case DebuggerActions.StepOver:
                    case DebuggerActions.StepOut:
                        if (m_stackLevel <= m_stepStackLevel)
                        {
                            StopExecution(luaDebugInfo, m_action, null);
                            break;
                        }

                        var breakpoint2 = GetBreakpoint(luaDebugInfo.Source, line);
                        if (breakpoint2 == null || !breakpoint2.Enabled)
                            break;
                        StopExecution(luaDebugInfo, m_action, breakpoint2);
                        break;
                }
            }
            finally
            {
                m_inDebugHook = false;
            }
        }

        private void StopExecution(
            LuaDebugInfo luaDebugInfo,
            DebuggerActions action,
            LuaDebugBreakpoint breakpoint)
        {
            m_state = LuaDebuggerState.Stopped;
            try
            {
                OnStopping(new StoppingEventArgs(luaDebugInfo.Source, luaDebugInfo.CurrentLine, action, breakpoint));
                OnWaitingForAction(new EventArgs());
                do
                {
                    if (!PeekMessage(out _, IntPtr.Zero, 0U, 0U, 0U))
                        Thread.Sleep(1);
                    Application.DoEvents();
                } while (m_state == LuaDebuggerState.Stopped);
            }
            finally
            {
                m_state = LuaDebuggerState.Running;
            }
        }

        private void OnWaitingForAction(EventArgs e)
        {
            var waitingForAction = WaitingForAction;
            if (waitingForAction == null)
                return;
            waitingForAction(this, e);
        }

        private void OnStopping(StoppingEventArgs e)
        {
            var stopping = Stopping;
            if (stopping == null)
                return;
            stopping(this, e);
        }

        private void OnFullTraceData(DebugHookEventArgs e)
        {
            var fullTraceData = FullTraceData;
            if (fullTraceData == null)
                return;
            fullTraceData(this, e);
        }

        private struct NativeMessage
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }
    }
}