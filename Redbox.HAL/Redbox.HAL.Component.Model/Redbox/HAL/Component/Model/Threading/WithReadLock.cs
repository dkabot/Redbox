using System;
using System.Threading;

namespace Redbox.HAL.Component.Model.Threading
{
    public struct WithReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim TheLock;
        private bool Disposed;

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            TheLock.ExitReadLock();
        }

        public WithReadLock(ReaderWriterLockSlim _lock)
            : this()
        {
            Disposed = false;
            TheLock = _lock;
            TheLock.EnterReadLock();
        }
    }
}