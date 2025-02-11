using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class LookupInstruction : Instruction
    {
        public override string Mnemonic => "LOOKUP";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 1 && (operandTokens[0].Type == TokenType.StringLiteral ||
                                             operandTokens[0].Type == TokenType.Symbol))
                return;
            result.AddError(ExecutionErrors.InvalidOperand, "Expected a string literal or symbol.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var id = (string)null;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            if (Operands.Count == 1)
            {
                var operand = Operands[0];
                if (operand.Type == TokenType.Symbol)
                {
                    id = context.GetSymbolValue(operand.Value, result.Errors) as string;
                    if (result.Errors.Count > 0)
                        return;
                }
                else if (operand.Type == TokenType.StringLiteral)
                {
                    id = operand.Value;
                }
            }
            else if (context.StackDepth > 0)
            {
                id = context.PopTop() as string;
            }

            if (id != null)
            {
                var location = service.Lookup(id);
                if (location == null)
                {
                    context.PushTop("NOT FOUND");
                }
                else
                {
                    context.PushTop(location.Slot);
                    context.PushTop(location.Deck);
                    context.PushTop("FOUND");
                }
            }
            else
            {
                AddError("E004",
                    "LOOKUP expects a valid string on the stack, symbol, or string literal representing the ID to find.",
                    result.Errors);
            }
        }
    }
}