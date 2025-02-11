using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class WaitInstruction : Instruction
    {
        public override string Mnemonic => "WAIT";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 1)
            {
                result.AddMissingOperandError("The WAIT mnemonic requires a single numeric literal or symbol.");
            }
            else
            {
                if (operandTokens[0].Type == TokenType.NumericLiteral || operandTokens[0].Type == TokenType.Symbol)
                    return;
                result.AddInvalidOperandError("Expected a numeric literal or symbol.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ms = 0;
            var symbolValue = (object)Operands[0].Value;
            if (Operands[0].Type == TokenType.Symbol)
            {
                symbolValue = context.GetSymbolValue(Operands[0].Value, result.Errors);
                if (symbolValue == null)
                    return;
            }

            int result1;
            if (int.TryParse(symbolValue.ToString(), out result1))
                ms = result1;
            ServiceLocator.Instance.GetService<IRuntimeService>().Wait(ms);
        }
    }
}