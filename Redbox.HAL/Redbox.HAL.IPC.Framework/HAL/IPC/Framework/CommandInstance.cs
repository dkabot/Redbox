using System;
using System.Collections.Generic;

namespace Redbox.HAL.IPC.Framework
{
    internal class CommandInstance
    {
        public readonly IDictionary<string, FormMethod> FormMethodCache = new Dictionary<string, FormMethod>();

        public Type CommandType { get; set; }

        public string CommandDescription { get; set; }

        public object GetInstance()
        {
            return Activator.CreateInstance(CommandType);
        }

        public bool HasDefault()
        {
            return FormMethodCache.ContainsKey("Default");
        }

        public bool HasOnlyDefault()
        {
            return FormMethodCache.Count == 1 && HasDefault();
        }

        public void InvokeDefault(
            CommandResult result,
            CommandContext context,
            CommandTokenizer tokenizer)
        {
            if (!HasDefault())
                return;
            FormMethodCache["Default"].Invoke(result, context, tokenizer, GetInstance());
        }

        public FormMethod GetMethod(string formName)
        {
            if (string.IsNullOrEmpty(formName))
                return null;
            var upper = formName.ToUpper();
            return FormMethodCache.ContainsKey(upper) ? FormMethodCache[upper] : null;
        }
    }
}