using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    internal class RandomSyncDecorator : SyncDecorator
    {
        private readonly bool VendDisk;
        private readonly int VendTime;

        internal RandomSyncDecorator(
            ExecutionContext ctx,
            ExecutionResult r,
            ILocation loc,
            int vendtime,
            bool vend)
            : base(ctx, r, loc)
        {
            VendDisk = vend;
            VendTime = vendtime;
        }

        internal int SecureReads { get; private set; }

        protected override void OnReadDisk(ScanResult result)
        {
            if (!result.ReadCode)
                return;
            SecureReads += result.SecureTokensFound;
        }

        internal override SyncResult AfterDiskRead(ScanResult sr)
        {
            if (!VendDisk)
                return SyncResult.Success;
            if (ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, Context.AppLog) !=
                ErrorCodes.Success)
            {
                Context.CreateMoveErrorResult(SyncLocation.Deck, SyncLocation.Slot);
                AddError("job error.");
                return SyncResult.HardwareError;
            }

            var service1 = ServiceLocator.Instance.GetService<IControllerService>();
            var vendItemResult = service1.VendItemInPicker(VendTime);
            if (!vendItemResult.Presented)
            {
                AddError("Job error.");
                return SyncResult.HardwareError;
            }

            if (vendItemResult.Status != ErrorCodes.PickerFull)
            {
                AddError("Job error.");
                return SyncResult.HardwareError;
            }

            if (ErrorCodes.PickerFull != service1.AcceptDiskAtDoor())
            {
                AddError("Job error.");
                return SyncResult.HardwareError;
            }

            var service2 = ServiceLocator.Instance.GetService<IControlSystem>();
            var num = (int)service2.TrackCycle();
            var syncResult = SyncResult.Success;
            using (var scanner = new Scanner(service2))
            {
                using (var scanResult = scanner.Read())
                {
                    if (!scanResult.SnapOk)
                    {
                        Context.CreateCameraCaptureErrorResult();
                        syncResult = SyncResult.HardwareError;
                    }
                }
            }

            return syncResult;
        }
    }
}