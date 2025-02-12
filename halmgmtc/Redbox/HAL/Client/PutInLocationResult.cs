namespace Redbox.HAL.Client
{
    public sealed class PutInLocationResult : JobExecutor
    {
        private readonly int Deck;
        private readonly int Slot;

        public PutInLocationResult(HardwareService service, int deck, int slot)
            : base(service)
        {
            Deck = deck;
            Slot = slot;
        }

        protected override string JobName => "put-disk-to-location";

        protected override void SetupJob(HardwareJob job)
        {
            job.Push(Deck, Slot);
        }
    }
}