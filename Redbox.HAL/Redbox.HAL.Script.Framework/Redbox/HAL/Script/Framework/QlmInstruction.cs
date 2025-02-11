using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class QlmInstruction : Instruction
    {
        public override string Mnemonic => "QLM";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 1 || operandTokens.Count == 2)
            {
                if (operandTokens[0].Type != TokenType.Symbol ||
                    (string.Compare(operandTokens[0].Value, "ENGAGE", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "DISENGAGE", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "LIFT", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "DROP", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "HALT", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "ENDQLMOP", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "CLEAR-PICKER", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "UNLOAD-DISK", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "THIN-DISK", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "SETUP", true) != 0 &&
                     string.Compare(operandTokens[0].Value, "STATUS", true) != 0))
                    result.AddError(ExecutionErrors.InvalidOperand,
                        "Expected a symbol: ENGAGE, DISENGAGE, HALT, UP, DOWN or STATUS.");
                else
                    Operands.AddRange(operandTokens);
            }
            else
            {
                result.AddError(ExecutionErrors.InvalidOperand,
                    "Expected a symbol: ENGAGE, DISENGAGE, UP, DOWN or HALT.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var executionContext = context;
            var operand = Operands[0];
            if (operand.Type != TokenType.Symbol)
                return;
            var service1 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var home = true;
            var keyValuePair1 = Operands.GetKeyValuePair("HOME");
            bool result1;
            if (keyValuePair1 != null && bool.TryParse(keyValuePair1.Value, out result1))
                home = result1;
            var service2 = ServiceLocator.Instance.GetService<IControlSystem>();
            if (string.Compare(operand.Value, "HALT", true) == 0)
            {
                context.PushTop(service2.HaltQlmLifter().Success ? SuccessMessage : (object)TimeoutMessage);
            }
            else if (string.Compare(operand.Value, "ENGAGE", true) == 0)
            {
                var errorCodes = service2.EngageQlm(home, executionContext.AppLog);
                if (errorCodes == ErrorCodes.Success && !context.IsImmediate)
                    service1.TestAndReset();
                context.PushTop(errorCodes.ToString().ToUpper());
            }
            else if (string.Compare(operand.Value, "LIFT", true) == 0)
            {
                service2.LiftQlm();
            }
            else if (string.Compare(operand.Value, "DROP", true) == 0)
            {
                service2.DropQlm();
            }
            else if (string.Compare(operand.Value, "DISENGAGE", true) == 0)
            {
                var errorCodes = service2.DisengageQlm(home, executionContext.AppLog);
                if (errorCodes == ErrorCodes.Success && !context.IsImmediate)
                    service1.TestAndReset();
                context.PushTop(errorCodes.ToString().ToUpper());
            }
            else if (string.Compare(operand.Value, "STATUS", true) == 0)
            {
                context.PushTop(service2.GetQlmStatus().ToString().ToUpper());
            }
            else if (string.Compare(operand.Value, "ENDQLMOP", true) == 0)
            {
                var failCode = ErrorCodes.Success;
                var keyValuePair2 = Operands.GetKeyValuePair("MOVE-EC");
                if (keyValuePair2 != null)
                {
                    var keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair2, result.Errors, context);
                    if (result.Errors.Count == 0)
                        failCode = Enum<ErrorCodes>.ParseIgnoringCase(keyValuePairValue, ErrorCodes.Success);
                }

                if (!MerchandizingHelper.Instance.CleanupJob(result, context, failCode) || context.IsImmediate)
                    return;
                service1.TestAndReset();
            }
            else if (string.Compare(operand.Value, "CLEAR-PICKER") == 0)
            {
                new UnloadHelper(context, result).CheckAndClearPicker(result);
            }
            else if (string.Compare(operand.Value, "SETUP") == 0)
            {
                var keyValuePair3 = Operands.GetKeyValuePair("CHECK-SLOTS");
                var checkSlots = true;
                bool result2;
                if (keyValuePair3 != null && bool.TryParse(keyValuePair3.Value, out result2))
                    checkSlots = result2;
                context.PushTop(MerchandizingHelper.Instance.Setup(result, checkSlots, context));
            }
            else if (string.Compare(operand.Value, "UNLOAD-DISK") == 0)
            {
                var service3 = ServiceLocator.Instance.GetService<IInventoryService>();
                var srcSlot = CurrentSlot(result, context);
                if (srcSlot == 0)
                    return;
                if (!service3.EmptyCountExceeds(0))
                    context.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
                var service4 = ServiceLocator.Instance.GetService<IDecksService>();
                var merchandizeResult = new UnloadHelper(context, result).UnloadDisk(service4.QlmDeck.Number, srcSlot);
                var upper = merchandizeResult.ToString().ToUpper();
                if (merchandizeResult != MerchandizeResult.Success)
                    LogHelper.Instance.WithContext("Unload-Disc returned status {0}", upper);
                context.PushTop(upper);
            }
            else
            {
                if (string.Compare(operand.Value, "THIN-DISK") != 0)
                    return;
                var id = GetID(result, context);
                if (string.IsNullOrEmpty(id))
                    return;
                var merchandizeResult = ThinHelper.MakeInstance(result, context).ThinDisk(id, MerchFlags.None);
                var upper = merchandizeResult.ToString().ToUpper();
                if (merchandizeResult != MerchandizeResult.Success)
                    LogHelper.Instance.WithContext("thin-disk returned status {0}", upper);
                context.PushTop(upper);
            }
        }

        private int CurrentSlot(ExecutionResult result, ExecutionContext context)
        {
            var keyValuePair = Operands.GetKeyValuePair("CURRENT-SLOT");
            if (keyValuePair == null)
            {
                result.Errors.Add(Error.NewError("E099", "Missing operand.", "The CURRENT-SLOT operand is missing."));
                return 0;
            }

            var keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair, result.Errors, context);
            return result.Errors.Count != 0 ? 0 : keyValuePairValue;
        }

        private static class Commands
        {
            public const string Engage = "ENGAGE";
            public const string Disengage = "DISENGAGE";
            public const string Halt = "HALT";
            public const string Status = "STATUS";
            public const string Lift = "LIFT";
            public const string Drop = "DROP";
            public const string EndQlmOp = "ENDQLMOP";
            public const string ClearPicker = "CLEAR-PICKER";
            public const string UnloadDisk = "UNLOAD-DISK";
            public const string Setup = "SETUP";
            public const string ThinDisk = "THIN-DISK";
        }
    }
}