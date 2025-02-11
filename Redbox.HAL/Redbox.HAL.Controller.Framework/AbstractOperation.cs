using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework
{
    public abstract class AbstractOperation<T> : IDisposable
    {
        protected readonly IControlSystem Controller;
        protected readonly IControllerService ControllerService;
        protected readonly IDecksService DeckService;
        protected readonly IInventoryService InventoryService;
        protected readonly IMotionControlService MotionService;
        protected readonly IPersistentCounterService PersistentCounterService;
        protected readonly IRuntimeService RuntimeService;
        private bool Disposed;

        protected AbstractOperation()
        {
            DeckService = ServiceLocator.Instance.GetService<IDecksService>();
            MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
            Controller = ServiceLocator.Instance.GetService<IControlSystem>();
            RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
            InventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
            PersistentCounterService = ServiceLocator.Instance.GetService<IPersistentCounterService>();
            ControllerService = ServiceLocator.Instance.GetService<IControllerService>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract T Execute();

        protected bool WaitToClear()
        {
            return WaitToClear(8000);
        }

        protected bool WaitToClear(int timeout)
        {
            var sensorPauseDelay = ControllerConfiguration.Instance.ClearSensorPauseDelay;
            using (var executionTimer = new ExecutionTimer())
            {
                do
                {
                    RuntimeService.SpinWait(sensorPauseDelay);
                    var readResult = Controller.ReadPickerSensors();
                    if (!readResult.Success)
                        return true;
                    if (ClearPickerBlockedCount(readResult) == 0)
                        return false;
                } while (executionTimer.ElapsedMilliseconds <= timeout);

                return true;
            }
        }

        protected bool ClearDiskFromPicker()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var sensorPauseDelay = ControllerConfiguration.Instance.ClearSensorPauseDelay;
            var flag1 = WaitToClear(8000);
            if (!flag1)
            {
                service.SpinWait(sensorPauseDelay);
                var flag2 = true;
                for (var index = 0; (index < 3) & flag2; ++index)
                {
                    if (flag2)
                    {
                        if (index > 0)
                            LogHelper.Instance.Log("2-5 didn't timeout; sensors show something in picker.",
                                LogEntryType.Info);
                        WaitToClear(3000);
                    }

                    var readResult = Controller.ReadPickerSensors();
                    if (!readResult.Success)
                        return true;
                    flag2 = ClearPickerBlockedCount(readResult) > 0;
                    service.SpinWait(sensorPauseDelay);
                }

                var readResult1 = Controller.ReadPickerSensors();
                if (!readResult1.Success)
                    return true;
                flag1 = ClearPickerBlockedCount(readResult1) > 0;
            }

            return flag1;
        }

        protected ErrorCodes WaitSensor(PickerInputs sensor, InputState state)
        {
            return WaitSensor(sensor, state, 4000);
        }

        protected ErrorCodes WaitSensor(PickerInputs sensor, InputState state, int timeout)
        {
            var waitSensorPauseTime = ControllerConfiguration.Instance.WaitSensorPauseTime;
            var errorCodes = ErrorCodes.Success;
            using (var executionTimer = new ExecutionTimer())
            {
                do
                {
                    RuntimeService.SpinWait(waitSensorPauseTime);
                    var sensorReadResult = Controller.ReadPickerSensors();
                    if (!sensorReadResult.Success)
                    {
                        errorCodes = ErrorCodes.SensorReadError;
                        goto label_9;
                    }

                    if (sensorReadResult.IsInState(sensor, state))
                        goto label_9;
                } while (executionTimer.ElapsedMilliseconds <= timeout);

                errorCodes = ErrorCodes.Timeout;
            }

            label_9:
            if (errorCodes != ErrorCodes.Success && ErrorCodes.Timeout != errorCodes)
                LogHelper.Instance.WithContext(LogEntryType.Error, "WaitSensor returned error {0}.",
                    errorCodes.ToString().ToUpper());
            return errorCodes;
        }

        protected ErrorCodes SettleDiskInSlot()
        {
            try
            {
                if (!Controller.SetFinger(GripperFingerState.Open).Success)
                    return ErrorCodes.GripperOpenTimeout;
                if (Controller.ExtendArm().TimedOut)
                {
                    Controller.RetractArm();
                    Controller.SetFinger(GripperFingerState.Closed);
                    Controller.SetFinger(GripperFingerState.Open);
                    if (!Controller.ExtendArm().Success)
                    {
                        Controller.RetractArm();
                        return ErrorCodes.GripperExtendTimeout;
                    }
                }

                if (!Controller.SetFinger(GripperFingerState.Closed).Success)
                    return ErrorCodes.GripperCloseTimeout;
                return !Controller.SetFinger(GripperFingerState.Open).Success ? ErrorCodes.GripperOpenTimeout :
                    Controller.RetractArm().Success ? ErrorCodes.Success : ErrorCodes.GripperRetractTimeout;
            }
            finally
            {
                Controller.SetFinger(GripperFingerState.Open);
                Controller.RetractArm();
            }
        }

        protected ErrorCodes PullFrom(ILocation location)
        {
            return PullFrom(location, ControllerConfiguration.Instance.NumberOfPulls);
        }

        protected ErrorCodes PullFrom(ILocation location, int pulls)
        {
            var state = !location.IsWide ? GripperFingerState.Rent : GripperFingerState.Open;
            for (var index = 0; index < pulls; ++index)
            {
                if (!Controller.SetFinger(state).Success)
                    return state != GripperFingerState.Open
                        ? ErrorCodes.GripperRentTimeout
                        : ErrorCodes.GripperOpenTimeout;
                if (DeckService.GetByNumber(MotionService.CurrentLocation.Deck).IsQlm)
                {
                    var qlmExtendTime = ControllerConfiguration.Instance.QlmExtendTime;
                    if (ControllerConfiguration.Instance.QlmTimedExtend)
                        Controller.TimedExtend(qlmExtendTime);
                    else
                        Controller.ExtendArm(ControllerConfiguration.Instance.QlmExtendTime);
                }
                else if (Controller.ExtendArm().TimedOut)
                {
                    Controller.RetractArm();
                    Controller.SetFinger(GripperFingerState.Closed);
                    Controller.SetFinger(state);
                    if (!Controller.ExtendArm().Success)
                    {
                        Controller.RetractArm();
                        return ErrorCodes.GripperExtendTimeout;
                    }
                }

                if (!Controller.SetFinger(GripperFingerState.Closed).Success)
                    return ErrorCodes.GripperCloseTimeout;
                if (!Controller.RetractArm().Success)
                    return ErrorCodes.GripperRetractTimeout;
            }

            return !Controller.SetFinger(GripperFingerState.Rent).Success
                ? ErrorCodes.GripperRentTimeout
                : ErrorCodes.Success;
        }

        protected ErrorCodes PushIntoSlot()
        {
            var errorCodes1 = PushWithArm(ControllerConfiguration.Instance.RollInExtendTime);
            if (errorCodes1 != ErrorCodes.Success)
                return errorCodes1;
            var errorCodes2 = PushWithArm(ControllerConfiguration.Instance.PushTime);
            if (errorCodes2 != ErrorCodes.Success)
                return errorCodes2;
            if (ControllerConfiguration.Instance.AdditionalPutPush)
            {
                var num = (int)PushWithArm(ControllerConfiguration.Instance.PushTime);
            }

            return ErrorCodes.Success;
        }

        protected int ClearPickerBlockedCount(IPickerSensorReadResult readResult)
        {
            var num = 0;
            if (readResult.IsInputActive(PickerInputs.Sensor2))
                ++num;
            if (readResult.IsInputActive(PickerInputs.Sensor3))
                ++num;
            if (readResult.IsInputActive(PickerInputs.Sensor4))
                ++num;
            if (readResult.IsInputActive(PickerInputs.Sensor5))
                ++num;
            return num;
        }

        protected ErrorCodes OpenDoorChecked()
        {
            return VendDoorState.Rent != Controller.VendDoorState && !Controller.VendDoorRent().Success
                ? ErrorCodes.VendDoorRentTimeout
                : ErrorCodes.Success;
        }

        protected ErrorCodes CloseTrackChecked()
        {
            return Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success
                ? ErrorCodes.TrackCloseTimeout
                : ErrorCodes.Success;
        }

        protected virtual void OnDispose()
        {
        }

        private ErrorCodes PushWithArm(int timeout)
        {
            if (!Controller.SetFinger(GripperFingerState.Closed).Success)
                return ErrorCodes.GripperCloseTimeout;
            Controller.TimedExtend(timeout);
            if (!Controller.SetFinger(GripperFingerState.Rent).Success)
                return ErrorCodes.GripperRentTimeout;
            return Controller.RetractArm().Success ? ErrorCodes.Success : ErrorCodes.GripperRetractTimeout;
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;
            Disposed = true;
            if (!disposing)
                return;
            OnDispose();
        }
    }
}