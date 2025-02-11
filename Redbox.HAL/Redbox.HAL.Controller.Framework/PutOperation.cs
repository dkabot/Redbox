using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class PutOperation : AbstractOperation<PutResult>
    {
        private const string AggressivePutFailuresCounter = "AggressivePutFailures";
        private const string AggressivePutSuccessCounter = "AggressivePutSuccesses";
        private readonly string ID;
        private readonly IFormattedLog Log;
        private readonly IPutObserver Observer;
        private readonly ILocation PutLocation;
        private readonly PutResult Result;

        internal PutOperation(
            string id,
            IPutObserver observer,
            ILocation putLocation,
            IFormattedLog log)
        {
            Result = new PutResult(id, putLocation);
            Log = log;
            PutLocation = putLocation;
            ID = id;
            Observer = observer;
        }

        public override PutResult Execute()
        {
            Result.Code = OnCorePut();
            if (Result.Code != ErrorCodes.Success)
                Observer.OnFailedPut(Result, Log);
            else
                Observer.OnSuccessfulPut(Result, Log);
            return Result;
        }

        private ErrorCodes OnCorePut()
        {
            if (PutLocation.IsWide)
            {
                var num1 = (int)SettleDiskInSlot();
            }

            if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
                return ErrorCodes.TrackCloseTimeout;
            if (!Controller.RetractArm().Success)
                return ErrorCodes.GripperRetractTimeout;
            if (!Controller.SetFinger(GripperFingerState.Rent).Success)
            {
                LogHelper.Instance.Log("PUT: SetGripperFinger to Rent timed out.", LogEntryType.Error);
                return ErrorCodes.GripperRentTimeout;
            }

            Controller.StartRollerIn();
            var num2 = ClearDiskFromPicker() ? 1 : 0;
            RuntimeService.SpinWait(100);
            if (num2 != 0)
            {
                LogHelper.Instance.WithContext(LogEntryType.Error,
                    "PUT: Disk cannot be placed into {0} because it cannot clear sensor 2.", PutLocation.ToString());
                Controller.LogPickerSensorState(LogEntryType.Error);
                Controller.StopRoller();
                RuntimeService.SpinWait(100);
                if (!Controller.TrackOpen().Success)
                    return ErrorCodes.TrackOpenTimeout;
                Controller.TrackClose();
                RuntimeService.SpinWait(100);
                if (Controller.RollerToPosition(RollerPosition.Position5, 8000).Success)
                    return OnSlotInUse();
                LogHelper.Instance.Log("Put: roll item back to sensor 5 returned TIMEOUT");
                LogHelper.Instance.Log("PUT: unable to roll item into slot, and cannot move it back into the picker.",
                    LogEntryType.Error);
                Controller.LogPickerSensorState(LogEntryType.Error);
                return ErrorCodes.PickerObstructed;
            }

            if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                LogHelper.Instance.Log("Sensor wait for 2, 5 didn't time out.", LogEntryType.Info);
            var errorCodes = PushIntoSlot();
            if (errorCodes != ErrorCodes.Success)
                LogHelper.Instance.Log(LogEntryType.Error, "PUT: item_cleared_gripper: PushIntoSlot returned {0}",
                    errorCodes.ToString().ToUpper());
            Controller.StopRoller();
            var sensorReadResult1 = Controller.ReadPickerSensors();
            if (!sensorReadResult1.Success)
                return ErrorCodes.SensorReadError;
            if (!sensorReadResult1.IsInputActive(PickerInputs.Sensor1))
            {
                UpdateInventory();
                return ErrorCodes.Success;
            }

            LogHelper.Instance.Log("[PUT] can't put disk in slot.", LogEntryType.Error);
            Controller.LogPickerSensorState(LogEntryType.Error);
            if (ControllerConfiguration.Instance.AggressiveClearPickerOnPut)
                return OnAggressivePut();
            if (!Controller.TrackOpen().Success)
                return ErrorCodes.TrackOpenTimeout;
            var num3 = (int)PullFrom(PutLocation);
            if (!Controller.TrackClose().Success)
                return ErrorCodes.TrackCloseTimeout;
            if (!Controller.RollerToPosition(RollerPosition.Position5, 5000).Success)
            {
                Controller.LogPickerSensorState(LogEntryType.Error);
                RuntimeService.SpinWait(100);
                Controller.StopRoller();
                return ErrorCodes.PickerObstructed;
            }

            var sensorReadResult2 = Controller.ReadPickerSensors();
            return sensorReadResult2.IsInputActive(PickerInputs.Sensor1) ||
                   sensorReadResult2.IsInputActive(PickerInputs.Sensor6)
                ? ErrorCodes.PickerObstructed
                : OnSlotInUse();
        }

        private ErrorCodes OnAggressivePut()
        {
            var num1 = (int)Controller.TrackCycle();
            Controller.StartRollerIn();
            var num2 = (int)PushIntoSlot();
            Controller.StopRoller();
            var sensorReadResult1 = Controller.ReadPickerSensors();
            if (!sensorReadResult1.Success)
                return ErrorCodes.SensorReadError;
            if (!sensorReadResult1.IsInputActive(PickerInputs.Sensor1))
            {
                UpdateAggressivePutCounter();
                UpdateInventory();
                return ErrorCodes.Success;
            }

            Controller.TrackOpen();
            var num3 = (int)PushIntoSlot();
            Controller.TrackClose();
            var sensorReadResult2 = Controller.ReadPickerSensors();
            if (!sensorReadResult2.Success)
                return ErrorCodes.SensorReadError;
            if (!sensorReadResult2.IsInputActive(PickerInputs.Sensor1))
            {
                UpdateInventory();
                UpdateAggressivePutCounter();
                return ErrorCodes.Success;
            }

            LogHelper.Instance.Log("AggressivePut not able to clear the disk.", LogEntryType.Error);
            Controller.LogPickerSensorState(LogEntryType.Error);
            PersistentCounterService.Increment("AggressivePutFailures");
            Controller.StartRollerOut();
            var num4 = (int)PullFrom(PutLocation);
            LogHelper.Instance.Log(LogEntryType.Error, "[PUT] gripperClear: false, rollerTo 5 returned {0}",
                Controller.RollerToPosition(RollerPosition.Position5, 5000).ToString());
            Controller.ReadPickerSensors().Log(LogEntryType.Error);
            RuntimeService.SpinWait(100);
            Controller.StopRoller();
            return ErrorCodes.PickerObstructed;
        }

        private ErrorCodes OnSlotInUse()
        {
            PutLocation.ID = "UNKNOWN";
            InventoryService.Save(PutLocation);
            return ErrorCodes.SlotInUse;
        }

        private void UpdateAggressivePutCounter()
        {
            LogHelper.Instance.Log("Aggressive PUT was able to clear disk.", LogEntryType.Info);
            PersistentCounterService.Increment("AggressivePutSuccesses");
        }

        private void UpdateInventory()
        {
            var original = (ILocation)null;
            Result.IsDuplicate = InventoryService.IsBarcodeDuplicate(ID, out original);
            Result.StoredMatrix = IsSuspiciousId(ID) ? "UNKNOWN" : ID;
            if (PutLocation.Deck == 7 && PutLocation.IsWide)
                Result.StoredMatrix = "UNKNOWN";
            if (Result.IsDuplicate)
            {
                ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
                Result.OriginalMatrixLocation = original;
                Result.StoredMatrix = ControllerConfiguration.Instance.MarkDuplicatesUnknown
                    ? "UNKNOWN"
                    : string.Format("{0}{1}", ID, "(DUPLICATE)");
                LogHelper.Instance.WithContext(
                    "The ID {0} is a duplicate ( original at Deck = {1} Slot = {2} ); marking as {3}", ID,
                    original.Deck, original.Slot, Result.StoredMatrix);
                if (ControllerConfiguration.Instance.MarkOriginalMatrixUnknown)
                {
                    LogHelper.Instance.WithContext(
                        "Mark the original matrix ID = {0} at Deck = {1} Slot = {2} ); marking as {3}", ID,
                        original.Deck, original.Slot, "UNKNOWN");
                    original.ID = "UNKNOWN";
                    InventoryService.Save(original);
                }

                PersistentCounterService.Increment("DUPLICATE-COUNT");
            }

            PutLocation.ID = Result.StoredMatrix;
            InventoryService.Save(PutLocation);
        }

        private bool IsSuspiciousId(string id)
        {
            if (InventoryConstants.IsKnownInventoryToken(id))
                return false;
            try
            {
                return Enum.IsDefined(typeof(ErrorCodes), id);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}