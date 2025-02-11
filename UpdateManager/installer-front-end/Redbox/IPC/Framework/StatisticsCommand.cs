using Redbox.Core;
using System.Reflection;

namespace Redbox.IPC.Framework
{
    [Command("statistics", Filter = "System|Statistics")]
    internal class StatisticsCommand
    {
        [CommandForm(Name = "show")]
        [Usage("STATISTICS show")]
        public void Show(CommandContext context)
        {
            foreach (FieldInfo field in typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                string str = string.Format("{0} = {1}", (object)field.Name, field.GetValue((object)Statistics.Instance));
                context.Messages.Add(str);
            }
            foreach (PropertyInfo property in typeof(Statistics).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                string str = string.Format("{0} = {1}", (object)property.Name, property.GetValue((object)Statistics.Instance, (object[])null));
                context.Messages.Add(str);
            }
        }
    }
}
