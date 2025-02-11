using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32;
using Redbox.Core;

namespace Redbox.IPC.Framework.Sockets
{
    public class TcpClientSession : ClientSession
    {
        private readonly bool _validateCallback = true;
        private Stream m_stream;
        private TcpClient m_transportClient;

        protected internal TcpClientSession(IPCProtocol protocol, int? timeout)
            : base(protocol)
        {
            Timeout = timeout ?? 30000;
            try
            {
                var obj = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox", "ValidateCallback", "true");
                _validateCallback = true;
                bool result;
                if (obj == null || !(obj is string) || !bool.TryParse(obj as string, out result))
                    return;
                _validateCallback = result;
                if (_validateCallback)
                    return;
                LogHelper.Instance.Log("Certificate callback validation is turned off");
            }
            catch
            {
            }
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
                        m_stream.WriteTimeout = Timeout;
                    }
                    else
                    {
                        var sslStream = new SslStream(TransportClient.GetStream(), false,
                            (sender, certificate, chain, sslPolicyErrors) =>
                            {
                                if (sslPolicyErrors == SslPolicyErrors.None)
                                    return true;
                                LogHelper.Instance.Log("SSL Stream RemoteCertificateValidationCallback failed.");
                                LogHelper.Instance.Log("Policy Errors: {0}", sslPolicyErrors);
                                LogHelper.Instance.Log("Certificate: {0}", certificate);
                                if (chain != null)
                                    chain.ChainStatus.ForEach(cs =>
                                        LogHelper.Instance.Log("Chain Status: {0}, Information: {1} ", cs.Status,
                                            cs.StatusInformation));
                                return !_validateCallback;
                            });
                        sslStream.ReadTimeout = Timeout;
                        sslStream.WriteTimeout = Timeout;
                        sslStream.AuthenticateAsClient(Protocol.Host);
                        m_stream = sslStream;
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

        public override bool Connect()
        {
            try
            {
                ConnectThrowOnError();
                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        public override void Dispose()
        {
            if (OwningPool != null)
                OwningPool.ReturnSession(this);
            base.Dispose();
        }

        public override void ConnectThrowOnError()
        {
            TransportClient = new TcpClient(Protocol.Host, int.Parse(Protocol.Port))
            {
                SendTimeout = Timeout,
                ReceiveTimeout = Timeout
            };
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
            finally
            {
                IsConnected = false;
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

        [Conditional("DEBUG")]
        private void LogCertificateValidationCallback(
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            LogHelper.Instance.Log("SSL Stream RemoteCertificateValidationCallback logging.");
            LogHelper.Instance.Log("Policy Errors: {0}", sslPolicyErrors);
            LogHelper.Instance.Log("Certificate: {0}", certificate);
            if (chain == null)
                return;
            chain.ChainStatus.ForEach(cs =>
                LogHelper.Instance.Log("Chain Status: {0}, Information: {1} ", cs.Status, cs.StatusInformation));
        }
    }
}