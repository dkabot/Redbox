using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class AudioInstruction : Instruction
    {
        public override string Mnemonic => "AUDIO";

        protected override bool IsOperandRecognized(string operand)
        {
            return Enum<AudioChannelState>.ParseIgnoringCase(operand, AudioChannelState.None) != 0;
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (IsOperandRecognized(operandTokens[0].Value))
                return;
            result.Errors.Add(Error.NewError("C001", "Unexpected form",
                string.Format("The operand {0} for AUDIO isn't recognized.", operandTokens[0].Value)));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<AudioChannelState>.ParseIgnoringCase(Operands[0].Value, AudioChannelState.None);
            HandleError(ServiceLocator.Instance.GetService<IControlSystem>().SetAudio(ignoringCase), context);
        }
    }
}