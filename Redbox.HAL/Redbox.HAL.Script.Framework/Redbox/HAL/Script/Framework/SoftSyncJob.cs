using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "soft-sync")]
    internal sealed class SoftSyncJob : NativeJobAdapter
    {
        internal SoftSyncJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            LogHelper.Instance.WithContext("Begin soft sync request.");
            if (!TestInventoryStore())
                return;
            DecksService.ForAllDecksDo(deck =>
            {
                var numberOfSlots = deck.NumberOfSlots;
                for (var index = 0; index < numberOfSlots; ++index)
                {
                    var slot = index + 1;
                    var location = InventoryService.Get(deck.Number, slot);
                    Context.CreateResult("SoftSync", "The inventory location has been successfully inspected.",
                        deck.Number, slot, location.ID, new DateTime?(), null);
                }

                return true;
            });
            Context.CreateInfoResult("SoftSyncEnd", "The soft sync request succeeded.");
        }
    }
}