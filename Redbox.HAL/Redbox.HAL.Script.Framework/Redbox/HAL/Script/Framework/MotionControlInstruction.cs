using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class MotionControlInstruction : Instruction
    {
        public override string Mnemonic => "MOTIONCONTROL";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            var ignoringCase = Enum<Operand>.ParseIgnoringCase(operandTokens[0].Value, Operand.None);
            if (operandTokens.Count >= 1 && ignoringCase != Operand.None)
                return;
            result.AddError(ExecutionErrors.MissingOperand, "MOTIONCONTROL instruction expects an operand.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<Operand>.ParseIgnoringCase(Operands[0].Value, Operand.None);
            var service = ServiceLocator.Instance.GetService<IMotionControlService>();
            switch (ignoringCase)
            {
                case Operand.Readlimits:
                    var controlLimitResponse = service.ReadLimits();
                    if (controlLimitResponse.ReadOk)
                    {
                        foreach (var limit in controlLimitResponse.Limits)
                            context.PushTop(string.Format("{0} LIMIT: {1} ", limit.Limit.ToString().ToUpper(),
                                limit.Blocked ? "BLOCKED" : (object)"CLEAR"));
                        context.PushTop(controlLimitResponse.Limits.Length);
                        break;
                    }

                    context.PushTop("INVALID LIMITS RESPONSE");
                    context.PushTop(1);
                    break;
                case Operand.Comstatus:
                    context.PushTop(service.CommunicationOk() ? "OK" : (object)"COMM ERROR");
                    break;
                case Operand.Initstatus:
                    context.PushTop(service.IsInitialized ? "OK" : (object)"ERROR");
                    break;
                case Operand.Positions:
                    var controllerPosition1 = service.ReadPositions();
                    if (!controllerPosition1.ReadOk)
                    {
                        context.PushTop("MOTIONCONTROL ERROR");
                        break;
                    }

                    context.PushTop(string.Format("X = {0} Y = {1}", controllerPosition1.XCoordinate.Value,
                        controllerPosition1.YCoordinate.Value));
                    break;
                case Operand.XPos:
                    var controllerPosition2 = service.ReadPositions();
                    if (!controllerPosition2.ReadOk)
                    {
                        context.PushTop("MOTIONCONTROL ERROR");
                        break;
                    }

                    context.PushTop(controllerPosition2.XCoordinate.Value);
                    break;
                case Operand.YPos:
                    var controllerPosition3 = service.ReadPositions();
                    if (!controllerPosition3.ReadOk)
                    {
                        context.PushTop("MOTIONCONTROL ERROR");
                        break;
                    }

                    context.PushTop(controllerPosition3.YCoordinate.Value);
                    break;
                case Operand.Homeall:
                    if (!service.CommunicationOk() && service.Reset(true) != ErrorCodes.Success)
                    {
                        context.PushTop(false);
                        break;
                    }

                    var errorCodes = service.InitAxes();
                    context.PushTop(errorCodes == ErrorCodes.Success);
                    break;
                case Operand.ComCheck:
                    context.PushTop((ErrorCodes)(service.CommunicationOk() ? 0 : 2));
                    break;
                case Operand.Restart:
                    context.PushTop(service.Reset(true) == ErrorCodes.Success
                        ? SuccessMessage
                        : (object)TimeoutMessage);
                    break;
                case Operand.TestAndReset:
                    context.PushTop(service.TestAndReset(true) ? SuccessMessage : (object)TimeoutMessage);
                    break;
            }
        }

        private enum Operand
        {
            None,
            Readlimits,
            Comstatus,
            Initstatus,
            Positions,
            XPos,
            YPos,
            Homeall,
            ComCheck,
            Restart,
            TestAndReset
        }
    }
}