namespace Redbox.HAL.Client
{
    public sealed class GetAndCenterResult : JobExecutor
    {
        private readonly bool Center;
        private readonly int Deck;
        private readonly int Slot;

        public GetAndCenterResult(HardwareService service, int deck, int slot, bool center)
            : base(service)
        {
            Deck = deck;
            Slot = slot;
            Center = center;
        }

        protected override string JobName => "get-and-center";

        protected override void SetupJob(HardwareJob job)
        {
            job.Push(Center.ToString(), Deck, Slot);
        }
    }
}