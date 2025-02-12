namespace Redbox.HAL.Client
{
    public sealed class PutInEmptySlotResult : JobExecutor
    {
        public PutInEmptySlotResult(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "file-disk-in-picker";
    }
}