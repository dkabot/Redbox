using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal class VMZIterator : IDisposable
    {
        private readonly VMZ VMZ;

        internal VMZIterator(ILocation location)
        {
            VMZ = VMZ.Instance;
            Location = location;
        }

        internal VMZIterator()
        {
            VMZ = VMZ.Instance;
            ResetToFirst();
        }

        internal ILocation Location { get; private set; }

        internal bool IsEmpty => Location.ID == "EMPTY";

        internal MerchFlags Flags => Location.Flags;

        public void Dispose()
        {
            Location = null;
        }

        public override string ToString()
        {
            return Location != null
                ? string.Format("{0} Flags = {1}", Location, Location.Flags.ToString())
                : "NONE";
        }

        internal void ResetToFirst()
        {
            ResetTo(VMZ.First);
        }

        internal void ResetTo(ILocation location)
        {
            Location = location;
        }

        internal bool Up()
        {
            Location = VMZ.ComputeNextHigherLocation(Location);
            return Location != null;
        }

        internal void Invalidate()
        {
            Location = null;
        }

        internal IteratorComareResult RelativeTo(ILocation reference)
        {
            return VMZ.RelativeTo(Location, reference);
        }
    }
}