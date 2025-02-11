using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class GetConfigInstruction : Instruction
    {
        public override string Mnemonic => "GETCFG";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count >= 2 && operandTokens[0].Type == TokenType.StringLiteral)
                return;
            result.AddInvalidOperandError("Expected a string literal operand.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("TYPE");
            var parameterArray = OperandsHelper.ConvertSymbolsToParameterArray(Operands, result.Errors, context);
            if (result.Errors.Count > 0)
                return;
            var name = string.Format(Operands[0].Value, parameterArray);
            var empty = (object)string.Empty;
            object propertyByName;
            try
            {
                propertyByName = ServiceLocator.Instance.GetService<IConfigurationService>()
                    .GetPropertyByName(keyValuePair.Value, name);
            }
            catch (ArgumentException ex)
            {
                AddError("E002", ex.ToString(), result.Errors);
                return;
            }

            if (propertyByName == null)
                return;
            context.PushTop(propertyByName);
        }
    }
}