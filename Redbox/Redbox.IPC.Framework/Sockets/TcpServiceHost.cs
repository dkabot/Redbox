using System;
using System.Net;
using System.Net.Sockets;
using Redbox.Core;

namespace Redbox.IPC.Framework.Sockets
{
    public class TcpServiceHost : IPCServiceHost
    {
        private readonly int? m_backlogSize;
        private Socket m_serverSocket;
        private IPEndPoint m_serverSocketEndPoint;

        public TcpServiceHost(int? backlogSize)
        {
            m_backlogSize = backlogSize;
        }

        public override bool Start()
        {
            LogHelper.Instance.Log("Find IP address of interface.", LogEntryType.Info);
            Alive = true;
            int result;
            if (!int.TryParse(Protocol.Port, out result))
            {
                LogHelper.Instance.Log(
                    string.Format("Unable to create a listener on port '{0}'; maybe your uri is wrong?", Protocol.Port),
                    LogEntryType.Fatal);
                return false;
            }

            IPAddress address;
            var bindToAddress = !IPAddress.TryParse(Protocol.Host, out address)
                ? string.Compare("localhost", Protocol.Host, true) != 0
                    ? IPAddressHelper.GetAddressForHostName(Protocol.Host)
                    : IPAddress.Parse("127.0.0.1")
                : address;
            if (bindToAddress == null)
            {
                LogHelper.Instance.Log(
                    string.Format(
                        "Unable to bind to host named '{0}' on port {1},  ensure there is a valid network interface associated to this host and that the designated port is available.",
                        Protocol.Host, Protocol.Port), LogEntryType.Fatal);
                return false;
            }

            LogHelper.Instance.Log("Start Worker Pool threads.");
            WorkerPool.Start();
            LogHelper.Instance.Log(string.Format("Resolved interface to IP address '{0}'.", bindToAddress),
                LogEntryType.Info);
            LogHelper.Instance.Log("Bind to interface.", LogEntryType.Info);
            CreateServerSocket(bindToAddress, result);
            LogHelper.Instance.Log("Begin processing incoming requests.", LogEntryType.Info);
            m_serverSocket.Bind(m_serverSocketEndPoint);
            m_serverSocket.Listen(m_backlogSize ?? 20);
            Statistics.Instance.ServerStartTime = DateTime.Now;
            BeginAccept();
            return true;
        }

        public override void Stop()
        {
            if (!Alive)
                return;
            Alive = false;
            try
            {
                m_serverSocket.Close();
                WorkerPool.Shutdown();
                BufferPool.Instance.Shutdown();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was rasied in TcpServiceHost.Stop", ex);
            }

            LogHelper.Instance.Log("End processing incoming requests.", LogEntryType.Info);
        }

        private void BeginAccept()
        {
            try
            {
                if (!Alive)
                    return;
                m_serverSocket.BeginAccept(EndAccept, this);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    "An unhandled exception was raised in TcpServiceListener, m_listener.BeginAcceptTcpClient.", ex);
                Stop();
            }
        }

        private void EndAccept(IAsyncResult result)
        {
            try
            {
                if (!Alive)
                    return;
                var socket = m_serverSocket.EndAccept(result);
                WorkerPool.QueueWorkItem(o =>
                {
                    var tcpServerSession = new TcpServerSession(socket, result.AsyncState as IPCServiceHost);
                    RegiserSession(tcpServerSession);
                    tcpServerSession.Start();
                });
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    "An unhandled exception was raised in TcpServiceListener, m_listener.EndAcceptTcpClientCallback.",
                    ex);
            }
            finally
            {
                BeginAccept();
            }
        }

        private void CreateServerSocket(IPAddress bindToAddress, int port)
        {
            m_serverSocketEndPoint = new IPEndPoint(bindToAddress, port);
            m_serverSocket = new Socket(m_serverSocketEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }
    }
}