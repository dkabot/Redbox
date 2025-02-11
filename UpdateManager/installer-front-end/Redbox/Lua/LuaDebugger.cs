using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Redbox.Lua
{
    internal class LuaDebugger : ILuaDebugFileFactory, ILuaDebugBreakpointFactory
    {
        private int m_stackLevel;
        private bool m_inDebugHook;
        private int m_stepStackLevel;
        private DebuggerActions m_action;
        private LuaDebuggerState m_state;
        private readonly LuaDebugFileContainer m_files = new LuaDebugFileContainer();

        public LuaDebugger(LuaVirtualMachine lua)
        {
            this.Lua = lua;
            this.FullTrace = false;
            this.FileFactory = (ILuaDebugFileFactory)this;
            this.BreakpointFactory = (ILuaDebugBreakpointFactory)this;
        }

        public void Run()
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return;
            this.m_action = DebuggerActions.Run;
            this.m_state = LuaDebuggerState.Running;
        }

        public void Stop() => this.m_action = DebuggerActions.Stop;

        public void StepInto()
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return;
            this.m_action = DebuggerActions.StepInto;
            this.m_state = LuaDebuggerState.Running;
        }

        public void StepOver()
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return;
            this.m_action = DebuggerActions.StepOver;
            this.m_stepStackLevel = this.m_stackLevel;
            this.m_state = LuaDebuggerState.Running;
        }

        public void StepOut()
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return;
            this.m_action = DebuggerActions.StepOut;
            this.m_stepStackLevel = this.m_stackLevel - 1;
            this.m_state = LuaDebuggerState.Running;
        }

        public LuaVar[] GetGlobalVars()
        {
            List<LuaVar> luaVarList = new List<LuaVar>();
            LuaTable luaTable = (LuaTable)this.Lua["_G"];
            int num = 1;
            foreach (object key in (IEnumerable)luaTable.Keys)
            {
                if (!(key.ToString() == "_G"))
                    luaVarList.Add(new LuaVar(num++, key.ToString(), luaTable[key]));
            }
            return luaVarList.ToArray();
        }

        public CallStackEntry[] GetCallStack()
        {
            List<CallStackEntry> callStackEntryList = new List<CallStackEntry>();
            LuaDebug luaDebug;
            for (int level = 0; this.Lua.GetStack(level, out luaDebug); ++level)
            {
                this.Lua.GetInfo("nSl", ref luaDebug);
                callStackEntryList.Add(new CallStackEntry(new LuaDebugInfo(luaDebug)));
            }
            return callStackEntryList.ToArray();
        }

        public LuaVar[] GetLocalVars()
        {
            LuaDebug luaDebug;
            if (!this.Lua.GetStack(0, out luaDebug))
                return new List<LuaVar>().ToArray();
            this.Lua.GetInfo("nSl", ref luaDebug);
            return this.GetLocalVars(luaDebug);
        }

        public LuaVar[] GetLocalVars(LuaDebug luaDebugInfo)
        {
            List<LuaVar> luaVarList = new List<LuaVar>();
            int n = 1;
            for (string local = this.Lua.GetLocal(luaDebugInfo, n); local != null; local = this.Lua.GetLocal(luaDebugInfo, n))
            {
                object obj = this.Lua.Stack.Pop();
                luaVarList.Add(new LuaVar(n++, local, obj));
            }
            return luaVarList.ToArray();
        }

        public void SetLocalVar(LuaDebug luaDebugInfo, ref LuaVar var, object newValue)
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return;
            var.Value = newValue;
            this.Lua.Stack.Push(newValue);
            this.Lua.SetLocal(luaDebugInfo, var.Index);
        }

        public bool SetLocalVar(LuaDebug luaDebugInfo, string varName, object newValue)
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return false;

            var localVars = this.GetLocalVars(luaDebugInfo);
            for (int i = 0; i < localVars.Count(); i++)
            {
                LuaVar localVar = localVars[i];
                if (string.Compare(varName, localVar.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.SetLocalVar(luaDebugInfo, ref localVar, newValue);
                    return true;
                }
            }
            return false;
        }

        public LuaVar[] GetUpValues(int functionIndex)
        {
            List<LuaVar> luaVarList = new List<LuaVar>();
            if (this.m_state != LuaDebuggerState.Stopped)
                return luaVarList.ToArray();
            int num = 1;
            for (string upValue = this.Lua.GetUpValue(functionIndex, num); upValue != null; upValue = this.Lua.GetUpValue(functionIndex, num))
            {
                object obj = this.Lua.Stack.Pop();
                luaVarList.Add(new LuaVar(num, upValue, obj));
                ++num;
            }
            return luaVarList.ToArray();
        }

        public void SetUpValue(int functionIndex, ref LuaVar var, object newValue)
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return;
            var.Value = newValue;
            this.Lua.Stack.Push(newValue);
            this.Lua.SetUpValue(functionIndex, var.Index);
        }

        public bool SetUpValue(int functionIndex, string varName, object newValue)
        {
            if (this.m_state != LuaDebuggerState.Stopped)
                return false;

            var upValues = this.GetUpValues(functionIndex);
            for (int i = 0; i < upValues.Count(); i++)
            {
                LuaVar upValue = upValues[i];
                if (string.Compare(varName, upValue.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.SetUpValue(functionIndex, ref upValue, newValue);
                    return true;
                }
            }
            return false;
        }

        public LuaDebugFile AddFile(string fileName)
        {
            LuaDebugFile file = this.GetFile(fileName);
            if (file == null)
            {
                file = this.FileFactory.CreateFile(this, fileName);
                this.m_files.Add(file);
            }
            return file;
        }

        public LuaDebugFile GetFile(string fileName)
        {
            foreach (LuaDebugFile file in this.m_files)
            {
                if (string.Compare(fileName, file.FileName, StringComparison.OrdinalIgnoreCase) == 0)
                    return file;
            }
            return (LuaDebugFile)null;
        }

        public void RemoveBreakpoint(string fileName, int line)
        {
            this.GetFile(fileName)?.RemoveBreakpoint(line);
        }

        public void ClearAllBreakpoints() => this.m_files.Clear();

        public LuaDebugFileContainer GetFiles() => this.m_files;

        public LuaDebugBreakpoint GetBreakpoint(string fileName, int line)
        {
            return this.GetFile(fileName)?.GetBreakpoint(line);
        }

        public LuaDebugBreakpoint AddBreakpoint(string fileName, int line)
        {
            LuaDebugFile luaDebugFile = this.GetFile(fileName) ?? this.AddFile(fileName);
            return luaDebugFile.GetBreakpoint(line) ?? luaDebugFile.AddBreakpoint(line);
        }

        public LuaDebugBreakpointContainer GetBreakpoints(string fileName)
        {
            LuaDebugFile file = this.GetFile(fileName);
            return file == null ? new LuaDebugBreakpointContainer() : file.Breakpoints;
        }

        LuaDebugFile ILuaDebugFileFactory.CreateFile(LuaDebugger debugger, string fileName)
        {
            return new LuaDebugFile(debugger, fileName);
        }

        LuaDebugBreakpoint ILuaDebugBreakpointFactory.CreateBreakpoint(LuaDebugFile file, int line)
        {
            return new LuaDebugBreakpoint(file, line);
        }

        public bool Enabled
        {
            get => this.m_state != 0;
            set => this.State = value ? LuaDebuggerState.Running : LuaDebuggerState.Disabled;
        }

        public bool FullTrace { get; set; }

        public LuaDebugFileContainer Files => this.m_files;

        public LuaVirtualMachine Lua { get; private set; }

        public ILuaDebugFileFactory FileFactory { get; set; }

        public ILuaDebugBreakpointFactory BreakpointFactory { get; set; }

        public event EventHandler<StoppingEventArgs> Stopping;

        public event EventHandler<EventArgs> WaitingForAction;

        public event EventHandler<DebugHookEventArgs> FullTraceData;

        public LuaDebuggerState State
        {
            get => this.m_state;
            set
            {
                switch (value)
                {
                    case LuaDebuggerState.Disabled:
                        this.m_stackLevel = 0;
                        if (this.m_state == LuaDebuggerState.Stopped)
                            this.Run();
                        if (this.m_state != LuaDebuggerState.Disabled)
                        {
                            this.Lua.RemoveDebugHook();
                            this.Lua.DebugHook -= new EventHandler<DebugHookEventArgs>(this.OnDebugHook);
                        }
                        this.m_state = LuaDebuggerState.Disabled;
                        break;
                    case LuaDebuggerState.Running:
                        switch (this.m_state)
                        {
                            case LuaDebuggerState.Disabled:
                                this.Lua.DebugHook += new EventHandler<DebugHookEventArgs>(this.OnDebugHook);
                                this.Lua.SetDebugHook(LuaHookEventMasks.LuaMaskAll, 0);
                                this.m_state = LuaDebuggerState.Running;
                                return;
                            case LuaDebuggerState.Stopped:
                                this.Run();
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

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(
          out LuaDebugger.NativeMessage msg,
          IntPtr hWnd,
          uint messageFilterMin,
          uint messageFilterMax,
          uint flags);

        private void OnDebugHook(object sender, DebugHookEventArgs e)
        {
            if (this.m_inDebugHook)
                return;
            this.m_inDebugHook = true;
            try
            {
                if (this.FullTrace)
                    this.OnFullTraceData(e);
                if (this.m_state == LuaDebuggerState.Disabled)
                    return;
                if (e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookCall)
                    ++this.m_stackLevel;
                else if (e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookRet || e.DebugInfo.EventCode == LuaHookEventCodes.LuaHookTailRet)
                    --this.m_stackLevel;
                if (e.DebugInfo.EventCode != LuaHookEventCodes.LuaHookCall && e.DebugInfo.EventCode != LuaHookEventCodes.LuaHookLine)
                    return;
                LuaDebug luaDebug = e.DebugInfo.LuaDebug;
                this.Lua.GetInfo("nS", ref luaDebug);
                LuaDebugInfo luaDebugInfo = new LuaDebugInfo(luaDebug);
                int line = luaDebugInfo.EventCode == LuaHookEventCodes.LuaHookCall ? luaDebugInfo.LineDefined : luaDebugInfo.CurrentLine;
                if (luaDebugInfo.Source.Length <= 0)
                    return;
                switch (this.m_action)
                {
                    case DebuggerActions.Run:
                        LuaDebugBreakpoint breakpoint1 = this.GetBreakpoint(luaDebugInfo.Source, line);
                        if (breakpoint1 == null || !breakpoint1.Enabled)
                            break;
                        this.StopExecution(luaDebugInfo, this.m_action, breakpoint1);
                        break;
                    case DebuggerActions.Stop:
                    case DebuggerActions.StepInto:
                        this.StopExecution(luaDebugInfo, this.m_action, (LuaDebugBreakpoint)null);
                        break;
                    case DebuggerActions.StepOver:
                    case DebuggerActions.StepOut:
                        if (this.m_stackLevel <= this.m_stepStackLevel)
                        {
                            this.StopExecution(luaDebugInfo, this.m_action, (LuaDebugBreakpoint)null);
                            break;
                        }
                        LuaDebugBreakpoint breakpoint2 = this.GetBreakpoint(luaDebugInfo.Source, line);
                        if (breakpoint2 == null || !breakpoint2.Enabled)
                            break;
                        this.StopExecution(luaDebugInfo, this.m_action, breakpoint2);
                        break;
                }
            }
            finally
            {
                this.m_inDebugHook = false;
            }
        }

        private void StopExecution(
          LuaDebugInfo luaDebugInfo,
          DebuggerActions action,
          LuaDebugBreakpoint breakpoint)
        {
            this.m_state = LuaDebuggerState.Stopped;
            try
            {
                this.OnStopping(new StoppingEventArgs(luaDebugInfo.Source, luaDebugInfo.CurrentLine, action, breakpoint));
                this.OnWaitingForAction(new EventArgs());
                do
                {
                    if (!LuaDebugger.PeekMessage(out LuaDebugger.NativeMessage _, IntPtr.Zero, 0U, 0U, 0U))
                        Thread.Sleep(1);
                    Application.DoEvents();
                }
                while (this.m_state == LuaDebuggerState.Stopped);
            }
            finally
            {
                this.m_state = LuaDebuggerState.Running;
            }
        }

        private void OnWaitingForAction(EventArgs e)
        {
            EventHandler<EventArgs> waitingForAction = this.WaitingForAction;
            if (waitingForAction == null)
                return;
            waitingForAction((object)this, e);
        }

        private void OnStopping(StoppingEventArgs e)
        {
            EventHandler<StoppingEventArgs> stopping = this.Stopping;
            if (stopping == null)
                return;
            stopping((object)this, e);
        }

        private void OnFullTraceData(DebugHookEventArgs e)
        {
            EventHandler<DebugHookEventArgs> fullTraceData = this.FullTraceData;
            if (fullTraceData == null)
                return;
            fullTraceData((object)this, e);
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
