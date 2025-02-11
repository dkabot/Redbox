using System;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-barcode-vmz-location", Operand = "GET-BARCODE-VMZ-LOCATION")]
    internal sealed class GetDiskLocationJob : NativeJobAdapter
    {
        internal GetDiskLocationJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var num = Context.PopTop<int>();
            for (var index = 0; index < num; ++index)
            {
                var id = Context.PopTop<string>();
                var target = InventoryService.Lookup(id);
                if (target == null)
                    Context.CreateInfoResult("BarcodeNotInInventory", "The barcode isn't in inventory.");
                else
                    Context.CreateResult(
                        SegmentManager.Instance.InCompressedZone(target) ? "BarcodeInVMZ" : "BarcodeOutsideVMZ",
                        "Barcode status", target.Deck, target.Slot, id, new DateTime?(), null);
            }
        }
    }
}