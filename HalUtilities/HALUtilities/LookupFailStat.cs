namespace HALUtilities
{
  internal class LookupFailStat
  {
    internal int Count { get; private set; }

    internal string Matrix { get; private set; }

    internal void Increment() => ++this.Count;

    internal LookupFailStat(string barcode) => this.Matrix = barcode;
  }
}
