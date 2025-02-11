using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-excluded-locations", Operand = "GET-EXCLUDED-LOCS")]
    internal sealed class GetExcludedLocationsJob : NativeJobAdapter
    {
        internal GetExcludedLocationsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var excludedSlots = InventoryService.GetExcludedSlots();
            using (new DisposeableList<ILocation>(excludedSlots))
            {
                if (excludedSlots.Count == 0)
                    Context.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
                else
                    excludedSlots.ForEach(loc => Context.CreateResult("ExcludedSlot", "The location is excluded.",
                        loc.Deck, loc.Slot, null, new DateTime?(), null));
            }
        }
    }
}