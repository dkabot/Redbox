namespace Redbox.HAL.Client.Executors
{
    public sealed class VendDiskInPickerJob : JobExecutor
    {
        public VendDiskInPickerJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "ms-vend-disk-in-picker";
    }
}