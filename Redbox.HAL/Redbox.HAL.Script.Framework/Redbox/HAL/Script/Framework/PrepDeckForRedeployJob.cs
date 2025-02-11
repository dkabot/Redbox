using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "prep-deck-for-redeploy", Operand = "PREP-DECK-FOR-REDEPLOY")]
    internal sealed class PrepDeckForRedeployJob : NativeJobAdapter
    {
        internal PrepDeckForRedeployJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var num1 = Context.PopTop<int>();
            var num2 = 0;
            var isVmzMachine = ControllerConfiguration.Instance.IsVMZMachine;
            for (var index = 0; index < num1; ++index)
            {
                var number = Context.PopTop<int>();
                var byNumber = DecksService.GetByNumber(number);
                if (byNumber == null)
                {
                    LogHelper.Instance.Log("Invalid deck '{0}' specified to prep redeploy deck job.", number);
                }
                else if (!isVmzMachine && byNumber.IsQlm)
                {
                    LogHelper.Instance.Log("Deck specified '{0}' is QLM; will not alter.", number);
                }
                else
                {
                    var slotRange = new Range(isVmzMachine ? VMZ.Instance.Bounds.End + 1 : 1, byNumber.NumberOfSlots);
                    var intList = InventoryService.SwapEmptyWith(byNumber, "UNKNOWN", MerchFlags.None, slotRange);
                    foreach (var num3 in intList)
                        Context.CreateResult("LocationChanged", "The location was set to UNKNOWN", number, num3, null,
                            new DateTime?(), null);
                    num2 += intList.Count;
                    LogHelper.Instance.Log("Inventory: changed {0} empty slots on deck {1} to UNKNOWN", intList.Count,
                        number);
                    intList.Clear();
                }
            }

            Context.CreateInfoResult("LocationChangedCount", num2.ToString());
        }
    }
}