using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.Command
{
    [Redbox.IPC.Framework.Command("queue")]
    internal class QueueCommand
    {
        [CommandForm(Name = "enqueue")]
        [Usage("queue enqueue label: entry:")]
        public void Enqueue(CommandContext context, [CommandKeyValue(IsRequired = true)] string label, [CommandKeyValue(IsRequired = true)] string entry)
        {
            ServiceLocator.Instance.GetService<IQueueService>().Enqueue(label, (object)entry.ToObject<Dictionary<string, object>>());
        }

        [CommandForm(Name = "dequeue")]
        [Usage("queue dequeue label:")]
        public void Dequeue(CommandContext context, [CommandKeyValue(IsRequired = true)] string label)
        {
            ServiceLocator.Instance.GetService<IQueueService>().Dequeue(label);
        }

        [CommandForm(Name = "peek")]
        [Usage("queue peek label:")]
        public void Peek(CommandContext context, [CommandKeyValue(IsRequired = true)] string label)
        {
            string str = ServiceLocator.Instance.GetService<IQueueService>().PeekRaw(label);
            if (str == null)
                return;
            context.Messages.Add(str.Replace(System.Environment.NewLine, ""));
        }
    }
}
