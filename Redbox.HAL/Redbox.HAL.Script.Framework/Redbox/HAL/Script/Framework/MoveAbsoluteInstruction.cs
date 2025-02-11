using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class MoveAbsoluteInstruction : Instruction
    {
        public override string Mnemonic => "MOVEABS";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count != 0 && operandTokens.Count <= 3)
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected key value pair AXIS=a, UNITS=n, SECONDARY-UNITS=n. The values of AXIS should be X or Y. The value for UNITS and SECONDARY-UNITS may be numeric literals or symbols.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var checkSensors = true;
            var keyValuePair1 = Operands.GetKeyValuePair("SENSOR-CHECK");
            bool result1;
            if (keyValuePair1 != null && bool.TryParse(keyValuePair1.Value, out result1))
                checkSensors = result1;
            var keyValuePair2 = Operands.GetKeyValuePair("AXIS");
            var keyValuePair3 = Operands.GetKeyValuePair("UNITS");
            var keyValuePair4 = Operands.GetKeyValuePair("SECONDARY-UNITS");
            var keyValuePairValue1 = GetKeyValuePairValue<int?>(keyValuePair3, result.Errors, context);
            var keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors, context);
            if (result.Errors.Count > 0 || !keyValuePairValue1.HasValue)
                return;
            var axis1 = Axis.X;
            var strA1 = keyValuePair2.Value;
            var axis2 = Axis.X;
            var strB1 = axis2.ToString();
            if (string.Compare(strA1, strB1, true) == 0)
            {
                axis1 = Axis.X;
            }
            else
            {
                var strA2 = keyValuePair2.Value;
                axis2 = Axis.Y;
                var strB2 = axis2.ToString();
                if (string.Compare(strA2, strB2, true) == 0)
                {
                    axis1 = Axis.Y;
                }
                else
                {
                    var strA3 = keyValuePair2.Value;
                    axis2 = Axis.XY;
                    var strB3 = axis2.ToString();
                    if (string.Compare(strA3, strB3, true) == 0)
                        axis1 = Axis.XY;
                }
            }

            var xunits = new int?();
            var yunits = new int?();
            if (axis1 == Axis.X)
            {
                xunits = keyValuePairValue1;
                yunits = new int?();
            }
            else if (Axis.XY == axis1)
            {
                xunits = keyValuePairValue1;
                yunits = keyValuePairValue2;
            }
            else if (Axis.Y == axis1)
            {
                xunits = new int?();
                yunits = keyValuePairValue1;
            }

            var errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveAbsolute(axis1, xunits, yunits, checkSensors);
            context.PushTop(errorCodes.ToString().ToUpper());
        }

        private static class KeyNames
        {
            public const string Axis = "AXIS";
            public const string Units = "UNITS";
            public const string SecondaryUnits = "SECONDARY-UNITS";
            public const string SensorCheck = "SENSOR-CHECK";
        }
    }
}