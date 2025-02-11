using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SerialBoardInstruction : Instruction
    {
        public override string Mnemonic => "SERIALBOARD";

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<ExpectedOperand>.ParseIgnoringCase(Operands[0].Value, ExpectedOperand.None);
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            switch (ignoringCase)
            {
                case ExpectedOperand.None:
                    LogHelper.Instance.WithContext("SerialBoardInstruction: converted operand '{0}' to None",
                        Operands[0].Value);
                    break;
                case ExpectedOperand.ClosePort:
                    var instance = service.Shutdown() ? SuccessMessage : TimeoutMessage;
                    Thread.Sleep(500);
                    context.PushTop(instance);
                    break;
                case ExpectedOperand.Reset:
                    HandleError(service.Initialize(), context);
                    break;
            }
        }

        private enum ExpectedOperand
        {
            None,
            ClosePort,
            Reset
        }
    }
}