namespace Redbox.HAL.Client
{
    public sealed class VerticalSync : JobExecutor
    {
        private readonly int Slot;

        public VerticalSync(HardwareService service, int slot)
            : base(service)
        {
            Slot = slot;
        }

        protected override string JobName => "vertical-sync";

        protected override void SetupJob(HardwareJob job)
        {
            job.Push(Slot);
        }
    }
}