using System.Collections;

namespace Redbox.Controls.Utilities
{
    public class SingleChildEnumerator : IEnumerator
    {
        private int _index = -1;
        private int _count;
        private object _child;

        public SingleChildEnumerator(object Child)
        {
            _child = Child;
            _count = Child != null ? 1 : 0;
        }

        object IEnumerator.Current => _index == 0 ? _child : (object)null;

        bool IEnumerator.MoveNext()
        {
            ++_index;
            return _index < _count;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }
}