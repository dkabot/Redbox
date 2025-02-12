namespace Redbox.HAL.Client.Executors
{
  public sealed class OneDiskQuickTest : JobExecutor
  {
    private readonly int SourceDeck;
    private readonly int SourceSlot;
    private readonly int DeckCount;

    public OneDiskQuickTest(HardwareService service, int sourceDeck, int sourceSlot, int count)
      : base(service)
    {
      this.SourceDeck = sourceDeck;
      this.SourceSlot = sourceSlot;
      this.DeckCount = count;
    }

    protected override string JobName => "one-disk-quick-deck-test";

    protected override void SetupJob()
    {
      this.Job.Push((object) this.DeckCount);
      this.Job.Push((object) this.SourceSlot);
      this.Job.Push((object) this.SourceDeck);
    }
  }
}
