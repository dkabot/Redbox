namespace Redbox.HAL.Client.Executors
{
    public sealed class FraudSensorPost : JobExecutor
    {
        public FraudSensorPost(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "fraud-sensor-post-test";
    }
}