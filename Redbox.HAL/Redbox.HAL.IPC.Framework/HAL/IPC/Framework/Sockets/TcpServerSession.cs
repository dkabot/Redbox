using System.Net.Sockets;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Sockets
{
    public sealed class TcpServerSession : AbstractServerSession
    {
        private readonly TcpClient TransportClient;

        public TcpServerSession(TcpClient client, IIpcServiceHost serviceHost, string id)
            : base(serviceHost, id)
        {
            TransportClient = client;
            Channel = new AsyncChannel(client.GetStream(), client.ReceiveBufferSize, id, true);
        }

        protected override IIPCChannel Channel { get; }

        protected override bool OnSessionEnd()
        {
            if (LogDetailedMessages)
                LogHelper.Instance.Log("[TcpServerSession-{0}] Session end.", Identifier);
            TransportClient.Close();
            Channel.Dispose();
            return true;
        }
    }
}