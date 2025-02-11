using Redbox.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Redbox.IPC.Framework.Sockets
{
    internal class TcpServerSession : ServerSession
    {
        public TcpServerSession(Socket client, IPCServiceHost serviceHost)
          : base(serviceHost)
        {
            this.ClientSocket = client;
            this.NegotiateSecurityLayer((Stream)new NetworkStream(this.ClientSocket, true));
        }

        protected override bool CloseClientsInternal()
        {
            try
            {
                this.ClientSocket.Close();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.CloseClientsInternal.", ex);
                return false;
            }
        }

        protected override bool IsConnectedInternal()
        {
            try
            {
                return !this.ClientSocket.Poll(10, SelectMode.SelectRead) || this.ClientSocket.Available != 0;
            }
            catch
            {
                return false;
            }
        }

        protected override bool CloseStreamsInternal()
        {
            try
            {
                this.Stream.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.CloseStreamsInternal.", ex);
                return false;
            }
        }

        protected override string GetRemoteHostIP()
        {
            if (this.ClientSocket == null || !this.ClientSocket.Connected)
                return string.Empty;
            IPEndPoint remoteEndPoint = (IPEndPoint)this.ClientSocket.RemoteEndPoint;
            switch (remoteEndPoint.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return remoteEndPoint.Address.ToString();
                case AddressFamily.InterNetworkV6:
                    return remoteEndPoint.Address.ToString() == "::1" ? "127.0.0.1" : "0.0.0.0";
                default:
                    return "0.0.0.0";
            }
        }

        internal Socket ClientSocket { get; set; }
    }
}
