using System.Collections.ObjectModel;
using System.Threading;

namespace Redbox.Lua
{
    internal class LuaFunction : LuaObject
    {
        public LuaFunction(LuaVirtualMachine virtualMachine, int reference) : base(virtualMachine, new int?(reference))
        {
        }

        public override string ToString() => "(function)";

        public ReadOnlyCollection<object> Call(params object[] args)
        {
            try
            {
                lock (LuaObject._lock)
                {
                    int count = this.VirtualMachineInstance.Stack.Count;
                    if (!this.VirtualMachineInstance.Stack.Check(args.Length + 6))
                        throw new LuaException("Lua stack overflow");
                    this.Push();
                    foreach (object obj in args)
                        this.VirtualMachineInstance.Stack.Push(obj);
                    this.VirtualMachineInstance.IsExecuting = true;
                    if (this.VirtualMachineInstance.Stack.ProtectedCall(args.Length, -1, 0) == 0)
                        return this.VirtualMachineInstance.Stack.PopTo(count);
                    this.VirtualMachineInstance.ThrowExceptionFromError(count);
                    Monitor.Pulse((object)this);
                }
            }
            finally
            {
                this.VirtualMachineInstance.IsExecuting = false;
            }
            return (ReadOnlyCollection<object>)null;
        }
    }
}
