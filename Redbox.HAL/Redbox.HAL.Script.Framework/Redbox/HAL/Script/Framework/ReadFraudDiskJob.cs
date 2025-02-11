using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "read-fraud-disc", Operand = "READ-FRAUD-DISK")]
    internal sealed class ReadFraudDiskJob : NativeJobAdapter
    {
        internal ReadFraudDiskJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var log = ApplicationLog.ConfigureLog(Context, true, "Fraud", false, "Fraud");
            log.Write("Read Fraud Disc Start.");
            if (MotionService.MoveVend(MoveMode.Get, log) != ErrorCodes.Success)
            {
                EndJob(true);
            }
            else if (ErrorCodes.PickerFull !=
                     ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor())
            {
                EndJob(true);
            }
            else
            {
                var num = (int)ControlSystem.TrackCycle();
                var errored = true;
                using (var scanner = new Scanner(ControlSystem))
                {
                    using (var scanResult = scanner.Read())
                    {
                        if (scanResult.ReadCode)
                        {
                            Context.CreateInfoResult("FraudItem", "This is the barcode to be marked as fraud.",
                                scanResult.ScannedMatrix);
                            errored = false;
                        }
                    }
                }

                EndJob(errored);
            }
        }

        private void EndJob(bool errored)
        {
            try
            {
                if (errored)
                    Context.CreateInfoResult("HardwareError", "A hardware error occurred.", "UNKNOWN");
                var sensorReadResult = ControlSystem.ReadPickerSensors();
                if (!sensorReadResult.Success)
                {
                    Context.CreateInfoResult("HardwareError",
                        string.Format("Read picker sensors returned error {0}", sensorReadResult.Error), "UNKNOWN");
                }
                else
                {
                    if (sensorReadResult.IsFull)
                        ServiceLocator.Instance.GetService<IControllerService>().VendItemInPicker();
                    ControlSystem.VendDoorClose();
                }
            }
            finally
            {
                AppLog.Write("Read fraud disk end.");
            }
        }
    }
}