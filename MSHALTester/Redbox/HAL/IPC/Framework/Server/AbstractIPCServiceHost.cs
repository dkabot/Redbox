using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.IPC.Framework.Server;

internal abstract class AbstractIPCServiceHost : IIpcServiceHost
{
    private readonly AtomicFlag AliveFlag = new(false);
    private readonly object m_sessionLockObject = new();
    private readonly List<ISession> m_sessions = new();

    protected AbstractIPCServiceHost(IIpcProtocol protocol, IHostInfo info)
    {
        Protocol = protocol;
        HostInfo = info;
    }

    public bool LogDetailedMessages { get; set; }

    public void Start()
    {
        Alive = true;
        OnStart();
    }

    public void Stop()
    {
        Alive = false;
        OnStop();
    }

    public void Unregister(ISession session)
    {
        var num = 0;
        lock (m_sessionLockObject)
        {
            m_sessions.Remove(session);
            num = m_sessions.Count;
        }

        if (!LogDetailedMessages)
            return;
        LogHelper.Instance.Log("[IPCServiceHost] Unregister - there are {0} active server sessions.", num);
    }

    public void Register(ISession session)
    {
        var num = 0;
        lock (m_sessionLockObject)
        {
            m_sessions.Add(session);
            num = m_sessions.Count;
        }

        if (!LogDetailedMessages)
            return;
        LogHelper.Instance.Log("[IPCServiceHost] Register - there are {0} active server sessions.", num);
    }

    public bool Alive
    {
        get => AliveFlag.IsSet;
        set
        {
            if (value)
                AliveFlag.Set();
            else
                AliveFlag.Clear();
        }
    }

    public IIpcProtocol Protocol { get; }

    public IHostInfo HostInfo { get; }

    protected abstract void OnStart();

    protected abstract void OnStop();
}