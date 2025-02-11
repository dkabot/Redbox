using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class VendItemOperation : AbstractOperation<VendItemResult>
    {
        private readonly int PollCount;

        internal VendItemOperation(int tries)
        {
            PollCount = tries;
        }

        public override VendItemResult Execute()
        {
            var vendItemResult = OnExecute();
            if (ErrorCodes.PickerEmpty == vendItemResult.Status && !Controller.VendDoorClose().Success)
                vendItemResult.Status = ErrorCodes.VendDoorCloseTimeout;
            return vendItemResult;
        }

        private VendItemResult OnExecute()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var vendItemResult = new VendItemResult
            {
                Presented = true,
                Status = ErrorCodes.PickerEmpty
            };
            LogHelper.Instance.WithContext(false, LogEntryType.Info, "VendDiskAtDoor...");
            Controller.RollerToPosition(RollerPosition.Position5, 3000);
            var errorCodes1 = OpenDoorChecked();
            if (errorCodes1 != ErrorCodes.Success)
            {
                vendItemResult.Status = errorCodes1;
                return vendItemResult;
            }

            var ms = 1000;
            var errorCodes2 = ControllerService.PushOut();
            if (ErrorCodes.TrackCloseTimeout == errorCodes2 || ErrorCodes.SensorReadError == errorCodes2)
            {
                vendItemResult.Status = errorCodes2;
                vendItemResult.Presented = false;
                return vendItemResult;
            }

            var num1 = (int)Controller.TrackCycle();
            var num2 = 0;
            for (var index = 0; index <= PollCount; ++index)
            {
                var sensorReadResult = Controller.ReadPickerSensors();
                if (!sensorReadResult.Success)
                {
                    vendItemResult.Status = ErrorCodes.SensorReadError;
                    return vendItemResult;
                }

                if (sensorReadResult.IsInputActive(PickerInputs.Sensor1) ||
                    sensorReadResult.IsInputActive(PickerInputs.Sensor2) ||
                    sensorReadResult.IsInputActive(PickerInputs.Sensor3) ||
                    sensorReadResult.IsInputActive(PickerInputs.Sensor4))
                {
                    sensorReadResult.Log(LogEntryType.Error);
                    if (num2 == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
                    {
                        vendItemResult.Presented = false;
                        LogHelper.Instance.WithContext(false, LogEntryType.Error,
                            "[VendItemInPicker] After 2 pushes, the disk has not cleared sensor 4.");
                        vendItemResult.Status = ErrorCodes.PickerFull;
                        return vendItemResult;
                    }

                    ++num2;
                    LogHelper.Instance.WithContext(false, LogEntryType.Error,
                        "[VendItemInPicker] Disc was pushed to the drum - push it back out.");
                    var num3 = (int)Controller.TrackCycle();
                    var num4 = (int)ControllerService.PushOut();
                    service.Wait(ms);
                }
                else
                {
                    num2 = 0;
                    if (sensorReadResult.IsInputActive(PickerInputs.Sensor5))
                    {
                        if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
                            LogHelper.Instance.WithContext(false, LogEntryType.Info,
                                "[VendItemInPicker] The item is still blocking sensor 5.");
                        service.Wait(ms);
                    }
                    else
                    {
                        if (!sensorReadResult.IsInputActive(PickerInputs.Sensor6))
                        {
                            vendItemResult.Status = ErrorCodes.PickerEmpty;
                            return vendItemResult;
                        }

                        service.Wait(ms);
                    }
                }
            }

            var sensorReadResult1 = Controller.ReadPickerSensors();
            vendItemResult.Status = sensorReadResult1.Success
                ? sensorReadResult1.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty
                : ErrorCodes.SensorReadError;
            return vendItemResult;
        }
    }
}