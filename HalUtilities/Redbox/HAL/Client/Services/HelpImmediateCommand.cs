using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Client.Services
{
  public class HelpImmediateCommand : ImmediateCommand
  {
    protected override void OnExecute(ImmediateCommandResult result)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (ImmediateCommand immediateCommand in (IEnumerable<ImmediateCommand>) ImmediateCommand.Commands.Values)
      {
        if (!string.IsNullOrEmpty(immediateCommand.Token))
          stringBuilder.AppendLine(immediateCommand.ToString());
      }
      result.Message = stringBuilder.ToString();
    }

    protected internal override string Token => "?";

    protected override string Help
    {
      get => "Provides a list of immediate commands and what active they perform.";
    }
  }
}
