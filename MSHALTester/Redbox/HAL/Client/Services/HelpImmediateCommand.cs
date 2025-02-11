using System.Text;

namespace Redbox.HAL.Client.Services;

public class HelpImmediateCommand : ImmediateCommand
{
    protected internal override string Token => "?";

    protected override string Help => "Provides a list of immediate commands and what active they perform.";

    protected override void OnExecute(ImmediateCommandResult result)
    {
        var stringBuilder = new StringBuilder();
        foreach (var immediateCommand in Commands.Values)
            if (!string.IsNullOrEmpty(immediateCommand.Token))
                stringBuilder.AppendLine(immediateCommand.ToString());
        result.Message = stringBuilder.ToString();
    }
}