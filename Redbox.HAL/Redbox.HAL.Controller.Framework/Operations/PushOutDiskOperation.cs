using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Operations
{
    internal sealed class PushOutDiskOperation : AbstractOperation<ErrorCodes>
    {
        private readonly IControllerService Service;

        internal PushOutDiskOperation(IControllerService cs)
        {
            Service = cs;
        }

        public override ErrorCodes Execute()
        {
            if (CloseTrackChecked() != ErrorCodes.Success)
                return ErrorCodes.TrackCloseTimeout;
            for (var index = 0; index < 3; ++index)
            {
                var sensorReadResult = Controller.ReadPickerSensors();
                if (!sensorReadResult.Success)
                    return ErrorCodes.SensorReadError;
                if (!sensorReadResult.IsFull)
                    return ErrorCodes.Success;
                if (sensorReadResult.IsInputActive(PickerInputs.Sensor4))
                {
                    Controller.StartRollerOut();
                    var num = (int)WaitSensor(PickerInputs.Sensor4, InputState.Inactive);
                    if (ControllerConfiguration.Instance.PushOutSleepTime2 > 0)
                        RuntimeService.Wait(ControllerConfiguration.Instance.PushOutSleepTime2);
                    Controller.StopRoller();
                }
                else
                {
                    if (!sensorReadResult.IsInputActive(PickerInputs.Sensor1) &&
                        !sensorReadResult.IsInputActive(PickerInputs.Sensor2) &&
                        !sensorReadResult.IsInputActive(PickerInputs.Sensor3))
                        return ErrorCodes.Success;
                    if (!Controller.RollerToPosition(RollerPosition.Position4).Success)
                    {
                        Controller.LogPickerSensorState(LogEntryType.Error);
                    }
                    else
                    {
                        RuntimeService.SpinWait(50);
                        Controller.StartRollerOut();
                        var num = (int)WaitSensor(PickerInputs.Sensor4, InputState.Inactive);
                        Controller.StopRoller();
                    }
                }
            }

            var sensorReadResult1 = Controller.ReadPickerSensors();
            if (!sensorReadResult1.Success)
                return ErrorCodes.SensorReadError;
            return !sensorReadResult1.IsInputActive(PickerInputs.Sensor1) &&
                   !sensorReadResult1.IsInputActive(PickerInputs.Sensor2) &&
                   !sensorReadResult1.IsInputActive(PickerInputs.Sensor3) &&
                   !sensorReadResult1.IsInputActive(PickerInputs.Sensor4)
                ? ErrorCodes.Success
                : ErrorCodes.PickerFull;
        }
    }
}