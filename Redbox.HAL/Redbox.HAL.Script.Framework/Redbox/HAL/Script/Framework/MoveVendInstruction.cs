using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class MoveVendInstruction : Instruction
    {
        private const string Mode = "MODE";
        private const string ResetOnCommunicationFailure = "RESETONCOMMERROR";

        public override string Mnemonic => "MOVEVEND";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count <= 2)
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected either no arguments, or single key value pair MODE=GET.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var mode = MoveMode.None;
            if (Operands.Count > 0)
            {
                var keyValuePair = Operands.GetKeyValuePair("MODE");
                if (keyValuePair != null)
                    mode = Enum<MoveMode>.ParseIgnoringCase(keyValuePair.Value, MoveMode.None);
            }

            var keyValuePair1 = Operands.GetKeyValuePair("RESETONCOMMERROR");
            if (keyValuePair1 != null)
            {
                var errorList = new ErrorList();
                bool.TryParse(keyValuePair1.Value, out _);
            }

            var upper = ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(mode, context.AppLog)
                .ToString().ToUpper();
            context.PushTop(upper);
        }
    }
}