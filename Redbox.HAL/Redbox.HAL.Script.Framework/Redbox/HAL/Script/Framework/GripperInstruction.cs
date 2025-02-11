using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class GripperInstruction : Instruction
    {
        public override string Mnemonic => "GRIPPER";

        protected override bool IsOperandRecognized(string operand)
        {
            return ValidateOperand(operand);
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count > 2 || operandTokens[0].Type == TokenType.Symbol ||
                ValidateOperand(operandTokens[0].Value))
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected a symbol: OPEN, CLOSE, RENT, STATUS, CLEAR, EXTEND, EXTEND-FOR-TIME, RETRACT, PUSH, PEEK and an optional TIMEOUT=m and WAIT=TRUE|FALSE key value pair.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var programName = context.ProgramName;
            var timeOutValue = GetTimeOutValue(result.Errors, context);
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol)
                return;
            var errors = new ErrorList();
            var str1 = operand.Value.ToUpper();
            if (context.GetSymbolValue(operand.Value, errors) is string symbolValue)
                str1 = symbolValue;
            var service1 = ServiceLocator.Instance.GetService<IControlSystem>();
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var service3 = ServiceLocator.Instance.GetService<IControllerService>();
            if (str1 != null)
            {
                switch (str1.Length)
                {
                    case 4:
                        switch (str1[3])
                        {
                            case 'H':
                                if (str1 == "PUSH")
                                    break;
                                goto label_62;
                            case 'K':
                                if (str1 == "PEEK")
                                {
                                    if (service2.CurrentLocation == null)
                                    {
                                        context.PushTop("LOCATIONOUTOFRANGE");
                                        return;
                                    }

                                    var peekResult = new PeekOperation().Execute();
                                    if (!peekResult.TestOk)
                                    {
                                        context.PushTop(peekResult.Error.ToString().ToUpper());
                                        return;
                                    }

                                    context.PushTop(peekResult.IsFull ? "FULL" : (object)"EMPTY");
                                    return;
                                }

                                goto label_62;
                            case 'N':
                                if (str1 == "OPEN")
                                {
                                    TestFingerOperation(GripperFingerState.Open, context);
                                    return;
                                }

                                goto label_62;
                            case 'T':
                                if (str1 == "RENT")
                                {
                                    TestFingerOperation(GripperFingerState.Rent, context);
                                    return;
                                }

                                goto label_62;
                            default:
                                goto label_62;
                        }

                        break;
                    case 5:
                        switch (str1[2])
                        {
                            case 'E':
                                if (str1 == "CLEAR")
                                {
                                    context.PushTop(service3.ClearGripper() == ErrorCodes.Success);
                                    return;
                                }

                                goto label_62;
                            case 'O':
                                if (str1 == "CLOSE")
                                {
                                    TestFingerOperation(GripperFingerState.Closed, context);
                                    return;
                                }

                                goto label_62;
                            default:
                                goto label_62;
                        }
                    case 6:
                        switch (str1[0])
                        {
                            case 'E':
                                if (str1 == "EXTEND")
                                {
                                    var response = service1.ExtendArm();
                                    if (response.TimedOut)
                                        service1.RetractArm();
                                    HandleError(response, context);
                                    return;
                                }

                                goto label_62;
                            case 'S':
                                if (str1 == "STATUS")
                                {
                                    var sensorReadResult = service1.ReadPickerSensors();
                                    if (!sensorReadResult.Success || sensorReadResult.IsFull)
                                    {
                                        context.PushTop("FULL");
                                        return;
                                    }

                                    context.PushTop("EMPTY");
                                    return;
                                }

                                goto label_62;
                            default:
                                goto label_62;
                        }
                    case 7:
                        switch (str1[0])
                        {
                            case 'P':
                                if (str1 == "PUSH-IN")
                                    break;
                                goto label_62;
                            case 'R':
                                if (str1 == "RETRACT")
                                {
                                    HandleError(service1.RetractArm(), context);
                                    return;
                                }

                                goto label_62;
                            default:
                                goto label_62;
                        }

                        break;
                    case 8:
                        if (str1 == "PUSH-OUT")
                        {
                            var errorCodes = service3.PushOut();
                            context.PushTop(errorCodes.ToString().ToUpper());
                            return;
                        }

                        goto label_62;
                    case 12:
                        if (str1 == "CHECK-PICKER")
                        {
                            var vendId = (string)null;
                            var keyValuePair = Operands.GetKeyValuePair("VEND-ID");
                            if (keyValuePair != null)
                            {
                                var keyValuePairValue =
                                    GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
                                if (result.Errors.Count > 0)
                                    return;
                                vendId = keyValuePairValue;
                            }

                            using (var checkPickerOperation = new CheckPickerOperation(context))
                            {
                                context.PushTop(checkPickerOperation.CheckPicker(result, vendId).ToString().ToUpper());
                                return;
                            }
                        }

                        goto label_62;
                    case 15:
                        if (str1 == "EXTEND-FOR-TIME")
                        {
                            if (timeOutValue.HasValue)
                            {
                                service1.TimedExtend(timeOutValue.Value);
                                return;
                            }

                            service1.TimedExtend();
                            return;
                        }

                        goto label_62;
                    case 17:
                        if (str1 == "VEND-DISC-AT-DOOR")
                        {
                            context.PushTop(service3.VendItemInPicker().Status.ToString().ToUpper());
                            return;
                        }

                        goto label_62;
                    case 19:
                        switch (str1[0])
                        {
                            case 'A':
                                if (str1 == "ACCEPT-DISC-AT-DOOR")
                                {
                                    context.PushTop(service3.AcceptDiskAtDoor().ToString().ToUpper());
                                    return;
                                }

                                goto label_62;
                            case 'R':
                                if (str1 == "REJECT-DISC-AT-DOOR")
                                {
                                    context.PushTop(service3.RejectDiskInPicker().ToString().ToUpper());
                                    return;
                                }

                                goto label_62;
                            default:
                                goto label_62;
                        }
                    default:
                        goto label_62;
                }

                using (var pickerFrontOperation = new ClearPickerFrontOperation())
                {
                    pickerFrontOperation.Execute();
                    return;
                }
            }

            label_62:
            var str2 = string.Format("Unrecognized operand {0}", str1);
            LogHelper.Instance.WithContext(LogEntryType.Error, str2);
            result.Errors.Add(Error.NewError("E001", "Unknown operand", str2));
        }

        private void TestFingerOperation(GripperFingerState state, ExecutionContext context)
        {
            HandleError(ServiceLocator.Instance.GetService<IControlSystem>().SetFinger(state), context);
        }

        private bool ValidateOperand(string operand)
        {
            return string.Compare(operand, "OPEN", true) == 0 || string.Compare(operand, "CLOSE", true) == 0 ||
                   string.Compare(operand, "STATUS", true) == 0 || string.Compare(operand, "EXTEND", true) == 0 ||
                   string.Compare(operand, "EXTEND-FOR-TIME", true) == 0 ||
                   string.Compare(operand, "RETRACT", true) == 0 || string.Compare(operand, "RENT", true) == 0 ||
                   string.Compare(operand, "PUSH", true) == 0 || string.Compare(operand, "PEEK", true) == 0 ||
                   string.Compare(operand, "CLEAR", true) == 0 || string.Compare(operand, "PUSH-OUT", true) == 0 ||
                   string.Compare(operand, "PUSH-IN", true) == 0 ||
                   string.Compare(operand, "ACCEPT-DISC-AT-DOOR", true) == 0 ||
                   string.Compare(operand, "REJECT-DISC-AT-DOOR", true) == 0 ||
                   string.Compare(operand, "VEND-DISC-AT-DOOR", true) == 0 ||
                   string.Compare(operand, "CHECK-PICKER", true) == 0;
        }

        private static class Commands
        {
            public const string Open = "OPEN";
            public const string Close = "CLOSE";
            public const string Status = "STATUS";
            public const string Extend = "EXTEND";
            public const string ExtendForTime = "EXTEND-FOR-TIME";
            public const string Retract = "RETRACT";
            public const string Rent = "RENT";
            public const string Push = "PUSH";
            public const string Peek = "PEEK";
            public const string Clear = "CLEAR";
            public const string PushOut = "PUSH-OUT";
            public const string PushIn = "PUSH-IN";
            public const string AcceptDiscAtDoor = "ACCEPT-DISC-AT-DOOR";
            public const string RejectDiscAtDoor = "REJECT-DISC-AT-DOOR";
            public const string VendDiscAtDoor = "VEND-DISC-AT-DOOR";
            public const string CheckPicker = "CHECK-PICKER";
        }
    }
}