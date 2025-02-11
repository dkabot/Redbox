using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal abstract class MerchSegment
    {
        protected MerchSegment(MerchFlags flags, bool debug)
        {
            Flags = flags;
            Debug = debug;
            if (!Debug)
                return;
            LogHelper.Instance.Log("Merchdata dump for [Context = {0}]", Flags.ToString());
            LogHelper.Instance.Log(" Lower priority: {0} Higher Priority = {1}", NextLower.ToString(),
                NextHigher.ToString());
        }

        internal virtual bool CanDump => false;

        internal MerchFlags Flags { get; }

        internal bool Debug { get; }

        protected internal virtual MerchFlags NextHigher => MerchFlags.None;

        protected internal virtual MerchFlags NextLower => MerchFlags.None;

        internal int ItemCount()
        {
            var high = FindHigh();
            var count = 0;
            if (high == null)
                return count;
            VMZ.Instance.ForAllBetweenDo(loc =>
            {
                if (loc.Flags == Flags)
                    ++count;
                return true;
            }, FindLow(), high);
            return count;
        }

        internal bool Contains(ILocation location)
        {
            var high = FindHigh();
            if (high == null)
                return false;
            var low = FindLow();
            var found = false;
            VMZ.Instance.ForAllBetweenDo(loc =>
            {
                if (!location.Equals(loc))
                    return true;
                found = true;
                return false;
            }, low, high);
            return found;
        }

        internal ILocation GetEmpty()
        {
            var empty = (ILocation)null;
            var high = FindHigh();
            if (high == null)
                return empty;
            VMZ.Instance.ForAllBetweenDo(loc =>
            {
                if (!loc.IsEmpty)
                    return true;
                empty = loc;
                return false;
            }, FindLow(), high);
            return empty;
        }

        internal ILocation FindHigh()
        {
            return VMZ.Instance.FindHighestLocation(Flags);
        }

        internal ILocation FindLow()
        {
            return VMZ.Instance.FindFirstLocation(Flags);
        }
    }
}