using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal struct CheckPickerOperation : IFileDiskHandler, IDisposable
    {
        private readonly ExecutionContext Context;

        public void Dispose()
        {
        }

        public void MachineFull(ExecutionResult result, string idInPicker)
        {
            Context.Send(nameof(MachineFull));
            Context.CreateMachineFullResult(idInPicker);
        }

        public FileDiscIterationResult MoveError(
            ExecutionResult result,
            string idInPicker,
            ErrorCodes moveResult,
            int deck,
            int slot)
        {
            if (ErrorCodes.Timeout == moveResult)
                return FileDiscIterationResult.NextLocation;
            Context.CreateResult("MoveFailure",
                string.Format("The MOVE instruction failed: {0}", moveResult.ToString().ToUpper()), deck, slot,
                idInPicker, new DateTime?(), null);
            return FileDiscIterationResult.Halt;
        }

        public void DiskFiled(ExecutionResult result, IPutResult putResult)
        {
            Context.CreateResult("ItemReturned", "check-picker cleared an item from the picker.",
                putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            Context.SetSymbolValue("MSTESTER-SYMBOL-CP-FILED-DISC",
                string.Format("[check-picker]Item in picker filed to {0}: Barcode: {1} Is duplicate: {2}",
                    putResult.PutLocation, putResult.StoredMatrix,
                    putResult.IsDuplicate ? "YES" : (object)"NO"));
        }

        public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
        {
            Context.CreateResult("PutRequiresRetry",
                string.Format("PUT {0} ID={1} returned status {2}", putResult.PutLocation,
                    putResult.StoredMatrix, putResult.ToString().ToUpper()), putResult.PutLocation.Deck,
                putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            if (putResult.IsSlotInUse)
                Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck,
                    putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            return FileDiscIterationResult.Continue;
        }

        public void FailedToFileDisk(ExecutionResult result, string idInPicker)
        {
            Context.Send("MaxPutAwayAttemptsExceeded");
            Context.CreateInfoResult("PutFailureMaxAttempts",
                "PutDiscAway was unable to file a disc in the picker away.", idInPicker);
        }

        internal ErrorCodes CheckPicker(ExecutionResult result, string vendId)
        {
            Context.Send("CheckPickerStart");
            LogHelper.Instance.WithContext("Check picker to see if we need to put something away.");
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var sensorReadResult1 = service.ReadPickerSensors();
            if (!sensorReadResult1.Success)
            {
                Context.CreateItemStuckResult(vendId);
                Context.SetSymbolValue("MSTESTER-SYMBOL-PICKER-FULL", "Sensor read failed.");
                return ErrorCodes.PickerObstructed;
            }

            if (!sensorReadResult1.IsFull)
            {
                OnComplete();
                return ErrorCodes.PickerEmpty;
            }

            if (ServiceLocator.Instance.GetService<IControllerService>().ClearGripper() != ErrorCodes.Success)
            {
                Context.CreateItemStuckResult(vendId);
                Context.SetSymbolValue("MSTESTER-SYMBOL-PICKER-FULL", "The gripper is obstructed.");
                return ErrorCodes.PickerObstructed;
            }

            var sensorReadResult2 = service.ReadPickerSensors();
            if (!sensorReadResult2.Success)
            {
                Context.CreateItemStuckResult(vendId);
                Context.SetSymbolValue("MSTESTER-SYMBOL-PICKER-FULL", "Sensor read failed.");
                return ErrorCodes.PickerObstructed;
            }

            if (!sensorReadResult2.IsFull)
            {
                OnComplete();
                return ErrorCodes.PickerEmpty;
            }

            Context.Send("ItemInPicker");
            LogHelper.Instance.WithContext("Picker is full, trying to find a slot for product.");
            if (service.VendDoorState != VendDoorState.Closed)
                service.VendDoorClose();
            var idInPicker =
                Scanner.ReadDiskInPicker(ReadDiskOptions.LeaveCaptureResult | ReadDiskOptions.LeaveNoReadResult,
                    Context);
            LogHelper.Instance.WithContext("The ID in the picker: {0}", idInPicker);
            using (var fileDiskOperation = new FileDiskOperation(this, Context))
            {
                var errorCodes = fileDiskOperation.PutDiscAway(result, idInPicker);
                if (errorCodes == ErrorCodes.Success)
                    OnComplete();
                return errorCodes == ErrorCodes.Success ? ErrorCodes.PickerEmpty : ErrorCodes.PickerFull;
            }
        }

        internal CheckPickerOperation(ExecutionContext context)
            : this()
        {
            Context = context;
        }

        private void OnComplete()
        {
            Context.Send("CheckPickerStop");
            LogHelper.Instance.WithContext("check-picker complete.");
        }
    }
}