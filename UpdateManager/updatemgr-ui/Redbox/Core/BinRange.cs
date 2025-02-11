namespace Redbox.Core
{
    internal class BinRange
    {
        public BinRange()
        {
        }

        public BinRange(int startingRange, int endingRange)
        {
            this.StartingRange = startingRange;
            this.EndingRange = endingRange;
        }

        public bool Between(int bin) => bin >= this.StartingRange && bin <= this.EndingRange;

        public int StartingRange { get; private set; }

        public int EndingRange { get; private set; }
    }
}
