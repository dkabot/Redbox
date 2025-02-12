namespace Redbox.HAL.Client.Executors
{
    public sealed class ReturnExecutor : JobExecutor
    {
        public ReturnExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "return";
    }
}