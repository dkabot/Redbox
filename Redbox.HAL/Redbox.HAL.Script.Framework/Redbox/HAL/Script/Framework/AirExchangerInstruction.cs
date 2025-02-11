using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class AirExchangerInstruction : Instruction
    {
        public override string Mnemonic => "AIRXCHGR";

        protected override bool IsOperandRecognized(string operand)
        {
            return IsValidOperand(operand);
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (IsValidOperand(operandTokens[0].Value))
                return;
            result.AddInvalidOperandError(string.Format("Unexpected operand '{0}'", operandTokens[0].Value));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<ExpectedOperand>.ParseIgnoringCase(Operands[0].Value, ExpectedOperand.None);
            var service = ServiceLocator.Instance.GetService<IAirExchangerService>();
            switch (ignoringCase)
            {
                case ExpectedOperand.None:
                    LogHelper.Instance.WithContext(LogEntryType.Error,
                        "{0} instruction: converted '{1}' to operand None", Mnemonic, Operands[0].Value);
                    break;
                case ExpectedOperand.IsConfigured:
                    context.PushTop(service.Configured);
                    break;
                case ExpectedOperand.FanOn:
                    service.TurnOnFan();
                    break;
                case ExpectedOperand.FanOff:
                    service.TurnOffFan();
                    break;
                case ExpectedOperand.FanStatus:
                    context.PushTop(service.FanStatus.ToString().ToUpper());
                    break;
                case ExpectedOperand.ResetFailureCounter:
                    service.ResetFailureCount();
                    context.PushTop(service.PersistentFailureCount());
                    break;
                case ExpectedOperand.BoardStatus:
                    context.PushTop(service.CheckStatus().ToString().ToUpper());
                    break;
                case ExpectedOperand.Reset:
                    context.PushTop(service.Reset().ToString().ToUpper());
                    break;
            }
        }

        private bool IsValidOperand(string operand)
        {
            return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != 0;
        }

        private enum ExpectedOperand
        {
            None,
            IsConfigured,
            FanOn,
            FanOff,
            FanStatus,
            ResetFailureCounter,
            BoardStatus,
            Reset
        }
    }
}