using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Pipes;

internal sealed class NamedPipeServerSession : AbstractServerSession
{
    internal NamedPipeServerSession(BasePipeChannel channel, IIpcServiceHost host, string id)
        : base(host, id)
    {
        Channel = channel;
    }

    protected override IIPCChannel Channel { get; }

    protected override bool OnSessionEnd()
    {
        Channel.Dispose();
        return true;
    }
}