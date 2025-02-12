namespace Redbox.HAL.Client
{
    public sealed class TakeDiskAtDoorJob : JobExecutor
    {
        public TakeDiskAtDoorJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "ms-take-disk-at-door";
    }
}