using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "read-barcode")]
    internal sealed class TesterReadDiskJob : NativeJobAdapter
    {
        internal TesterReadDiskJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                AddError("Picker sensor read error");
            }
            else if (!sensorReadResult.IsFull)
            {
                Context.CreateInfoResult("NoDisk", "The picker is empty.");
                AddError("Picker empty");
            }
            else
            {
                using (var scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, ControlSystem))
                {
                    using (var scanResult = scanner.Read())
                    {
                        scanResult.PushTo(Context);
                        if (!scanResult.ReadCode ||
                            !InventoryService.IsBarcodeDuplicate(scanResult.ScannedMatrix, out _))
                            return;
                        Context.CreateDuplicateItemResult(scanResult.ScannedMatrix);
                    }
                }
            }
        }
    }
}