using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SetInstruction : Instruction
    {
        public override string Mnemonic => "SET";

        internal override bool CreatesVariableDef => true;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 2)
                result.AddMissingOperandError(
                    "A symbol and one of: symbol, numeric literal, string literal or key value pair expected.");
            else
                CheckAssignment(operandTokens[0].Type, result);
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol)
                return;
            var obj = Operands[1].ConvertValue();
            if (IsSymbolic(Operands[1]) && Operands[1].Value.Equals(obj))
            {
                obj = context.GetSymbolValue(Operands[1].Value, result.Errors);
                if (result.Errors.Count > 0)
                    return;
            }

            context.SetSymbolValue(operand.Value, obj);
        }
    }
}