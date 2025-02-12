namespace Redbox.HAL.Client
{
    public sealed class AdjacentBinTest : JobExecutor
    {
        public AdjacentBinTest(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "adjacent-bin-test";
    }
}