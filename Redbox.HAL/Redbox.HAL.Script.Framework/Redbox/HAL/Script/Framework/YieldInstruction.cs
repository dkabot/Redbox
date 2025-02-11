using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class YieldInstruction : Instruction
    {
        public override string Mnemonic => "YIELD";

        public override bool IsCooperativeMultitask => true;

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override bool IsOperandRecognized(string operand)
        {
            return "RUNTIME-ONLY".Equals(operand);
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 0 || operandTokens[0].Type == TokenType.NumericLiteral ||
                operandTokens[0].IsSymbolOrConst)
                return;
            result.AddInvalidOperandError("Expected a numeric literal.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (Operands.Count == 1)
            {
                var nullable = new double?();
                if (Operands[0].IsSymbolOrConst)
                {
                    var symbolValue = context.GetSymbolValue(Operands[0].Value, result.Errors);
                    if (symbolValue == null || result.Errors.Count > 0)
                        return;
                    if (symbolValue is int || symbolValue is decimal)
                        nullable = (double)Convert.ChangeType(symbolValue, typeof(double));
                }
                else if (Operands[0].Type == TokenType.NumericLiteral)
                {
                    nullable = (double)Convert.ChangeType(Operands[0].ConvertValue(), typeof(double));
                }

                if (nullable.HasValue)
                {
                    var dateTime = DateTime.Now.AddMilliseconds(nullable.Value);
                    context.StartTime = dateTime;
                }
            }

            ExecutionEngine.Instance.PerformContextSwitch(false);
        }
    }
}