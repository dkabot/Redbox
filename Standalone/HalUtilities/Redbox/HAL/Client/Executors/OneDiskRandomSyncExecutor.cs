namespace Redbox.HAL.Client.Executors
{
    public sealed class OneDiskRandomSyncExecutor : JobExecutor
    {
        private readonly int VendFrequency;
        private readonly int VendTime;

        public OneDiskRandomSyncExecutor(HardwareService service, int vendTime, int frequency)
            : base(service)
        {
            VendTime = vendTime;
            VendFrequency = frequency;
        }

        protected override string JobName => "one-disk-random-sync";

        protected override void SetupJob()
        {
            Job.Push(VendTime);
            Job.Push(VendFrequency);
        }
    }
}