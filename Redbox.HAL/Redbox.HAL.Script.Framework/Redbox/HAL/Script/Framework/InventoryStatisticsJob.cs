using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-inventory-stats")]
    internal sealed class InventoryStatisticsJob : NativeJobAdapter
    {
        internal InventoryStatisticsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var applicationLog = ApplicationLog.ConfigureLog(Context, true, "Service", false, "InventoryStats");
            if (!TestInventoryStore())
                return;
            var totalMachineSlots = 0;
            DecksService.ForAllDecksDo(deck =>
            {
                if (!deck.IsQlm)
                    totalMachineSlots += deck.NumberOfSlots;
                return true;
            });
            var machineEmptyCount = InventoryService.GetMachineEmptyCount();
            var unknowns = InventoryService.GetUnknowns();
            var excludedSlotsCount = InventoryService.GetExcludedSlotsCount();
            var num1 = machineEmptyCount - ControllerConfiguration.Instance.ReturnSlotBuffer;
            if (num1 < 0)
                num1 = 0;
            applicationLog.Write("Inventory snapshot.");
            applicationLog.WriteFormatted(" Total Empty: {0}", machineEmptyCount);
            applicationLog.WriteFormatted(" Usable empty: {0}", num1);
            applicationLog.WriteFormatted(" Unknown: {0}", unknowns.Count);
            applicationLog.WriteFormatted(" Excluded: {0}", excludedSlotsCount);
            applicationLog.WriteFormatted(" Total Slots: {0}", totalMachineSlots);
            applicationLog.WriteFormatted(" Dumpbin capacity: {0}", DumpbinService.Capacity);
            applicationLog.WriteFormatted(" Dumpbin current count: {0}", DumpbinService.CurrentCount());
            Context.CreateInfoResult("TotalEmptyCount", machineEmptyCount.ToString());
            Context.CreateInfoResult("UsableEmptyCount", num1.ToString());
            var context1 = Context;
            var num2 = unknowns.Count;
            var message1 = num2.ToString();
            context1.CreateInfoResult("UnknownCount", message1);
            Context.CreateInfoResult("ExcludedCount", excludedSlotsCount.ToString());
            Context.CreateInfoResult("TotalMachineSlots", totalMachineSlots.ToString());
            var context2 = Context;
            num2 = DumpbinService.Capacity;
            var message2 = num2.ToString();
            context2.CreateInfoResult("DumpBinCapacity", message2);
            var context3 = Context;
            num2 = DumpbinService.CurrentCount();
            var message3 = num2.ToString();
            context3.CreateInfoResult("DumpBinCount", message3);
            if (!InventoryService.MachineIsFull())
                return;
            Context.CreateMachineFullResult();
        }
    }
}