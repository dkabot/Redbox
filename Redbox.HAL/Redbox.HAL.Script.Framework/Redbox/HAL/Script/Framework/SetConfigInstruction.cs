using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using KioskConfiguration = Redbox.HAL.Controller.Framework.KioskConfiguration;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SetConfigInstruction : Instruction
    {
        public override string Mnemonic => "SETCFG";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Type == TokenType.StringLiteral)
                return;
            result.AddInvalidOperandError("Expected a string literal.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("TYPE");
            var parameterArray = OperandsHelper.ConvertSymbolsToParameterArray(Operands, result.Errors, context);
            if (result.Errors.Count > 0)
                return;
            var name = Operands[0].Value;
            if (string.Compare(keyValuePair.Value, "KIOSK", true) == 0)
                KioskConfiguration.Instance.SetConfig(name, parameterArray[0] as string, true);
            else
                ServiceLocator.Instance.GetService<IConfigurationService>()
                    .SetPropertyByName(keyValuePair.Value, name, parameterArray);
        }
    }
}