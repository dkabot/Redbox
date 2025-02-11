using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Redbox.Core;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;

namespace Redbox.IPC.Framework
{
    public abstract class IPCServiceHost
    {
        private static IPCServiceHost m_serviceHost;
        private readonly IList<ISession> m_sessions = new List<ISession>();
        private readonly object m_syncObject = new object();

        protected IPCServiceHost()
        {
            EncryptionProtocol = SslProtocols.None;
            Properties = new Dictionary<string, object>();
        }

        public bool Alive { get; set; }

        public string Name { get; protected set; }

        public string Version { get; protected set; }

        public string Copyright { get; protected set; }

        public IPCProtocol Protocol { get; protected set; }

        public WorkerPool WorkerPool { get; internal set; }

        public X509Certificate2 Certificate { get; set; }

        public SslProtocols EncryptionProtocol { get; set; }

        public IDictionary<string, object> Properties { get; }

        public bool EnableFilters { get; set; }

        public List<string> Filters { get; } = new List<string>();

        public Action<string, string> ParamAction { get; set; }

        public Action<ISession> RegisterSessionAction { get; set; }

        public static IPCServiceHost Create(
            IPCProtocol protocol,
            string product,
            string version,
            string copyright,
            int minThreads,
            int maxThreads,
            int bufferSize,
            int numberOfBuffers,
            int? backlogSize)
        {
            return Create(protocol, product, version, copyright, minThreads, maxThreads, bufferSize, numberOfBuffers,
                backlogSize, null);
        }

        public static IPCServiceHost Create(
            IPCProtocol protocol,
            string product,
            string version,
            string copyright,
            int minThreads,
            int maxThreads,
            int bufferSize,
            int numberOfBuffers,
            int? backlogSize,
            string poolName)
        {
            switch (protocol.Channel)
            {
                case ChannelType.Socket:
                    m_serviceHost = new TcpServiceHost(backlogSize);
                    break;
                case ChannelType.NamedPipe:
                    m_serviceHost = new NamedPipeServiceHost();
                    break;
                case ChannelType.Remoting:
                    m_serviceHost = new RemotingServiceHost(maxThreads);
                    break;
                default:
                    LogHelper.Instance.Log(
                        string.Format(
                            "Unsupported channel type '{0}' specified; try using sockets or pipes in your protocol uri.",
                            protocol.Channel), LogEntryType.Error);
                    return null;
            }

            BufferPool.Instance.Initialize(bufferSize, numberOfBuffers);
            m_serviceHost.WorkerPool = poolName != null
                ? new WorkerPool(minThreads, maxThreads, poolName)
                : new WorkerPool(minThreads, maxThreads);
            m_serviceHost.Name = product;
            m_serviceHost.Version = version;
            m_serviceHost.Protocol = protocol;
            m_serviceHost.Copyright = copyright;
            return m_serviceHost;
        }

        public abstract bool Start();

        public abstract void Stop();

        public virtual void UnregisterSession(ISession session)
        {
            UnregisterSession(session, true);
        }

        public virtual void UnregisterSession(ISession session, bool cleanupAbandonedSessions)
        {
            LogHelper.Instance.Log("Unregister server session.", LogEntryType.Debug);
            int count;
            lock (m_syncObject)
            {
                if (!m_sessions.Remove(session))
                    return;
                if (cleanupAbandonedSessions)
                    for (var index = 0; index < m_sessions.Count; ++index)
                        if (m_sessions[index] == null || !m_sessions[index].IsConnected())
                        {
                            m_sessions.RemoveAt(index);
                            LogHelper.Instance.Log("Unregister abandon server session.", LogEntryType.Debug);
                        }

                count = m_sessions.Count;
            }

            LogHelper.Instance.Log(string.Format("There are {0} server sessions active.", count), LogEntryType.Debug);
        }

        public virtual void RegiserSession(ISession session)
        {
            int count;
            lock (m_syncObject)
            {
                foreach (var property in Properties)
                    session.Properties.Add(property);
                session.EnableFilters = EnableFilters;
                session.SetFilters(Filters);
                session.SetParamAction(ParamAction);
                m_sessions.Add(session);
                count = m_sessions.Count;
                if (RegisterSessionAction != null)
                    RegisterSessionAction(session);
            }

            LogHelper.Instance.Log(
                string.Format("Register server session - there are {0} active server sessions.", count),
                LogEntryType.Debug);
        }
    }
}