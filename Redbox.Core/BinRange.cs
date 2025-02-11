namespace Redbox.Core
{
    public class BinRange
    {
        public BinRange()
        {
        }

        public BinRange(int startingRange, int endingRange)
        {
            StartingRange = startingRange;
            EndingRange = endingRange;
        }

        public int StartingRange { get; }

        public int EndingRange { get; }

        public bool Between(int bin)
        {
            return bin >= StartingRange && bin <= EndingRange;
        }
    }
}