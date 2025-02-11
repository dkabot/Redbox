using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "ms-pull-in-dvd")]
    internal sealed class MSPullInDVDJob : NativeJobAdapter
    {
        private string ErrorCode = "SUCCESS";

        internal MSPullInDVDJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                ErrorCode = "PICKERSENSORREADFAILURE";
            }
            else if (!sensorReadResult.IsFull)
            {
                Context.CreateInfoResult("InvalidJobUse", "Picker is clear.");
                AddError("pull-in-dvd failed.");
            }
            else
            {
                try
                {
                    var service = ServiceLocator.Instance.GetService<IMotionControlService>();
                    var state = GripperFingerState.Rent;
                    var currentLocation = service.CurrentLocation;
                    if (currentLocation != null && ServiceLocator.Instance.GetService<IDecksService>()
                            .GetFrom(currentLocation).IsSlotSellThru(currentLocation.Slot))
                        state = GripperFingerState.Open;
                    if (!ControlSystem.TrackOpen().Success)
                    {
                        ErrorCode = "TRACK-OPEN-TIMEOUT";
                    }
                    else
                    {
                        ControlSystem.StartRollerOut();
                        for (var index = 0; index < ControllerConfiguration.Instance.NumberOfPulls; ++index)
                        {
                            if (!ControlSystem.SetFinger(state).Success)
                            {
                                ErrorCode = "GRIPPER FINGER TIMEOUT";
                                return;
                            }

                            if (!ControlSystem.ExtendArm().Success)
                            {
                                ErrorCode = "GRIPPER EXTEND TIMEOUT";
                                return;
                            }

                            if (!ControlSystem.SetFinger(GripperFingerState.Closed).Success)
                            {
                                ErrorCode = "GRIPPER FINGER TIMEOUT";
                                return;
                            }

                            if (!ControlSystem.RetractArm().Success)
                            {
                                ErrorCode = "GRIPPER RETRACT TIMEOUT";
                                return;
                            }
                        }

                        ControlSystem.SetFinger(GripperFingerState.Open);
                        if (!ControlSystem.TrackClose().Success)
                        {
                            ErrorCode = "TRACK CLOSE TIMEOUT";
                        }
                        else
                        {
                            if (ControlSystem.RollerToPosition(RollerPosition.Position4).Success)
                                return;
                            ErrorCode = "ROLLER POS4 TIMEOUT";
                        }
                    }
                }
                finally
                {
                    if ("SUCCESS" != ErrorCode)
                    {
                        Context.CreateInfoResult("MachineError", ErrorCode);
                        AddError("pull-in-dvd failed.");
                    }

                    ControlSystem.TrackClose();
                    ControlSystem.StopRoller();
                    ControlSystem.SetFinger(GripperFingerState.Rent);
                    ControlSystem.RetractArm();
                }
            }
        }
    }
}