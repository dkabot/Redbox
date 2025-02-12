namespace HALUtilities
{
    internal class LocStat
    {
        internal LocStat(int deck, int slot)
        {
            Deck = deck;
            Slot = slot;
        }

        internal int Deck { get; private set; }

        internal int Slot { get; private set; }

        internal int Count { get; private set; }

        internal void Increment()
        {
            ++Count;
        }
    }
}