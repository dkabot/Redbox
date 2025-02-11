using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "vend-unknown", Operand = "VEND-UNKNOWN")]
    internal sealed class VendUnknownJob : NativeJobAdapter, IGetObserver
    {
        private const string HW_ERR_MSG =
            "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.";

        internal VendUnknownJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        public void OnStuck(IGetResult result)
        {
        }

        public bool OnEmpty(IGetResult result)
        {
            if (!ControllerConfiguration.Instance.TestSlotDuringRemoval)
            {
                result.Update(ErrorCodes.ItemStuck);
                return false;
            }

            using (var peekOperation = new PeekOperation())
            {
                var peekResult = peekOperation.Execute();
                if (!peekResult.TestOk)
                {
                    result.Update(peekResult.Error);
                    return false;
                }

                if (!peekResult.IsFull)
                    return true;
                result.Update(ErrorCodes.ItemStuck);
                return false;
            }
        }

        protected override void ExecuteInner()
        {
            ApplicationLog.ConfigureLog(Context, true, "Unknowns", false, "VendUnknown").Write("Vend unknown start.");
            Context.Send("VendStart");
            ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(30);
            Context.Send("VendGraphicDisplay");
            using (var checkPickerOperation = new CheckPickerOperation(Context))
            {
                if (ErrorCodes.PickerEmpty != checkPickerOperation.CheckPicker(Result, null))
                {
                    AddError("Unable to clear the picker.");
                    return;
                }
            }

            var deck = Context.PopTop<int>();
            var slot = Context.PopTop<int>();
            var service1 = ServiceLocator.Instance.GetService<IInventoryService>();
            var location = service1.Get(deck, slot);
            LogHelper.Instance.Log("Move to {0} to pick requested item.", location.ToString());
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            if (service2.MoveTo(deck, slot, MoveMode.Get, Context.AppLog) != ErrorCodes.Success)
            {
                Context.Send("MachineError");
                Context.CreateResult("MoveError",
                    "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.",
                    deck, slot, null, new DateTime?(), null);
                AddError("MACHINE ERROR");
            }
            else
            {
                var service3 = ServiceLocator.Instance.GetService<IControllerService>();
                var getResult = service3.Get(this);
                if (getResult.IsSlotEmpty || getResult.ItemStuck)
                {
                    if (!ControllerConfiguration.Instance.RotateDrumDuringUnknownRemoval2)
                    {
                        Context.CreateResult("SlotEmpty", "The slot is empty.", deck, slot, null, new DateTime?(),
                            null);
                        VendEnd();
                    }
                    else if (getResult.IsSlotEmpty)
                    {
                        Context.AppLog.WriteFormatted("Slot is empty, moving to next.");
                        Context.CreateResult("SlotEmpty", "The slot is empty.", deck, slot, null, new DateTime?(),
                            null);
                        VendEnd();
                    }
                    else
                    {
                        LogHelper.Instance.Log("Disc stuck in slot, moving to next.");
                        Context.CreateResult("GetAfterPeekShowsFull", "This Location was NOT updated in inventory.",
                            deck, slot, getResult.Previous, getResult.ReturnTime, null);
                        ShowEmptyStuck(location);
                    }
                }
                else if (!getResult.Success)
                {
                    Context.CreateResult("GetError",
                        "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.",
                        deck, slot, null, new DateTime?(), null);
                    AddError("MACHINE ERROR");
                }
                else
                {
                    Context.Send("MoveToVend");
                    if (MotionService.MoveVend(MoveMode.None, Context.AppLog) != ErrorCodes.Success)
                    {
                        Context.Send("MachineError");
                        Context.CreateResult("MoveError",
                            "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.",
                            deck, slot, null, new DateTime?(), null);
                        AddError("MACHINE ERROR");
                    }
                    else
                    {
                        Context.Send("PickupGraphicDisplay");
                        var vendItemResult = service3.VendItemInPicker();
                        if (vendItemResult.Status == ErrorCodes.PickerEmpty)
                        {
                            Context.Send("ItemVended");
                            Context.AppLog.Write("The item was taken.");
                            Context.CreateResult("ItemVended", "The item was successfully vended.", deck, slot,
                                getResult.Previous, getResult.ReturnTime, null);
                            VendEnd();
                        }
                        else if (vendItemResult.Status != ErrorCodes.PickerFull)
                        {
                            Context.Send("MachineError");
                            Context.CreateInfoResult("MachineError",
                                "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.");
                            AddError("MACHINE ERROR");
                        }
                        else
                        {
                            LogHelper.Instance.Log("The item was not taken; pull it in and file it away.");
                            if (ErrorCodes.PickerFull != service3.AcceptDiskAtDoor())
                            {
                                Context.CreateResult("RollInToSensor3TimedOut",
                                    "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.",
                                    deck, slot, null, new DateTime?(), null);
                                AddError("MACHINE ERROR");
                            }
                            else if (service2.MoveTo(location, MoveMode.Put, Context.AppLog) != ErrorCodes.Success)
                            {
                                Context.Send("MachineError");
                                Context.CreateResult("MoveError",
                                    "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.",
                                    deck, slot, null, new DateTime?(), null);
                                AddError("MACHINE ERROR");
                            }
                            else
                            {
                                var num = (int)ControlSystem.TrackCycle();
                                if (!service3.Put("UNKNOWN").Success)
                                {
                                    Context.CreateResult("PutError",
                                        "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.",
                                        deck, slot, null, new DateTime?(), null);
                                    AddError("MACHINE ERROR");
                                }
                                else
                                {
                                    location.ReturnDate = getResult.ReturnTime;
                                    service1.Save(location);
                                    Context.CreateResult("UnclaimedVend",
                                        "The vend request failed because the item was not taken.  It has been returned.",
                                        deck, slot, getResult.Previous, new DateTime?(), null);
                                    VendEnd();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void VendEnd()
        {
            ControlSystem.VendDoorClose();
            ControlSystem.TrackClose();
            var num = (int)MotionService.MoveVend(MoveMode.Get, Context.AppLog);
        }
    }
}