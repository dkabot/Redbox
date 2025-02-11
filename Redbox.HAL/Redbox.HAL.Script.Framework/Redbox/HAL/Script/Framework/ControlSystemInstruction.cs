using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class ControlSystemInstruction : Instruction
    {
        public override string Mnemonic => "CONTROLSYSTEM";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 0)
            {
                result.AddInvalidOperandError(string.Format("The {0} instruction expects one operand.", Mnemonic));
            }
            else
            {
                if (Enum<Ops>.ParseIgnoringCase(operandTokens[0].Value, Ops.None) != Ops.None)
                    return;
                result.AddInvalidOperandError(string.Format("The operand {0} is not recognized by {1}.",
                    operandTokens[0].Value, Mnemonic));
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<Ops>.ParseIgnoringCase(Operands[0].Value, Ops.None);
            var service1 = ServiceLocator.Instance.GetService<IControlSystem>();
            var response = (IControlResponse)null;
            var service2 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
            switch (ignoringCase)
            {
                case Ops.Center:
                    context.PushTop(service1.Center(CenterDiskMethod.VendDoorAndBack).ToString());
                    break;
                case Ops.RinglightOn:
                    response = service1.ToggleRingLight(true, new int?());
                    break;
                case Ops.RinglightOff:
                    response = service1.ToggleRingLight(false, new int?());
                    break;
                case Ops.PickerSensorsOn:
                    response = service1.SetSensors(true);
                    break;
                case Ops.PickerSensorsOff:
                    response = service1.SetSensors(false);
                    break;
                case Ops.EngageQlm:
                    context.PushTop(service1.EngageQlm(service2.NilLog).ToString().ToUpper());
                    break;
                case Ops.DisengageQlm:
                    context.PushTop(service1.DisengageQlm(true, service2.NilLog).ToString().ToUpper());
                    break;
                case Ops.DropQlm:
                    response = service1.DropQlm();
                    break;
                case Ops.LiftQlm:
                    response = service1.LiftQlm();
                    break;
                case Ops.HaltQlmOperation:
                    response = service1.HaltQlmLifter();
                    break;
                default:
                    result.AddError(ExecutionErrors.InvalidSymbolValue,
                        string.Format("The value {0} is unrecognized.", Operands[0].Value));
                    break;
            }

            if (response == null)
                return;
            HandleError(response, context);
        }

        private enum Ops
        {
            None,
            Center,
            RinglightOn,
            RinglightOff,
            PickerSensorsOn,
            PickerSensorsOff,
            EngageQlm,
            DisengageQlm,
            DropQlm,
            LiftQlm,
            HaltQlmOperation
        }
    }
}