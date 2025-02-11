using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class SegmentManager : IGetObserver
    {
        private readonly bool Debug;
        internal readonly ReadOnlyCollection<MerchFlags> DecreasingPriorityOrder;

        private readonly MerchFlags[] IncreasingPriorityFlags = new MerchFlags[3]
        {
            MerchFlags.Thin,
            MerchFlags.ThinRedeploy,
            MerchFlags.Rebalance
        };

        internal readonly ReadOnlyCollection<MerchFlags> IncreasingPriorityOrder;
        private readonly VMZ VMZ;

        private SegmentManager()
        {
            Debug = true;
            VMZ = VMZ.Instance;
            IncreasingPriorityOrder = Array.AsReadOnly(IncreasingPriorityFlags);
            var array = new MerchFlags[IncreasingPriorityFlags.Length];
            var index1 = IncreasingPriorityFlags.Length - 1;
            var index2 = 0;
            while (index1 >= 0)
            {
                array[index2] = IncreasingPriorityFlags[index1];
                --index1;
                ++index2;
            }

            DecreasingPriorityOrder = Array.AsReadOnly(array);
        }

        internal static SegmentManager Instance => Singleton<SegmentManager>.Instance;

        public void OnStuck(IGetResult result)
        {
            ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
        }

        public bool OnEmpty(IGetResult result)
        {
            result.Update(ErrorCodes.ItemStuck);
            ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
            return false;
        }

        internal bool IsThinType(MerchFlags flags)
        {
            return !IsNonThinType(flags);
        }

        internal bool IsNonThinType(MerchFlags flags)
        {
            return flags == MerchFlags.None || MerchFlags.Unload == flags;
        }

        internal List<ILocation> GetNonThinItemsInCompressedZone()
        {
            var rv = new List<ILocation>();
            var highest = FindHighest();
            if (highest == null)
                return rv;
            VMZ.Instance.ForAllBetweenDo(loc =>
            {
                if (loc.IsEmpty || !IsNonThinType(loc.Flags))
                    return true;
                rv.Add(loc);
                return true;
            }, highest);
            return rv;
        }

        internal bool InCompressedZone(ILocation target)
        {
            var highest = FindHighest();
            if (highest == null)
                return false;
            var found = false;
            VMZ.Instance.ForAllBetweenDo(loc =>
            {
                if (!loc.Equals(target))
                    return true;
                found = true;
                return false;
            }, highest);
            return found;
        }

        internal ILocation FindHighest()
        {
            foreach (var flags in IncreasingPriorityOrder)
            {
                var high = MerchSegmentFactory.Get(flags).FindHigh();
                if (high != null)
                    return high;
            }

            return null;
        }

        internal ILocation SelectLocation(
            MerchFlags flags,
            MerchTransferManager manager,
            out MerchandizeResult result)
        {
            result = MerchandizeResult.Success;
            var start = MerchSegmentFactory.Get(flags);
            var high = start.FindHigh();
            var location = FindLastHigherPriority(start);
            var flag = location != null;
            if (!flag)
                location = VMZ.Instance.First;
            var _mr = result;
            var target = (ILocation)null;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            VMZIterator iterator;
            if (high != null)
            {
                VMZ.ForAllBetweenDo(loc =>
                {
                    if (loc.IsEmpty)
                    {
                        target = loc;
                        return false;
                    }

                    if (IsThinType(loc.Flags) || service.IsStuck(loc))
                        return true;
                    var merchandizeResult = MoveNonThin(loc, manager);
                    if (merchandizeResult == MerchandizeResult.Success)
                    {
                        target = loc;
                        return false;
                    }

                    if (MerchandizeResult.EmptyStuck == merchandizeResult)
                        return true;
                    _mr = merchandizeResult;
                    return false;
                }, location, high);
                result = _mr;
                if (target != null)
                {
                    if (Debug)
                        LogHelper.Instance.Log("[SelectLocation] Found an interior location at {0} for flags {1}",
                            target.ToString(), flags.ToString());
                    return target;
                }

                iterator = new VMZIterator(high);
                if (!iterator.Up())
                {
                    result = MerchandizeResult.VMZFull;
                    return null;
                }
            }
            else
            {
                iterator = new VMZIterator(location);
                if (flag && !iterator.Up())
                {
                    result = MerchandizeResult.VMZFull;
                    return null;
                }
            }

            target = FindTarget(iterator, flags, manager, out result);
            if (target != null && Debug)
                LogHelper.Instance.Log("[SelectLocation] Found an interior location at {0} for flags {1}",
                    target.ToString(), flags.ToString());
            return target;
        }

        internal MerchSegment FindContainingOrHigher(VMZIterator iterator)
        {
            foreach (var flags in DecreasingPriorityOrder)
            {
                if (ThinHelper.Debug)
                    LogHelper.Instance.Log("[FindContainingOrHigher] Look at segment {0}", flags.ToString());
                var containingOrHigher = MerchSegmentFactory.Get(flags);
                if (containingOrHigher.ItemCount() != 0)
                {
                    if (containingOrHigher.Contains(iterator.Location))
                        return containingOrHigher;
                    var low = containingOrHigher.FindLow();
                    if (iterator.RelativeTo(low) == IteratorComareResult.Below)
                    {
                        if (ThinHelper.Debug)
                            LogHelper.Instance.Log("[FindContainingOrHigher] Iterator at {0} is below {1}",
                                iterator.Location.ToString(), low.ToString());
                        return containingOrHigher;
                    }
                }
            }

            return null;
        }

        internal MerchandizeResult MoveNonThin(
            ILocation source,
            ILocation outside,
            MerchTransferManager manager)
        {
            using (var emptyOutsideOf = ServiceLocator.Instance.GetService<IEmptySearchPatternService>()
                       .FindEmptyOutsideOf(outside))
            {
                if (emptyOutsideOf.FoundEmpty == 0)
                {
                    LogHelper.Instance.Log("[SegmentManager]: There are no empty slots available for transfer.");
                    return MerchandizeResult.MachineFull;
                }

                var flags = (long)source.Flags;
                var transferResult = ServiceLocator.Instance.GetService<IControllerService>()
                    .Transfer(source, emptyOutsideOf.EmptyLocations, this);
                if (transferResult.Transferred)
                {
                    manager.ProcessTransfer(source, transferResult.Destination);
                    return MerchandizeResult.Success;
                }

                switch (transferResult.TransferError)
                {
                    case ErrorCodes.SlotEmpty:
                    case ErrorCodes.ItemStuck:
                        return MerchandizeResult.EmptyStuck;
                    case ErrorCodes.SlotInUse:
                        return MerchandizeResult.TransferFailure;
                    default:
                        return MerchandizeResult.HardwareError;
                }
            }
        }

        internal MerchandizeResult MoveNonThin(ILocation source, MerchTransferManager manager)
        {
            var outside = FindHighest() ?? source;
            return MoveNonThin(source, outside, manager);
        }

        internal MerchandizeResult MoveThin(
            ILocation source,
            ILocation empty,
            MerchTransferManager mgr)
        {
            ServiceLocator.Instance.GetService<IInventoryService>();
            var xferResult = ServiceLocator.Instance.GetService<IControllerService>().Transfer(source, empty);
            if (xferResult.Transferred)
            {
                if (Debug)
                    LogHelper.Instance.Log("MerchMetadata: xfer of {0} [Flags = {1}] to {2} was successful.",
                        source.ToString(), xferResult.Destination.Flags.ToString(), empty.ToString());
            }
            else
            {
                LogHelper.Instance.Log("MerchMetadata: xfer of {0} [Flags = {1}] failed with error {2}",
                    source.ToString(), source.Flags.ToString(), xferResult.TransferError.ToString());
            }

            mgr.Process(xferResult);
            return MerchandizeResult.Success;
        }

        private int Compare(MerchFlags left, MerchFlags right)
        {
            return IncreasingPriorityOrder.IndexOf(left) - IncreasingPriorityOrder.IndexOf(right);
        }

        private ILocation FindTarget(
            VMZIterator iterator,
            MerchFlags flags,
            MerchTransferManager manager,
            out MerchandizeResult result)
        {
            result = MerchandizeResult.Success;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            while (!iterator.IsEmpty)
            {
                if (IsThinType(iterator.Location.Flags))
                {
                    if (Compare(iterator.Location.Flags, flags) < 0)
                    {
                        var empty = MerchSegmentFactory.Get(iterator.Location.Flags).GetEmpty();
                        if (empty != null)
                        {
                            result = MoveThin(iterator.Location, empty, manager);
                            if (result == MerchandizeResult.Success)
                                return iterator.Location;
                            if (MerchandizeResult.EmptyStuck != result)
                                return null;
                        }

                        result = MerchandizeResult.VMZFull;
                        goto label_18;
                    }
                }
                else if (!service.IsStuck(iterator.Location))
                {
                    var merchandizeResult = MoveNonThin(iterator.Location, manager);
                    if (merchandizeResult == MerchandizeResult.Success)
                        return iterator.Location;
                    if (MerchandizeResult.EmptyStuck != merchandizeResult)
                    {
                        result = merchandizeResult;
                        goto label_18;
                    }
                }

                if (!iterator.Up())
                    result = MerchandizeResult.VMZFull;
                else
                    continue;
                label_18:
                return null;
            }

            return iterator.Location;
        }

        private void AddTransfer(ITransferResult xferResult, ExecutionContext context)
        {
            if (!xferResult.Transferred)
                return;
            context.CreateResult("DiskTransferSuccessful",
                Instance.InCompressedZone(xferResult.Source)
                    ? "The item was moved internally in the VMZ."
                    : "Transferred disk from VMZ to drum.", xferResult.Source.Deck, xferResult.Source.Slot, "EMPTY",
                new DateTime?(), xferResult.Destination.ID);
            if (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(xferResult.Destination))
                context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.",
                    new int?(), new int?(), xferResult.Destination.ID, new DateTime?(), null);
            else
                context.CreateResult("DiskTransferSuccessful",
                    Instance.InCompressedZone(xferResult.Destination)
                        ? "The item was moved internally in the VMZ."
                        : "Transferred disk from VMZ to drum.", xferResult.Destination.Deck,
                    xferResult.Destination.Slot, xferResult.Destination.ID, new DateTime?(), "EMPTY");
        }

        private ILocation FindLastHigherPriority(MerchSegment start)
        {
            var nextHigher = start.NextHigher;
            ILocation high;
            while (true)
            {
                var merchSegment = MerchSegmentFactory.Get(nextHigher);
                if (merchSegment != null)
                {
                    high = merchSegment.FindHigh();
                    if (high == null)
                        nextHigher = merchSegment.NextHigher;
                    else
                        break;
                }
                else
                {
                    goto label_7;
                }
            }

            if (Debug)
                LogHelper.Instance.Log("FindLastHigher: [Context = {0}]: found higher priority [Context = {1}] at {2}",
                    start.Flags.ToString(), nextHigher.ToString(), high.ToString());
            return high;
            label_7:
            if (Debug)
                LogHelper.Instance.Log("FindLastHigherPriority: [Context = {0}]: no higher priority segments found.",
                    start.Flags.ToString());
            return null;
        }
    }
}