namespace Redbox.HAL.Client.Executors
{
    public sealed class ReturnUnknownExecutor : JobExecutor
    {
        public ReturnUnknownExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "return-unknown";
    }
}