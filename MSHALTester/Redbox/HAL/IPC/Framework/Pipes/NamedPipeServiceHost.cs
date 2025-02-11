using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Server;

namespace Redbox.HAL.IPC.Framework.Pipes;

internal sealed class NamedPipeServiceHost : AbstractIPCServiceHost
{
    private int m_identifier;

    internal NamedPipeServiceHost(IIpcProtocol protocol, IHostInfo info)
        : base(protocol, info)
    {
    }

    protected override void OnStart()
    {
        LogHelper.Instance.Log("[NamedPipeServiceHost] Accepting connections on pipe {0}", Protocol.GetPipeName());
        var namedPipeServer = NamedPipeServer.Create(Protocol.GetPipeName());
        try
        {
            while (Alive)
            {
                var channel = namedPipeServer.WaitForClientConnect();
                if (channel != null)
                {
                    var id = Interlocked.Increment(ref m_identifier).ToString();
                    var session = new NamedPipeServerSession(channel, this, id);
                    Register(session);
                    ThreadPool.QueueUserWorkItem(o => session.Start());
                }
            }

            LogHelper.Instance.Log("[NamedPipeServiceHost] Exiting.");
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("[NamedPipeServiceHost] Caught an exception", ex);
        }
    }

    protected override void OnStop()
    {
        LogHelper.Instance.Log("[NamedPipeServiceHost] Stop on pipe {0}", Protocol.GetPipeName());
        using (var pipeClientSession = NamedPipeClientSession.MakeSession(Protocol, "Shutdown"))
        {
            pipeClientSession.ConnectThrowOnError();
        }
    }
}