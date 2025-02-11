using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public class CameraInsutrction : Instruction
    {
        public override string Mnemonic => "CAMERA";

        protected override bool IsOperandRecognized(string operand)
        {
            return IsValidOperand(operand);
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Type == TokenType.Symbol && IsValidOperand(operandTokens[0].Value))
                return;
            result.AddInvalidOperandError(string.Format(
                "Unrecognized operand {0}; Expected a symbol: START, STOP, SNAP, STATUS, and an optional CENTER={TRUE|FALSE} operand.",
                operandTokens[0].Value));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var flag = false;
            bool boolValue;
            if (FindBooleanOperand(ExpectedOperand.Center.ToString().ToUpper(), result, out boolValue))
                flag = boolValue;
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
            switch (Enum<ExpectedOperand>.ParseIgnoringCase(Operands[0].Value, ExpectedOperand.None))
            {
                case ExpectedOperand.Start:
                    if (flag)
                    {
                        var num = (int)service.Center(CenterDiskMethod.VendDoorAndBack);
                    }

                    context.PushTop(configuredDevice.Start());
                    break;
                case ExpectedOperand.Stop:
                    context.PushTop(configuredDevice.Stop());
                    break;
                case ExpectedOperand.Snap:
                    var snapResult = configuredDevice.Snap();
                    if (!snapResult.SnapOk)
                    {
                        context.PushTop("CAPTURE ERROR");
                        break;
                    }

                    context.PushTop(snapResult.Path);
                    context.PushTop("SUCCESS");
                    break;
                case ExpectedOperand.Status:
                    context.PushTop(configuredDevice.IsConnected ? "RUNNING" : (object)"STOPPED");
                    break;
                case ExpectedOperand.ShowProperties:
                    AddError("E999", string.Format("The operand '{0}' is deprecated.", Operands[0].Value),
                        result.Errors);
                    break;
                case ExpectedOperand.IsWorking:
                    AddError("E666", "IS-WORKING is a deprecated operand", result.Errors);
                    break;
                case ExpectedOperand.IRCamera:
                    context.PushTop(false);
                    break;
                case ExpectedOperand.ResetReturnCounter:
                    ReturnJob.ResetCameraErrorCounter();
                    break;
                case ExpectedOperand.ReturnErrorCount:
                    context.PushTop(ReturnJob.CameraErrorCount);
                    break;
                case ExpectedOperand.ValidateSettings:
                    configuredDevice.ValidateSettings();
                    break;
                case ExpectedOperand.Restart:
                    context.PushTop(configuredDevice.Restart().ToString().ToUpper());
                    break;
            }
        }

        private bool FindBooleanOperand(string operand, ExecutionResult result, out bool boolValue)
        {
            boolValue = false;
            var keyValuePair = Operands.GetKeyValuePair(operand);
            bool result1;
            if (keyValuePair == null || !bool.TryParse(keyValuePair.Value, out result1))
                return false;
            boolValue = result1;
            return true;
        }

        private bool IsValidOperand(string operand)
        {
            return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != 0;
        }

        protected static class ResultCodes
        {
            public const string TimedOut = "CAPTURE TIMED OUT";
            public const string CaptureError = "CAPTURE ERROR";
            public const string Success = "SUCCESS";
        }

        private enum ExpectedOperand
        {
            None,
            Center,
            Start,
            Stop,
            Snap,
            Status,
            ShowProperties,
            IsWorking,
            IRCamera,
            ResetReturnCounter,
            ReturnErrorCount,
            ValidateSettings,
            Restart
        }
    }
}