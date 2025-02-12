// Decompiled with JetBrains decompiler
// Type: Redbox.HAL.Core.NamedLock
// Assembly: halmgmtc, Version=1.10.0.14, Culture=neutral, PublicKeyToken=null
// MVID: 43EAA224-6C05-488D-991F-8F4EF81ECBA8
// Assembly location: U:\win10\Program Files\Redbox\halservice\bin\.staged_\halmgmtc.exe

using System;
using System.Globalization;
using System.Threading;

namespace Redbox.HAL.Core
{
    public sealed class NamedLock : IDisposable
    {
        private Mutex m_instanceMutex;
        private readonly string m_name;
        private readonly LockScope m_scope;

        public NamedLock(string name, LockScope scope)
        {
            this.m_name = name;
            this.m_scope = scope;
            try
            {
                bool createdNew;
                this.m_instanceMutex = new Mutex(false, string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}\\{1}", (object)this.m_scope, (object)this.m_name), out createdNew);
                this.IsOwned = createdNew;
            }
            catch
            {
                this.IsOwned = false;
            }
        }

        public bool IsOwned { get; private set; }

        public void Dispose()
        {
            if (this.m_instanceMutex == null)
                return;
            this.m_instanceMutex.Close();
            this.m_instanceMutex = (Mutex)null;
        }
    }
}