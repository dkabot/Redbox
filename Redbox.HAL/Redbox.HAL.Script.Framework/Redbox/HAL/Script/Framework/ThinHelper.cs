using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal abstract class ThinHelper
    {
        protected ThinHelper(ExecutionResult result, ExecutionContext context)
        {
            Result = result;
            Context = context;
        }

        protected ApplicationLog AppLog => Context.AppLog;

        protected ExecutionResult Result { get; }

        protected ExecutionContext Context { get; }

        internal static bool Debug => ControllerConfiguration.Instance.EnableVMZTrace;

        internal static ThinHelper MakeInstance(ExecutionResult result, ExecutionContext context)
        {
            return !ControllerConfiguration.Instance.IsVMZMachine
                ? new QlmThinHelper(result, context)
                : (ThinHelper)new VMZThinHelper(result, context);
        }

        internal MerchandizeResult ProcessSuccessfulThin(ILocation loc, string id, MerchFlags flags)
        {
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the QLM.",
                    loc.Deck, loc.Slot, id, new DateTime?(), null);
                return MerchandizeResult.Success;
            }

            if (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(loc))
            {
                Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.",
                    new int?(), new int?(), id, new DateTime?(), null);
            }
            else
            {
                Context.CreateResult(
                    ControllerConfiguration.Instance.IsVMZMachine ? "DiskTransferSuccessful" : "ThinTransferSuccessful",
                    "The item was transferred from storage to the VMZ.", loc.Deck, loc.Slot, id, new DateTime?(), null);
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                loc.Flags = flags;
                var location = loc;
                service.Save(location);
            }

            return MerchandizeResult.Success;
        }

        internal virtual MerchandizeResult ThinDisk(string id, MerchFlags flags)
        {
            Context.AppLog.Write(string.Format("Attempt thin of item {0} (flags = {1})", id, flags.ToString()));
            var service1 = ServiceLocator.Instance.GetService<IInventoryService>();
            var location = service1.Lookup(id);
            if (location == null)
            {
                Context.CreateLookupErrorResult("ThinLookupFailed", id);
                return MerchandizeResult.LookupFailure;
            }

            if (ControllerConfiguration.Instance.IsVMZMachine && MerchSegmentFactory.Get(flags).Contains(location))
            {
                LogHelper.Instance.Log("The disk {0} {1} is already in its segment; do not move.", id,
                    location.ToString());
                location.Flags = flags;
                service1.Save(location);
                Context.CreateResult("VMZThinNotMoved", "The thin item is already in the VMZ; do not move.",
                    location.Deck, location.Slot, id, new DateTime?(), null);
                return MerchandizeResult.DiskInVMZ;
            }

            MerchandizeResult result;
            var target = FindEmptyTarget(flags, out result);
            if (Debug)
                LogHelper.Instance.Log("FindEmptyTarget returned {0}; location = {1}", result.ToString(),
                    target == null ? "None" : (object)target.ToString());
            if (target == null)
            {
                if (!ControllerConfiguration.Instance.IsVMZMachine || result != MerchandizeResult.VMZFull)
                    return result;
                if (!MerchSegmentFactory.Get(flags).CanDump)
                    return MerchandizeResult.VMZFull;
                var service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
                if (service2.IsFull())
                    return result;
                target = service2.PutLocation;
            }

            var source1 = service1.Lookup(id);
            if (source1 == null)
            {
                Context.CreateLookupErrorResult("ThinLookupFailed", id);
                return MerchandizeResult.LookupFailure;
            }

            var merchandizeResult1 = FetchThin(source1, id);
            if (merchandizeResult1 != MerchandizeResult.Success)
                return merchandizeResult1;
            var merchandizeResult2 = ThinDiskInner(id, source1, target, flags);
            if (merchandizeResult2 == MerchandizeResult.Success)
                return merchandizeResult2;
            var source2 = (int)ReturnToSource(source1, id);
            return merchandizeResult2;
        }

        protected virtual MerchandizeResult ThinDiskInner(
            string id,
            ILocation source,
            ILocation target,
            MerchFlags flags)
        {
            return ThinDiskToLocation(source, target, id, flags);
        }

        protected abstract ILocation FindEmptyTarget(MerchFlags flags, out MerchandizeResult result);

        protected MerchandizeResult ThinDiskToLocation(
            ILocation source,
            ILocation target,
            string id,
            MerchFlags flags)
        {
            if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                LogHelper.Instance.WithContext("Transfer from {0} --> {1}.", source.ToString(), target.ToString());
            var service = ServiceLocator.Instance.GetService<IDumpbinService>();
            if (target == service.PutLocation && service.IsFull())
                return MerchandizeResult.DumpBinFull;
            var failCode = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveTo(target, MoveMode.Put, Context.AppLog);
            if (failCode != ErrorCodes.Success)
            {
                MerchandizingHelper.Instance.CleanupJob(Result, Context, failCode);
                MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
                return MerchandizeResult.HardwareError;
            }

            var num = (int)ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
            var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(id);
            if (putResult.Success)
                return ProcessSuccessfulThin(target, id, flags);
            if (!putResult.IsSlotInUse)
                return MerchandizeResult.TransferFailure;
            Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", target.Deck, target.Slot, id,
                new DateTime?(), null);
            return ControllerConfiguration.Instance.IsVMZMachine
                ? MerchandizeResult.TransferFailure
                : MerchandizeResult.SlotInUse;
        }

        protected MerchandizeResult ReturnToSource(ILocation source, string id)
        {
            LogHelper.Instance.WithContext("Unable to find a slot for the thin; put it back and bail.");
            if (ServiceLocator.Instance.GetService<IMotionControlService>()
                    .MoveTo(source, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
            {
                MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
                return MerchandizeResult.HardwareError;
            }

            var num = (int)ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
            return ServiceLocator.Instance.GetService<IControllerService>().Put(id).Success
                ? MerchandizeResult.HardwareError
                : MerchandizeResult.MachineFull;
        }

        protected MerchandizeResult FetchThin(ILocation source, string id)
        {
            var deck = source.Deck;
            var slot = source.Slot;
            Context.CreateResult("ThinDiskFromDrum", "Removing disk from drum for thinning.", deck, slot, id,
                new DateTime?(), null);
            var failCode = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveTo(source, MoveMode.Get, Context.AppLog);
            if (failCode != ErrorCodes.Success)
            {
                MerchandizingHelper.Instance.CleanupJob(Result, Context, failCode);
                MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
                return MerchandizeResult.HardwareError;
            }

            var service = ServiceLocator.Instance.GetService<IControllerService>();
            var getResult = ControllerConfiguration.Instance.IsVMZMachine
                ? service.Get(new FetchThinGetObserver())
                : service.Get();
            if (getResult.IsSlotEmpty)
                return MerchandizeResult.EmptyStuck;
            if (getResult.Success)
                return MerchandizeResult.Success;
            if (service.ClearGripper() != ErrorCodes.Success)
            {
                Context.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, new DateTime?(),
                    null);
                LogHelper.Instance.WithContext("The gripper is obstructed - bailing thin.");
                return MerchandizeResult.HardwareError;
            }

            var sensorReadResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadPickerSensors();
            if (!sensorReadResult.Success || sensorReadResult.IsFull)
                return MerchandizeResult.HardwareError;
            Context.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, new DateTime?(),
                null);
            return MerchandizeResult.EmptyStuck;
        }
    }
}