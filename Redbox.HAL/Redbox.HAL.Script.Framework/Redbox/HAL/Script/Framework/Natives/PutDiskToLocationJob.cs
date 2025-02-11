using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "put-disk-to-location")]
    internal sealed class PutDiskToLocationJob : NativeJobAdapter
    {
        internal PutDiskToLocationJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var slot = Context.PopTop<int>();
            var deck = Context.PopTop<int>();
            var errorCodes = MotionService.MoveTo(deck, slot, MoveMode.Put, AppLog);
            if (errorCodes != ErrorCodes.Success)
            {
                Context.CreateInfoResult("JobError",
                    string.Format("MOVE DECK={0} SLOT={1} MODE=PUT {2}", deck, slot, errorCodes.ToString().ToUpper()));
            }
            else
            {
                var id = "UNKNOWN";
                using (var scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, ControlSystem))
                {
                    using (var scanResult = scanner.Read())
                    {
                        id = scanResult.ScannedMatrix;
                    }
                }

                var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(id);
                if (putResult.Success)
                    Context.CreateInfoResult("JobSuccess", string.Format("PUT SUCCESS (barcode = {0})", id));
                else
                    Context.CreateInfoResult("JobError", string.Format("PUT {0}", putResult));
            }
        }
    }
}