namespace Redbox.HAL.Client
{
    public sealed class ResetFraudSensorJob : JobExecutor
    {
        public ResetFraudSensorJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "reset-fraud-sensor";
    }
}