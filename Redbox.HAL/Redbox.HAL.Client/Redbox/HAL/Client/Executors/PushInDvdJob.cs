namespace Redbox.HAL.Client.Executors
{
    public sealed class PushInDvdJob : JobExecutor
    {
        public PushInDvdJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "push-in-dvd";
    }
}