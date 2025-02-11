using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "file-disk-in-picker")]
    internal sealed class FileDiskInPickerJob : NativeJobAdapter
    {
        internal FileDiskInPickerJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
                Context.CreateInfoResult("JobError",
                    string.Format("Read sensors returned error {0}", sensorReadResult.Error));
            else if (!sensorReadResult.IsFull)
                Context.CreateInfoResult("JobError", "Gripper is empty.");
            else
                using (var emptyLocations = ServiceLocator.Instance.GetService<IEmptySearchPatternService>()
                           .FindEmptyLocations())
                {
                    if (emptyLocations.FoundEmpty == 0)
                    {
                        Context.CreateInfoResult("JobError", "Empty slot not found.");
                    }
                    else
                    {
                        var matrix =
                            Scanner.ReadDiskInPicker(
                                ReadDiskOptions.LeaveCaptureResult | ReadDiskOptions.CheckForDuplicate, Context);
                        var emptyLocation = emptyLocations.EmptyLocations[0];
                        if (File(emptyLocation, matrix))
                            return;
                        var errorCodes = MotionService.InitAxes();
                        Context.CreateInfoResult("InitStatus",
                            string.Format("Init motors returned {0}",
                                errorCodes == ErrorCodes.Success ? "SUCCESS" : (object)"FAILURE"));
                        if (errorCodes != ErrorCodes.Success)
                            return;
                        File(emptyLocation, matrix);
                    }
                }
        }

        private bool File(ILocation location, string matrix)
        {
            var putToResult = ServiceLocator.Instance.GetService<IControllerService>().PutTo(matrix, location);
            if (putToResult.MoveResult != ErrorCodes.Success)
            {
                Context.CreateInfoResult("JobError",
                    string.Format("MOVE {0} MODE=PUT {1}", location, putToResult.MoveResult.ToString()));
                return false;
            }

            if (putToResult.Success)
            {
                Context.CreateInfoResult("JobSuccess", string.Format("Filed {0}, id={1}", location, matrix));
                return true;
            }

            Context.CreateInfoResult("JobError",
                string.Format("PUT failed at {0}, status={1}", location, putToResult.Code.ToString()));
            return false;
        }
    }
}