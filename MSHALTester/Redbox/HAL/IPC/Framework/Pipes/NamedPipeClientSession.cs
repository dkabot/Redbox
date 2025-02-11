using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

public sealed class NamedPipeClientSession : Client.ClientSession
{
    private readonly string ID;
    private readonly BasePipeChannel m_channel;

    private NamedPipeClientSession(string id, IIpcProtocol protocol)
        : base(protocol, id)
    {
        ID = id;
        m_channel = new ClientPipeChannel(protocol.GetPipeName(), id);
    }

    protected override IIPCChannel Channel => m_channel;

    public static NamedPipeClientSession MakeSession(IIpcProtocol protocol, string identifier)
    {
        return new NamedPipeClientSession(identifier, protocol);
    }

    protected override bool OnConnect()
    {
        return m_channel.Connect();
    }
}