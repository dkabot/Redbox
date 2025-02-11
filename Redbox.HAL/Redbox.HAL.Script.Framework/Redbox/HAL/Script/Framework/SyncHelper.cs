using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal static class SyncHelper
    {
        internal static SyncResult SyncSlot(
            ExecutionResult result,
            ExecutionContext context,
            ILocation loc,
            SyncDecorator decorator)
        {
            var from = ServiceLocator.Instance.GetService<IDecksService>().GetFrom(loc);
            var service1 = ServiceLocator.Instance.GetService<IInventoryService>();
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            if (from == null)
            {
                context.AppLog.Write(string.Format("The deck {0} is not a valid deck.", loc.Deck));
                decorator.AddError("Invalid deck passed to sync.");
                return SyncResult.SlotExcluded;
            }

            if (!from.IsSlotValid(loc.Slot))
            {
                context.AppLog.Write(string.Format("The slot {0} is not a valid on this deck ( = {1} ).", loc.Slot,
                    loc.Deck));
                decorator.AddError("Invalid slot passed to sync.");
                return SyncResult.SlotExcluded;
            }

            if (loc.Excluded)
            {
                var msg = loc.IsWide
                    ? string.Format("Location Deck = {0} Slot = {1} is excluded from sync (wide slot on deck 7).",
                        loc.Deck, loc.Slot)
                    : string.Format("Location Deck = {0} Slot = {1} is excluded from sync.", loc.Deck, loc.Slot);
                context.CreateResult("ExcludedSlotSkipped", "The location was not synced because it is marked excluded",
                    loc.Deck, loc.Slot, null, new DateTime?(), null);
                context.AppLog.Write(msg);
                decorator.OnExcludedLocation();
                return SyncResult.SlotExcluded;
            }

            var deck = loc.Deck;
            var slot = loc.Slot;
            context.Send(string.Format("SYNC {0},{1}", deck, slot));
            var err1 = service2.MoveTo(loc, MoveMode.Get, context.AppLog);
            if (err1 != ErrorCodes.Success)
            {
                context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck, slot,
                    null, new DateTime?(), null);
                decorator.OnMoveError(err1);
                return SyncResult.HardwareError;
            }

            var disk = decorator.GetDisk(context.AppLog);
            if (disk.IsSlotEmpty)
            {
                context.CreateResult("InventoryState", "This location was updated in inventory.", deck, slot, "EMPTY",
                    new DateTime?(), disk.Previous);
                loc.ReturnDate = new DateTime?();
                service1.Save(loc);
                return SyncResult.SlotEmpty;
            }

            if (!disk.Success)
            {
                context.CreateResult("MachineError", "An error occurred in the get.", deck, slot, null, new DateTime?(),
                    null);
                decorator.OnGetFailure(disk);
                return SyncResult.GetError;
            }

            using (var scanResult = decorator.ReadDisk())
            {
                if (!scanResult.SnapOk)
                    context.CreateResult("MachineError", "The CAMERA CAPTURE request timed out.", deck, slot, null,
                        new DateTime?(), null);
                using (var fraudValidator = new FraudValidator(context))
                {
                    var num = (int)fraudValidator.Validate(scanResult);
                }

                var syncResult = decorator.AfterDiskRead(scanResult);
                if (syncResult != SyncResult.Success)
                    return syncResult;
                var err2 = service2.MoveTo(loc, MoveMode.Put, context.AppLog);
                if (err2 != ErrorCodes.Success)
                {
                    decorator.OnMoveError(err2);
                    context.CreateMoveErrorResult(deck, slot);
                    return SyncResult.HardwareError;
                }

                var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(scanResult.ScannedMatrix);
                if (!putResult.Success)
                {
                    decorator.OnPutError(scanResult.ScannedMatrix, putResult);
                    return SyncResult.HardwareError;
                }

                context.CreateResult("InventoryState", "This location was updated in inventory.", deck, slot,
                    scanResult.ScannedMatrix, disk.ReturnTime, disk.Previous);
                var merchFlags = scanResult.ScannedMatrix == disk.Previous ? disk.Flags : MerchFlags.None;
                loc.ReturnDate = !scanResult.ReadCode ? disk.ReturnTime : new DateTime?();
                loc.Flags = merchFlags;
                service1.Save(loc);
                if (!scanResult.SnapOk)
                    if (ControllerConfiguration.Instance.ErrorSyncOnCameraFailure)
                    {
                        decorator.AddError("The CAMERA isn't working.");
                        return SyncResult.HardwareError;
                    }
            }

            return SyncResult.Success;
        }
    }
}