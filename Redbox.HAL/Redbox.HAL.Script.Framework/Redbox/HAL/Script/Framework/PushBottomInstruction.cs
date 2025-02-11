using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class PushBottomInstruction : Instruction
    {
        public override string Mnemonic => "PUSHB";

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            foreach (var operand in Operands)
            {
                var obj = operand.ConvertValue();
                object instance;
                if (operand.Type == TokenType.Symbol && operand.Value.Equals(obj))
                {
                    instance = context.GetSymbolValue(operand.Value, result.Errors);
                    if (instance == null)
                        break;
                }
                else
                {
                    instance = operand.ConvertValue();
                }

                context.PushBottom(instance);
            }
        }
    }
}