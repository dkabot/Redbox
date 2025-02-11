using System;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "vmz-details", Operand = "VMZ-DETAILS")]
    internal sealed class GetVMZDetailJob : NativeJobAdapter
    {
        internal GetVMZDetailJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            using (var vmzIterator = new VMZIterator())
            {
                do
                {
                    Context.CreateResult("VMZLocationDetail", vmzIterator.Flags.ToString(), vmzIterator.Location.Deck,
                        vmzIterator.Location.Slot, vmzIterator.Location.ID, new DateTime?(), null);
                } while (vmzIterator.Up());
            }

            Context.CreateInfoResult("DumpBinInventoryCount", DumpbinService.CurrentCount().ToString());
        }
    }
}