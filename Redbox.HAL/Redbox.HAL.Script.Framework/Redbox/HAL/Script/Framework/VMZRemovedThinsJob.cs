using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "vmz-removed-thins", Operand = "VMZ-REMOVED-THINS")]
    internal sealed class VMZRemovedThinsJob : NativeJobAdapter
    {
        internal VMZRemovedThinsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var log = ApplicationLog.ConfigureLog(Context, true, "VMZ", false, "vmz-removed-disks");
            log.Write("** Remove thins from VMZ **");
            VMZ.Instance.DumpZone(log);
            var totalEmptyNow = ServiceLocator.Instance.GetService<IInventoryService>().GetMachineEmptyCount();
            var highest = SegmentManager.Instance.FindHighest();
            if (highest != null)
                VMZ.Instance.ForAllBetweenDo(loc =>
                {
                    if (loc.IsEmpty)
                        return true;
                    if (SegmentManager.Instance.IsThinType(loc.Flags))
                    {
                        Context.CreateResult("ThinTransferSuccessful", "The disk was removed from the VMZ.", loc.Deck,
                            loc.Slot, loc.ID, new DateTime?(), null);
                        Context.CreateResult(string.Format("{0}DiskRemovedFromVMZ", loc.Flags.ToString()),
                            "Disk merchandizing metadata.", loc.Deck, loc.Slot, loc.ID, new DateTime?(), null);
                    }
                    else
                    {
                        Context.CreateResult("NonThinRemoved", "The disk was removed from the VMZ.", loc.Deck, loc.Slot,
                            loc.ID, new DateTime?(), null);
                    }

                    ++totalEmptyNow;
                    return true;
                }, highest);
            Context.CreateInfoResult("MachineEmptySlots", totalEmptyNow.ToString());
        }
    }
}