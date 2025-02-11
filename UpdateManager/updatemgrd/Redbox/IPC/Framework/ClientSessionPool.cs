using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.IPC.Framework
{
    internal class ClientSessionPool : IDisposable
    {
        private bool m_isDisposed;
        private readonly object m_lock = new object();

        public ClientSessionPool(IPCProtocol protocol, int poolSize)
          : this(protocol, poolSize, new int?())
        {
        }

        public ClientSessionPool(IPCProtocol protocol, int poolSize, int? timeout)
        {
            this.Sessions = new List<ClientSession>();
            this.Protocol = protocol;
            this.Timeout = timeout;
            this.PoolSize = poolSize;
        }

        public void Dispose()
        {
            lock (this.m_lock)
            {
                if (this.m_isDisposed)
                    return;
                this.m_isDisposed = true;
                foreach (ClientSession session in this.Sessions)
                {
                    try
                    {
                        if (session.IsConnected && session.IsConnectionAvailable())
                            session.Close();
                        else
                            session.CustomClose();
                    }
                    catch
                    {
                    }
                }
                this.Sessions.Clear();
            }
        }

        public bool ReturnSession(ClientSession session)
        {
            lock (this.m_lock)
            {
                if (this.m_isDisposed)
                    return false;
                this.Sessions.Remove(session);
                session.Close();
                Monitor.PulseAll(this.m_lock);
            }
            return true;
        }

        public ClientSession GetSession(int timeout, int attempts)
        {
            ClientSession session;
            lock (this.m_lock)
            {
                if (this.m_isDisposed)
                    throw new ObjectDisposedException("Session pool is diposed.");
                while (this.Sessions.Count == this.PoolSize)
                {
                    Monitor.Wait(this.m_lock, timeout);
                    if (--attempts == 0)
                        throw new TimeoutException(string.Format("Unable to acquire client session from {0} pool", (object)this.Protocol.Channel));
                }
                session = this.CreateSession();
                this.Sessions.Add(session);
            }
            return session;
        }

        public int PoolSize { get; internal set; }

        public int? Timeout { get; internal set; }

        public IPCProtocol Protocol { get; internal set; }

        internal ClientSession CreateSession()
        {
            ClientSession clientSession = ClientSession.GetClientSession(this.Protocol, this.Timeout);
            clientSession.OwningPool = this;
            return clientSession;
        }

        internal List<ClientSession> Sessions { get; set; }
    }
}
