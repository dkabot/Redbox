using System;
using System.ComponentModel;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("help")]
[Description(
    "The HELP command, with no parameters, returns a list of all available commands. The HELP command with a single parameter matching the name of another command will display the help specifically for that command, if available.")]
public class HelpCommand
{
    public void Default(CommandContext context)
    {
        if (context.Parameters.Count == 0)
            foreach (var allCommand in CommandRepository.AllCommands)
                context.Messages.Add(allCommand.ToUpper());
        else
            context.ForEachSymbolDo(each =>
            {
                var command = CommandRepository.GetCommand(each);
                if (command == null)
                    return;
                if (context.Messages.Count > 0)
                    context.Messages.Add(Environment.NewLine);
                if (command.CommandDescription != null)
                    context.Messages.Add(command.CommandDescription);
                foreach (var key in command.FormMethodCache.Keys)
                {
                    var usage = command.FormMethodCache[key].Usage;
                    var description = command.FormMethodCache[key].Description;
                    if (!string.IsNullOrEmpty(usage))
                    {
                        context.Messages.Add(usage);
                        if (!string.IsNullOrEmpty(description))
                        {
                            context.Messages.Add(description);
                            context.Messages.Add(Environment.NewLine);
                        }
                    }
                }
            });
    }
}