namespace Redbox.HAL.Client
{
    public sealed class QuickDeckTest : JobExecutor
    {
        public QuickDeckTest(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "quick-deck-test";
    }
}