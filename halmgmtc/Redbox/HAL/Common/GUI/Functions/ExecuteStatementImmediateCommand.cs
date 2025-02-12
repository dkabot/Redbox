namespace Redbox.HAL.Common.GUI.Functions
{
    public class ExecuteStatementImmediateCommand : ImmediateCommand
    {
        protected override void OnExecute(ImmediateCommandResult result)
        {
            if (Arguments.Count == 0 || string.IsNullOrEmpty(Arguments[0]))
                return;
            result.CommandResult = Service.ExecuteImmediate(Arguments[0], 120000, out var _);
        }
    }
}