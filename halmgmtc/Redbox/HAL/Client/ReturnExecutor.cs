namespace Redbox.HAL.Client
{
    public sealed class ReturnExecutor : JobExecutor
    {
        public ReturnExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "return";
    }
}