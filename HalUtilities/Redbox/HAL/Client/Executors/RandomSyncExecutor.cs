namespace Redbox.HAL.Client.Executors
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

        protected override void SetupJob()
        {
            Job.Push(VendTime);
            Job.Push(VendFrequency);
        }
    }
}