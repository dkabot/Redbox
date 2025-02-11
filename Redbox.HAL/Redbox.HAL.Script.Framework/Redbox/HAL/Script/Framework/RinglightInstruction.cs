using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class RinglightInstruction : Instruction
    {
        public override string Mnemonic => "RINGLIGHT";

        protected override bool IsOperandRecognized(string operand)
        {
            return IsOperandValid(operand);
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 1)
            {
                result.AddMissingOperandError("Expected a symbol: ON, OFF.");
            }
            else
            {
                if (operandTokens[0].Type == TokenType.Symbol && IsOperandValid(operandTokens[0].Value))
                    return;
                result.AddInvalidOperandError("Expected a symbol: ON, OFF.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<RinglightState>.ParseIgnoringCase(Operands[0].Value, RinglightState.Off);
            ServiceLocator.Instance.GetService<IControlSystem>()
                .ToggleRingLight(RinglightState.On == ignoringCase, new int?());
        }

        private bool IsOperandValid(string op)
        {
            return Enum<RinglightState>.ParseIgnoringCase(op, RinglightState.None) != 0;
        }

        private enum RinglightState
        {
            None,
            On,
            Off
        }
    }
}