using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "mark-kiosk-inventory")]
    internal sealed class MarkKioskInventoryJob : NativeJobAdapter
    {
        internal MarkKioskInventoryJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var inventoryToken = Context.PopTop<string>();
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            var inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
            var predicate = (Predicate<IDeck>)(deck =>
            {
                inventoryService.ResetAndMark(deck, inventoryToken);
                return true;
            });
            service.ForAllDecksDo(predicate);
        }
    }
}