namespace Redbox.HAL.Client
{
    public sealed class RandomSyncExecutor : JobExecutor
    {
        private readonly int VendFrequency;
        private readonly int VendTime;

        public RandomSyncExecutor(HardwareService service, int vendTime, int frequency)
            : base(service)
        {
            VendTime = vendTime;
            VendFrequency = frequency;
        }

        protected override string JobName => "random-sync";

        protected override void SetupJob(HardwareJob job)
        {
            job.Push(VendTime);
            job.Push(VendFrequency);
        }
    }
}