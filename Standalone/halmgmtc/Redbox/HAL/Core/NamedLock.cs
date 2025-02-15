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
        private readonly string m_name;
        private readonly LockScope m_scope;
        private Mutex m_instanceMutex;

        public NamedLock(string name, LockScope scope)
        {
            m_name = name;
            m_scope = scope;
            try
            {
                bool createdNew;
                m_instanceMutex = new Mutex(false,
                    string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", m_scope, m_name), out createdNew);
                IsOwned = createdNew;
            }
            catch
            {
                IsOwned = false;
            }
        }

        public bool IsOwned { get; private set; }

        public void Dispose()
        {
            if (m_instanceMutex == null)
                return;
            m_instanceMutex.Close();
            m_instanceMutex = null;
        }
    }
}