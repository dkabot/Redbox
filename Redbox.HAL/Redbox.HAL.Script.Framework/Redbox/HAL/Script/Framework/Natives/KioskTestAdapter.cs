using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    internal class KioskTestAdapter : IDisposable
    {
        private readonly ApplicationLog AppLog;
        protected readonly ExecutionContext Context;
        protected readonly IControlSystem ControlSystem;
        protected readonly IMotionControlService MotionService;
        protected readonly ExecutionResult Result;

        internal KioskTestAdapter(ExecutionResult r, ExecutionContext c)
        {
            Context = c;
            Result = r;
            ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
            MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
            AppLog = ApplicationLog.ConfigureLog(Context, true, "KioskTest", false, "KioskTest");
        }

        public void Dispose()
        {
        }

        protected virtual ErrorCodes OnUnitComplete()
        {
            return ErrorCodes.Success;
        }

        protected virtual bool OnVendDisk(string matrix)
        {
            var service = ServiceLocator.Instance.GetService<IControllerService>();
            var vendItemResult = service.VendItemInPicker(2);
            if (ErrorCodes.PickerEmpty == vendItemResult.Status)
            {
                LogHelper.Instance.WithContext(LogEntryType.Error, "The item {0} was taken.", matrix);
                return false;
            }

            return ErrorCodes.PickerFull == vendItemResult.Status &&
                   ErrorCodes.PickerFull == service.AcceptDiskAtDoor();
        }

        internal void Run()
        {
            var flag = Context.PopTop<bool>();
            var deck1 = Context.PopTop<int>();
            var slot1 = Context.PopTop<int>();
            var deck2 = Context.PopTop<int>();
            var slot2 = Context.PopTop<int>();
            if (MotionService.MoveTo(deck1, slot1, MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
            {
                Context.CreateInfoResult("MoveError", "Failed to move to initial slot");
                HandleMachineError();
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IControllerService>();
                if (!service.Get().Success)
                {
                    Context.CreateInfoResult("GetError", "Failed to pull disc from slot");
                    HandleMachineError();
                }
                else
                {
                    var str = Scanner.ReadDiskInPicker(ReadDiskOptions.None, Context);
                    if (flag && !VendDisk(str))
                    {
                        HandleMachineError();
                    }
                    else if (MotionService.MoveTo(deck2, slot2, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
                    {
                        Context.CreateInfoResult("MoveFailed",
                            "Kiosk was unable to move to put position of destination slot");
                        HandleMachineError();
                    }
                    else
                    {
                        var num = (int)ControlSystem.TrackCycle();
                        if (!service.Put(str).Success)
                        {
                            Context.CreateInfoResult("PutFailed", "Kiosk failed to put disc into slot");
                            HandleMachineError();
                        }
                        else
                        {
                            var errorCodes = OnUnitComplete();
                            if (errorCodes != ErrorCodes.Success)
                            {
                                Context.CreateInfoResult("PostUnitFailed",
                                    string.Format("Post unit test failed with status {0}", errorCodes));
                                HandleMachineError();
                            }
                            else
                            {
                                Context.CreateInfoResult("Success", "SUCCESS");
                            }
                        }
                    }
                }
            }
        }

        private bool VendDisk(string matrix)
        {
            Context.AppLog.Write("Performing a Vend Test.");
            if (MotionService.MoveVend(MoveMode.None, AppLog) != ErrorCodes.Success)
            {
                Context.CreateInfoResult("MoveVendError", "Failed to move to vend position");
                return false;
            }

            var errorCodes = ControlSystem.TrackCycle();
            if (errorCodes == ErrorCodes.Success)
                return OnVendDisk(matrix);
            Context.CreateInfoResult("TrackCycleFailure", string.Format("Track cycle returned error {0}.", errorCodes));
            return false;
        }

        private void HandleMachineError()
        {
            LogHelper.Instance.WithContext(LogEntryType.Error, "An unrecoverable error occurred in KioskTest.");
            Context.CreateInfoResult("MachineError", "An unrecoverable error occurred in KioskTest");
            AddError("MACHINE ERROR");
        }

        private void AddError(string errMsg)
        {
            Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
        }
    }
}