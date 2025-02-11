using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class QuickReturnInstruction : Instruction
    {
        public override string Mnemonic => "QUICKRETURN";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            var forms = CheckForm(operandTokens[0].Value);
            if (forms == Forms.Unknown)
            {
                result.AddInvalidOperandError(string.Format("Unknown form of QuickReturn: {0}",
                    operandTokens[0].Value));
            }
            else
            {
                var details = string.Empty;
                if (forms != Forms.PollStatus)
                    details = "Only the POLLSTATUS form of QUICKRETURN is supported.";
                if (string.IsNullOrEmpty(details))
                    return;
                result.AddInvalidOperandError(details);
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (Operands.Count == 0)
                AddError("E002", "There are no operands for this instruction.", result.Errors);
            else
                switch (CheckForm(Operands[0].Value))
                {
                    case Forms.Unknown:
                        AddError("E002", "There is an unknown form of this instruction.", result.Errors);
                        break;
                    case Forms.PollStatus:
                        context.PushTop("INACTIVE");
                        break;
                    default:
                        result.AddError(ExecutionErrors.InvalidOperand,
                            "The QUICKRETURN operand is no longer supported.");
                        break;
                }
        }

        private Forms CheckForm(string token)
        {
            var ignoringCase = Enum<Forms>.ParseIgnoringCase(token, Forms.Unknown);
            if (ignoringCase == Forms.Unknown)
                LogHelper.Instance.Log(string.Format("Unrecognized FORM argument: {0}", token), LogEntryType.Error);
            return ignoringCase;
        }

        internal enum Forms
        {
            Unknown,
            PollStatus
        }
    }
}