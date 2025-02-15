using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using IMessageSink = Redbox.Core.IMessageSink;

namespace Redbox.IPC.Framework.Remoting
{
    public class RemotingServerSession : ISession, IMessageSink
    {
        private Action<ISession> _beforeCommandAction;
        private Action<string, string> _paramAction;
        private ParameterDictionary m_context;
        private Dictionary<string, object> m_properties;

        public List<string> Filters { get; } = new List<string>();

        public bool Send(string message)
        {
            throw new NotImplementedException("Not valid for RemotingChannel");
        }

        public void Start()
        {
            throw new NotImplementedException("Not valid for RemotingChannel");
        }

        public void SetParamAction(Action<string, string> paramAction)
        {
            _paramAction = paramAction;
        }

        public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
        {
            _beforeCommandAction = beforeCommandAction;
        }

        public void SetFilters(List<string> filters)
        {
            Filters.Clear();
            filters.ForEach(Filters.Add);
        }

        public bool EnableFilters { get; set; }

        public bool IsConnected()
        {
            return true;
        }

        public ParameterDictionary Context => m_context ?? (m_context = new ParameterDictionary());

        public IDictionary<string, object> Properties => (IDictionary<string, object>)m_properties ??
                                                         (m_properties = new Dictionary<string, object>());

        public event EventHandler Disconnect;

        public string ProcessRequest(string commandString)
        {
            if (_beforeCommandAction != null)
                _beforeCommandAction(this);
            var commandResult = CommandService.Instance.Execute(this, CallContext.GetData("RemoteHostIP") as string,
                commandString, Filters, EnableFilters, _paramAction);
            if (commandResult == null)
                return null;
            ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
            return commandResult.ToString();
        }
    }
}