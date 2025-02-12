namespace Redbox.HAL.Client.Executors
{
    public sealed class MSPullInDvdJob : JobExecutor
    {
        public MSPullInDvdJob(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "ms-pull-in-dvd";
    }
}