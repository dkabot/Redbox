using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class InventoryInstruction : Instruction
    {
        public override string Mnemonic => "INVENTORY";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (Enum<InventoryOperands>.ParseIgnoringCase(operandTokens[0].Value, InventoryOperands.None) !=
                InventoryOperands.None || operandTokens[0].Value
                    .Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
                return;
            result.Errors.Add(Error.NewError("C001", "Unexpected form",
                string.Format("The operand {0} for INVENTORY isn't recognized.", operandTokens[0].Value)));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<InventoryOperands>.ParseIgnoringCase(Operands[0].Value, InventoryOperands.None);
            var inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
            var service = ServiceLocator.Instance.GetService<IDecksService>();
            if (ignoringCase == InventoryOperands.None && Operands[0].Value
                    .Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
            {
                var deckOperand = GetDeckOperand(result.Errors, context);
                if (!deckOperand.HasValue)
                    context.PushTop("ERROR");
                var byNumber = service.GetByNumber(deckOperand.Value);
                var slotRange = new Range(1, byNumber.NumberOfSlots);
                var instance = inventoryService.SwapEmptyWith(byNumber, "UNKNOWN", MerchFlags.None, slotRange);
                context.PushTop(instance);
                LogHelper.Instance.Log("Inventory: reset {0} empty slots on deck {1} to UNKNOWN", instance,
                    deckOperand.Value);
            }
            else if (InventoryOperands.IsDuplicate == ignoringCase)
            {
                var keyValuePairValue =
                    GetKeyValuePairValue<string>(Operands.GetKeyValuePair("ID"), result.Errors, context);
                if (result.Errors.Count > 0)
                    context.PushTop("FAILURE");
                else
                    context.PushTop(inventoryService.IsBarcodeDuplicate(keyValuePairValue, out _));
            }
            else if (InventoryOperands.ClearAll == ignoringCase)
            {
                service.ForAllDecksDo(_d0 =>
                {
                    inventoryService.MarkDeckInventory(_d0, "EMPTY");
                    return true;
                });
            }
            else if (InventoryOperands.EmptyCount == ignoringCase)
            {
                context.PushTop(inventoryService.GetMachineEmptyCount());
            }
            else
            {
                ILocation location;
                if (!GetLocation(result, context, out location))
                {
                    AddError("E004",
                        "Deck or slot are undefined, reissue the INVENTORY instruction specifying both deck and slot values.",
                        result.Errors);
                    context.PushTop("ERROR");
                }
                else if (InventoryOperands.GetReturnTime == ignoringCase)
                {
                    var returnDate = location.ReturnDate;
                    context.PushTop(!returnDate.HasValue ? "NONE" : (object)returnDate.Value.ToString());
                }
                else if (InventoryOperands.SetUnknown == ignoringCase)
                {
                    location.ID = "UNKNOWN";
                    inventoryService.Save(location);
                    context.PushTop(SuccessMessage);
                }
                else if (InventoryOperands.SetEmpty == ignoringCase)
                {
                    inventoryService.Reset(location);
                    context.PushTop(SuccessMessage);
                }
                else
                {
                    result.Errors.Add(Error.NewError("E999", "Invalid operand.",
                        string.Format("The operand {0} is invalid.", Operands[0].Value)));
                }
            }
        }

        private enum InventoryOperands
        {
            None,
            Deck,
            Slot,
            SetUnknown,
            SetEmpty,
            GetReturnTime,
            SoftSync,
            IsDuplicate,
            ClearAll,
            EmptyCount
        }
    }
}