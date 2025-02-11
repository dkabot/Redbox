using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "file-marker-disk", Operand = "FILE-MARKER-DISK")]
    internal sealed class FileMarkerDiskJob : NativeJobAdapter
    {
        internal FileMarkerDiskJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var deck = Context.PopTop<int>();
            var slot = Context.PopTop<int>();
            var errorCodes1 = MotionService.MoveVend(MoveMode.Get, Context.AppLog);
            if (errorCodes1 != ErrorCodes.Success)
            {
                HandleHWError(errorCodes1.ToString());
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IControllerService>();
                var errorCodes2 = service.AcceptDiskAtDoor();
                switch (errorCodes2)
                {
                    case ErrorCodes.PickerFull:
                        var errorCodes3 = ServiceLocator.Instance.GetService<IMotionControlService>()
                            .MoveTo(deck, slot, MoveMode.Put, Context.AppLog);
                        if (errorCodes3 != ErrorCodes.Success)
                        {
                            HandleHWError(errorCodes3.ToString());
                            break;
                        }

                        var num = (int)ControlSystem.TrackCycle();
                        if (service.Put("UNKNOWN").Success)
                        {
                            Context.CreateResult("ItemReturned", "The marker disk was filed.", deck, slot, "UNKNOWN",
                                new DateTime?(), null);
                            break;
                        }

                        var errorCodes4 = MotionService.MoveVend(MoveMode.None, Context.AppLog);
                        if (errorCodes4 != ErrorCodes.Success)
                        {
                            HandleHWError(errorCodes4.ToString());
                            break;
                        }

                        var vendItemResult = service.VendItemInPicker();
                        if (vendItemResult.Status == ErrorCodes.PickerEmpty)
                        {
                            Context.CreateInfoResult("ItemTakenByMerchandizer",
                                "The disk was taken by the merchandizer.");
                            AddError("Disk not filed.");
                            break;
                        }

                        if (vendItemResult.Status == ErrorCodes.PickerFull)
                        {
                            Context.CreateInfoResult("ItemNotTaken", "The disk was taken by the merchandizer.");
                            AddError("Disk not filed.");
                            break;
                        }

                        HandleHWError(vendItemResult.Status.ToString());
                        break;
                    case ErrorCodes.PickerEmpty:
                        Context.CreateInfoResult("NoReceivedItem", "No disk was placed in the picker.");
                        break;
                    default:
                        HandleHWError(errorCodes2.ToString());
                        break;
                }
            }
        }

        private void HandleHWError(string detail)
        {
            Context.Send("MachineError");
            Context.CreateInfoResult("MachineError", detail);
            AddError("Hardware Error during job.");
        }
    }
}