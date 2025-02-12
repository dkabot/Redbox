namespace Redbox.HAL.Client
{
    public sealed class VendDiskInPickerJob : JobExecutor
    {
        public VendDiskInPickerJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "ms-vend-disk-in-picker";
    }
}