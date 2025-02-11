using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SubtractInstruction : Instruction
    {
        public override string Mnemonic => "SUB";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 2)
            {
                result.AddInvalidOperandError(
                    "Expectd a symbol as target of the subtraction and either a numeric literal or symbol as the subtrand.");
            }
            else if (operandTokens[0].Type != TokenType.Symbol)
            {
                result.AddInvalidOperandError("Expected a (non-CONST) symbol as target of the subtraction operation.");
            }
            else
            {
                if (operandTokens[1].Type == TokenType.NumericLiteral || operandTokens[1].Type == TokenType.Symbol ||
                    operandTokens[1].Type == TokenType.ConstSymbol)
                    return;
                result.AddInvalidOperandError("Expectd either a numeric literal or symbol as the subtrand.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand1 = Operands[0];
            var d1 = context.GetSymbolValue(operand1.Value, result.Errors);
            if (d1 == null || result.Errors.Count > 0)
                return;
            var d2 = (object)0;
            var operand2 = Operands[1];
            if (operand2.Type == TokenType.NumericLiteral)
            {
                d2 = operand2.ConvertValue();
            }
            else if (operand2.Type == TokenType.Symbol || operand2.Type == TokenType.ConstSymbol)
            {
                d2 = context.GetSymbolValue(operand2.Value, result.Errors);
                if (d2 == null || result.Errors.Count > 0)
                    return;
            }

            var type = d1.GetType();
            if (type != typeof(int) && type != typeof(decimal))
            {
                AddError("E004", "Only numeric values of type integer and decimal may be subtracted.", result.Errors);
            }
            else
            {
                if (type == typeof(int))
                    d1 = (int)decimal.Subtract(Convert.ToDecimal(d1), Convert.ToDecimal(d2));
                else if (type == typeof(decimal))
                    d1 = decimal.Subtract((decimal)d1, (decimal)d2);
                context.SetSymbolValue(operand1.Value, d1);
            }
        }
    }
}