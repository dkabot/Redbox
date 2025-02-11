namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "reset-fraud-sensor", Operand = "RESET-FRAUD-SENSOR")]
    internal sealed class ResetFraudSensorJob : NativeJobAdapter
    {
        internal ResetFraudSensorJob(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        protected override void ExecuteInner()
        {
            ApplicationLog.ConfigureLog(Context, true, "Fraud", true, "reset-fraud-sensor");
            Context.CreateInfoResult("FraudSensorNotConfigured", "The fraud sensor is not configured.");
            AddError("Failed to reset fraud sensor.");
        }
    }
}