namespace Redbox.HAL.Client.Services
{
  public class ExecuteStatementImmediateCommand : ImmediateCommand
  {
    protected override void OnExecute(ImmediateCommandResult result)
    {
      if (this.Arguments.Count == 0 || string.IsNullOrEmpty(this.Arguments[0]))
        return;
      result.CommandResult = ImmediateCommand.Service.ExecuteImmediate(this.Arguments[0], new int?(120000), out HardwareJob _);
    }
  }
}
