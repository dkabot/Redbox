using System;

namespace Redbox.Lua
{
    public class LuaObject : IDisposable
    {
        protected static object _lock = new object();
        private bool m_disposed;

        protected LuaObject(LuaVirtualMachine virtualMachine, int? reference)
        {
            VirtualMachineInstance = virtualMachine;
            Reference = reference;
        }

        protected int? Reference { get; }

        protected LuaVirtualMachine VirtualMachineInstance { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LuaObject()
        {
            Dispose(false);
        }

        public virtual void Push()
        {
            if (VirtualMachineInstance == null)
                return;
            var reference1 = Reference;
            if (!reference1.HasValue)
                return;
            var stack = VirtualMachineInstance.Stack;
            reference1 = Reference;
            var reference2 = reference1.Value;
            stack.GetReference(reference2);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (!(obj is LuaObject luaObject) || !luaObject.Reference.HasValue || !Reference.HasValue)
                return false;
            return VirtualMachineInstance.CompareReference(luaObject.Reference.Value, Reference.Value) ||
                   base.Equals(obj);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;
            var reference1 = Reference;
            if (reference1.HasValue && VirtualMachineInstance != null)
            {
                var virtualMachineInstance = VirtualMachineInstance;
                reference1 = Reference;
                var reference2 = reference1.Value;
                virtualMachineInstance.RemoveReference(reference2);
            }

            m_disposed = true;
        }
    }
}