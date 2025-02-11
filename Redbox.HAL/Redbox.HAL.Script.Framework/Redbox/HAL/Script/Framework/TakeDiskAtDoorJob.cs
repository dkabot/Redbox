using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "ms-take-disk-at-door")]
    internal sealed class TakeDiskAtDoorJob : NativeJobAdapter
    {
        internal TakeDiskAtDoorJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                Context.CreateInfoResult("HardwareError",
                    string.Format("Read picker sensors returned error {0}", sensorReadResult.Error.ToString()));
            }
            else if (sensorReadResult.IsFull)
            {
                Context.CreateInfoResult("PickerFull", "The picker is full.");
            }
            else
            {
                var errorCodes1 = ServiceLocator.Instance.GetService<IMotionControlService>()
                    .MoveVend(MoveMode.None, Context.AppLog);
                if (errorCodes1 != ErrorCodes.Success)
                {
                    Context.CreateInfoResult("HardwareError",
                        string.Format("MOVE failed with error {0}", errorCodes1.ToString()));
                }
                else
                {
                    var errorCodes2 = ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor();
                    if (ErrorCodes.PickerFull != errorCodes2)
                    {
                        Context.CreateInfoResult("ReceiveFailure",
                            string.Format("Accept disk returned status {0}", errorCodes2.ToString().ToUpper()));
                    }
                    else
                    {
                        Context.CreateInfoResult("ItemReceived", "Item received in picker.");
                        var num = (int)ControlSystem.TrackCycle();
                    }
                }
            }
        }
    }
}