using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;
using System;

namespace HALUtilities
{
  internal sealed class PipeMessageServer : AbstractPipeSever
  {
    private readonly IIPCMessageFactory Factory = (IIPCMessageFactory) new IPCMessageFactory();

    protected override void OnStop()
    {
      IIPCMessage msg = this.Factory.Create(MessageType.Quit, MessageSeverity.High, string.Empty);
      ClientPipeChannel channel = new ClientPipeChannel(this.PipeName, "MessageServerQuit");
      if (!channel.Connect())
        return;
      this.Factory.Write(msg, (IIPCChannel) channel);
      IIPCMessage ipcMessage = this.Factory.Read((IIPCChannel) channel);
      if (ipcMessage.Type == MessageType.Ack)
        return;
      Console.WriteLine("Client: didn't get an ACK (received = {0})", (object) ipcMessage.Type.ToString());
    }

    protected override void Process(BasePipeChannel channel)
    {
      IIPCMessage ipcMessage = this.Factory.Read((IIPCChannel) channel);
      LogHelper.Instance.Log("[PipeMessageServer] {0} Message received.", (object) DateTime.Now);
      LogHelper.Instance.Log(ipcMessage.ToString());
      this.Factory.Write(this.Factory.CreateAck(ipcMessage.UID), (IIPCChannel) channel);
      channel.Dispose();
    }

    internal PipeMessageServer(string name)
      : base(name)
    {
    }
  }
}
