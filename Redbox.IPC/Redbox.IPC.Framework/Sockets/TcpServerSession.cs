using System;
using System.Net;
using System.Net.Sockets;
using Redbox.Core;

namespace Redbox.IPC.Framework.Sockets
{
    public class TcpServerSession : ServerSession
    {
        public TcpServerSession(Socket client, IPCServiceHost serviceHost)
            : base(serviceHost)
        {
            ClientSocket = client;
            NegotiateSecurityLayer(new NetworkStream(ClientSocket, true));
        }

        internal Socket ClientSocket { get; set; }

        protected override bool CloseClientsInternal()
        {
            try
            {
                ClientSocket.Close();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.CloseClientsInternal.",
                    ex);
                return false;
            }
        }

        protected override bool IsConnectedInternal()
        {
            try
            {
                return !ClientSocket.Poll(10, SelectMode.SelectRead) || ClientSocket.Available != 0;
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
                Stream.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServerSession.CloseStreamsInternal.",
                    ex);
                return false;
            }
        }

        protected override string GetRemoteHostIP()
        {
            if (ClientSocket == null || !ClientSocket.Connected)
                return string.Empty;
            var remoteEndPoint = (IPEndPoint)ClientSocket.RemoteEndPoint;
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
    }
}