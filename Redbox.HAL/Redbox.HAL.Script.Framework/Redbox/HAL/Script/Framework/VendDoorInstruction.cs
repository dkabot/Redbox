using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class VendDoorInstruction : Instruction
    {
        public override string Mnemonic => "VENDDOOR";

        protected override bool IsOperandRecognized(string operand)
        {
            return Enum<DoorCommands>.ParseIgnoringCase(operand, DoorCommands.None) != 0;
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 1)
            {
                result.AddMissingOperandError("Expected a symbol: OPEN, CLOSE, RENT, or STATUS.");
            }
            else
            {
                if (operandTokens[0].Type == TokenType.Symbol && IsOperandRecognized(operandTokens[0].Value))
                    return;
                result.AddInvalidOperandError("Expected a symbol: OPEN, CLOSE, RENT, or STATUS.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol)
                return;
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            switch (Enum<DoorCommands>.ParseIgnoringCase(operand.Value, DoorCommands.None))
            {
                case DoorCommands.Open:
                    context.PushTop(TimeoutMessage);
                    break;
                case DoorCommands.Rent:
                    HandleError(service.VendDoorRent(), context);
                    break;
                case DoorCommands.Close:
                    HandleError(service.VendDoorClose(), context);
                    break;
                case DoorCommands.Status:
                    context.PushTop(service.ReadVendDoorPosition().ToString().ToUpper());
                    break;
                default:
                    result.AddError(ExecutionErrors.InvalidSymbolValue,
                        string.Format("The value {0} is unrecognized.", operand.Value));
                    break;
            }
        }

        private enum DoorCommands
        {
            None,
            Open,
            Rent,
            Close,
            Status
        }
    }
}