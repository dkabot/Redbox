using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "prep-zone-for-unload", Operand = "PREP-ZONE-FOR-UNLOAD")]
    internal sealed class PrepZoneForUnloadJob : NativeJobAdapter
    {
        internal PrepZoneForUnloadJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "VMZ", false, null);
            var num = Context.PopTop<int>();
            var deckCount = Context.PopTop<int>();
            var totalReset = 0;
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                AddError("Prep Zone for unload is supported only on VMZ machines. I'm sorry.");
                applicationLog.Write("Prep Zone for unload is supported only on VMZ machines. I'm sorry.");
            }
            else
            {
                var doorSensorResult = ServiceLocator.Instance.GetService<IDoorSensorService>().Query();
                if (doorSensorResult != DoorSensorResult.Ok)
                {
                    var str = string.Format("Door sensor query returned error {0}", doorSensorResult.ToString());
                    AddError(str);
                    applicationLog.Write(str);
                    Context.CreateInfoResult("DoorOpen", str);
                }
                else
                {
                    var first = DecksService.First;
                    if (num >= first.Quadrants.Count)
                    {
                        AddError("The drum as fully rotated.");
                        applicationLog.Write("The drum is fully rotated.");
                        Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
                    }
                    else
                    {
                        var index1 = num > 0 ? first.Quadrants.Count - num : 0;
                        var currentQuadrant = first.Quadrants[index1];
                        DecksService.ForAllDecksDo(deck =>
                        {
                            if (deckCount > 0 && deck.Number > deckCount)
                                return false;
                            totalReset += InventoryService
                                .SwapEmptyWith(deck, "UNKNOWN", MerchFlags.Unload, currentQuadrant.Slots).Count;
                            return true;
                        });
                        applicationLog.WriteFormatted("{0} locations were marked for unload covering slots {1} -> {2}.",
                            totalReset, currentQuadrant.Slots.Start, currentQuadrant.Slots.End);
                        Context.CreateInfoResult("LocationChangedCount", totalReset.ToString());
                        if (num == 0)
                            index1 = first.Quadrants.Count;
                        int index2;
                        if ((index2 = index1 - 1) == 0)
                        {
                            AddError("The drum as fully rotated.");
                            applicationLog.Write("The drum is fully rotated.");
                            Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
                        }
                        else
                        {
                            var slot = first.Quadrants[index2].Slots.Start - 5;
                            if (slot <= 0)
                                slot = first.NumberOfSlots - Math.Abs(slot);
                            applicationLog.Write(string.Format(
                                "PrepZone: Rotations = {0}, move to target Deck = {1} Slot = {2}", num, first.Number,
                                slot));
                            var errorCodes = MotionService.MoveTo(1, slot, MoveMode.None, Context.AppLog);
                            if (errorCodes == ErrorCodes.Success)
                                return;
                            var str = string.Format("The MOVE instruction failed with error {0}",
                                errorCodes.ToString());
                            Context.CreateInfoResult("MachineError", str);
                            AddError(str);
                        }
                    }
                }
            }
        }
    }
}