using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-motioncontrol", Operand = "RESET-MOTIONCONTROL")]
    internal sealed class ResetArcusJob : NativeJobAdapter
    {
        internal ResetArcusJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var errorCodes = MotionService.Reset(false);
            if (errorCodes != ErrorCodes.Success)
            {
                Context.CreateInfoResult("ResetFailure",
                    string.Format("RESET of the controller failed with error {0}.", errorCodes));
                AddError("Reset failure.");
            }
            else
            {
                Context.CreateInfoResult("ResetSuccess", "RESET OK.");
            }
        }
    }
}