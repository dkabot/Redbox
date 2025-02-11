using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework
{
    public class CommandContext
    {
        public readonly ErrorList Errors = new ErrorList();
        public readonly MessageList Messages = new MessageList();
        public readonly IDictionary<string, string> Parameters = new Dictionary<string, string>();

        public ISession Session { get; internal set; }

        public IMessageSink MessageSink { get; internal set; }

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