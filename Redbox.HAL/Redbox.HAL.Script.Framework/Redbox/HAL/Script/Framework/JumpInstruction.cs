using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class JumpInstruction : Instruction
    {
        public override string Mnemonic => "JUMP";

        internal override Branch BranchType => Branch.Unconditional;

        internal override string TargetLabel => Operands[0].Value;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 1 && operandTokens[0].Type == TokenType.Symbol)
                return;
            result.AddInvalidOperandError(
                string.Format("The {0} instruction expects a symbol that equates to a label name.", Mnemonic));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol || context.Registers.JumpToLabel(operand.Value))
                return;
            AddError("E002", "The label " + operand.Value + " does not exist.", result.Errors);
        }
    }
}