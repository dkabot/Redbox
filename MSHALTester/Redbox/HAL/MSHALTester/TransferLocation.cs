namespace Redbox.HAL.MSHALTester;

internal sealed class TransferLocation
{
    internal readonly int Deck;
    internal readonly int Slot;

    internal TransferLocation(int deck, int slot)
    {
        Deck = deck;
        Slot = slot;
    }

    internal bool IsValid => -1 != Deck && -1 != Slot;
}