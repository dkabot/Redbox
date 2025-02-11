using System;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class QlmDoorInstruction : Instruction
    {
        public override string Mnemonic => "QLMDOOR";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 1)
            {
                result.AddMissingOperandError("Expected a symbol: LOCK, UNLOCK.");
            }
            else
            {
                var ignoringCase = Enum<QlmOperands>.ParseIgnoringCase(operandTokens[0].Value, QlmOperands.None);
                if (operandTokens[0].Type == TokenType.Symbol && ignoringCase != QlmOperands.None)
                    return;
                result.AddInvalidOperandError("Expected a symbol: LOCK, UNLOCK, or STATUS.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol)
                return;
            switch (Enum<QlmOperands>.ParseIgnoringCase(operand.Value, QlmOperands.None))
            {
                case QlmOperands.Lock:
                    context.PushTop(TimeoutMessage);
                    break;
                case QlmOperands.Unlock:
                    context.PushTop(TimeoutMessage);
                    break;
                case QlmOperands.Status:
                    context.PushTop("CLOSED");
                    break;
                default:
                    result.AddError(ExecutionErrors.InvalidSymbolValue,
                        string.Format("The symbol {0} is invalid.", operand.Value));
                    break;
            }
        }

        private enum QlmOperands
        {
            None,
            Lock,
            Unlock,
            Status
        }
    }
}