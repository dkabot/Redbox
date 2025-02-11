using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal struct UnloadHelper : IFileDiskHandler
    {
        private readonly ExecutionContext Context;
        private readonly ExecutionResult Result;

        public void MachineFull(ExecutionResult result, string idInPicker)
        {
            Context.CreateInfoResult("QlmUnloadMachineFull",
                "The FINDEMPTY request failed because the machine is full.", idInPicker);
        }

        public FileDiscIterationResult MoveError(
            ExecutionResult result,
            string idInPicker,
            ErrorCodes moveResult,
            int deck,
            int slot)
        {
            if (moveResult == ErrorCodes.Success)
                return FileDiscIterationResult.Continue;
            if (ErrorCodes.Timeout == moveResult)
                return FileDiscIterationResult.NextLocation;
            Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck, slot,
                idInPicker, new DateTime?(), null);
            return FileDiscIterationResult.Halt;
        }

        public void DiskFiled(ExecutionResult result, IPutResult putResult)
        {
            Context.CreateResult("QlmUnloadPutIntoDrum", "An item in the picker was put away.",
                putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
        }

        public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
        {
            if (putResult.IsSlotInUse)
            {
                Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck,
                    putResult.PutLocation.Slot, putResult.StoredMatrix, new DateTime?(), null);
                return FileDiscIterationResult.Continue;
            }

            Context.CreateInfoResult("MachineError",
                "The PUT request failed; try to put the product away into an empty slot.", putResult.StoredMatrix);
            return FileDiscIterationResult.Continue;
        }

        public void FailedToFileDisk(ExecutionResult result, string idInPicker)
        {
            Context.CreateInfoResult("MachineError", "The PUT request failed; unable to put the disk away.",
                idInPicker);
        }

        internal void CheckAndClearPicker(ExecutionResult result)
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var sensorReadResult1 = service.ReadPickerSensors();
            if (!sensorReadResult1.Success)
            {
                result.Errors.Add(Error.NewError("E001", "Item stuck",
                    "There is an item stuck in the gripper; can't continue."));
            }
            else
            {
                if (!sensorReadResult1.IsFull)
                    return;
                var id = "UNKNOWN";
                if (ServiceLocator.Instance.GetService<IControllerService>().ClearGripper() != ErrorCodes.Success)
                {
                    Context.CreateItemStuckResult(id);
                    result.Errors.Add(Error.NewError("E001", "Item stuck",
                        "There is an item stuck in the gripper; can't continue."));
                }
                else
                {
                    var sensorReadResult2 = service.ReadPickerSensors();
                    if (!sensorReadResult2.Success)
                    {
                        result.Errors.Add(Error.NewError("E001", "Item stuck",
                            "There is an item stuck in the gripper; can't continue."));
                    }
                    else if (!sensorReadResult2.IsFull)
                    {
                        LogHelper.Instance.WithContext("Item was cleared from picker.");
                    }
                    else
                    {
                        var str = Scanner.ReadDiskInPicker(ReadDiskOptions.None, Context);
                        using (var fileDiskOperation = new FileDiskOperation(Context))
                        {
                            if (fileDiskOperation.PutDiscAway(result, str) == ErrorCodes.Success)
                                return;
                            Context.CreateItemStuckResult(str);
                            result.Errors.Add(Error.NewError("E001", "Item stuck",
                                "There is an item stuck in the gripper; can't continue."));
                        }
                    }
                }
            }
        }

        internal MerchandizeResult UnloadDisk(int srcDeck, int srcSlot)
        {
            var isVmzMachine = ControllerConfiguration.Instance.IsVMZMachine;
            ServiceLocator.Instance.GetService<IPersistentCounterService>();
            var service1 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var service2 = ServiceLocator.Instance.GetService<IControllerService>();
            var failCode1 = service1.MoveTo(srcDeck, srcSlot, MoveMode.Get, Context.AppLog);
            if (failCode1 != ErrorCodes.Success)
            {
                MerchandizingHelper.Instance.CleanupJob(Result, Context, failCode1);
                Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", srcDeck,
                    srcSlot, null, new DateTime?(), null);
                MerchandizingHelper.Instance.AddError(Result, "The MOVE instruction failed.");
                return MerchandizeResult.HardwareError;
            }

            var getResult = service2.Get();
            if (getResult.IsSlotEmpty)
            {
                Context.CreateResult("QlmUnloadQlmSlotEmpty", "There was no item found at this location.", srcDeck,
                    srcSlot, null, new DateTime?(), null);
                return MerchandizeResult.EmptyStuck;
            }

            if (!getResult.Success)
            {
                if (isVmzMachine && getResult.ItemStuck)
                {
                    var service3 = ServiceLocator.Instance.GetService<IInventoryService>();
                    var location = service3.Get(srcDeck, srcSlot);
                    location.Flags = MerchFlags.None;
                    service3.Save(location);
                    Context.CreateResult("UnloadEmptyStuck", "The disk at this location is stuck.", srcDeck, srcSlot,
                        null, new DateTime?(), null);
                    return MerchandizeResult.EmptyStuck;
                }

                Context.CreateResult("MachineError",
                    string.Format("A hardware error occurred in the get from {0}.",
                        isVmzMachine ? "VMZ" : (object)"QLM"), srcDeck, srcSlot, null, new DateTime?(), null);
                MerchandizingHelper.Instance.AddError(Result,
                    string.Format("An error occurred getting an item from the {0}.",
                        isVmzMachine ? "VMZ" : (object)"QLM"));
                if (service2.ClearGripper() == ErrorCodes.Success)
                    MerchandizingHelper.Instance.CleanupJob(Result, Context, ErrorCodes.Success);
                return MerchandizeResult.HardwareError;
            }

            var str = "UNKNOWN";
            using (var scanResult = Scanner.SmartReadDisk())
            {
                str = scanResult.ScannedMatrix;
                if (!scanResult.SnapOk)
                {
                    if (ControllerConfiguration.Instance.IsVMZMachine)
                        Result.Errors.Add(Error.NewError("E997", "CAMERA CAPTURE error.", "The camera isn't working."));
                }
                else if (!scanResult.ReadCode)
                {
                    Context.CreateResult("QlmUnloadBadRead",
                        "The READ request failed because no barcode could be read.", srcDeck, srcSlot, null,
                        new DateTime?(), null);
                }
            }

            Context.CreateResult("QlmUnloadBarcodeFromQlm",
                string.Format("The item pulled from the {0} has the following barcode.",
                    isVmzMachine ? "VMZ" : (object)"QLM"), srcDeck, srcSlot, str, new DateTime?(), null);
            if (ControllerConfiguration.Instance.IsVMZMachine)
            {
                var failCode2 = service1.MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
                if (failCode2 != ErrorCodes.Success)
                {
                    MerchandizingHelper.Instance.CleanupJob(Result, Context, failCode2);
                    MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
                    return MerchandizeResult.HardwareError;
                }

                var putResult = service2.Put(str);
                if (putResult.Success)
                {
                    DiskFiled(Result, putResult);
                    return MerchandizeResult.Success;
                }
            }

            return FileUnload(str, srcDeck, srcSlot);
        }

        internal UnloadHelper(ExecutionContext context, ExecutionResult result)
        {
            Context = context;
            Result = result;
        }

        private MerchandizeResult FileUnload(string readMatrix, int srcDeck, int srcSlot)
        {
            using (var fileDiskOperation = new FileDiskOperation(this, Context))
            {
                var errorCodes = fileDiskOperation.PutDiscAway(Result, readMatrix);
                if (errorCodes == ErrorCodes.Success)
                    return MerchandizeResult.Success;
                var failCode = ServiceLocator.Instance.GetService<IMotionControlService>()
                    .MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
                if (failCode != ErrorCodes.Success)
                {
                    MerchandizingHelper.Instance.CleanupJob(Result, Context, failCode);
                    MerchandizingHelper.Instance.ExitOnMoveError(Result, Context);
                    return MerchandizeResult.HardwareError;
                }

                var num = (int)ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
                if (!ServiceLocator.Instance.GetService<IControllerService>().Put(readMatrix).Success)
                    Context.CreateResult("MachineError", "There was an error executing the PUT instruction.", srcDeck,
                        srcSlot, readMatrix, new DateTime?(), null);
                MerchandizingHelper.Instance.CleanupJob(Result, Context, ErrorCodes.Success);
                MerchandizingHelper.Instance.AddError(Result, "Unable to put disk away.");
                return errorCodes == ErrorCodes.MachineFull
                    ? MerchandizeResult.MachineFull
                    : MerchandizeResult.HardwareError;
            }
        }
    }
}