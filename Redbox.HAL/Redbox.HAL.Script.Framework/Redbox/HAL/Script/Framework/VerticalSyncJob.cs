using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "vertical-sync", Operand = "VERTSYNC")]
    internal sealed class VerticalSyncJob : NativeJobAdapter
    {
        internal VerticalSyncJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var errors = 0;
            var log = ApplicationLog.ConfigureLog(Context, false, "Sync", true, null);
            var cs = ServiceLocator.Instance.GetService<IControllerService>();
            var slot = Context.PopTop<int>();
            log.Write(string.Format("Start vertical sync for slot {0}", slot));
            DecksService.ForAllDecksDo(deck =>
            {
                if (deck.IsQlm)
                    return true;
                var location = InventoryService.Get(deck.Number, slot);
                var transferResult = cs.Transfer(location, location);
                if (transferResult.Transferred)
                    return true;
                HandleError(deck.Number, slot,
                    string.Format("Transfer failed with error {0}", transferResult.TransferError));
                ++errors;
                return false;
            });
            if (errors != 0)
                return;
            log.Write("The sync completed successfully.");
            Context.CreateInfoResult("SyncSuccess", "The test succeeded.");
            var num = (int)MotionService.MoveVend(MoveMode.Get, log);
        }

        private void HandleError(int deck, int slot, string msg)
        {
            Context.CreateResult("SyncFailure", msg, deck, slot, null, new DateTime?(), null);
            AddError("Job error.");
        }
    }
}