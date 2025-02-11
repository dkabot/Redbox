using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class PushFormatInstruction : Instruction
    {
        public override string Mnemonic => "PUSHF";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Type == TokenType.StringLiteral)
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected a string literal and any number of optional subtitution parameters.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("END");
            var flag = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", true) == 0;
            var parameterArray = ConvertSymbolsToParameterArray(result.Errors, context);
            if (result.Errors.Count > 0)
                return;
            var instance = string.Format(Operands[0].Value, parameterArray);
            if (flag)
                context.PushBottom(instance);
            else
                context.PushTop(instance);
        }

        private static class StackEnds
        {
            public const string Bottom = "bottom";
            public const string Top = "top";
        }

        private static class KeyNames
        {
            public const string End = "END";
        }
    }
}