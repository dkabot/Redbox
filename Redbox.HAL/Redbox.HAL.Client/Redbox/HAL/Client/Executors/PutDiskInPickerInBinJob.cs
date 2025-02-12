namespace Redbox.HAL.Client.Executors
{
    public sealed class PutDiskInPickerInBinJob : JobExecutor
    {
        public PutDiskInPickerInBinJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "put-disk-in-picker-in-bin";
    }
}