namespace Redbox.HAL.Client.Services
{
  public class ExecuteServiceCommand : ImmediateCommand
  {
    protected override void OnExecute(ImmediateCommandResult result)
    {
      if (this.Arguments.Count == 0 || string.IsNullOrEmpty(this.Arguments[0]))
        return;
      result.CommandResult = ImmediateCommand.Service.ExecuteServiceCommand(this.Arguments[0]);
    }

    protected internal override string Token => "%";

    protected override string Help => "Executes the given HAL Service command.";
  }
}
