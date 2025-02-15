namespace Redbox.HAL.Client;

public sealed class SyncRange
{
    public SyncRange()
    {
    }

    public SyncRange(int startDeck, int endDeck, SlotRange slots)
    {
        StartDeck = startDeck;
        EndDeck = endDeck;
        Slots = slots;
    }

    public int EndDeck { get; set; }

    public SlotRange Slots { get; set; }

    public int StartDeck { get; set; }
}