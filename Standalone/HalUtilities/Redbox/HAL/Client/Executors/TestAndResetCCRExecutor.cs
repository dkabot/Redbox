namespace Redbox.HAL.Client.Executors
{
    public sealed class TestAndResetCCRExecutor : JobExecutor
    {
        public TestAndResetCCRExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "test-and-reset-ccr";
    }
}