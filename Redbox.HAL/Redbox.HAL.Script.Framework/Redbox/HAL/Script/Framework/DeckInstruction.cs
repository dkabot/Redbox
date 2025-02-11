using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class DeckInstruction : Instruction
    {
        public override string Mnemonic => "DECK";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (Enum<DeckOperands>.ParseIgnoringCase(operandTokens[0].Value, DeckOperands.None) != DeckOperands.None)
                return;
            result.Errors.Add(Error.NewError("C001", "Unexpected form",
                string.Format("The operand {0} for DECK isn't recognized.", operandTokens[0].Value)));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<DeckOperands>.ParseIgnoringCase(Operands[0].Value, DeckOperands.None);
            var nullableNumericOperand = GetNullableNumericOperand(result, "NUMBER", context);
            if (!nullableNumericOperand.HasValue)
            {
                AddError("E777", "Missing 'NUMBER' operand to DECK.", result.Errors);
            }
            else
            {
                var byNumber = ServiceLocator.Instance.GetService<IDecksService>()
                    .GetByNumber(nullableNumericOperand.Value);
                if (ignoringCase == DeckOperands.MaxSlots)
                {
                    context.PushTop(byNumber.NumberOfSlots);
                }
                else
                {
                    if (DeckOperands.Reset != ignoringCase)
                        return;
                    ServiceLocator.Instance.GetService<IInventoryService>().ResetAndMark(byNumber, "EMPTY");
                }
            }
        }

        private enum DeckOperands
        {
            None,
            MaxSlots,
            Reset
        }
    }
}