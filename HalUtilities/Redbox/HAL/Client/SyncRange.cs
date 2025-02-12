namespace Redbox.HAL.Client
{
  public sealed class SyncRange
  {
    public SyncRange()
    {
    }

    public SyncRange(int startDeck, int endDeck, SlotRange slots)
    {
      this.StartDeck = startDeck;
      this.EndDeck = endDeck;
      this.Slots = slots;
    }

    public int EndDeck { get; set; }

    public SlotRange Slots { get; set; }

    public int StartDeck { get; set; }
  }
}
