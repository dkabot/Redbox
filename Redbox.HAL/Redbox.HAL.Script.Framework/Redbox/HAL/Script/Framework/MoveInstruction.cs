using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class MoveInstruction : Instruction
    {
        public override string Mnemonic => "MOVE";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Value.StartsWith("SLOT-OFFSET", StringComparison.CurrentCultureIgnoreCase))
                return;
            if (string.Compare(operandTokens[0].Value, "WITH-OFFSET", true) == 0)
            {
                if (operandTokens.GetKeyValuePair("AXIS") == null)
                {
                    AddError("E004", string.Format("Axis KVP is required for the '{0}' form.", "WITH-OFFSET"),
                        result.Errors);
                }
                else
                {
                    if (operandTokens.GetKeyValuePair("UNITS") != null)
                        return;
                    AddError("E004", string.Format("Offset KVP is required for the '{0}' form.", "WITH-OFFSET"),
                        result.Errors);
                }
            }
            else if (operandTokens[0].Value.StartsWith("LOCATION", StringComparison.CurrentCultureIgnoreCase))
            {
                AddError("E004", "The LOCATION form is no longer supported.", result.Errors);
            }
            else
            {
                if (operandTokens.Count >= 2)
                    return;
                result.AddError(ExecutionErrors.InvalidOperand,
                    "Expected key value pair DECK=y, SLOT=x, MODE=m.  The values of x and y may be numeric literals or symbols.");
            }
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var mode = MoveMode.None;
            var keyValuePair1 = Operands.GetKeyValuePair("MODE");
            if (keyValuePair1 != null)
                mode = Enum<MoveMode>.ParseIgnoringCase(keyValuePair1.Value, MoveMode.None);
            var service1 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var keyValuePair2 = Operands.GetKeyValuePair("SLOT-OFFSET");
            if (Operands.Count == 1 && keyValuePair2 != null)
            {
                var keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair2, result.Errors, context);
                if (result.Errors.Count > 0)
                    context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
                else if (service1.CurrentLocation == null)
                    context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
                else
                    using (var decoratorService = new MoveDecoratorService(context))
                    {
                        var errorCodes = decoratorService.ShowEmptyStuck(service1.CurrentLocation, keyValuePairValue);
                        if (errorCodes == ErrorCodes.Success)
                            context.PushTop(keyValuePairValue);
                        context.PushTop(errorCodes.ToString().ToUpper());
                    }
            }
            else
            {
                ILocation location;
                if (!GetLocation(result, context, out location))
                {
                    AddError("E004",
                        "Deck or slot are undefined, and no location specified; reissue the MOVE instruction specifying both deck and slot values or the location.",
                        result.Errors);
                }
                else
                {
                    var service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
                    if (context.IsImmediate && service2.IsBin(location))
                    {
                        var upper = ErrorCodes.InvalidMoveToSecureLocation.ToString().ToUpper();
                        context.PushTop(upper);
                    }
                    else
                    {
                        ErrorCodes errorCodes;
                        if (string.Compare(Operands[0].Value, "WITH-OFFSET", true) == 0)
                        {
                            var keyValuePair3 = Operands.GetKeyValuePair("AXIS");
                            var keyValuePairValue = GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("UNITS"),
                                result.Errors, context);
                            if (!keyValuePairValue.HasValue)
                            {
                                AddError("E004", "Units conversion error.", result.Errors);
                                return;
                            }

                            var data = Enum<Axis>.ParseIgnoringCase(keyValuePair3.Value, Axis.X) == Axis.X
                                ? new OffsetMoveData(keyValuePairValue.Value, 0)
                                : new OffsetMoveData(0, keyValuePairValue.Value);
                            errorCodes = service1.MoveTo(location, mode, context.AppLog, ref data);
                        }
                        else
                        {
                            errorCodes = service1.MoveTo(location, mode, context.AppLog);
                        }

                        context.PushTop(errorCodes.ToString().ToUpper());
                    }
                }
            }
        }

        private static class KeyNames
        {
            internal const string Deck = "DECK";
            internal const string Slot = "SLOT";
            internal const string Mode = "MODE";
            internal const string Location = "LOCATION";
            internal const string Password = "PASSWORD";
            internal const string SlotOffset = "SLOT-OFFSET";
            internal const string WithOffset = "WITH-OFFSET";
            internal const string Axis = "AXIS";
            internal const string Units = "UNITS";
        }
    }
}