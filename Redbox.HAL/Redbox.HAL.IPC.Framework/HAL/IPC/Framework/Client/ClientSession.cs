using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Client
{
    public abstract class ClientSession : IIpcClientSession, IDisposable
    {
        protected readonly bool LogDetailed;
        private int? m_timeout;

        protected ClientSession(IIpcProtocol protocol, string id, bool logDetails)
        {
            Protocol = protocol;
            Identifier = id;
            LogDetailed = logDetails;
        }

        protected ClientSession(IIpcProtocol protocol, string id)
            : this(protocol, id, LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
        {
        }

        public string Identifier { get; }

        protected abstract IIPCChannel Channel { get; }

        internal bool SupportsEvents => ServerEvent != null;

        public void Dispose()
        {
            if (IsDisposed)
                return;
            if (LogDetailed)
                LogHelper.Instance.Log("[ClientSession] Disposing id {0}", Identifier);
            IsDisposed = true;
            try
            {
                ExecuteCommand("quit");
                Channel.Dispose();
            }
            catch (IOException ex)
            {
            }
            finally
            {
                OnDispose();
            }
        }

        public void ConnectThrowOnError()
        {
            IsConnected = OnConnect();
            if (!IsConnected)
                throw new TimeoutException(string.Format("Failed to connect on {0}", Protocol.ToString()));
            ConsumeMessages();
        }

        public bool IsStatusOK(List<string> messages)
        {
            var flag = false;
            if (messages.Count > 0)
                flag = messages[messages.Count - 1].StartsWith("203 Command");
            return flag;
        }

        public List<string> ExecuteCommand(string command)
        {
            if (LogDetailed)
                LogHelper.Instance.Log("[ClientSession-{0}] executing {1}", Identifier, command);
            RedboxChannelDecorator.Write(Channel, command);
            var messageList = ConsumeMessages();
            if (LogDetailed)
                foreach (var str in messageList)
                    LogHelper.Instance.Log("[ClientSession-{0}]  Response msg {1}", Identifier, str);
            return messageList;
        }

        public int Timeout
        {
            get => m_timeout ?? 30000;
            set => m_timeout = value;
        }

        public bool IsConnected { get; private set; }

        public IIpcProtocol Protocol { get; protected set; }

        public bool IsDisposed { get; private set; }

        public event Action<string> ServerEvent;

        protected virtual void OnDispose()
        {
        }

        protected abstract bool OnConnect();

        protected MessageList ConsumeMessages()
        {
            var response = new ClientResponse(this, LogDetailed);
            Channel.Read(response);
            if (!response.IsComplete)
            {
                response.Clear();
                var errors = new ErrorList();
                errors.Add(Error.NewError("J888", string.Format("Timeout threshold {0} exceeded.", Timeout),
                    "Reissue the command when the service is not as busy."));
                ProtocolHelper.FormatErrors(errors, response.Messages);
                response.Messages.Add("545 Command failed.");
            }

            return response.Messages;
        }

        internal void OnServerEvent(string line)
        {
            if (ServerEvent == null)
                return;
            ServerEvent(line);
        }
    }
}