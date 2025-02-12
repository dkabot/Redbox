namespace Redbox.HAL.Client.Executors
{
    public sealed class PutInEmptySlotResult : JobExecutor
    {
        public PutInEmptySlotResult(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "file-disk-in-picker";
    }
}