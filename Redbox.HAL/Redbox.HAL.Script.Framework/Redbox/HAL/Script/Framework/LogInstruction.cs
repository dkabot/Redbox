using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class LogInstruction : Instruction
    {
        private const string EntryType = "ENTRYTYPE";

        public override string Mnemonic => "LOG";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 0 && (operandTokens[0].Type == TokenType.StringLiteral ||
                                             operandTokens[0].Type == TokenType.Symbol))
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected a string literal and any number of optional substitution parameters.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (Operands.Count <= 0)
                return;
            var parameterArray = ConvertSymbolsToParameterArray(result.Errors, context);
            if (result.Errors.Count > 0)
                return;
            var type = LogEntryType.Info;
            var keyValuePair = Operands.GetKeyValuePair("ENTRYTYPE");
            if (keyValuePair != null)
                type = Enum<LogEntryType>.ParseIgnoringCase(keyValuePair.Value, LogEntryType.Info);
            var format = Operands[0].Value;
            if (Operands[0].Type == TokenType.Symbol)
            {
                var symbolValue = context.GetSymbolValue(Operands[0].Value, result.Errors);
                if (symbolValue == null || result.Errors.Count > 0)
                    return;
                if (!(symbolValue is string))
                {
                    result.Errors.Add(Error.NewError("E001", "Incorrect type argument.",
                        "The symbol specified for LOG is not a string type."));
                    return;
                }
            }

            var str = string.Format(format, parameterArray);
            LogHelper.Instance.Log(type, "[{0}, {1}]: {2}", context.ProgramName, context.ID, str);
        }
    }
}