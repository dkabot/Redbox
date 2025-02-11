using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "push-in-dvd", Operand = "PUSH-IN-DVD")]
    internal sealed class PushInDVDJob : NativeJobAdapter
    {
        internal PushInDVDJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            try
            {
                ControlSystem.StartRollerIn();
                service.Wait(500);
                ControlSystem.StopRoller();
                if (!ControlSystem.TrackOpen().Success)
                {
                    HandleMachineError("TRACK OPEN TIMEOUT");
                }
                else if (!ControlSystem.SetFinger(GripperFingerState.Closed).Success)
                {
                    HandleMachineError("GRIPPER CLOSE TIMEOUT");
                }
                else
                {
                    var pushTime = ControllerConfiguration.Instance.PushTime;
                    ControlSystem.ExtendArm(pushTime);
                    if (!ControlSystem.SetFinger(GripperFingerState.Rent).Success)
                    {
                        HandleMachineError("GRIPPER RENT TIMEOUT");
                    }
                    else if (!ControlSystem.RetractArm().Success)
                    {
                        HandleMachineError("GRIPPER RETRACT TIMEOUT");
                    }
                    else if (!ControlSystem.SetFinger(GripperFingerState.Closed).Success)
                    {
                        HandleMachineError("GRIPPER CLOSE TIMEOUT");
                    }
                    else
                    {
                        ControlSystem.ExtendArm(pushTime);
                        if (!ControlSystem.SetFinger(GripperFingerState.Rent).Success)
                        {
                            HandleMachineError("GRIPPER RENT TIMEOUT");
                        }
                        else
                        {
                            if (ControlSystem.RetractArm().Success)
                                return;
                            HandleMachineError("GRIPPER RETRACT TIMEOUT");
                        }
                    }
                }
            }
            finally
            {
                ControlSystem.TrackClose();
                ControlSystem.RetractArm();
            }
        }

        private void HandleMachineError(string error)
        {
            Context.SetSymbolValue("ERROR-STATE", error);
            Context.CreateInfoResult("MachineError", "There was an error pushing the DVD in.");
        }
    }
}