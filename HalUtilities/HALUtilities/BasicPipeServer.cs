using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;
using System;
using System.Text;

namespace HALUtilities
{
  internal sealed class BasicPipeServer : AbstractPipeSever
  {
    private readonly byte[] AckMessage;

    protected override void OnStop()
    {
      using (ClientPipeChannel clientPipeChannel = new ClientPipeChannel(this.PipeName, "basic shutdown"))
      {
        if (!clientPipeChannel.Connect())
          return;
        clientPipeChannel.Write(Encoding.ASCII.GetBytes("QUIT"));
        clientPipeChannel.Read();
      }
    }

    protected override void Process(BasePipeChannel channel)
    {
      LogHelper.Instance.Log("[BasicPipeServer] {0} Session connected.", (object) DateTime.Now);
      channel.Write(Encoding.ASCII.GetBytes("Hello"));
      LogHelper.Instance.Log("[BasicPipeServer] Bytes received: {0}", (object) channel.Read().Length);
      channel.Write(this.AckMessage);
      channel.Dispose();
    }

    internal BasicPipeServer(string name)
      : base(name)
    {
      this.AckMessage = Encoding.ASCII.GetBytes("OK");
    }
  }
}
