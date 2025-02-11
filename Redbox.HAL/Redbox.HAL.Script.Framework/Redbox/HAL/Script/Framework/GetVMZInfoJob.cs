using System;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "vmz-merch-summary", Operand = "VMZ-MERCH-SUMMARY")]
    internal sealed class GetVMZInfoJob : NativeJobAdapter
    {
        internal GetVMZInfoJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            foreach (var flags in SegmentManager.Instance.DecreasingPriorityOrder)
            {
                var merchSegment = MerchSegmentFactory.Get(flags);
                var high = merchSegment.FindHigh();
                if (high == null)
                {
                    Context.CreateInfoResult(string.Format("No{0}s", flags.ToString()),
                        string.Format("There are no {0} in the VMZ.", flags.ToString()));
                }
                else
                {
                    Context.CreateInfoResult(string.Format("VMZ{0}Count", flags.ToString()),
                        merchSegment.ItemCount().ToString());
                    Context.CreateResult(string.Format("Last{0}Info", flags.ToString()),
                        string.Format("The high {0} was found.", flags.ToString()), high.Deck, high.Slot, high.ID,
                        new DateTime?(), null);
                }
            }

            Context.CreateInfoResult("DumpBinInventoryCount", DumpbinService.CurrentCount().ToString());
        }
    }
}