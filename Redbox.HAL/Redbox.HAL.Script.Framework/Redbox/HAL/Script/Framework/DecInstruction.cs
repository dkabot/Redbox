using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class DecInstruction : Instruction
    {
        public override string Mnemonic => "DEC";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 1)
            {
                result.AddInvalidOperandError(string.Format("The {0} instruction expects one operand.", Mnemonic));
            }
            else
            {
                if (operandTokens[0].Type != TokenType.ConstSymbol)
                    return;
                result.AddError(ExecutionErrors.InvalidOperand, "Illegal attempt to " + Mnemonic + " a CONST symbol.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol)
                return;
            var d = context.GetSymbolValue(operand.Value, result.Errors);
            if (d == null || result.Errors.Count > 0)
                return;
            var type = d.GetType();
            if (type != typeof(int) && type != typeof(decimal))
            {
                AddError("E002", "Only numeric values of type integer and decimal may be decremented.", result.Errors);
            }
            else
            {
                if (type == typeof(int))
                    d = (int)d - 1;
                else if (type == typeof(decimal))
                    d = (decimal)d - 1;
                context.SetSymbolValue(operand.Value, Convert.ChangeType(d, type));
            }
        }
    }
}