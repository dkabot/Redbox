using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "ms-vend-disk-in-picker")]
    internal sealed class VendDiskInPickerJob : NativeJobAdapter
    {
        internal VendDiskInPickerJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                Context.CreateInfoResult("HardwareError",
                    string.Format("Read picker sensors returned error {0}", sensorReadResult.Error));
            }
            else if (!sensorReadResult.IsFull)
            {
                Context.CreateInfoResult("EmptyPicker", "No disk in picker.");
            }
            else
            {
                var errorCodes1 = MotionService.MoveVend(MoveMode.None, AppLog);
                if (errorCodes1 != ErrorCodes.Success)
                {
                    Context.CreateInfoResult("HardwareError",
                        string.Format("MOVEVEND returned error {0}", errorCodes1.ToString().ToUpper()));
                }
                else
                {
                    var service = ServiceLocator.Instance.GetService<IControllerService>();
                    var vendItemResult = service.VendItemInPicker();
                    if (ErrorCodes.PickerEmpty == vendItemResult.Status)
                    {
                        Context.CreateInfoResult("ItemVended", "The item was vended.");
                    }
                    else if (ErrorCodes.PickerFull != vendItemResult.Status)
                    {
                        Context.CreateInfoResult("HardwareError",
                            vendItemResult.Presented
                                ? string.Format("VendItem returned {0}", vendItemResult.Status.ToString().ToUpper())
                                : "The disk was not presented.");
                    }
                    else
                    {
                        var errorCodes2 = service.AcceptDiskAtDoor();
                        if (ErrorCodes.PickerFull == errorCodes2)
                            return;
                        Context.CreateInfoResult("TakeDiskError",
                            string.Format("Take disk returned error {0}", errorCodes2.ToString().ToUpper()));
                    }
                }
            }
        }
    }
}