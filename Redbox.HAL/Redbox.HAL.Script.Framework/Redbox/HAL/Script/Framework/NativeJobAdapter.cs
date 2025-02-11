using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal abstract class NativeJobAdapter
    {
        protected NativeJobAdapter(ExecutionResult result, ExecutionContext context)
        {
            Result = result;
            Context = context;
        }

        protected ApplicationLog AppLog => Context.AppLog;

        protected IInventoryService InventoryService => ServiceLocator.Instance.GetService<IInventoryService>();

        protected IDumpbinService DumpbinService => ServiceLocator.Instance.GetService<IDumpbinService>();

        protected IMotionControlService MotionService => ServiceLocator.Instance.GetService<IMotionControlService>();

        protected IDecksService DecksService => ServiceLocator.Instance.GetService<IDecksService>();

        protected IControlSystem ControlSystem => ServiceLocator.Instance.GetService<IControlSystem>();

        protected IControllerService ControlService => ServiceLocator.Instance.GetService<IControllerService>();

        protected ExecutionResult Result { get; }

        protected ExecutionContext Context { get; }

        internal string Name { get; set; }
        protected abstract void ExecuteInner();

        protected void AddError(string errMsg)
        {
            Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
        }

        protected void RegisterForCorrectionEvents()
        {
            if (!ControllerConfiguration.Instance.HandleHardwareCorrectionEvents)
                return;
            Context.StartHardwareCorrectionEvent += s =>
            {
                LogHelper.Instance.WithContext(false, LogEntryType.Info,
                    "Hardware correction start event ( Statistic = {0} ).", s.Statistic);
                Context.Send(string.Format("HardwareCorrectionStart:{0}", s.Statistic));
            };
            Context.EndHardwareCorrectionEvent += s =>
            {
                LogHelper.Instance.WithContext("Hardware correction end event ( Statistic = {0} status = {1} ).",
                    s.Statistic, s.CorrectionOk);
                Context.Send(s.CorrectionOk
                    ? string.Format("HardwareCorrectionSuccess:{0}", s.Statistic)
                    : string.Format("HardwareCorrectionFailure:{0}", s.Statistic));
            };
        }

        protected void ShowEmptyStuck(ILocation location)
        {
            using (var decoratorService = new MoveDecoratorService(Context))
            {
                if (decoratorService.ShowEmptyStuck(location) == ErrorCodes.Success)
                    return;
                AddError("There was an error moving to the offset position.");
            }
        }

        protected bool MoveAndGet(int deck, int slot)
        {
            return MotionService.MoveTo(deck, slot, MoveMode.Get, AppLog) == ErrorCodes.Success &&
                   ServiceLocator.Instance.GetService<IControllerService>().Get().Success;
        }

        protected bool MoveAndPut(int deck, int slot, string id)
        {
            if (MotionService.MoveTo(deck, slot, MoveMode.Put, AppLog) != ErrorCodes.Success)
                return false;
            var num = (int)ControlSystem.TrackCycle();
            return ServiceLocator.Instance.GetService<IControllerService>().Put(id).Success;
        }

        protected bool ContextSwitchRequested()
        {
            if (Context.DeferredStatusChangeRequested.HasValue)
            {
                LogHelper.Instance.Log(
                    string.Format("SwitchRequested: deferred change to {0}",
                        Context.DeferredStatusChangeRequested.Value.ToString()), LogEntryType.Info);
                Context.PromoteDeferredStatusChange();
                return true;
            }

            Context.FlushMessages();
            if (Context.Messages.Count != 0)
                LogHelper.Instance.Log(
                    string.Format("Context {0} {1}: msg count = {2}", Context.ID, Context.ProgramName,
                        Context.Messages.Count), LogEntryType.Info);
            return false;
        }

        protected bool TestInventoryStore()
        {
            if (InventoryService.CheckIntegrity() == ErrorCodes.Success)
                return true;
            Context.AppLog.WriteFormatted("The inventory store is corrupt.");
            Context.CreateInfoResult("InventoryStoreError", "Inventory database integrity test failed.");
            AddError("Datastore corrupt");
            return false;
        }

        internal void Execute()
        {
            ExecuteInner();
        }
    }
}