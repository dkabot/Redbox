namespace HALUtilities
{
    internal class LookupFailStat
    {
        internal LookupFailStat(string barcode)
        {
            Matrix = barcode;
        }

        internal int Count { get; private set; }

        internal string Matrix { get; private set; }

        internal void Increment()
        {
            ++Count;
        }
    }
}