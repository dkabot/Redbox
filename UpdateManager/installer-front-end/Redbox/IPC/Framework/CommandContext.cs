using Redbox.Core;
using System;

namespace Redbox.IPC.Framework
{
    internal class CommandContext
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
                if (this.m_parameters == null)
                    this.m_parameters = new ParameterDictionary();
                return this.m_parameters;
            }
        }

        public MessageList Messages
        {
            get
            {
                if (this.m_messages == null)
                    this.m_messages = new MessageList();
                return this.m_messages;
            }
        }

        public ErrorList Errors
        {
            get
            {
                if (this.m_errors == null)
                    this.m_errors = new ErrorList();
                return this.m_errors;
            }
        }

        internal static bool IsSymbol(string value)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(":") == -1;
        }

        internal void ForEachSymbolDo(Action<string> action)
        {
            this.ForEachSymbolDo(action, (string[])null);
        }

        internal void ForEachNamedParameterDo(Action<string> action)
        {
            this.ForEachNamedParameterDo(action, (string[])null);
        }

        internal void ForEachSymbolDo(Action<string> action, string[] exclusions)
        {
            foreach (string key in this.Parameters.Keys)
            {
                if (CommandContext.IsSymbol(key))
                {
                    string tempKey = key;
                    if (exclusions == null || !Array.Exists<string>(exclusions, (Predicate<string>)(eachExclusion => eachExclusion == tempKey)))
                        action(key);
                }
            }
        }

        internal void ForEachNamedParameterDo(Action<string> action, string[] exclusions)
        {
            foreach (string key in this.Parameters.Keys)
            {
                if (!CommandContext.IsSymbol(key))
                {
                    string tempKey = key;
                    if (exclusions == null || !Array.Exists<string>(exclusions, (Predicate<string>)(eachExclusion => eachExclusion == tempKey)))
                        action(key);
                }
            }
        }
    }
}
