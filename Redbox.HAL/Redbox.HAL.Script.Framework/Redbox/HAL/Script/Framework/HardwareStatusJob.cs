using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "hardware-status")]
    internal sealed class HardwareStatusJob : NativeJobAdapter
    {
        internal HardwareStatusJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IDoorSensorService>();
            var num = 0;
            if (ControllerConfiguration.Instance.FixArcusDuringStatusJob && !MotionService.TestAndReset())
            {
                Context.CreateInfoResult("MotionControllerCommunicationError",
                    "Could not talk with the motion controller.");
                ++num;
            }

            if (ControllerConfiguration.Instance.EnableDoorCheckInStatus && service.Query() != DoorSensorResult.Ok)
            {
                Context.CreateInfoResult("MotionControllerCommunicationError",
                    "The door sensors are not in a safe mode.");
                ++num;
            }

            if (ServiceLocator.Instance.GetService<IControllerService>().ClearGripper() != ErrorCodes.Success)
            {
                ++num;
                Context.CreateInfoResult("GripperObstructed", "The gripper is obstructed.");
            }

            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success || sensorReadResult.IsFull)
            {
                ++num;
                Context.CreateInfoResult("GripperObstructed", "The picker is full.");
            }

            if (VendDoorState.Closed != ControlSystem.VendDoorState && !ControlSystem.VendDoorClose().Success)
            {
                ++num;
                Context.CreateInfoResult("VendDoorOpen", "Unable to close the vend door.");
            }

            if (!TestInventoryStore())
                ++num;
            if (num == 0)
                Context.CreateInfoResult("HardwareStatusOk", "The three tests passed!");
            else
                Context.CreateInfoResult("HardwareStatusInError", "One or more ops failed.");
        }
    }
}