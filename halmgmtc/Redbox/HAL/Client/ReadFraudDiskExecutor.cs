namespace Redbox.HAL.Client
{
    public sealed class ReadFraudDiskExecutor : JobExecutor
    {
        public ReadFraudDiskExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "read-fraud-disc";
    }
}