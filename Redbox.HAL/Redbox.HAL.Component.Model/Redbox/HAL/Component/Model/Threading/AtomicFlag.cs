using System.Threading;

namespace Redbox.HAL.Component.Model.Threading
{
    public sealed class AtomicFlag
    {
        private const int DefaultFlagged = 1;
        private const int DefaultUnflagged = 0;
        private readonly int Flagged;
        private readonly int Unflagged;
        private int m_flagState;

        public AtomicFlag(bool initial)
            : this(initial, 1, 0)
        {
        }

        public AtomicFlag()
            : this(false, 1, 0)
        {
        }

        public AtomicFlag(bool initial, int flagged, int unflagged)
        {
            Flagged = flagged;
            Unflagged = unflagged;
            Interlocked.Exchange(ref m_flagState, initial ? Flagged : Unflagged);
        }

        public bool IsSet => Thread.VolatileRead(ref m_flagState) == Flagged;

        public bool Set()
        {
            return Interlocked.CompareExchange(ref m_flagState, Flagged, Unflagged) == Unflagged;
        }

        public bool Clear()
        {
            return Interlocked.CompareExchange(ref m_flagState, Unflagged, Flagged) == Flagged;
        }
    }
}