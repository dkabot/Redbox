namespace HALUtilities
{
    internal class TypeStat
    {
        internal TypeStat(StuckType t)
        {
            Type = t;
        }

        internal int Count { get; private set; }

        internal StuckType Type { get; private set; }

        internal void Increment()
        {
            ++Count;
        }
    }
}