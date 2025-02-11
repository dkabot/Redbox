using System;
using System.Globalization;
using System.Threading;

namespace Redbox.Core
{
    internal class NamedLock : IDisposable
    {
        private bool m_ownsMutex;
        private Mutex m_instanceMutex;
        private readonly string m_name;
        private readonly LockScope m_scope;

        public NamedLock(string name, LockScope scope)
        {
            this.m_name = name;
            this.m_scope = scope;
            this.AcquireLock();
        }

        public bool Exists() => this.m_instanceMutex != null && this.m_ownsMutex;

        public void Dispose()
        {
            this.m_instanceMutex.Close();
            this.m_instanceMutex = (Mutex)null;
        }

        internal void AcquireLock()
        {
            this.m_instanceMutex = new Mutex(false, string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}\\{1}", (object)this.m_scope, (object)this.m_name), out this.m_ownsMutex);
        }
    }
}
