namespace Redbox.HAL.Client.Executors
{
    public sealed class AdjacentBinTest : JobExecutor
    {
        public AdjacentBinTest(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "adjacent-bin-test";
    }
}