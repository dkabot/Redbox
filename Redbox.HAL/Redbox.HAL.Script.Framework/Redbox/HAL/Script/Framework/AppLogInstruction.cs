using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class AppLogInstruction : Instruction
    {
        public override string Mnemonic => "APPLOG";

        protected override bool IsOperandRecognized(string operand)
        {
            return operand.Equals("CONFIGURE");
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Value.Equals("CONFIGURE") || operandTokens[0].Type == TokenType.StringLiteral ||
                operandTokens[0].Type == TokenType.Symbol || operandTokens[0].Type == TokenType.ConstSymbol)
                return;
            result.AddInvalidOperandError(
                "Expected a string literal and any number of optional substitution parameters.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            if (Operands[0].Value.Equals("CONFIGURE"))
            {
                Configure(result, context);
            }
            else
            {
                var format = Operands[0].Value;
                if (Operands[0].Type == TokenType.Symbol || Operands[0].Type == TokenType.ConstSymbol)
                {
                    var symbolValue = context.GetSymbolValue(Operands[0].Value, result.Errors);
                    if (symbolValue == null || result.Errors.Count > 0)
                        return;
                    if (!(symbolValue is string))
                    {
                        result.AddError(ExecutionErrors.InvalidSymbolValue,
                            "The symbol specified for APPLOG is not a string type.");
                        return;
                    }
                }

                var parameterArray = ConvertSymbolsToParameterArray(result.Errors, context);
                if (result.Errors.Count > 0)
                    return;
                var msg = string.Format(format, parameterArray);
                context.AppLog?.Write(msg);
            }
        }

        private void Configure(ExecutionResult result, ExecutionContext context)
        {
            ConvertSymbolsToParameterArray(result.Errors, context);
            if (result.Errors.Count > 0)
                return;
            var logAppends = false;
            var keyValuePair1 = Operands.GetKeyValuePair(OperandArgs.Name.ToString().ToUpper());
            if (keyValuePair1 != null)
            {
                var str = keyValuePair1.Value;
                if (keyValuePair1.Type == TokenType.ConstSymbol || keyValuePair1.Type == TokenType.Symbol)
                {
                    var symbolValue = (string)context.GetSymbolValue(keyValuePair1.Value, result.Errors);
                    if (result.Errors.ContainsError())
                        return;
                }

                logAppends = true;
            }

            var flushOnWrite = true;
            var keyValuePair2 = Operands.GetKeyValuePair(OperandArgs.FlushOnWrite.ToString().ToUpper());
            bool result1;
            if (keyValuePair2 != null && bool.TryParse(keyValuePair2.Value, out result1))
                flushOnWrite = result1;
            var subDirectory = string.Empty;
            var keyValuePair3 = Operands.GetKeyValuePair(OperandArgs.Subdirectory.ToString().ToUpper());
            if (keyValuePair3 != null)
            {
                var symbolValue = keyValuePair3.Value;
                if (keyValuePair3.Type == TokenType.ConstSymbol || keyValuePair3.Type == TokenType.Symbol)
                {
                    symbolValue = (string)context.GetSymbolValue(keyValuePair3.Value, result.Errors);
                    if (result.Errors.ContainsError())
                        return;
                }

                subDirectory = symbolValue;
            }

            ApplicationLog.ConfigureLog(context, logAppends, subDirectory, flushOnWrite, null);
        }

        private enum OperandArgs
        {
            Operation,
            Name,
            Subdirectory,
            FlushOnWrite
        }
    }
}