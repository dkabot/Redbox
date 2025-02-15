namespace Redbox.HAL.Client.Executors
{
    public sealed class OneDiskQuickTest : JobExecutor
    {
        private readonly int DeckCount;
        private readonly int SourceDeck;
        private readonly int SourceSlot;

        public OneDiskQuickTest(HardwareService service, int sourceDeck, int sourceSlot, int count)
            : base(service)
        {
            SourceDeck = sourceDeck;
            SourceSlot = sourceSlot;
            DeckCount = count;
        }

        protected override string JobName => "one-disk-quick-deck-test";

        protected override void SetupJob()
        {
            Job.Push(DeckCount);
            Job.Push(SourceSlot);
            Job.Push(SourceDeck);
        }
    }
}