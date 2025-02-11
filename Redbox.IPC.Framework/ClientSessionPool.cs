using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.IPC.Framework
{
    public class ClientSessionPool : IDisposable
    {
        private readonly object m_lock = new object();
        private bool m_isDisposed;

        public ClientSessionPool(IPCProtocol protocol, int poolSize)
            : this(protocol, poolSize, new int?())
        {
        }

        public ClientSessionPool(IPCProtocol protocol, int poolSize, int? timeout)
        {
            Sessions = new List<ClientSession>();
            Protocol = protocol;
            Timeout = timeout;
            PoolSize = poolSize;
        }

        public int PoolSize { get; internal set; }

        public int? Timeout { get; internal set; }

        public IPCProtocol Protocol { get; internal set; }

        internal List<ClientSession> Sessions { get; set; }

        public void Dispose()
        {
            lock (m_lock)
            {
                if (m_isDisposed)
                    return;
                m_isDisposed = true;
                foreach (var session in Sessions)
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

                Sessions.Clear();
            }
        }

        public bool ReturnSession(ClientSession session)
        {
            lock (m_lock)
            {
                if (m_isDisposed)
                    return false;
                Sessions.Remove(session);
                session.Close();
                Monitor.PulseAll(m_lock);
            }

            return true;
        }

        public ClientSession GetSession(int timeout, int attempts)
        {
            ClientSession session;
            lock (m_lock)
            {
                if (m_isDisposed)
                    throw new ObjectDisposedException("Session pool is diposed.");
                while (Sessions.Count == PoolSize)
                {
                    Monitor.Wait(m_lock, timeout);
                    if (--attempts == 0)
                        throw new TimeoutException(string.Format("Unable to acquire client session from {0} pool",
                            Protocol.Channel));
                }

                session = CreateSession();
                Sessions.Add(session);
            }

            return session;
        }

        internal ClientSession CreateSession()
        {
            var clientSession = ClientSession.GetClientSession(Protocol, Timeout);
            clientSession.OwningPool = this;
            return clientSession;
        }
    }
}