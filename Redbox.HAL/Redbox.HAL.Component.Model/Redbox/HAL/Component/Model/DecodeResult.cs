namespace Redbox.HAL.Component.Model
{
    public sealed class DecodeResult : IDecodeResult
    {
        public DecodeResult(string matrix)
        {
            Matrix = matrix;
        }

        public string Matrix { get; }

        public int Count { get; private set; } = 1;

        public void IncrementCount()
        {
            ++Count;
        }
    }
}