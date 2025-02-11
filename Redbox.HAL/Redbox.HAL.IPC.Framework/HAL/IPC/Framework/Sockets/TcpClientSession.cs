using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Sockets
{
    internal sealed class TcpClientSession : Client.ClientSession
    {
        private readonly int m_clientPort;
        private IIPCChannel m_channel;
        private Stream Stream;
        private TcpClient TransportClient;

        internal TcpClientSession(IIpcProtocol protocol, int port, string identifier)
            : base(protocol, identifier)
        {
            m_clientPort = port;
        }

        protected override IIPCChannel Channel => m_channel;

        protected override void OnDispose()
        {
            try
            {
                TransportClient.Close();
            }
            catch (Exception ex)
            {
            }
        }

        protected override bool OnConnect()
        {
            TransportClient = new TcpClient(Protocol.Host, m_clientPort);
            var stream = (Stream)TransportClient.GetStream();
            if (!Protocol.IsSecure)
            {
                stream.ReadTimeout = Timeout;
            }
            else
            {
                var sslStream = new SslStream(stream, false, (sender, certificate, chain, sslPolicyErrors) => true);
                sslStream.ReadTimeout = Timeout;
                sslStream.WriteTimeout = Timeout;
                sslStream.AuthenticateAsClient(Protocol.Host);
                stream = sslStream;
            }

            Stream = stream;
            m_channel = new AsyncChannel(stream, TransportClient.ReceiveBufferSize, Identifier);
            return m_channel.Connect();
        }
    }
}