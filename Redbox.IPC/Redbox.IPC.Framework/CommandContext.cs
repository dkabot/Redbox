using System;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public class CommandContext
    {
        private ErrorList m_errors;
        private MessageList m_messages;
        private ParameterDictionary m_parameters;

        public ISession Session { get; internal set; }

        public IMessageSink MessageSink { get; internal set; }

        public ParameterDictionary Parameters
        {
            get
            {
                if (m_parameters == null)
                    m_parameters = new ParameterDictionary();
                return m_parameters;
            }
        }

        public MessageList Messages
        {
            get
            {
                if (m_messages == null)
                    m_messages = new MessageList();
                return m_messages;
            }
        }

        public ErrorList Errors
        {
            get
            {
                if (m_errors == null)
                    m_errors = new ErrorList();
                return m_errors;
            }
        }

        internal static bool IsSymbol(string value)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(":") == -1;
        }

        internal void ForEachSymbolDo(Action<string> action)
        {
            ForEachSymbolDo(action, null);
        }

        internal void ForEachNamedParameterDo(Action<string> action)
        {
            ForEachNamedParameterDo(action, null);
        }

        internal void ForEachSymbolDo(Action<string> action, string[] exclusions)
        {
            foreach (var key in Parameters.Keys)
                if (IsSymbol(key))
                {
                    var tempKey = key;
                    if (exclusions == null || !Array.Exists(exclusions, eachExclusion => eachExclusion == tempKey))
                        action(key);
                }
        }

        internal void ForEachNamedParameterDo(Action<string> action, string[] exclusions)
        {
            foreach (var key in Parameters.Keys)
                if (!IsSymbol(key))
                {
                    var tempKey = key;
                    if (exclusions == null || !Array.Exists(exclusions, eachExclusion => eachExclusion == tempKey))
                        action(key);
                }
        }
    }
}