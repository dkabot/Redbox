using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using Redbox.Core;

namespace Redbox.Lua
{
    public class LuaFunction : LuaObject
    {
        private static readonly ConcurrentDictionary<Guid, TrackLuaCall> _trackLuaCall =
            new ConcurrentDictionary<Guid, TrackLuaCall>();

        public LuaFunction(LuaVirtualMachine virtualMachine, int reference) : base(virtualMachine, reference)
        {
        }

        public override string ToString()
        {
            return "(function)";
        }

        public ReadOnlyCollection<object> Call(params object[] args)
        {
            var trackLuaCall = (TrackLuaCall)null;
            var key = Guid.NewGuid();
            try
            {
                trackLuaCall = new TrackLuaCall
                {
                    EnteredCall = DateTime.Now,
                    Stack = Environment.StackTrace
                };
                _trackLuaCall[key] = trackLuaCall;
                lock (_lock)
                {
                    var count = VirtualMachineInstance.Stack.Count;
                    if (!VirtualMachineInstance.Stack.Check(args.Length + 6))
                        throw new LuaException("Lua stack overflow");
                    Push();
                    foreach (var obj in args)
                        VirtualMachineInstance.Stack.Push(obj);
                    VirtualMachineInstance.IsExecuting = true;
                    if (VirtualMachineInstance.Stack.ProtectedCall(args.Length, -1, 0) == 0)
                        return VirtualMachineInstance.Stack.PopTo(count);
                    VirtualMachineInstance.ThrowExceptionFromError(count);
                    Monitor.Pulse(this);
                }
            }
            finally
            {
                VirtualMachineInstance.IsExecuting = false;
                if (trackLuaCall != null)
                    _trackLuaCall.TryRemove(key, out _);
            }

            return null;
        }

        public static void LogLuaFunctions()
        {
            LogHelper.Instance.Log(string.Format("Logging the stack for {0} LuaFunction objects", _trackLuaCall.Count));
                _trackLuaCall.Values.ForEach(x =>
            {
                x.Elapsed = (long)(DateTime.Now - x.EnteredCall).TotalMilliseconds;
                LogHelper.Instance.Log(x.ToJson());
            });
        }

        public class TrackLuaCall
        {
            public DateTime EnteredCall { get; set; }

            public long Elapsed { get; set; }

            public string Stack { get; set; }
        }
    }
}