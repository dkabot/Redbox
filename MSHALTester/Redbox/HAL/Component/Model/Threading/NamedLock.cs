using System;
using System.Globalization;
using System.Threading;

namespace Redbox.HAL.Component.Model.Threading;

public sealed class NamedLock : IDisposable
{
    private readonly Mutex m_instanceMutex;

    public NamedLock(string name)
    {
        try
        {
            bool createdNew;
            m_instanceMutex = new Mutex(false, string.Format(CultureInfo.InvariantCulture, "Local\\{0}", name),
                out createdNew);
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
    }
}