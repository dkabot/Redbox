namespace Redbox.HAL.Client.Executors
{
    public sealed class ResetFraudSensorJob : JobExecutor
    {
        public ResetFraudSensorJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "reset-fraud-sensor";
    }
}