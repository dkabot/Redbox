using System;

namespace Redbox.HAL.Component.Model.Threading
{
    public struct AtomicFlagHelper : IDisposable
    {
        private bool Disposed;
        private readonly AtomicFlag Flag;

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Flag.Clear();
        }

        public AtomicFlagHelper(AtomicFlag flag)
            : this()
        {
            Disposed = false;
            Flag = flag;
        }
    }
}