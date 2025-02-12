namespace Redbox.HAL.Client
{
    public sealed class PutDiskInPickerInBinJob : JobExecutor
    {
        public PutDiskInPickerInBinJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "put-disk-in-picker-in-bin";
    }
}