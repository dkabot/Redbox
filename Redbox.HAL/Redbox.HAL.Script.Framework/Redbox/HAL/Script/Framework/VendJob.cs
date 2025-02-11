using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "vend")]
    internal sealed class VendJob : NativeJobAdapter, IGetObserver
    {
        private readonly EventWaitHandle SignalEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private IGetResult CurrentGetResult;
        private VendMethod Method;
        private string ReceivedSignal;

        internal VendJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        public void OnStuck(IGetResult result)
        {
            ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
        }

        public bool OnEmpty(IGetResult result)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            if (ControllerConfiguration.Instance.PeekDuringVend)
                using (var peekOperation = new PeekOperation())
                {
                    var peekResult = peekOperation.Execute();
                    if (peekResult.TestOk && !peekResult.IsFull)
                        return true;
                    result.Update(ErrorCodes.ItemStuck);
                    service.UpdateEmptyStuck(result.Location);
                    return false;
                }

            result.Update(ErrorCodes.ItemStuck);
            service.UpdateEmptyStuck(result.Location);
            return false;
        }

        protected override void ExecuteInner()
        {
            try
            {
                Method = ConvertToMethod(Context.PopTop<string>());
                if (VendMethod.Preposition == Method)
                    RegisterForCorrectionEvents();
                var log = ApplicationLog.ConfigureLog(Context, true, "vend", false,
                    Method == VendMethod.Location ? "field-remove" : "vend");
                Context.RegisterHandler(signal =>
                {
                    ReceivedSignal = signal;
                    SignalEvent.Set();
                });
                LogHelper.Instance.WithContext("Vend start, Method = {0}", Method);
                Context.Send("VendStart");
                var num1 = Context.PopTop<int>();
                var num2 = num1;
                if (VendMethod.Location == Method)
                    using (var checkPickerOperation = new CheckPickerOperation(Context))
                    {
                        if (checkPickerOperation.CheckPicker(Result, null) != ErrorCodes.PickerEmpty)
                        {
                            AddError("Failed to put disc away.");
                            return;
                        }
                    }

                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                while (num1 > 0)
                {
                    if (num2 != num1)
                    {
                        Context.Send("VendGraphicDisplay");
                        if (ContextSwitchRequested())
                            break;
                    }

                    ILocation location;
                    string str;
                    if (VendMethod.Location == Method)
                    {
                        var deck = Context.PopTop<int>();
                        var slot = Context.PopTop<int>();
                        location = service.Get(deck, slot);
                        str = location.ID;
                    }
                    else
                    {
                        str = Context.PopTop<string>();
                        using (var checkPickerOperation = new CheckPickerOperation(Context))
                        {
                            var errorCodes = checkPickerOperation.CheckPicker(Result, str);
                            if (ErrorCodes.PickerEmpty != errorCodes)
                            {
                                LogHelper.Instance.WithContext("Check picker returned an error status {0}",
                                    errorCodes.ToString());
                                WaitForSignal();
                                AddError("Item stuck in the gripper.");
                                break;
                            }
                        }

                        if (Method == VendMethod.ID)
                            Context.Send("FindItemToVend");
                        location = service.Lookup(str);
                        if (location == null)
                        {
                            Context.Send("ItemNotFound");
                            Context.CreateLookupErrorResult("VendError", str);
                            if (--num1 <= 0)
                            {
                                WaitForSignal();
                                break;
                            }

                            continue;
                        }
                    }

                    LogHelper.Instance.WithContext(LogEntryType.Info, "Trying to vend ID: {0} at {1}", str, location);
                    var transactionResult = MoveAndGet(location, str);
                    if (TransactionResult.MachineError == transactionResult)
                    {
                        WaitForSignal();
                        OnMachineError(str);
                        break;
                    }

                    if (TransactionResult.ContinueToNext == transactionResult)
                    {
                        if (--num1 <= 0)
                        {
                            WaitForSignal();
                            break;
                        }
                    }
                    else
                    {
                        if (ControllerConfiguration.Instance.PrepositionVendFraudMove &&
                            VendMethod.Preposition == Method)
                        {
                            var num3 = (int)ServiceLocator.Instance.GetService<IMotionControlService>()
                                .MoveTo(6, location.Slot, MoveMode.None, AppLog);
                            if (!OnPrePosition(location, str))
                                break;
                        }

                        var error = MotionService.MoveVend(MoveMode.None, log);
                        if (error != ErrorCodes.Success)
                        {
                            Context.Send("MachineError");
                            WaitForSignal();
                            Context.OnMoveVendError(error, str);
                            break;
                        }

                        var num4 = (int)ControlSystem.TrackCycle();
                        if (Method == VendMethod.Preposition && !OnPrePosition(location, str))
                            break;
                        Context.Send("PickupGraphicDisplay");
                        var vendItemResult =
                            ServiceLocator.Instance.GetService<IControllerService>().VendItemInPicker();
                        if (ErrorCodes.PickerEmpty == vendItemResult.Status)
                        {
                            Context.Send("ItemVended");
                            LogHelper.Instance.WithContext("The item {0} was taken.", str);
                            Context.CreateResult("ItemVended", "The item was successfully vended.", location.Deck,
                                location.Slot, str, new DateTime?(), null);
                            --num1;
                        }
                        else
                        {
                            if (ErrorCodes.PickerFull != vendItemResult.Status)
                            {
                                OnMachineError(str);
                                break;
                            }

                            if (!vendItemResult.Presented)
                            {
                                LogHelper.Instance.WithContext(LogEntryType.Error,
                                    "The disk was not presented - home the Y axis.");
                                var num5 = (int)MotionService.HomeAxis(Axis.Y);
                                ControlSystem.RollerToPosition(RollerPosition.Position6, 3000);
                            }

                            var num6 = (int)OnFraudAttempt(location, str);
                            break;
                        }
                    }
                }
            }
            finally
            {
                if (ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0)
                {
                    var service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
                    var counter = service.Find("EmptyOrStuckCount");
                    if (counter != null && counter.Value > ControllerConfiguration.Instance.EmptyStuckAlertThreshold)
                    {
                        LogHelper.Instance.Log(LogEntryType.Error,
                            "The Empty/Stuck counter for alert generation has exceeded its threshold (value = {0})",
                            counter.Value);
                        if (ControllerConfiguration.Instance.AlertForEmptyStuck)
                        {
                            Context.CreateInfoResult("EmptyStuckThresholdExceeded",
                                "There are too many empty/stuck failures transactions on this kiosk, please investigate.");
                            service.Reset(counter);
                        }
                    }
                }

                try
                {
                    SignalEvent.Close();
                }
                catch
                {
                }

                var num = (int)MotionService.MoveVend(MoveMode.Get, Context.AppLog);
            }
        }

        private bool OnPrePosition(ILocation bcLocation, string idToFind)
        {
            if (!WaitForSignal())
            {
                FileDisk(bcLocation, idToFind);
                using (var checkPickerOperation = new CheckPickerOperation(Context))
                {
                    if (checkPickerOperation.CheckPicker(Result, idToFind) != ErrorCodes.PickerEmpty)
                    {
                        AddError("Failed to put disc away.");
                        return false;
                    }
                }

                AddError("The vend job failed with errored SIGNAL status.");
                return false;
            }

            Context.CreateResult("PrepositionedItem", "Preposition picker has item ready at vend door.",
                bcLocation.Deck, bcLocation.Slot, null, new DateTime?(), null);
            Method = VendMethod.ID;
            return true;
        }

        private ErrorCodes OnFraudAttempt(ILocation source, string id)
        {
            if (ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor() != ErrorCodes.PickerFull)
            {
                LogHelper.Instance.WithContext("Unable to move disc back to sensor 3; assume the disc is vended.");
                return FileFraudedDisk(source, id);
            }

            var num = (int)ControlSystem.TrackCycle();
            var scanResult = (ScanResult)null;
            using (var scanner = new Scanner(ControlSystem))
            {
                scanResult = scanner.Read();
            }

            using (scanResult)
            {
                if (!scanResult.SnapOk)
                {
                    Context.CreateCameraCaptureErrorResult();
                    return FileFraudedDisk(source, id);
                }

                if (!scanResult.ReadCode)
                    LogHelper.Instance.WithContext(
                        "The barcode in the picker could not be read; scanned ID = {0}; item assumed vended.",
                        scanResult.ScannedMatrix);
                if (scanResult.ScannedMatrix == id)
                {
                    LogHelper.Instance.WithContext(
                        "Item ID = {0} was not taken; the barcode in the picker matched the item requested to vend.",
                        id);
                    Context.CreateResult("UnclaimedVend",
                        "The vend request failed because the item was not taken.  It has been returned.", source.Deck,
                        source.Slot, id, new DateTime?(), null);
                    FileDisk(source, scanResult.ScannedMatrix);
                    var sensorReadResult = ControlSystem.ReadPickerSensors();
                    return !sensorReadResult.Success ? ErrorCodes.SensorReadError :
                        sensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty;
                }

                Context.AppLog.Write(string.Format(
                    "Item ID = {0} was not taken; it did not match the barcode of the item in the picker ID = {1}.", id,
                    scanResult.ScannedMatrix));
                LogHelper.Instance.WithContext(
                    "The barcode in the picker could not be matched to the requested item; returned ID = {0}; item assumed vended.",
                    scanResult.ScannedMatrix);
            }

            ControlSystem.LogInputs(ControlBoards.Picker);
            ControlSystem.LogInputs(ControlBoards.Aux);
            return FileFraudedDisk(source, id);
        }

        private ErrorCodes FileFraudedDisk(ILocation source, string id)
        {
            string.Format("Item ID = {0} was vended.", id);
            Context.CreateResult("ItemVended", "The item was successfully vended.", source.Deck, source.Slot, id,
                new DateTime?(), null);
            FileDisk(source, id);
            var sensorReadResult = ControlSystem.ReadPickerSensors();
            if (!sensorReadResult.Success)
                return ErrorCodes.SensorReadError;
            return !sensorReadResult.IsFull ? ErrorCodes.PickerEmpty : ErrorCodes.PickerFull;
        }

        private bool FileDisk(ILocation location, string id)
        {
            if (ServiceLocator.Instance.GetService<IMotionControlService>()
                    .MoveTo(location, MoveMode.Put, Context.AppLog) != ErrorCodes.Success ||
                !ServiceLocator.Instance.GetService<IControllerService>().Put(id).Success)
                return false;
            if (CurrentGetResult != null)
            {
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                location.Flags = CurrentGetResult.Flags;
                location.ReturnDate = CurrentGetResult.ReturnTime;
                var location1 = location;
                service.Save(location1);
            }

            return true;
        }

        private VendMethod ConvertToMethod(string method)
        {
            if ("PREPOSITION-PICKER" == method)
                return VendMethod.Preposition;
            if ("BY-ID" == method)
                return VendMethod.ID;
            return "BY-LOCATION" == method ? VendMethod.Location : VendMethod.None;
        }

        private TransactionResult MoveAndGet(ILocation loc, string barcode)
        {
            CurrentGetResult = null;
            LogHelper.Instance.WithContext("Move {0} to get requested barcode = {1}.", loc, barcode);
            switch (ServiceLocator.Instance.GetService<IMotionControlService>()
                        .MoveTo(loc, MoveMode.Get, Context.AppLog))
            {
                case ErrorCodes.Success:
                    var service = ServiceLocator.Instance.GetService<IControllerService>();
                    CurrentGetResult = service.Get(this);
                    if (CurrentGetResult.Success)
                        return TransactionResult.Success;
                    if (CurrentGetResult.IsSlotEmpty)
                    {
                        Context.Send("ItemNotFound");
                        Context.CreateResult("VendError", "Get returned empty slot.", loc.Deck, loc.Slot,
                            CurrentGetResult.Previous, new DateTime?(), barcode);
                        return TransactionResult.ContinueToNext;
                    }

                    if (CurrentGetResult.ItemStuck && ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0 &&
                        Method != VendMethod.Location)
                        ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment("EmptyOrStuckCount");
                    var num = (int)service.ClearGripper();
                    var sensorReadResult = ControlSystem.ReadPickerSensors();
                    if (!sensorReadResult.Success)
                    {
                        Context.Send("MachineError");
                        return TransactionResult.MachineError;
                    }

                    if (!sensorReadResult.IsFull)
                    {
                        if (ErrorCodes.GripperExtendTimeout == CurrentGetResult.HardwareError)
                        {
                            Context.AppLog.WriteFormatted(
                                "Gripper extend failed at location {0}; updating empty/stuck counter.", loc.ToString());
                            InventoryService.UpdateEmptyStuck(CurrentGetResult.Location);
                        }

                        Context.Send("GripperTimedOut");
                        Context.CreateResult("VendError", "Gripper timed out.", loc.Deck, loc.Slot,
                            CurrentGetResult.Previous, new DateTime?(), barcode);
                        return TransactionResult.ContinueToNext;
                    }

                    Context.Send("MachineError");
                    return TransactionResult.MachineError;
                case ErrorCodes.Timeout:
                    Context.CreateInfoResult("MoveFailure", "The MOVE instruction returned TIMEOUT.", barcode);
                    return TransactionResult.ContinueToNext;
                default:
                    Context.Send("MachineError");
                    return TransactionResult.MachineError;
            }
        }

        private void OnMachineError(string barcode)
        {
            var str = "An unrecoverable error occurred in vend.";
            LogHelper.Instance.WithContext(LogEntryType.Error, str);
            Context.CreateInfoResult("MachineError", str, barcode);
            AddError("MACHINE ERROR");
        }

        private bool WaitForSignal()
        {
            if (VendMethod.Preposition != Method)
                return true;
            var millisecondsTimeout = 300000;
            if (!SignalEvent.WaitOne(millisecondsTimeout))
            {
                LogHelper.Instance.WithContext("Didn't receive a signal in the specified time ( {0}ms )",
                    millisecondsTimeout);
                Context.PushTop("TIMEOUT");
                return false;
            }

            if (string.IsNullOrEmpty(ReceivedSignal))
                ReceivedSignal = "NONE";
            var flag = ReceivedSignal.Equals("continue", StringComparison.CurrentCultureIgnoreCase);
            LogHelper.Instance.WithContext("Received a signal '{0}' ( {1} ) within timeout.", ReceivedSignal,
                flag ? "expected" : (object)"not expected");
            return flag;
        }
    }
}