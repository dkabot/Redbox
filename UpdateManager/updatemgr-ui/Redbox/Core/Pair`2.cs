namespace Redbox.Core
{
    internal class Pair<T1, T2>
    {
        public Pair()
        {
        }

        public Pair(T1 first, T2 second)
        {
            this.First = first;
            this.Second = second;
        }

        public T1 First { set; get; }

        public T2 Second { set; get; }

        public override string ToString()
        {
            return string.Format("Pair - First: {0}, Second: {1}", (object)this.First, (object)this.Second);
        }
    }
}
