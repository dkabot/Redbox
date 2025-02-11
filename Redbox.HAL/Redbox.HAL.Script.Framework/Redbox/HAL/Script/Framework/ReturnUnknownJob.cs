using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "return-unknown")]
    internal class ReturnUnknownJob : NativeJobAdapter, IFileDiskHandler
    {
        private static readonly CenterDiskMethod[] m_methods = new CenterDiskMethod[2]
        {
            CenterDiskMethod.None,
            CenterDiskMethod.DrumAndBack
        };

        private readonly int SignalWait = 30000;
        private readonly EventWaitHandle WaitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private bool IsFieldInsert;
        private string Previous = "UNKNOWN";
        private string ReceivedSignal = string.Empty;

        internal ReturnUnknownJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
            SignalWait = ControllerConfiguration.Instance.SignalWaitTimeout;
        }

        public FileDiscIterationResult MoveError(
            ExecutionResult result,
            string idInPicker,
            ErrorCodes moveResult,
            int deck,
            int slot)
        {
            Context.Send("MachineError");
            Context.CreateMoveErrorResult(deck, slot);
            return FileDiscIterationResult.Halt;
        }

        public void MachineFull(ExecutionResult result, string id)
        {
            Context.Send(nameof(MachineFull));
        }

        public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
        {
            Context.CreateInfoResult("PutFailure",
                string.Format("There was an error while executing the PUT instruction: {0}",
                    putResult.ToString().ToUpper()));
            if (!putResult.IsSlotInUse)
                return FileDiscIterationResult.Halt;
            Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck,
                putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            return FileDiscIterationResult.Continue;
        }

        public void FailedToFileDisk(ExecutionResult result, string idInPicker)
        {
            Context.CreateInfoResult("PutFailureMaxAttempts", "Failed to file disk.", idInPicker);
            Context.Send("MaxPutAwayAttemptsExceeded");
            if (MotionService.MoveVend(MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
            {
                Context.Send("MachineError");
                Context.CreateInfoResult("MachineError", "The MOVEVEND instruction failed.");
                AddError("Machine error.");
            }
            else
            {
                ServiceLocator.Instance.GetService<IControllerService>().VendItemInPicker();
                AddError("Unable to put away disk.");
            }
        }

        public void DiskFiled(ExecutionResult result, IPutResult putResult)
        {
            Context.Send(string.Format("ItemPlacementDone:{0}", putResult.StoredMatrix));
            Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck,
                putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            if (IsFieldInsert || !InventoryConstants.CodeIsUnknown(putResult.StoredMatrix) || !("NONE" != Previous))
                return;
            DateTime result1;
            if (!DateTime.TryParse(Previous, out result1))
            {
                AppLog.WriteFormatted("Unable to parse date/time information {0}.", Previous);
                AddError("Failed to parse date/time");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                putResult.PutLocation.ReturnDate = result1;
                var putLocation = putResult.PutLocation;
                service.Save(putLocation);
            }
        }

        protected override void ExecuteInner()
        {
            try
            {
                Previous = Context.PopTop<string>();
                IsFieldInsert = "FMA-INSERT" == Previous;
                Context.RegisterHandler(signal =>
                {
                    try
                    {
                        ReceivedSignal = signal;
                        LogHelper.Instance.WithContext(false, LogEntryType.Info, "Received signal '{0}'",
                            ReceivedSignal);
                        WaitEvent.Set();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("[return-unknown] Handler received an exception", ex);
                    }
                });
                var applicationLog = ApplicationLog.ConfigureLog(Context, true, IsFieldInsert ? "Return" : "Unknowns",
                    true, IsFieldInsert ? "FieldInsert" : "ReturnUnknown");
                applicationLog.Write("Return-unknown start.");
                if (InventoryService.MachineIsFull())
                {
                    Context.CreateMachineFullResult();
                    Context.Send("MachineFull");
                    AddError("The machine is full.");
                }
                else
                {
                    var sensorReadResult = ControlSystem.ReadPickerSensors();
                    if (!sensorReadResult.Success)
                    {
                        Context.CreateInfoResult("ItemStuckInGripper", "There is a disc stuck in the picker.");
                        AddError("Gripper is obstructed.");
                    }
                    else if (sensorReadResult.IsFull)
                    {
                        applicationLog.Write("The gripper is obstructed - exiting.");
                        Context.CreateInfoResult("ItemStuckInGripper", "There is a disc stuck in the picker.");
                        AddError("Gripper is obstructed.");
                    }
                    else
                    {
                        if (ContextSwitchRequested())
                            return;
                        var errorCodes1 = MotionService.MoveVend(MoveMode.Get, Context.AppLog);
                        if (errorCodes1 != ErrorCodes.Success)
                        {
                            Context.CreateInfoResult("MachineError",
                                string.Format("There was an error while executing the MOVEVEND instruction: {0}",
                                    errorCodes1.ToString()));
                            Context.Send("MachineError");
                            AddError("Unable to move to the vend door.");
                        }
                        else
                        {
                            Context.Send("BlockingOnSensor");
                            switch (ControlService.AcceptDiskAtDoor())
                            {
                                case ErrorCodes.VendDoorRentTimeout:
                                    Context.CreateInfoResult("MachineError", "VENDDOOR RENT timed out.");
                                    AddError("Unable to open the vend door.");
                                    break;
                                case ErrorCodes.VendDoorCloseTimeout:
                                    Context.CreateInfoResult("MachineError", "Venddoor close timeout.");
                                    AddError("Unable to close the vend door.");
                                    break;
                                case ErrorCodes.PickerFull:
                                    applicationLog.Write("Item received into picker.");
                                    var num1 = (int)ControlSystem.TrackCycle();
                                    using (var scanResult = Scanner.SmartReadDisk(m_methods))
                                    {
                                        if (!scanResult.SnapOk)
                                        {
                                            Context.CreateCameraCaptureErrorResult();
                                        }
                                        else if (!scanResult.ReadCode)
                                        {
                                            Context.CreateNoBarcodeReadResult();
                                            if (IsFieldInsert)
                                                switch (ControlService.VendItemInPicker().Status)
                                                {
                                                    case ErrorCodes.PickerFull:
                                                        var errorCodes2 = ControlService.AcceptDiskAtDoor();
                                                        if (ErrorCodes.PickerEmpty == errorCodes2)
                                                        {
                                                            applicationLog.Write("The disk was taken by the user.");
                                                            return;
                                                        }

                                                        if (ErrorCodes.PickerFull != errorCodes2)
                                                        {
                                                            Context.CreateInfoResult("ItemStuckInGripper",
                                                                string.Format("Accept disk at door returned error {0}",
                                                                    errorCodes2.ToString().ToUpper()));
                                                            AddError("Item stuck in the vend door.");
                                                            return;
                                                        }

                                                        break;
                                                    case ErrorCodes.PickerEmpty:
                                                        Context.CreateInfoResult("BackwardDiscTaken",
                                                            "The backward disc was taken.");
                                                        return;
                                                    default:
                                                        Context.Send("MachineError");
                                                        Context.CreateInfoResult("MachineError",
                                                            "Machine error ejecting the disc.");
                                                        AddError("Hardware error.");
                                                        return;
                                                }
                                        }
                                        else
                                        {
                                            applicationLog.Write(string.Format("The ID in the picker: {0}",
                                                scanResult.ScannedMatrix));
                                            Context.CreateInfoResult("BarcodeRead", "The returned item's barcode.",
                                                scanResult.ScannedMatrix);
                                            Context.Send(string.Format("ReturnEnd:{0}", scanResult.ScannedMatrix));
                                            if (!IsFieldInsert)
                                            {
                                                var errorCodes3 = ScanAndWait(scanResult);
                                                if (ErrorCodes.PickerEmpty == errorCodes3 ||
                                                    ErrorCodes.PickerFull != errorCodes3)
                                                    break;
                                            }
                                        }

                                        using (var fileDiskOperation = new FileDiskOperation(this, Context))
                                        {
                                            if (fileDiskOperation.PutDiscAway(Result, scanResult.ScannedMatrix) !=
                                                ErrorCodes.Success)
                                                break;
                                            var num2 = (int)MotionService.MoveVend(MoveMode.Get, Context.AppLog);
                                            break;
                                        }
                                    }
                                case ErrorCodes.PickerObstructed:
                                    var num3 = (int)ControlService.PushOut();
                                    Context.CreateInfoResult("ItemStuckInGripper", "More than 3 sensors are lit.");
                                    AddError("Item Stuck in gripper.");
                                    break;
                                case ErrorCodes.PickerEmpty:
                                    Context.CreateInfoResult("NoReceivedItem", "No disc was received by picker.");
                                    break;
                                default:
                                    Context.Send("MachineError");
                                    Context.CreateInfoResult("MachineError", "Unable to accept disc in picker.");
                                    AddError("Machine error during accept disc.");
                                    break;
                            }
                        }
                    }
                }
            }
            finally
            {
                WaitEvent.Close();
            }
        }

        private ErrorCodes ScanAndWait(ScanResult scanResult)
        {
            using (var fraudValidator = new FraudValidator(Context))
            {
                if (FraudValidationResult.Photocopy != fraudValidator.Validate(scanResult))
                    return ErrorCodes.PickerFull;
                Context.Send(string.Format("PhotocopyBarcode:{0}", scanResult.ScannedMatrix));
                if (!WaitEvent.WaitOne(SignalWait) ||
                    ReceivedSignal.Equals("continue", StringComparison.CurrentCultureIgnoreCase))
                    return ErrorCodes.PickerFull;
                var vendItemResult = ControlService.VendItemInPicker();
                switch (vendItemResult.Status)
                {
                    case ErrorCodes.PickerFull:
                        var errorCodes = ControlService.AcceptDiskAtDoor();
                        if (ErrorCodes.PickerEmpty == errorCodes)
                        {
                            Context.AppLog.Write("The disk was taken by the user.");
                        }
                        else if (ErrorCodes.PickerFull != errorCodes)
                        {
                            Context.CreateInfoResult("ItemStuckInGripper",
                                string.Format("Accept disk at door returned error {0}",
                                    errorCodes.ToString().ToUpper()));
                            AddError("Item stuck in the vend door.");
                        }

                        return errorCodes;
                    case ErrorCodes.PickerEmpty:
                        Context.CreateInfoResult("BackwardDiscTaken", "The backward disc was taken.");
                        return vendItemResult.Status;
                    default:
                        Context.Send("MachineError");
                        Context.CreateInfoResult("MachineError", "Machine error ejecting the disc.");
                        AddError("Hardware error.");
                        return vendItemResult.Status;
                }
            }
        }
    }
}