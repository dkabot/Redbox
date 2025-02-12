namespace Redbox.HAL.Client
{
    public sealed class PushInDvdJob : JobExecutor
    {
        public PushInDvdJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "push-in-dvd";
    }
}