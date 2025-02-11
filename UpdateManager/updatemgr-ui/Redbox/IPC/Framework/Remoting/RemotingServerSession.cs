using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Redbox.IPC.Framework.Remoting
{
    internal class RemotingServerSession : ISession, Redbox.Core.IMessageSink
    {
        private Action<string, string> _paramAction;
        private Action<ISession> _beforeCommandAction;
        private List<string> _filters = new List<string>();
        private bool _enableFilters;
        private ParameterDictionary m_context;
        private Dictionary<string, object> m_properties;

        public bool Send(string message)
        {
            throw new NotImplementedException("Not valid for RemotingChannel");
        }

        public void Start() => throw new NotImplementedException("Not valid for RemotingChannel");

        public void SetParamAction(Action<string, string> paramAction)
        {
            this._paramAction = paramAction;
        }

        public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
        {
            this._beforeCommandAction = beforeCommandAction;
        }

        public void SetFilters(List<string> filters)
        {
            this._filters.Clear();
            filters.ForEach(new Action<string>(this._filters.Add));
        }

        public List<string> Filters => this._filters;

        public bool EnableFilters
        {
            get => this._enableFilters;
            set => this._enableFilters = value;
        }

        public string ProcessRequest(string commandString)
        {
            if (this._beforeCommandAction != null)
                this._beforeCommandAction((ISession)this);
            CommandResult commandResult = CommandService.Instance.Execute((ISession)this, CallContext.GetData("RemoteHostIP") as string, commandString, this.Filters, this.EnableFilters, this._paramAction);
            if (commandResult == null)
                return (string)null;
            ProtocolHelper.FormatErrors(commandResult.Errors, (IList<string>)commandResult.Messages);
            return commandResult.ToString();
        }

        public bool IsConnected() => true;

        public ParameterDictionary Context
        {
            get => this.m_context ?? (this.m_context = new ParameterDictionary());
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                return (IDictionary<string, object>)this.m_properties ?? (IDictionary<string, object>)(this.m_properties = new Dictionary<string, object>());
            }
        }

        public event EventHandler Disconnect;
    }
}
