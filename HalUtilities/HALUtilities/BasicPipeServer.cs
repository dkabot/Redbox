using System;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;

namespace HALUtilities
{
    internal sealed class BasicPipeServer : AbstractPipeSever
    {
        private readonly byte[] AckMessage;

        internal BasicPipeServer(string name)
            : base(name)
        {
            AckMessage = Encoding.ASCII.GetBytes("OK");
        }

        protected override void OnStop()
        {
            using (var clientPipeChannel = new ClientPipeChannel(PipeName, "basic shutdown"))
            {
                if (!clientPipeChannel.Connect())
                    return;
                clientPipeChannel.Write(Encoding.ASCII.GetBytes("QUIT"));
                clientPipeChannel.Read();
            }
        }

        protected override void Process(BasePipeChannel channel)
        {
            LogHelper.Instance.Log("[BasicPipeServer] {0} Session connected.", DateTime.Now);
            channel.Write(Encoding.ASCII.GetBytes("Hello"));
            LogHelper.Instance.Log("[BasicPipeServer] Bytes received: {0}", channel.Read().Length);
            channel.Write(AckMessage);
            channel.Dispose();
        }
    }
}