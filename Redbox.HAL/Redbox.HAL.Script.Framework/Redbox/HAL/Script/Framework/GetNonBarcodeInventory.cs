using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-non-barcode-inventory")]
    internal sealed class GetNonBarcodeInventory : NativeJobAdapter
    {
        internal GetNonBarcodeInventory(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var unknowns = InventoryService.GetUnknowns();
            using (new DisposeableList<ILocation>(unknowns))
            {
                unknowns.ForEach(each => Context.CreateResult("UnknownLocation", "The location is UNKNOWN.", each.Deck,
                    each.Slot, "UNKNOWN", new DateTime?(), null));
            }

            var emptySlots = InventoryService.GetEmptySlots();
            using (new DisposeableList<ILocation>(emptySlots))
            {
                emptySlots.ForEach(each => Context.CreateResult("EmptyLocation", "The location is EMPTY.", each.Deck,
                    each.Slot, "EMPTY", new DateTime?(), null));
            }
        }
    }
}