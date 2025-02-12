namespace Redbox.HAL.Client
{
    public sealed class LogTimeoutsExecutor : JobExecutor
    {
        private readonly string Path;

        public LogTimeoutsExecutor(HardwareService service, string path)
            : base(service)
        {
            Path = path;
        }

        protected override string JobName => "log-persistent-counters";

        protected override string Label => "Tester Counters";

        protected override void SetupJob(HardwareJob job)
        {
            job.Push(Path);
        }
    }
}