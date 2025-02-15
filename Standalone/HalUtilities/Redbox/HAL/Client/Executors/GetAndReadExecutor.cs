namespace Redbox.HAL.Client.Executors
{
    public sealed class GetAndReadExecutor : JobExecutor
    {
        private readonly bool Center;
        private readonly int Deck;
        private readonly int Slot;

        public GetAndReadExecutor(HardwareService service, int deck, int slot, bool center)
            : base(service)
        {
            Deck = deck;
            Slot = slot;
            Center = center;
        }

        protected override string JobName => "get-and-read";

        protected override void SetupJob()
        {
            Job.Push(Center.ToString(), Deck, Slot);
        }
    }
}