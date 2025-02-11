using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class Quadrant : IQuadrant
    {
        internal Quadrant(int offset)
            : this(offset, null)
        {
        }

        internal Quadrant(int offset, IRange<int> slots)
        {
            Offset = offset;
            Slots = slots;
            IsExcluded = false;
        }

        public int Offset { get; }

        public IRange<int> Slots { get; }

        public bool IsExcluded { get; internal set; }
    }
}