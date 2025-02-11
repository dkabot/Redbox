using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class ResultInstruction : Instruction
    {
        public override string Mnemonic => "RESULT";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count < 2)
            {
                result.AddInvalidOperandError(
                    "Expected the key value parameters CODE=c and MESSAGE=m.  Optional pairs can be: DECK=n, SLOT=n, and END=BOTTOM|TOP.");
            }
            else
            {
                if (operandTokens.GetKeyValuePair("CODE") != null && operandTokens.GetKeyValuePair("MESSAGE") != null)
                    return;
                result.AddInvalidOperandError(
                    string.Format("Expected key value parameters CODE=c and MESSAGE=m. (line {0})", LineNumber));
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("END");
            var flag = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", true) == 0;
            var keyValuePairValue1 =
                GetKeyValuePairValue<string>(Operands.GetKeyValuePair("CODE"), result.Errors, context);
            var keyValuePairValue2 =
                GetKeyValuePairValue<string>(Operands.GetKeyValuePair("MESSAGE"), result.Errors, context);
            var keyValuePairValue3 =
                GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("DECK"), result.Errors, context);
            var keyValuePairValue4 =
                GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("SLOT"), result.Errors, context);
            var keyValuePairValue5 =
                GetKeyValuePairValue<string>(Operands.GetKeyValuePair("ITEM"), result.Errors, context);
            var keyValuePairValue6 =
                GetKeyValuePairValue<string>(Operands.GetKeyValuePair("RETURNTIME"), result.Errors, context);
            var keyValuePairValue7 =
                GetKeyValuePairValue<string>(Operands.GetKeyValuePair("PREVIOUS"), result.Errors, context);
            if (result.Errors.Count > 0)
                return;
            var returnTime = new DateTime?();
            if (!string.IsNullOrEmpty(keyValuePairValue6))
            {
                DateTime result1;
                if (!DateTime.TryParse(keyValuePairValue6, out result1))
                {
                    if (!"NONE".Equals(keyValuePairValue6))
                        LogHelper.Instance.Log(string.Format("Unable to parse date time {0}", keyValuePairValue6),
                            LogEntryType.Error);
                    returnTime = new DateTime?();
                }
                else
                {
                    returnTime = result1;
                }
            }

            context.CreateResult(flag ? StackEnd.Bottom : StackEnd.Top, keyValuePairValue1, keyValuePairValue2,
                keyValuePairValue3, keyValuePairValue4, keyValuePairValue5, returnTime, keyValuePairValue7);
        }

        public static class Prefixes
        {
            public const string ResultPrefix = "RESULT";
        }

        private static class StackEnds
        {
            public const string Bottom = "bottom";
            public const string Top = "top";
        }

        private static class KeyNames
        {
            public const string End = "END";
            public const string Deck = "DECK";
            public const string Slot = "SLOT";
            public const string Code = "CODE";
            public const string Item = "ITEM";
            public const string Message = "MESSAGE";
            public const string ReturnTime = "RETURNTIME";
            public const string PreviousID = "PREVIOUS";
        }
    }
}