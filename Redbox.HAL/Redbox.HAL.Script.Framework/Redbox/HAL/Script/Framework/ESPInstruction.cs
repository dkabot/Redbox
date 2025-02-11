using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class ESPInstruction : Instruction
    {
        public override string Mnemonic => "ESP";

        protected override void ParseOperands(TokenList operands, ExecutionResult result)
        {
            if (operands.Count == 1)
                return;
            result.AddInvalidOperandError(string.Format("The {0} instruction expects one operand.", Mnemonic));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
            if (string.Compare(Operands[0].Value, "DUMP", true) == 0)
            {
                service.DumpESP(false);
            }
            else
            {
                if (string.Compare(Operands[0].Value, "DUMP-STORE", true) != 0)
                    return;
                service.DumpESP(true);
            }
        }
    }
}