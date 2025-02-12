namespace Redbox.HAL.Client.Executors
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

        protected override void SetupJob()
        {
            Job.Push(Center.ToString(), Deck, Slot);
        }
    }
}