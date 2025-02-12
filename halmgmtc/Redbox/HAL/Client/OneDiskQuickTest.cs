namespace Redbox.HAL.Client
{
    public sealed class OneDiskQuickTest : JobExecutor
    {
        public OneDiskQuickTest(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "one-disk-quick-deck-test";
    }
}