using System;
using System.Collections;

namespace Redbox.Controls.Utilities
{
    public class EmptyEnumerator : IEnumerator
    {
        private static IEnumerator _instance;

        private static IEnumerator GetInstance()
        {
            return _instance == null ? _instance = (IEnumerator)new EmptyEnumerator() : _instance;
        }

        public static IEnumerator Instance => GetInstance();

        private EmptyEnumerator()
        {
        }

        public void Reset()
        {
        }

        public bool MoveNext()
        {
            return false;
        }

        public object Current => throw new InvalidOperationException();
    }
}