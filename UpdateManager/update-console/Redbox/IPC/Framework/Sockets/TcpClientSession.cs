using Microsoft.Win32;
using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Redbox.IPC.Framework.Sockets
{
    internal class TcpClientSession : ClientSession
    {
        private Stream m_stream;
        private TcpClient m_transportClient;
        private bool _validateCallback = true;

        public override bool Connect()
        {
            try
            {
                this.ConnectThrowOnError();
                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        public override void Dispose()
        {
            if (this.OwningPool != null)
                this.OwningPool.ReturnSession((ClientSession)this);
            base.Dispose();
        }

        public override void ConnectThrowOnError()
        {
            this.TransportClient = new TcpClient(this.Protocol.Host, int.Parse(this.Protocol.Port))
            {
                SendTimeout = this.Timeout,
                ReceiveTimeout = this.Timeout
            };
            this.ConsumeMessages();
            this.IsConnected = true;
        }

        protected internal TcpClientSession(IPCProtocol protocol, int? timeout)
          : base(protocol)
        {
            this.Timeout = timeout ?? 30000;
            try
            {
                object obj = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox", "ValidateCallback", (object)"true");
                this._validateCallback = true;
                bool result;
                if (obj == null || !(obj is string) || !bool.TryParse(obj as string, out result))
                    return;
                this._validateCallback = result;
                if (this._validateCallback)
                    return;
                LogHelper.Instance.Log("Certificate callback validation is turned off");
            }
            catch
            {
            }
        }

        protected internal override bool IsConnectionAvailable()
        {
            return !this.TransportClient.Client.Poll(10, SelectMode.SelectRead) || this.TransportClient.Client.Available != 0;
        }

        protected internal override void CustomClose()
        {
            try
            {
                this.Stream.Close();
                this.TransportClient.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                this.IsConnected = false;
            }
        }

        protected override bool CanRead()
        {
            return this.Stream.CanRead && this.TransportClient.Client.Available > 0;
        }

        protected override int GetAvailableData() => this.TransportClient.Client.Available;

        protected override Stream Stream
        {
            get
            {
                if (this.m_stream == null)
                {
                    if (!this.Protocol.IsSecure)
                    {
                        this.m_stream = (Stream)this.TransportClient.GetStream();
                        this.m_stream.ReadTimeout = this.Timeout;
                        this.m_stream.WriteTimeout = this.Timeout;
                    }
                    else
                    {
                        SslStream sslStream = new SslStream((Stream)this.TransportClient.GetStream(), false, (RemoteCertificateValidationCallback)((sender, certificate, chain, sslPolicyErrors) =>
                        {
                            if (sslPolicyErrors == SslPolicyErrors.None)
                                return true;
                            LogHelper.Instance.Log("SSL Stream RemoteCertificateValidationCallback failed.");
                            LogHelper.Instance.Log("Policy Errors: {0}", (object)sslPolicyErrors);
                            LogHelper.Instance.Log("Certificate: {0}", (object)certificate);
                            if (chain != null)
                                ((IEnumerable<X509ChainStatus>)chain.ChainStatus).ForEach<X509ChainStatus>((Action<X509ChainStatus>)(cs => LogHelper.Instance.Log("Chain Status: {0}, Information: {1} ", (object)cs.Status, (object)cs.StatusInformation)));
                            return !this._validateCallback;
                        }));
                        sslStream.ReadTimeout = this.Timeout;
                        sslStream.WriteTimeout = this.Timeout;
                        sslStream.AuthenticateAsClient(this.Protocol.Host);
                        this.m_stream = (Stream)sslStream;
                    }
                }
                return this.m_stream;
            }
        }

        [Conditional("DEBUG")]
        private void LogCertificateValidationCallback(
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            LogHelper.Instance.Log("SSL Stream RemoteCertificateValidationCallback logging.");
            LogHelper.Instance.Log("Policy Errors: {0}", (object)sslPolicyErrors);
            LogHelper.Instance.Log("Certificate: {0}", (object)certificate);
            if (chain == null)
                return;
            ((IEnumerable<X509ChainStatus>)chain.ChainStatus).ForEach<X509ChainStatus>((Action<X509ChainStatus>)(cs => LogHelper.Instance.Log("Chain Status: {0}, Information: {1} ", (object)cs.Status, (object)cs.StatusInformation)));
        }

        private TcpClient TransportClient
        {
            get => this.m_transportClient;
            set
            {
                this.m_transportClient = value;
                if (this.m_transportClient == null)
                    return;
                this.ReadBuffer = new byte[this.m_transportClient.ReceiveBufferSize];
            }
        }
    }
}
