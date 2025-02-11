using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class MessageInstruction : Instruction
    {
        public override string Mnemonic => "MSG";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count < 1)
            {
                result.AddError(ExecutionErrors.InvalidOperand,
                    "Expected a string literal and any number of optional subtitution parameters.");
            }
            else
            {
                if (operandTokens[0].Type == TokenType.StringLiteral || operandTokens[0].Type == TokenType.ConstSymbol)
                    return;
                result.AddError(ExecutionErrors.InvalidOperand,
                    "Expected a string literal and any number of optional subtitution parameters.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (Operands.Count <= 0)
                return;
            var parameterArray = ConvertSymbolsToParameterArray(result.Errors, context);
            if (result.Errors.ContainsError())
                return;
            var message =
                string.Format(
                    Operands[0].Type == TokenType.StringLiteral
                        ? Operands[0].Value
                        : (string)context.GetSymbolValue(Operands[0].Value, result.Errors), parameterArray);
            context.Send(message);
        }
    }
}