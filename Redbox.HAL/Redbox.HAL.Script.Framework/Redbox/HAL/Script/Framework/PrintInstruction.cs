using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class PrintInstruction : Instruction
    {
        public override string Mnemonic => "PRINT";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Type == TokenType.StringLiteral)
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected a string literal and any number of optional subtitution parameters.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (Operands.Count > 0)
            {
                var parameterArray = ConvertSymbolsToParameterArray(result.Errors, context);
                if (result.Errors.Count > 0)
                    return;
                Console.WriteLine(Operands[0].Value, parameterArray);
            }
            else
            {
                if (context.StackDepth <= 0)
                    return;
                var obj = context.PopTop();
                if (obj == null)
                    return;
                Console.WriteLine(obj);
            }
        }
    }
}