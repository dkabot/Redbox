using System.Reflection;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("statistics")]
public class StatisticsCommand
{
    [CommandForm(Name = "show")]
    [Usage("STATISTICS show")]
    public void Show(CommandContext context)
    {
        var bindingAttr = BindingFlags.Instance | BindingFlags.Public;
        foreach (var field in typeof(Statistics).GetFields(bindingAttr))
        {
            var str = string.Format("{0} = {1}", field.Name, field.GetValue(Statistics.Instance));
            context.Messages.Add(str);
        }

        foreach (var property in typeof(Statistics).GetProperties(bindingAttr))
        {
            var str = string.Format("{0} = {1}", property.Name, property.GetValue(Statistics.Instance, null));
            context.Messages.Add(str);
        }
    }
}