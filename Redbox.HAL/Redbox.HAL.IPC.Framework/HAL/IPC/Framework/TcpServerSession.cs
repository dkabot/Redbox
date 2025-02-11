using System.Net.Sockets;

namespace Redbox.HAL.IPC.Framework
{
    public class TcpServerSession : ServerSession
    {
        public TcpServerSession(TcpClient client, IPCServiceHost serviceHost)
            : base(serviceHost, client.ReceiveBufferSize)
        {
            TransportClient = client;
            TransportStream = client.GetStream();
            NegotiateSecurityLayer(TransportStream);
        }

        protected override int ReadBufferSize => 1024;

        private TcpClient TransportClient { get; }

        private NetworkStream TransportStream { get; }

        protected override bool CloseClientsInternal()
        {
            TransportClient.Close();
            return true;
        }

        protected override bool CloseStreamsInternal()
        {
            Stream.Dispose();
            return true;
        }
    }
}