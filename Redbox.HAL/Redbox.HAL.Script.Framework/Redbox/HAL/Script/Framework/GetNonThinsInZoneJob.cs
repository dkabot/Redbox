using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-nonthins-in-vmz", Operand = "GET-NONTHINS-IN-VMZ")]
    internal sealed class GetNonThinsInZoneJob : NativeJobAdapter
    {
        internal GetNonThinsInZoneJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var inCompressedZone = SegmentManager.Instance.GetNonThinItemsInCompressedZone();
            using (new DisposeableList<ILocation>(inCompressedZone))
            {
                foreach (var location in inCompressedZone)
                    Context.CreateResult("NonThinInVMZ", "The item is a non-thin in the VMZ.", location.Deck,
                        location.Slot, location.ID, new DateTime?(), null);
                Context.CreateInfoResult("NonThinInVMZCount", inCompressedZone.Count.ToString());
            }
        }
    }
}