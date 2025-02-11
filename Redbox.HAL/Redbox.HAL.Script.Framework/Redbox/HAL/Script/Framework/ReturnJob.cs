using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class ReturnJob : NativeJobAdapter, IFileDiskHandler
    {
        private static int m_cameraErrors;

        private static readonly CenterDiskMethod[] CenterMethods = new CenterDiskMethod[2]
        {
            CenterDiskMethod.None,
            CenterDiskMethod.DrumAndBack
        };

        private bool CameraErrored;

        internal ReturnJob(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        internal static int CameraErrorCount =>
            !ControllerConfiguration.Instance.FixCameraAfterReturn ? 0 : m_cameraErrors;

        public void MachineFull(ExecutionResult result, string id)
        {
            Context.Send(nameof(MachineFull));
        }

        public FileDiscIterationResult MoveError(
            ExecutionResult result,
            string idInPicker,
            ErrorCodes moveResult,
            int deck,
            int slot)
        {
            if (ErrorCodes.Timeout == moveResult)
                return FileDiscIterationResult.NextLocation;
            Context.Send("MachineError");
            Context.CreateResult("MachineError", "The MOVE instruction failed.", deck, slot, idInPicker,
                new DateTime?(), null);
            return FileDiscIterationResult.Halt;
        }

        public void DiskFiled(ExecutionResult result, IPutResult putResult)
        {
            Context.Send(string.Format("ItemPlacementDone:{0}", putResult.StoredMatrix));
            Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck,
                putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            if (!InventoryConstants.CodeIsUnknown(putResult.StoredMatrix))
                return;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            putResult.PutLocation.ReturnDate = DateTime.Now;
            var putLocation = putResult.PutLocation;
            service.Save(putLocation);
        }

        public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
        {
            if (putResult.IsSlotInUse)
            {
                Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck,
                    putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
                return FileDiscIterationResult.Continue;
            }

            Context.Send("MachineError");
            Context.CreateResult(putResult.ToString().ToUpper(), "The PUT instruction failed.",
                putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
            return FileDiscIterationResult.Halt;
        }

        public void FailedToFileDisk(ExecutionResult result, string idInPicker)
        {
            Context.Send("MaxPutAwayAttemptsExceeded");
        }

        internal static void ResetCameraErrorCounter()
        {
            m_cameraErrors = 0;
        }

        protected override void ExecuteInner()
        {
            ServiceLocator.Instance.GetService<IRuntimeService>();
            var scannedId = "UNKNOWN";
            var log = ApplicationLog.ConfigureLog(Context, true, "Return", false, null);
            try
            {
                LogHelper.Instance.WithContext("Return start.");
                var sensorReadResult1 = ControlSystem.ReadPickerSensors();
                if (!sensorReadResult1.Success)
                {
                    Context.CreateInfoResult("MachineError", "Unable to read picker sensors.");
                    AddError("Unable to read picker sensors.");
                }
                else
                {
                    if (sensorReadResult1.IsFull)
                        using (var checkPickerOperation = new CheckPickerOperation(Context))
                        {
                            if (checkPickerOperation.CheckPicker(Result, null) != ErrorCodes.PickerEmpty)
                            {
                                AddError("Failed to put disk away.");
                                return;
                            }
                        }

                    if (InventoryService.MachineIsFull())
                    {
                        Context.Send("MachineFull");
                    }
                    else if (MotionService.MoveVend(MoveMode.Get, log) != ErrorCodes.Success)
                    {
                        OnMoveVendError();
                    }
                    else
                    {
                        if (ContextSwitchRequested())
                            return;
                        Context.Send("DisableCancel");
                        Context.Send("BlockingOnSensor");
                        var errorCodes1 = ServiceLocator.Instance.GetService<IControllerService>().AcceptDiskAtDoor();
                        if (ErrorCodes.SensorReadError == errorCodes1)
                        {
                            Context.Send("MachineError");
                            AddError("Machine error during accept disc.");
                        }
                        else if (ErrorCodes.PickerEmpty == errorCodes1)
                        {
                            log.Write("An item was not put in the picker.");
                        }
                        else if (ErrorCodes.VendDoorRentTimeout == errorCodes1)
                        {
                            Context.Send("MachineError");
                            Context.CreateInfoResult("MachineError", "Unable to set the vend door to RENT.");
                        }
                        else
                        {
                            if (ErrorCodes.PickerObstructed == errorCodes1 ||
                                ErrorCodes.VendDoorCloseTimeout == errorCodes1)
                            {
                                var errorCodes2 = RejectDisk();
                                if (ErrorCodes.PickerEmpty == errorCodes2 || ErrorCodes.PickerFull != errorCodes2)
                                    return;
                            }

                            LogHelper.Instance.WithContext("Item received into picker.");
                            var num = (int)ControlSystem.TrackCycle();
                            if (!ReadDisk(out scannedId))
                            {
                                Context.Send("ItemInsertError");
                                ReturnToUserOrFile(FileDiskInKiosk, o => { }, scannedId);
                            }
                            else if (ServiceLocator.Instance.GetService<IBarcodeValidatorService>().IsValid(scannedId))
                            {
                                if (ControllerConfiguration.Instance.MarkDuplicatesUnknown &&
                                    ControllerConfiguration.Instance.LeaveDuplicateResultInReturn &&
                                    InventoryService.IsBarcodeDuplicate(scannedId, out _))
                                    Context.CreateInfoResult("CameraCaptureFailure",
                                        "Return has detected a duplicate barcode.");
                                FileDiskInKiosk(scannedId);
                            }
                            else
                            {
                                Context.Send("UnrecognizedBarcodeReturned");
                                Context.AppLog.WriteFormatted(
                                    string.Format("The barcode {0} is not a recognized Redbox barcode.", scannedId));
                                ReturnToUserOrFile(FileUnrecognizedDisk,
                                    barcode => Context.CreateInfoResult("NonRedboxDiskReturnedToUser",
                                        "The disk was returned to the user.", barcode), scannedId);
                            }

                            var sensorReadResult2 = ControlSystem.ReadPickerSensors();
                            if (!sensorReadResult2.Success || sensorReadResult2.IsFull)
                                return;
                            if (MotionService.MoveVend(MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
                                AddError("Unable to move to the vend position.");
                            else
                                Context.Send("Bail");
                        }
                    }
                }
            }
            finally
            {
                if (ControllerConfiguration.Instance.IsVMZMachine)
                {
                    Context.CreateInfoResult("EmptySlotCount",
                        ServiceLocator.Instance.GetService<IInventoryService>().GetMachineEmptyCount().ToString());
                    Context.CreateInfoResult("DumpBinRemainingCapacity",
                        ServiceLocator.Instance.GetService<IDumpbinService>().RemainingSpace().ToString());
                    Context.CreateInfoResult("ReturnSlotBufferValue",
                        ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
                }

                if (CameraErrored && ControllerConfiguration.Instance.FixCameraAfterReturn)
                    ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().Restart();
            }
        }

        private void ReturnToUserOrFile(
            Action<string> onFileDisk,
            Action<string> onDiskReturned,
            string barcode)
        {
            if (MotionService.MoveVend(MoveMode.None, Context.AppLog) != ErrorCodes.Success)
            {
                OnMoveVendError();
            }
            else
            {
                LogHelper.Instance.WithContext("Start pushing item out of picker.");
                var service = ServiceLocator.Instance.GetService<IControllerService>();
                var vendItemResult = service.VendItemInPicker();
                if (ErrorCodes.PickerEmpty == vendItemResult.Status)
                {
                    onDiskReturned(barcode);
                    Context.AppLog.WriteFormatted("The disk was returned to the user.");
                }
                else if (ErrorCodes.PickerFull != vendItemResult.Status)
                {
                    Context.Send("MachineError");
                    AddError("Hardware error.");
                }
                else
                {
                    LogHelper.Instance.WithContext("Disk not taken; accepting disk into kiosk.");
                    var errorCodes = service.AcceptDiskAtDoor();
                    if (ErrorCodes.PickerEmpty == errorCodes)
                    {
                        onDiskReturned(barcode);
                        Context.AppLog.WriteFormatted("The disk was returned to the user.");
                    }
                    else if (ErrorCodes.PickerFull == errorCodes)
                    {
                        Context.AppLog.Write("Customer did not take disc; accept into kiosk.");
                        var num = (int)ControlSystem.TrackCycle();
                        onFileDisk(barcode);
                    }
                    else
                    {
                        if (ErrorCodes.PickerObstructed != errorCodes && ErrorCodes.VendDoorCloseTimeout != errorCodes)
                            return;
                        if (ErrorCodes.PickerFull == RejectDisk())
                        {
                            var num = (int)ControlSystem.TrackCycle();
                            onFileDisk(barcode);
                        }
                    }
                }
            }
        }

        private void FileUnrecognizedDisk(string matrix)
        {
            var appLog = Context.AppLog;
            if (ControllerConfiguration.Instance.IsVMZMachine)
            {
                var service = ServiceLocator.Instance.GetService<IDumpbinService>();
                if (service.IsFull())
                {
                    FileDiskInKiosk("UNKNOWN");
                }
                else
                {
                    var errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>()
                        .MoveTo(service.PutLocation, MoveMode.Put, appLog);
                    if (errorCodes != ErrorCodes.Success)
                    {
                        Context.CreateInfoResult("MachineError",
                            string.Format("MOVE failed with error {0}", errorCodes.ToString()));
                        AddError("The MOVE instruction failed.");
                    }
                    else
                    {
                        var putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(matrix);
                        if (!putResult.Success)
                        {
                            Context.CreateInfoResult("PutError",
                                string.Format("PUT failed with error {0}", putResult));
                            AddError("The PUT instruction failed.");
                        }
                        else
                        {
                            Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", matrix);
                            appLog.WriteFormatted("The barcode {0} was put into the dump bin.", matrix);
                        }
                    }
                }
            }
            else
            {
                FileDiskInKiosk("UNKNOWN");
            }
        }

        private void FileDiskInKiosk(string scannedId)
        {
            ServiceLocator.Instance.GetService<IPersistentCounterService>();
            LogHelper.Instance.WithContext("The ID in the picker: {0}", scannedId);
            using (var fileDiskOperation = new FileDiskOperation(this, Context))
            {
                if (fileDiskOperation.PutDiscAway(Result, scannedId) == ErrorCodes.Success)
                    return;
                AddError("Failed to file disk.");
            }
        }

        private ErrorCodes RejectDisk()
        {
            var errorCodes = ServiceLocator.Instance.GetService<IControllerService>().RejectDiskInPicker();
            switch (errorCodes)
            {
                case ErrorCodes.VendDoorRentTimeout:
                case ErrorCodes.VendDoorCloseTimeout:
                case ErrorCodes.SensorReadError:
                    Context.Send("MachineError");
                    AddError("MACHINE ERROR");
                    break;
                case ErrorCodes.PickerObstructed:
                    Context.CreateItemStuckResult(null);
                    AddError("There is an item stuck in the gripper.");
                    break;
            }

            return errorCodes;
        }

        private void OnMoveVendError()
        {
            Context.Send("MachineError");
            Context.CreateInfoResult("MachineError", "Unable to move to the vend position.");
            AddError("Unable to move to the vend position.");
        }

        private bool ReadDisk(out string scannedId)
        {
            return !ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice()
                .SupportsSecureReads
                ? OnReadWithLegacy(out scannedId)
                : OnReadWithIR(out scannedId);
        }

        private bool OnReadWithIR(out string scannedId)
        {
            scannedId = "UNKNOWN";
            var methods = new CenterDiskMethod[CenterMethods.Length];
            Array.Copy(CenterMethods, methods, CenterMethods.Length);
            Context.ContextLog.WriteFormatted("Number Of Center Methods before resize {0}", methods.Length);
            Scanner.CheckAndResizeForAdditionalFraudAttempts(ref methods);
            Context.AppLog.WriteFormatted("Number Of Center Methods: {0}, RetryEnabled: {1}, AdditionalAttempts: {2}",
                methods.Length, ControllerConfiguration.Instance.RetryReadOnNoMarkersFound,
                ControllerConfiguration.Instance.AdditionalFraudReadAttempts);
            Context.ContextLog.WriteFormatted("Number Of Center Methods {0}", methods.Length);
            for (var index = 0; index < methods.Length; ++index)
            {
                var num1 = (int)ControlSystem.Center(methods[index]);
                using (var scanner = new Scanner(ControlSystem))
                {
                    using (var scanResult = scanner.Read())
                    {
                        scanResult.ReadAttempts = index + 1;
                        if (!scanResult.SnapOk)
                        {
                            Scanner.CreateJsonResult(scanResult);
                            return OnCameraFailure();
                        }

                        m_cameraErrors = 0;
                        if (index + 1 < methods.Length && ControllerConfiguration.Instance.RetryReadOnNoMarkersFound &&
                            scanResult.SecureTokensFound <= 0)
                        {
                            Context.AppLog.WriteFormatted("{0} - ReturnJob Retry Read Image - No Secure Tokens Found.",
                                index);
                        }
                        else
                        {
                            if (scanResult.ReadCode)
                            {
                                using (var fraudValidator = new FraudValidator(Context))
                                {
                                    var num2 = (int)fraudValidator.Validate(scanResult);
                                    scannedId = scanResult.ScannedMatrix;
                                }

                                Scanner.CreateJsonResult(scanResult);
                                return OnBarcodeRead(scannedId);
                            }

                            if (index + 1 > methods.Length)
                                Scanner.CreateJsonResult(scanResult);
                        }
                    }
                }
            }

            scannedId = "UNKNOWN";
            return OnNoRead(scannedId);
        }

        private bool OnReadWithLegacy(out string scannedId)
        {
            scannedId = "UNKNOWN";
            using (var scanner = new Scanner(ControlSystem))
            {
                for (var index = 0; index < CenterMethods.Length; ++index)
                {
                    var num = (int)ControlSystem.Center(CenterMethods[index]);
                    using (var result = scanner.Read())
                    {
                        result.ReadAttempts = index + 1;
                        if (!result.SnapOk)
                        {
                            Scanner.CreateJsonResult(result);
                            return OnCameraFailure();
                        }

                        m_cameraErrors = 0;
                        if (result.ReadCode)
                        {
                            scannedId = result.ScannedMatrix;
                            Scanner.CreateJsonResult(result);
                            result.CopyToFraudFolder();
                            return OnBarcodeRead(scannedId);
                        }

                        if (index + 1 > CenterMethods.Length)
                            Scanner.CreateJsonResult(result);
                    }
                }
            }

            scannedId = "UNKNOWN";
            return OnNoRead(scannedId);
        }

        private bool OnNoRead(string scannedId)
        {
            Context.CreateInfoResult("BadIdRead", "The READ request failed.", scannedId);
            return false;
        }

        private bool OnCameraFailure()
        {
            CameraErrored = true;
            if (!ControllerConfiguration.Instance.FixCameraAfterReturn ||
                ++m_cameraErrors >= ControllerConfiguration.Instance.ReturnCameraAlertThreshold)
                Context.CreateInfoResult("CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.");
            return false;
        }

        private bool OnBarcodeRead(string matrix)
        {
            Context.Send(string.Format("ReturnEnd:{0}", matrix));
            return true;
        }
    }
}