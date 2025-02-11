namespace Redbox.Core
{
    public class Pair<T1, T2>
    {
        public Pair()
        {
        }

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public T1 First { set; get; }

        public T2 Second { set; get; }

        public override string ToString()
        {
            return string.Format("Pair - First: {0}, Second: {1}", First, Second);
        }
    }
}