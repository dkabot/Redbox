using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class CleanManager
    {
        private CleanManager()
        {
        }

        internal static CleanManager Instance => Singleton<CleanManager>.Instance;

        internal void CleanSlot(
            ref CleanResult result,
            ExecutionContext context,
            SegmentManager manager)
        {
            var merchTransferManager = new MerchTransferManager(context, ThinHelper.Debug);
            try
            {
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                using (var iterator = new VMZIterator())
                {
                    var appLog = context.AppLog;
                    do
                    {
                        if (iterator.IsEmpty)
                        {
                            if (!CompressToEmpty(iterator, manager, merchTransferManager, ref result))
                                goto label_20;
                        }
                        else if (service.IsStuck(iterator.Location))
                        {
                            LogHelper.Instance.WithContext("CleanManager: don't process location {0} stuckFlags {1}",
                                iterator.Location.ToString(), iterator.Location.StuckCount.ToString());
                        }
                        else if (manager.IsNonThinType(iterator.Flags))
                        {
                            var result1 = manager.MoveNonThin(iterator.Location, iterator.Location,
                                merchTransferManager);
                            if (result1 == MerchandizeResult.Success)
                            {
                                CompressToEmpty(iterator, manager, merchTransferManager, ref result);
                                return;
                            }

                            if (MerchandizeResult.MachineFull == result1)
                            {
                                result.VisitedAllSlots = true;
                                return;
                            }

                            if (MerchandizeResult.EmptyStuck != result1)
                            {
                                result.Process(result1);
                                return;
                            }
                        }
                    } while (iterator.Up());

                    goto label_15;
                    label_20:
                    return;
                    label_15:
                    result.VisitedAllSlots = true;
                }
            }
            finally
            {
                result.Accumulate(merchTransferManager);
            }
        }

        private bool CompressToEmpty(
            VMZIterator iterator,
            SegmentManager manager,
            MerchTransferManager xferMgr,
            ref CleanResult result)
        {
            var containingOrHigher = manager.FindContainingOrHigher(iterator);
            if (containingOrHigher == null)
            {
                if (ThinHelper.Debug)
                    LogHelper.Instance.Log("[CleanSlot] Did not find an enclosing segment for location {0}",
                        iterator.Location.ToString());
                return true;
            }

            if (ThinHelper.Debug)
                LogHelper.Instance.Log("[CleanSlot] Found enclosing segment [flags = {0}] for location {1}",
                    containingOrHigher.Flags.ToString(), iterator.Location.ToString());
            var high = containingOrHigher.FindHigh();
            if (ServiceLocator.Instance.GetService<IInventoryService>().IsStuck(high))
                return true;
            var result1 = manager.MoveThin(high, iterator.Location, xferMgr);
            result.Process(result1);
            return MerchandizeResult.EmptyStuck == result1;
        }
    }
}