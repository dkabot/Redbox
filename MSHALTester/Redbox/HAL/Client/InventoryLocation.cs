using System;

namespace Redbox.HAL.Client;

internal sealed class InventoryLocation : IInventoryLocation
{
    internal InventoryLocation(
        int deck,
        int slot,
        string matrix,
        int es,
        DateTime? returnTime,
        string merch,
        bool excluded)
    {
        Location = new Location
        {
            Deck = deck,
            Slot = slot
        };
        Matrix = matrix;
        EmptyStuck = es;
        ReturnTime = returnTime;
        MerchMetadata = merch;
        Excluded = excluded;
    }

    public Location Location { get; }

    public string Matrix { get; }

    public int EmptyStuck { get; }

    public string MerchMetadata { get; }

    public DateTime? ReturnTime { get; }

    public bool Excluded { get; }
}