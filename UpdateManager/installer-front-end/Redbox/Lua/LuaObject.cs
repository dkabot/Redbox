using System;

namespace Redbox.Lua
{
    internal class LuaObject : IDisposable
    {
        private bool m_disposed;
        protected static object _lock = new object();

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        ~LuaObject() => this.Dispose(false);

        public virtual void Push()
        {
            if (this.VirtualMachineInstance == null)
                return;
            int? reference1 = this.Reference;
            if (!reference1.HasValue)
                return;
            LuaStack stack = this.VirtualMachineInstance.Stack;
            reference1 = this.Reference;
            int reference2 = reference1.Value;
            stack.GetReference(reference2);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (!(obj is LuaObject luaObject) || !luaObject.Reference.HasValue || !this.Reference.HasValue)
                return false;
            return this.VirtualMachineInstance.CompareReference(luaObject.Reference.Value, this.Reference.Value) || base.Equals(obj);
        }

        protected LuaObject(LuaVirtualMachine virtualMachine, int? reference)
        {
            this.VirtualMachineInstance = virtualMachine;
            this.Reference = reference;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.m_disposed)
                return;
            int? reference1 = this.Reference;
            if (reference1.HasValue && this.VirtualMachineInstance != null)
            {
                LuaVirtualMachine virtualMachineInstance = this.VirtualMachineInstance;
                reference1 = this.Reference;
                int reference2 = reference1.Value;
                virtualMachineInstance.RemoveReference(reference2);
            }
            this.m_disposed = true;
        }

        protected int? Reference { get; private set; }

        protected LuaVirtualMachine VirtualMachineInstance { get; private set; }
    }
}
