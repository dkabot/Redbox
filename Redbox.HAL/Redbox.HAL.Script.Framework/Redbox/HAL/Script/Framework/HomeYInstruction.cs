using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class HomeYInstruction : Instruction
    {
        public override string Mnemonic => "HOMEY";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 0 || (operandTokens.Count == 1 && operandTokens[0].IsKeyValuePair &&
                                             IsOperandValid(operandTokens[0].Value)))
                return;
            result.AddInvalidOperandError("The HOMEY mnemonic supports the CLEARGRIPPER key value pair.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var flag = true;
            if (Operands.Count == 1)
            {
                var keyValuePair = Operands.GetKeyValuePair("CLEARGRIPPER");
                bool result1;
                if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out result1))
                    flag = result1;
            }

            if (flag && ControllerConfiguration.Instance.ClearPickerOnHome)
                using (var pickerFrontOperation = new ClearPickerFrontOperation())
                {
                    pickerFrontOperation.Execute();
                }

            var upper = ServiceLocator.Instance.GetService<IMotionControlService>().HomeAxis(Axis.Y).ToString()
                .ToUpper();
            context.PushTop(upper);
        }

        private bool IsOperandValid(string op)
        {
            return op.StartsWith("CLEARGRIPPER");
        }
    }
}