using Redbox.Core;
using System;
using System.Collections.Generic;

namespace Redbox.IPC.Framework
{
    internal interface ISession : IMessageSink
    {
        void Start();

        void SetFilters(List<string> filters);

        bool EnableFilters { get; set; }

        void SetParamAction(Action<string, string> paramAction);

        void SetBeforeCommandAction(Action<ISession> beforeCommandAction);

        bool IsConnected();

        ParameterDictionary Context { get; }

        IDictionary<string, object> Properties { get; }

        event EventHandler Disconnect;
    }
}
