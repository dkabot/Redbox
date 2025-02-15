namespace Redbox.HAL.Common.GUI.Functions
{
    public class ExecuteServiceCommand : ImmediateCommand
    {
        protected internal override string Token => "%";

        protected override string Help => "Executes the given HAL Service command.";

        protected override void OnExecute(ImmediateCommandResult result)
        {
            if (Arguments.Count == 0 || string.IsNullOrEmpty(Arguments[0]))
                return;
            result.CommandResult = Service.ExecuteServiceCommand(Arguments[0]);
        }
    }
}