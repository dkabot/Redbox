using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "put-disk-in-picker-in-bin")]
    internal sealed class PutDiskInDumpBinJob : NativeJobAdapter
    {
        internal PutDiskInDumpBinJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IDumpbinService>();
            var log = ApplicationLog.ConfigureLog(Context, true, "VMZ", true, "Tester-Put-In-Bin");
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                Context.CreateInfoResult("PickerSensorReadError",
                    string.Format("Read picker sensors failed with error {0}", sensorReadResult.Error));
                AddError("Hardware error.");
            }
            else if (!sensorReadResult.IsFull)
            {
                Context.CreateInfoResult("PickerEmpty", "The picker is empty.");
                log.Write("The picker is empty.");
                AddError("The picker is empty.");
            }
            else if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                Context.CreateInfoResult("Misconfiguration", "The dump bin is not configured.");
                log.Write("The dumpbin is not configured.");
                AddError("The bin is not configured.");
            }
            else if (service.IsFull())
            {
                Context.CreateInfoResult("DumpBinFull", "The dump bin is full.");
                AddError("The dumpbin is full.");
            }
            else
            {
                var errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>()
                    .MoveTo(service.PutLocation, MoveMode.Put, log);
                if (errorCodes != ErrorCodes.Success)
                {
                    Context.CreateInfoResult("MoveError",
                        string.Format("MOVE failed with error {0}", errorCodes.ToString()));
                    AddError("The MOVE instruction failed.");
                }
                else
                {
                    var id = Scanner.ReadDiskInPicker(ReadDiskOptions.LeaveCaptureResult, Context);
                    var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(id);
                    if (!putResult.Success)
                    {
                        Context.CreateInfoResult("PutError",
                            string.Format("PUT failed with error {0}", putResult));
                        AddError("The PUT instruction failed.");
                    }
                    else
                    {
                        Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", id);
                        log.WriteFormatted("The barcode {0} was put into the dump bin.", id);
                        service.DumpContents(AppLog);
                    }
                }
            }
        }
    }
}