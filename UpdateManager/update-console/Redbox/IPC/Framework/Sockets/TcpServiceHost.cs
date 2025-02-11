using Redbox.Core;
using System;
using System.Net;
using System.Net.Sockets;

namespace Redbox.IPC.Framework.Sockets
{
    internal class TcpServiceHost : IPCServiceHost
    {
        private Socket m_serverSocket;
        private readonly int? m_backlogSize;
        private IPEndPoint m_serverSocketEndPoint;

        public TcpServiceHost(int? backlogSize) => this.m_backlogSize = backlogSize;

        public override bool Start()
        {
            LogHelper.Instance.Log("Find IP address of interface.", LogEntryType.Info);
            this.Alive = true;
            int result;
            if (!int.TryParse(this.Protocol.Port, out result))
            {
                LogHelper.Instance.Log(string.Format("Unable to create a listener on port '{0}'; maybe your uri is wrong?", (object)this.Protocol.Port), LogEntryType.Fatal);
                return false;
            }
            IPAddress address;
            IPAddress bindToAddress = !IPAddress.TryParse(this.Protocol.Host, out address) ? (string.Compare("localhost", this.Protocol.Host, true) != 0 ? IPAddressHelper.GetAddressForHostName(this.Protocol.Host) : IPAddress.Parse("127.0.0.1")) : address;
            if (bindToAddress == null)
            {
                LogHelper.Instance.Log(string.Format("Unable to bind to host named '{0}' on port {1},  ensure there is a valid network interface associated to this host and that the designated port is available.", (object)this.Protocol.Host, (object)this.Protocol.Port), LogEntryType.Fatal);
                return false;
            }
            LogHelper.Instance.Log("Start Worker Pool threads.");
            this.WorkerPool.Start();
            LogHelper.Instance.Log(string.Format("Resolved interface to IP address '{0}'.", (object)bindToAddress), LogEntryType.Info);
            LogHelper.Instance.Log("Bind to interface.", LogEntryType.Info);
            this.CreateServerSocket(bindToAddress, result);
            LogHelper.Instance.Log("Begin processing incoming requests.", LogEntryType.Info);
            this.m_serverSocket.Bind((EndPoint)this.m_serverSocketEndPoint);
            this.m_serverSocket.Listen(this.m_backlogSize ?? 20);
            Statistics.Instance.ServerStartTime = DateTime.Now;
            this.BeginAccept();
            return true;
        }

        public override void Stop()
        {
            if (!this.Alive)
                return;
            this.Alive = false;
            try
            {
                this.m_serverSocket.Close();
                this.WorkerPool.Shutdown();
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
                if (!this.Alive)
                    return;
                this.m_serverSocket.BeginAccept(new AsyncCallback(this.EndAccept), (object)this);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServiceListener, m_listener.BeginAcceptTcpClient.", ex);
                this.Stop();
            }
        }

        private void EndAccept(IAsyncResult result)
        {
            try
            {
                if (!this.Alive)
                    return;
                Socket socket = this.m_serverSocket.EndAccept(result);
                this.WorkerPool.QueueWorkItem((WorkItem)(o =>
                {
                    TcpServerSession tcpServerSession = new TcpServerSession(socket, result.AsyncState as IPCServiceHost);
                    this.RegiserSession((ISession)tcpServerSession);
                    tcpServerSession.Start();
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in TcpServiceListener, m_listener.EndAcceptTcpClientCallback.", ex);
            }
            finally
            {
                this.BeginAccept();
            }
        }

        private void CreateServerSocket(IPAddress bindToAddress, int port)
        {
            this.m_serverSocketEndPoint = new IPEndPoint(bindToAddress, port);
            this.m_serverSocket = new Socket(this.m_serverSocketEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.m_serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }
    }
}
