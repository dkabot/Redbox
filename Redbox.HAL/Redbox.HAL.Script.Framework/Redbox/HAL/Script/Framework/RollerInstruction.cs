using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class RollerInstruction : Instruction
    {
        public override string Mnemonic => "ROLLER";

        protected override bool IsOperandRecognized(string operand)
        {
            return IsOperandValid(operand);
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].IsKeyValuePair || operandTokens[0].Type == TokenType.Symbol ||
                !IsOperandValid(operandTokens[0].Value))
                return;
            result.AddInvalidOperandError("Expected a symbol: IN, OUT, or STOP.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            GetWaitFlag();
            var nullable = GetTimeOutValue(result.Errors, context);
            var operand = Operands[0];
            if (operand.IsKeyValuePair)
            {
                var keyValuePair = (KeyValuePair)operand.ConvertValue();
                var position = RollerPosition.Position4;
                int result1;
                if (int.TryParse(keyValuePair.Value, out result1))
                    position = (RollerPosition)result1;
                if (!nullable.HasValue)
                    nullable = 8000;
                HandleError(service.RollerToPosition(position, nullable.Value), context);
            }
            else
            {
                if (operand.Type != TokenType.Symbol)
                    return;
                if (string.Compare(operand.Value, "IN", true) == 0)
                {
                    HandleError(service.StartRollerIn(), context);
                }
                else if (string.Compare(operand.Value, "OUT", true) == 0)
                {
                    HandleError(service.StartRollerOut(), context);
                }
                else
                {
                    if (string.Compare(operand.Value, "STOP", true) != 0)
                        return;
                    HandleError(service.StopRoller(), context);
                }
            }
        }

        private bool IsOperandValid(string operand)
        {
            return string.Compare(operand, "IN", true) == 0 || string.Compare(operand, "OUT", true) == 0 ||
                   string.Compare(operand, "STOP", true) == 0;
        }

        private static class Commands
        {
            public const string In = "IN";
            public const string Out = "OUT";
            public const string Stop = "STOP";
        }
    }
}