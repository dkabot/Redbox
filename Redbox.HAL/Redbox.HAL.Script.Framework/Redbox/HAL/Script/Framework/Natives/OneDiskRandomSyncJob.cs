using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "one-disk-random-sync", HideFromList = true)]
    internal sealed class OneDiskRandomSyncJob : NativeJobAdapter
    {
        internal OneDiskRandomSyncJob(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, false, "RandomSync", true, null);
            var num1 = Context.PopTop<int>();
            var vendTime = Context.PopTop<int>();
            var service1 = ServiceLocator.Instance.GetService<IControlSystem>();
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var service3 = ServiceLocator.Instance.GetService<IControllerService>();
            Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", vendTime, num1);
            Context.AppLog.Write("One disk random sync test start.");
            var locationSelector = new RandomLocationSelector(DecksService, InventoryService);
            var isVmzMachine = ControllerConfiguration.Instance.IsVMZMachine;
            var num2 = 0;
            var flag = false;
            try
            {
                if (service2.MoveTo(1, 1, MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
                {
                    flag = true;
                }
                else if (!service3.Get().Success)
                {
                    flag = true;
                }
                else
                {
                    while (!flag)
                    {
                        var location = locationSelector.SelectTargetLocation();
                        if (location != null)
                        {
                            if (location.Excluded)
                            {
                                Context.AppLog.WriteFormatted("Not syncing excluded location {0}", location);
                            }
                            else if (!isVmzMachine && location.Deck == 8)
                            {
                                if (ControlSystem.EngageQlm(Context.AppLog) != ErrorCodes.Success)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    Thread.Sleep(1500);
                                    if (ControlSystem.DisengageQlm(Context.AppLog) != ErrorCodes.Success)
                                        flag = true;
                                }
                            }
                            else
                            {
                                Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
                                Context.Send(string.Format("Iteration {0}", ++num2));
                                var num3 = num1 == 0 ? 0 : num2 % num1 == 0 ? 1 : 0;
                                var id = "UNKNOWN";
                                using (var scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, service1))
                                {
                                    using (var scanResult = scanner.Read())
                                    {
                                        id = scanResult.ScannedMatrix;
                                        if (!scanResult.SnapOk)
                                        {
                                            Context.CreateCameraCaptureErrorResult();
                                            flag = true;
                                        }
                                    }
                                }

                                if (service2.MoveTo(location, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
                                {
                                    flag = true;
                                    break;
                                }

                                if (!service3.Put(id).Success)
                                {
                                    flag = true;
                                    break;
                                }

                                if (service2.MoveTo(location, MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
                                {
                                    flag = true;
                                    break;
                                }

                                if (!service3.Get().Success || OnVendReceive(vendTime) != ErrorCodes.Success)
                                    flag = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (flag)
                        return;
                    if (service2.MoveTo(1, 1, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
                    {
                        flag = true;
                    }
                    else
                    {
                        if (service3.Put("UNKNOWN").Success)
                            return;
                        flag = true;
                    }
                }
            }
            finally
            {
                applicationLog.WriteFormatted("Job visited {0} locations", num2);
                if (flag)
                    AddError("Job error");
            }
        }

        private ErrorCodes OnVendReceive(int vendTime)
        {
            var errorCodes1 = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveVend(MoveMode.None, Context.AppLog);
            if (errorCodes1 != ErrorCodes.Success)
                return errorCodes1;
            var service = ServiceLocator.Instance.GetService<IControllerService>();
            var vendItemResult = service.VendItemInPicker(vendTime);
            if (!vendItemResult.Presented)
                return ErrorCodes.PickerObstructed;
            if (vendItemResult.Status != ErrorCodes.PickerFull)
                return vendItemResult.Status;
            var errorCodes2 = service.AcceptDiskAtDoor();
            return ErrorCodes.PickerFull != errorCodes2 ? errorCodes2 : ErrorCodes.Success;
        }
    }
}