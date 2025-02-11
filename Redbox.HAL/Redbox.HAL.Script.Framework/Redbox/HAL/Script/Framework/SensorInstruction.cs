using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SensorInstruction : Instruction
    {
        public override string Mnemonic => "SENSOR";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Type == TokenType.Symbol && IsOperandRecognized(operandTokens[0].Value))
                return;
            result.AddInvalidOperandError(
                "Expected a symbol: PICKER-ON, PICKER-OFF, READ-PICKER, READ-AUX-INPUTS, READ-PICKER-INPUTS");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var operand = Operands[0];
            if (string.Compare(operand.Value, "PICKER-ON", true) == 0)
            {
                context.PushTop(service.SetSensors(true).Success);
            }
            else if (string.Compare(operand.Value, "PICKER-OFF", true) == 0)
            {
                context.PushTop(service.SetSensors(false).Success);
            }
            else if (string.Compare(operand.Value, "READ-AUX-INPUTS", true) == 0)
            {
                PushInputs(context, service.ReadAuxInputs());
            }
            else if (string.Compare(operand.Value, "READ-PICKER-INPUTS", true) == 0)
            {
                PushInputs(context, service.ReadPickerInputs());
            }
            else
            {
                if (string.Compare(operand.Value, "READ", true) != 0)
                    return;
                var sensorReadResult = service.ReadPickerSensors(false);
                if (!sensorReadResult.Success)
                {
                    context.PushTop(0);
                }
                else
                {
                    var count = 1;
                    sensorReadResult.Foreach(each => context.PushTop(string.Format("SENSOR{0}-{1}", count++,
                        sensorReadResult.IsInputActive(each) ? "BLOCKED" : (object)"CLEAR")));
                    context.PushTop(sensorReadResult.InputCount);
                }
            }
        }

        protected override bool IsOperandRecognized(string op)
        {
            return OperandIsValid(op);
        }

        private bool OperandIsValid(string operand)
        {
            return string.Compare(operand, "PICKER-ON", true) == 0 ||
                   string.Compare(operand, "PICKER-OFF", true) == 0 || string.Compare(operand, "READ", true) == 0 ||
                   string.Compare(operand, "READ-PICKER-INPUTS", true) == 0 ||
                   string.Compare(operand, "READ-AUX-INPUTS", true) == 0;
        }

        private void PushInputs<T>(ExecutionContext ctx, IReadInputsResult<T> sensors)
        {
            if (sensors.Success)
            {
                sensors.Foreach(each => ctx.PushTop(sensors.IsInputActive(each) ? "1" : (object)"0"));
                ctx.PushTop(sensors.InputCount);
            }

            ctx.PushTop(sensors.Error);
        }

        private static class KeyNames
        {
            public const string AuxillarySensor = "AUX-SENSOR";
            public const string PickerSensor = "PICKER-SENSOR";
        }

        private static class Commands
        {
            public const string PickerOn = "PICKER-ON";
            public const string PickerOff = "PICKER-OFF";
            public const string Read = "READ";
            public const string ReadAuxInputs = "READ-AUX-INPUTS";
            public const string ReadPickerInputs = "READ-PICKER-INPUTS";
        }
    }
}