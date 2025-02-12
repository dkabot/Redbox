using System;

namespace Redbox.HAL.Client
{
  internal sealed class InventoryLocation : IInventoryLocation
  {
    public Location Location { get; private set; }

    public string Matrix { get; private set; }

    public int EmptyStuck { get; private set; }

    public string MerchMetadata { get; private set; }

    public DateTime? ReturnTime { get; private set; }

    public bool Excluded { get; private set; }

    internal InventoryLocation(
      int deck,
      int slot,
      string matrix,
      int es,
      DateTime? returnTime,
      string merch,
      bool excluded)
    {
      this.Location = new Location()
      {
        Deck = deck,
        Slot = slot
      };
      this.Matrix = matrix;
      this.EmptyStuck = es;
      this.ReturnTime = returnTime;
      this.MerchMetadata = merch;
      this.Excluded = excluded;
    }
  }
}
