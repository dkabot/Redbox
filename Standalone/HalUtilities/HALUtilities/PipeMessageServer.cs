using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;

namespace HALUtilities
{
    internal sealed class PipeMessageServer : AbstractPipeSever
    {
        private readonly IIPCMessageFactory Factory = new IPCMessageFactory();

        internal PipeMessageServer(string name)
            : base(name)
        {
        }

        protected override void OnStop()
        {
            var msg = Factory.Create(MessageType.Quit, MessageSeverity.High, string.Empty);
            var channel = new ClientPipeChannel(PipeName, "MessageServerQuit");
            if (!channel.Connect())
                return;
            Factory.Write(msg, channel);
            var ipcMessage = Factory.Read(channel);
            if (ipcMessage.Type == MessageType.Ack)
                return;
            Console.WriteLine("Client: didn't get an ACK (received = {0})", ipcMessage.Type.ToString());
        }

        protected override void Process(BasePipeChannel channel)
        {
            var ipcMessage = Factory.Read(channel);
            LogHelper.Instance.Log("[PipeMessageServer] {0} Message received.", DateTime.Now);
            LogHelper.Instance.Log(ipcMessage.ToString());
            Factory.Write(Factory.CreateAck(ipcMessage.UID), channel);
            channel.Dispose();
        }
    }
}