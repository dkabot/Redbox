using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ClearGripperOperation : AbstractOperation<ErrorCodes>
    {
        public override ErrorCodes Execute()
        {
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            var errorCodes = CloseTrackChecked();
            if (errorCodes != ErrorCodes.Success)
                return errorCodes;
            for (var index = 0; index < 2; ++index)
            {
                var readResult = Controller.ReadPickerSensors();
                if (!readResult.Success)
                    return readResult.Error;
                if (!readResult.IsFull)
                    return ErrorCodes.Success;
                if (!readResult.IsInputActive(PickerInputs.Sensor1))
                {
                    if (!readResult.IsInputActive(PickerInputs.Sensor6))
                        return ErrorCodes.Success;
                    Controller.SetFinger(GripperFingerState.Rent);
                    try
                    {
                        if (service.AtVendDoor && VendDoorState.Closed != Controller.VendDoorState)
                            Controller.VendDoorRent();
                        Controller.RollerToPosition(RollerPosition.Position3, 3000);
                    }
                    finally
                    {
                        if (VendDoorState.Closed != Controller.VendDoorState)
                            Controller.VendDoorClose();
                    }
                }
                else
                {
                    if (readResult.IsInputActive(PickerInputs.Sensor6))
                        return ErrorCodes.PickerObstructed;
                    if (ClearPickerBlockedCount(readResult) == 0)
                    {
                        try
                        {
                            Controller.StartRollerIn();
                            var num = (int)PushIntoSlot();
                        }
                        finally
                        {
                            Controller.StopRoller();
                        }
                    }
                    else
                    {
                        Controller.SetFinger(GripperFingerState.Rent);
                        Controller.RollerToPosition(RollerPosition.Position5);
                    }
                }
            }

            return ErrorCodes.PickerObstructed;
        }
    }
}