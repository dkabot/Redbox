using Redbox.Core;
using Redbox.IPC.Framework.NamedPipes;
using Redbox.IPC.Framework.Remoting;
using Redbox.IPC.Framework.Sockets;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Redbox.IPC.Framework
{
    internal abstract class IPCServiceHost
    {
        private bool _enableFilters;
        private List<string> _filters = new List<string>();
        private Action<string, string> _paramAction;
        private Action<ISession> _registerSessionAction;
        private static IPCServiceHost m_serviceHost;
        private readonly object m_syncObject = new object();
        private readonly IList<ISession> m_sessions = (IList<ISession>)new List<ISession>();

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
            return IPCServiceHost.Create(protocol, product, version, copyright, minThreads, maxThreads, bufferSize, numberOfBuffers, backlogSize, (string)null);
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
                    IPCServiceHost.m_serviceHost = (IPCServiceHost)new TcpServiceHost(backlogSize);
                    break;
                case ChannelType.NamedPipe:
                    IPCServiceHost.m_serviceHost = (IPCServiceHost)new NamedPipeServiceHost();
                    break;
                case ChannelType.Remoting:
                    IPCServiceHost.m_serviceHost = (IPCServiceHost)new RemotingServiceHost(maxThreads);
                    break;
                default:
                    LogHelper.Instance.Log(string.Format("Unsupported channel type '{0}' specified; try using sockets or pipes in your protocol uri.", (object)protocol.Channel), LogEntryType.Error);
                    return (IPCServiceHost)null;
            }
            BufferPool.Instance.Initialize(bufferSize, numberOfBuffers);
            IPCServiceHost.m_serviceHost.WorkerPool = poolName != null ? new WorkerPool(minThreads, maxThreads, poolName) : new WorkerPool(minThreads, maxThreads);
            IPCServiceHost.m_serviceHost.Name = product;
            IPCServiceHost.m_serviceHost.Version = version;
            IPCServiceHost.m_serviceHost.Protocol = protocol;
            IPCServiceHost.m_serviceHost.Copyright = copyright;
            return IPCServiceHost.m_serviceHost;
        }

        public abstract bool Start();

        public abstract void Stop();

        public virtual void UnregisterSession(ISession session)
        {
            this.UnregisterSession(session, true);
        }

        public virtual void UnregisterSession(ISession session, bool cleanupAbandonedSessions)
        {
            LogHelper.Instance.Log("Unregister server session.", LogEntryType.Debug);
            int count;
            lock (this.m_syncObject)
            {
                if (!this.m_sessions.Remove(session))
                    return;
                if (cleanupAbandonedSessions)
                {
                    for (int index = 0; index < this.m_sessions.Count; ++index)
                    {
                        if (this.m_sessions[index] == null || !this.m_sessions[index].IsConnected())
                        {
                            this.m_sessions.RemoveAt(index);
                            LogHelper.Instance.Log("Unregister abandon server session.", LogEntryType.Debug);
                        }
                    }
                }
                count = this.m_sessions.Count;
            }
            LogHelper.Instance.Log(string.Format("There are {0} server sessions active.", (object)count), LogEntryType.Debug);
        }

        public virtual void RegiserSession(ISession session)
        {
            int count;
            lock (this.m_syncObject)
            {
                foreach (KeyValuePair<string, object> property in (IEnumerable<KeyValuePair<string, object>>)this.Properties)
                    session.Properties.Add(property);
                session.EnableFilters = this.EnableFilters;
                session.SetFilters(this.Filters);
                session.SetParamAction(this._paramAction);
                this.m_sessions.Add(session);
                count = this.m_sessions.Count;
                if (this._registerSessionAction != null)
                    this._registerSessionAction(session);
            }
            LogHelper.Instance.Log(string.Format("Register server session - there are {0} active server sessions.", (object)count), LogEntryType.Debug);
        }

        public bool Alive { get; set; }

        public string Name { get; protected set; }

        public string Version { get; protected set; }

        public string Copyright { get; protected set; }

        public IPCProtocol Protocol { get; protected set; }

        public WorkerPool WorkerPool { get; internal set; }

        public X509Certificate2 Certificate { get; set; }

        public SslProtocols EncryptionProtocol { get; set; }

        public IDictionary<string, object> Properties { get; private set; }

        public bool EnableFilters
        {
            get => this._enableFilters;
            set => this._enableFilters = value;
        }

        public List<string> Filters => this._filters;

        public Action<string, string> ParamAction
        {
            get => this._paramAction;
            set => this._paramAction = value;
        }

        public Action<ISession> RegisterSessionAction
        {
            get => this._registerSessionAction;
            set => this._registerSessionAction = value;
        }

        protected IPCServiceHost()
        {
            this.EncryptionProtocol = SslProtocols.None;
            this.Properties = (IDictionary<string, object>)new Dictionary<string, object>();
        }
    }
}
