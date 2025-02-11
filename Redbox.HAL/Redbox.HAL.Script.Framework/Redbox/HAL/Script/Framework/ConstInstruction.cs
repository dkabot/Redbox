using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class ConstInstruction : Instruction
    {
        public override string Mnemonic => "CONST";

        internal override bool CreatesVariableDef => true;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 2)
            {
                result.AddInvalidOperandError(string.Format("The {0} expects a symbol operand with a value.",
                    Mnemonic));
            }
            else if (operandTokens[0].Type != TokenType.ConstSymbol)
            {
                result.AddError(ExecutionErrors.InvalidOperand, "Expected first operand to be a symbol operand.");
            }
            else
            {
                if (operandTokens[1].Type == TokenType.StringLiteral ||
                    operandTokens[1].Type == TokenType.NumericLiteral)
                    return;
                result.AddError(ExecutionErrors.InvalidOperand,
                    "Expected second operand to be a string literal or numeric literal.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand1 = Operands[0];
            var operand2 = Operands[1];
            var errorList = new ErrorList();
            var str = operand2.Value;
            if (operand1.Type != TokenType.ConstSymbol)
                return;
            if (context.GetSymbolValue(operand1.Value, result.Errors) != null)
            {
                AddError("E002", "Attempt to redefine CONST " + operand1.Value + "; not allowing update.",
                    result.Errors);
            }
            else
            {
                var obj = operand2.ConvertValue();
                context.SetSymbolValue(operand1.Value, obj);
            }
        }
    }
}