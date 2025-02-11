using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class RejectDiskOperation : AbstractOperation<ErrorCodes>
    {
        private readonly int Attempts;

        internal RejectDiskOperation(int attempts)
        {
            Attempts = attempts;
        }

        public override ErrorCodes Execute()
        {
            var errorCodes = OnReject();
            if ((ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes) &&
                VendDoorState.Closed != Controller.VendDoorState && !Controller.VendDoorClose().Success)
                errorCodes = ErrorCodes.VendDoorCloseTimeout;
            return errorCodes;
        }

        private ErrorCodes OnReject()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var attempts = Attempts;
            var ms = 1000;
            if (OpenDoorChecked() != ErrorCodes.Success)
                return ErrorCodes.VendDoorRentTimeout;
            while (attempts-- >= 0)
            {
                var num1 = (int)Controller.TrackCycle();
                var sensorReadResult1 = Controller.ReadPickerSensors();
                if (!sensorReadResult1.Success)
                    return ErrorCodes.SensorReadError;
                if (sensorReadResult1.BlockedCount == 0)
                    return !Controller.VendDoorClose().Success ? ErrorCodes.PickerObstructed : ErrorCodes.PickerEmpty;
                sensorReadResult1.Log();
                var flag = sensorReadResult1.IsInputActive(PickerInputs.Sensor4) ||
                           sensorReadResult1.IsInputActive(PickerInputs.Sensor3) ||
                           sensorReadResult1.IsInputActive(PickerInputs.Sensor2) ||
                           sensorReadResult1.IsInputActive(PickerInputs.Sensor1);
                if (sensorReadResult1.IsInputActive(PickerInputs.Sensor6))
                {
                    if (flag)
                    {
                        var num2 = (int)ControllerService.PushOut();
                        service.Wait(ms);
                    }
                    else
                    {
                        if (Controller.RollerToPosition(RollerPosition.Position3, 8000).Success)
                        {
                            var sensorReadResult2 = Controller.ReadPickerSensors();
                            if (!sensorReadResult2.Success)
                                return ErrorCodes.SensorReadError;
                            if (!sensorReadResult2.IsFull)
                                return ErrorCodes.PickerEmpty;
                            if (sensorReadResult2.BlockedCount <= 3)
                                return !Controller.VendDoorClose().Success
                                    ? ErrorCodes.PickerObstructed
                                    : ErrorCodes.PickerFull;
                        }

                        service.Wait(ms);
                    }
                }
                else if (flag)
                {
                    var num3 = (int)ControllerService.PushOut();
                    service.Wait(1500);
                }
            }

            var sensorReadResult = Controller.ReadPickerSensors();
            if (!sensorReadResult.Success)
                return ErrorCodes.SensorReadError;
            if (!sensorReadResult.IsFull)
                return ErrorCodes.PickerEmpty;
            return sensorReadResult.BlockedCount > 3 || !Controller.VendDoorClose().Success
                ? ErrorCodes.PickerObstructed
                : ErrorCodes.PickerFull;
        }
    }
}