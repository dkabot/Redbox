using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public sealed class TcpClientSession : ClientSession
{
    private readonly int m_clientPort;
    private Stream m_stream;
    private TcpClient m_transportClient;

    internal TcpClientSession(IPCProtocol protocol, int port)
        : base(protocol)
    {
        m_clientPort = port;
    }

    protected override Stream Stream
    {
        get
        {
            if (m_stream == null)
            {
                if (!Protocol.IsSecure)
                {
                    m_stream = TransportClient.GetStream();
                    m_stream.ReadTimeout = Timeout;
                }
                else
                {
                    var sslStream1 = new SslStream(TransportClient.GetStream(), false,
                        (sender, certificate, chain, sslPolicyErrors) => true);
                    sslStream1.ReadTimeout = Timeout;
                    sslStream1.WriteTimeout = Timeout;
                    var sslStream2 = sslStream1;
                    sslStream2.AuthenticateAsClient(Protocol.Host);
                    m_stream = sslStream2;
                }
            }

            return m_stream;
        }
    }

    private TcpClient TransportClient
    {
        get => m_transportClient;
        set
        {
            m_transportClient = value;
            if (m_transportClient == null)
                return;
            ReadBuffer = new byte[m_transportClient.ReceiveBufferSize];
        }
    }

    protected override void OnConnectThrowOnError()
    {
        TransportClient = new TcpClient(Protocol.Host, m_clientPort);
        ConsumeMessages();
        IsConnected = true;
    }

    protected internal override bool IsConnectionAvailable()
    {
        return !TransportClient.Client.Poll(10, SelectMode.SelectRead) || TransportClient.Client.Available != 0;
    }

    protected internal override void CustomClose()
    {
        try
        {
            Stream.Close();
            TransportClient.Close();
        }
        catch (Exception ex)
        {
        }
    }

    protected override bool CanRead()
    {
        return Stream.CanRead && TransportClient.Client.Available > 0;
    }

    protected override int GetAvailableData()
    {
        return TransportClient.Client.Available;
    }
}