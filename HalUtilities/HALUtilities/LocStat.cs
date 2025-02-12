namespace HALUtilities
{
  internal class LocStat
  {
    internal int Deck { get; private set; }

    internal int Slot { get; private set; }

    internal int Count { get; private set; }

    internal void Increment() => ++this.Count;

    internal LocStat(int deck, int slot)
    {
      this.Deck = deck;
      this.Slot = slot;
    }
  }
}
