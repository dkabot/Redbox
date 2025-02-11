using System.Reflection;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    [Command("statistics", Filter = "System|Statistics")]
    public class StatisticsCommand
    {
        [CommandForm(Name = "show")]
        [Usage("STATISTICS show")]
        public void Show(CommandContext context)
        {
            foreach (var field in typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var str = string.Format("{0} = {1}", field.Name, field.GetValue(Statistics.Instance));
                context.Messages.Add(str);
            }

            foreach (var property in typeof(Statistics).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var str = string.Format("{0} = {1}", property.Name, property.GetValue(Statistics.Instance, null));
                context.Messages.Add(str);
            }
        }
    }
}