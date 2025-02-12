using Redbox.HAL.Component.Model;
using System;

namespace HALUtilities.KioskTest
{
  internal sealed class TestLocation : ILocation
  {
    public bool IsEmpty { get; private set; }

    public bool IsWide { get; private set; }

    public int Deck { get; private set; }

    public int Slot { get; private set; }

    public string ID { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool Excluded { get; set; }

    public int StuckCount { get; set; }

    public MerchFlags Flags { get; set; }

    internal TestLocation(int deck, int slot)
    {
      this.Deck = deck;
      this.Slot = slot;
      this.ID = "EMPTY";
      this.StuckCount = 0;
      this.Excluded = false;
    }
  }
}
