using Redbox.Core;
using System;
using System.Collections;

namespace Redbox.IPC.Framework
{
    [Command("macro", Filter = "System|Macro")]
    internal class MacroCommand
    {
        [Usage("MACRO list")]
        [CommandForm(Name = "list")]
        public void List(CommandContext context)
        {
            foreach (string key in (IEnumerable)CommandService.Instance.Properties.Keys)
            {
                string str = string.Format("{0} expands to '{1}'.", (object)key, (object)CommandService.Instance.Properties[key]);
                context.Messages.Add(str);
            }
        }

        [CommandForm(Name = "create")]
        [Usage("MACRO create symbol value: 'expansion'")]
        public void Create(CommandContext context, [CommandKeyValue(IsRequired = true)] string value)
        {
            context.ForEachSymbolDo((Action<string>)(each => CommandService.Instance.Properties[each] = value), new string[3]
            {
        "list",
        "create",
        "delete"
            });
        }

        [CommandForm(Name = "delete")]
        [Usage("MACRO delete symbol ...")]
        public void Delete(CommandContext context)
        {
            context.ForEachSymbolDo((Action<string>)(each => CommandService.Instance.Properties.Remove(each)), new string[3]
            {
        "list",
        "create",
        "delete"
            });
        }
    }
}
