using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "pick-at-offset")]
    internal sealed class PickAtOffsetJob : NativeJobAdapter
    {
        internal PickAtOffsetJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var deck = Context.PopTop<int>();
            var slot = Context.PopTop<int>();
            var axis = Context.PopTop<Axis>();
            var num = Context.PopTop<int>();
            var service1 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
            var service2 = ServiceLocator.Instance.GetService<IInventoryService>();
            var location = service2.Get(deck, slot);
            var data = axis == Axis.X ? new OffsetMoveData(num, 0) : new OffsetMoveData(0, num);
            var errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveTo(location, MoveMode.None, service1.NilLog, ref data);
            if (errorCodes != ErrorCodes.Success)
            {
                Context.CreateInfoResult("MoveError",
                    string.Format("MOVE failed with status {0}", errorCodes.ToString().ToUpper()));
                AddError("Hardware Error");
            }
            else
            {
                ControlSystem.TrackOpen();
                for (var index = 0; index < 2; ++index)
                {
                    var response1 = ControlSystem.SetFinger(GripperFingerState.Rent);
                    if (!response1.Success)
                    {
                        FormatPickError("GripperRentFailure", response1);
                        return;
                    }

                    var response2 = ControlSystem.ExtendArm();
                    if (!response2.Success)
                    {
                        if (response2.TimedOut)
                            ControlSystem.RetractArm();
                        FormatPickError("GripperExtendFailure", response2);
                        return;
                    }

                    var response3 = ControlSystem.SetFinger(GripperFingerState.Closed);
                    if (!response3.Success)
                    {
                        FormatPickError("GripperCloseFailure", response3);
                        return;
                    }

                    var response4 = ControlSystem.RetractArm();
                    if (!response4.Success)
                    {
                        FormatPickError("GripperRetractFailure", response4);
                        return;
                    }
                }

                var response5 = ControlSystem.SetFinger(GripperFingerState.Rent);
                if (!response5.Success)
                {
                    FormatPickError("GripperRentFailure", response5);
                }
                else
                {
                    ControlSystem.StartRollerOut();
                    Thread.Sleep(150);
                    var response6 = ControlSystem.TrackClose();
                    if (!response6.Success)
                    {
                        FormatPickError("TrackCloseFailure", response6);
                    }
                    else
                    {
                        var position = ControlSystem.RollerToPosition(RollerPosition.Position4);
                        if (!position.Success)
                        {
                            FormatPickError("RollerToPos4Failure", position);
                        }
                        else
                        {
                            Context.CreateInfoResult("PickSuccess", "Successfully picked the disk.");
                            service2.Reset(location);
                        }
                    }
                }
            }
        }

        private void FormatPickError(string s, IControlResponse response)
        {
            Context.CreateInfoResult("PickError", string.Format("GET failed: {0} ({1})", s, response));
            AddError("Hardware Error");
        }
    }
}