using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    public abstract class AbstractServerSession : ISession, IMessageSink
    {
        protected readonly string Identifier;
        private readonly object m_syncLock = new object();
        private readonly List<string> MessageSinkQueue = new List<string>();
        protected readonly StringBuilder ReadBuilder = new StringBuilder();
        protected readonly IIpcServiceHost ServiceHost;
        private readonly string Welcome;

        protected AbstractServerSession(IIpcServiceHost host, string id)
            : this(host, id, LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
        {
        }

        protected AbstractServerSession(IIpcServiceHost host, string id, bool logDetails)
        {
            ServiceHost = host;
            LogDetailedMessages = logDetails;
            Properties = new Dictionary<string, object>();
            Identifier = id;
            Welcome = string.Format("Welcome! {0}, Version {1}, {2}", host.HostInfo.Product, host.HostInfo.Version,
                host.HostInfo.Copyright);
        }

        public IDictionary<string, object> Properties { get; private set; }

        protected abstract IIPCChannel Channel { get; }

        public void Start()
        {
            LogHelper.Instance.Log("[AbstractServerSession-{0}] Start", Identifier);
            RedboxChannelDecorator.Write(Channel, Welcome);
            CommLoop();
        }

        public bool Send(string message)
        {
            var msg = string.Format("[MSG] {0}", message);
            if (LogDetailedMessages)
                LogHelper.Instance.Log(msg);
            lock (m_syncLock)
            {
                MessageSinkQueue.Add(msg);
            }

            return true;
        }

        public event EventHandler Disconnect;

        public bool LogDetailedMessages { get; }

        protected abstract bool OnSessionEnd();

        protected void CommLoop()
        {
            while (true)
                using (var response = new ServerResponse())
                {
                    Channel.Read(response);
                    if (!response.IsComplete)
                    {
                        QuitSession(false);
                        break;
                    }

                    if (LogDetailedMessages)
                        LogHelper.Instance.Log("[AbstractServerSession-{0}] Received command = '{1}'", Identifier,
                            response.Command);
                    if (response.Command.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        QuitSession(true);
                        break;
                    }

                    var commandResult = CommandService.Instance.Execute(this, response.Command);
                    ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
                    FlushSinkQueue(commandResult.Messages);
                    var line = commandResult.ToString();
                    if (LogDetailedMessages)
                        LogHelper.Instance.Log("[AbstractServerSession - {0}]  Response = {1}", Identifier, line);
                    RedboxChannelDecorator.Write(Channel, line);
                }
        }

        protected bool QuitSession(bool sendBye)
        {
            if (sendBye)
                RedboxChannelDecorator.Write(Channel, "Goodbye!");
            if (Disconnect != null)
                Disconnect(this, EventArgs.Empty);
            var flag = false;
            try
            {
                OnSessionEnd();
                if (LogDetailedMessages)
                    LogHelper.Instance.Log("[AbstractServerSession - {0}] Session end.", Identifier);
                flag = true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Caught exception in QuitSession", ex);
            }

            try
            {
                ServiceHost.Unregister(this);
                return flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("QuitSession: unable to unregister session.", ex);
                return false;
            }
        }

        private void FlushSinkQueue(ICollection<string> messages)
        {
            lock (m_syncLock)
            {
                while (MessageSinkQueue.Count > 0)
                {
                    var messageSink = MessageSinkQueue[0];
                    MessageSinkQueue.RemoveAt(0);
                    messages.Add(messageSink);
                }
            }
        }
    }
}