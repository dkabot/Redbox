using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class LocInstruction : Instruction
    {
        public override string Mnemonic => "LOC";

        protected override ExpectedOperands Expected => ExpectedOperands.Optional;

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (Keys.Recognized(operandTokens[0].Value))
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                string.Format("The '{0}' operand is not recognized.", operandTokens[0].Value));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var service1 = ServiceLocator.Instance.GetService<IInventoryService>();
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            if (Operands.Count >= 1)
            {
                if (Operands[0].Value.Equals("CLEARRETURNTIME"))
                {
                    SetReturnTime(result, new DateTime?(), true, context);
                }
                else if (Operands[0].Value.Equals("INC-STUCK"))
                {
                    ILocation location;
                    if (!GetLocation(result, context, out location))
                        context.PushTop("ERROR");
                    else
                        context.PushTop(service1.UpdateEmptyStuck(location) ? "SUCCESS" : (object)"ERROR");
                }
                else if (Operands[0].Value.Equals("CLEAR-STUCK"))
                {
                    ILocation location;
                    if (!GetLocation(result, context, out location))
                    {
                        context.PushTop("ERROR");
                    }
                    else
                    {
                        location.StuckCount = 0;
                        context.PushTop(service1.Save(location) ? "SUCCESS" : (object)"ERROR");
                    }
                }
                else if (Operands[0].Value.Equals("RESET"))
                {
                    ILocation location;
                    GetLocation(result, context, out location);
                    context.PushTop(service1.Reset(location) ? "SUCCESS" : (object)"ERROR");
                }
                else if (Operands[0].Value.Equals("GET-INFO"))
                {
                    ILocation location;
                    if (GetLocation(result, context, out location))
                    {
                        var stuckCount = location.StuckCount;
                        context.PushTop(stuckCount);
                        var instance = location.Flags.ToString();
                        context.PushTop(instance);
                        var returnDate = location.ReturnDate;
                        context.PushTop(GetDateRepresentation(returnDate));
                        var id = location.ID;
                        context.PushTop(id);
                        context.PushTop("SUCCESS");
                    }
                    else
                    {
                        context.PushTop("ERROR");
                    }
                }
                else if (Operands[0].Value.Equals("GETRETURNTIME"))
                {
                    ILocation location;
                    if (GetLocation(result, context, out location))
                        context.PushTop(GetDateRepresentation(location.ReturnDate));
                    else
                        context.PushTop("ERROR");
                }
                else if (Operands[0].Value.Equals("MARKRETURNTIME"))
                {
                    SetReturnTime(result, DateTime.Now, true, context);
                }
                else if (Operands[0].Value.Equals("AT-DRUM-LOC"))
                {
                    if (service2.CurrentLocation != null)
                        context.PushTop(true);
                    else
                        context.PushTop(false);
                }
                else if (Operands[0].Value.Equals("SETINVENTORY"))
                {
                    var keyValuePairValue =
                        GetKeyValuePairValue<string>(Operands.GetKeyValuePair("ID"), result.Errors, context);
                    if (result.Errors.Count > 0)
                    {
                        context.PushTop("FAILURE");
                    }
                    else
                    {
                        ILocation location;
                        if (!GetLocation(result, context, out location))
                        {
                            LogHelper.Instance.Log("Unable to get deck and slot information.", LogEntryType.Error);
                            context.PushTop("FAILURE");
                        }

                        location.ID = keyValuePairValue;
                        service1.Save(location);
                        context.PushTop("SUCCESS");
                    }
                }
                else if (Operands[0].Value.Equals("RESETRT"))
                {
                    if (Operands[1].Type != TokenType.StringLiteral)
                    {
                        AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
                    }
                    else
                    {
                        DateTime result1;
                        if (!DateTime.TryParse(Operands[1].Value, out result1))
                        {
                            LogHelper.Instance.Log(
                                "Location RESETRT Instruction - Unable to parse date/time information.",
                                LogEntryType.Error);
                            AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
                        }
                        else
                        {
                            SetReturnTime(result, result1, true, context);
                        }
                    }
                }
                else if (Operands[0].Value.Equals("TIMESTAMP"))
                {
                    SetReturnTime(result, DateTime.Now, false, context);
                }
                else if (Operands[0].Value.Equals("SETRETURNTIME"))
                {
                    if (Operands[1].Type != TokenType.Symbol)
                    {
                        LogHelper.Instance.Log("Location SETRETURNTIME - Operand expected to be a symbol",
                            LogEntryType.Error);
                        AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
                    }

                    var symbolValue = context.GetSymbolValue(Operands[1].Value, result.Errors) as string;
                    if ("NONE".Equals(symbolValue))
                        return;
                    DateTime result2;
                    if (!DateTime.TryParse(symbolValue, out result2))
                    {
                        LogHelper.Instance.Log(
                            "Location SETRETURNTIME Instruction - Unable to parse date/time information.",
                            LogEntryType.Error);
                        AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
                    }
                    else
                    {
                        SetReturnTime(result, result2, true, context);
                    }
                }
                else if (Operands[0].Value.Equals("IS-EXCLUDED"))
                {
                    ILocation location;
                    if (GetLocation(result, context, out location))
                    {
                        context.PushTop(location.Excluded);
                        context.PushTop("SUCCESS");
                    }
                    else
                    {
                        context.PushTop("LocationOutOfRange".ToUpper());
                    }
                }
                else if (Operands[0].Value.Equals("SLOTTYPE"))
                {
                    ILocation location;
                    if (!GetLocation(result, context, out location))
                        context.PushTop("UNKNOWN");
                    else
                        context.PushTop(location.IsWide ? "WIDE" : (object)"NOTWIDE");
                }
                else
                {
                    result.Errors.Add(Error.NewError("E001", "Unrecognized LOC form.",
                        string.Format("The operand {0} isn't recognized.", Operands[0].Value)));
                }
            }
            else if (service2.AtVendDoor)
            {
                context.PushTop("VENDDOOR");
                context.PushTop("VENDDOOR");
            }
            else if (service2.CurrentLocation == null)
            {
                context.PushTop("UNDEFINED");
                context.PushTop("UNDEFINED");
            }
            else
            {
                context.PushTop(service2.CurrentLocation.Slot);
                context.PushTop(service2.CurrentLocation.Deck);
            }
        }

        protected override bool IsOperandRecognized(string op)
        {
            return Keys.Recognized(op);
        }

        private void SetReturnTime(
            ExecutionResult result,
            DateTime? newTime,
            bool leaveResult,
            ExecutionContext context)
        {
            ILocation location1;
            if (!GetLocation(result, context, out location1))
            {
                LogHelper.Instance.Log("Unable to get deck and slot information.", LogEntryType.Error);
                context.PushTop("ERROR");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                location1.ReturnDate = newTime;
                var location2 = location1;
                var instance = service.Save(location2);
                if (!leaveResult)
                    return;
                context.PushTop(instance);
            }
        }

        private static class Keys
        {
            internal const string ClearReturnTime = "CLEARRETURNTIME";
            internal const string IncrementStuck = "INC-STUCK";
            internal const string ClearStuck = "CLEAR-STUCK";
            internal const string Reset = "RESET";
            internal const string GetInfo = "GET-INFO";
            internal const string GetReturnTime = "GETRETURNTIME";
            internal const string MarkReturnTime = "MARKRETURNTIME";
            internal const string AtDrumLocation = "AT-DRUM-LOC";
            internal const string SetInventory = "SETINVENTORY";
            internal const string ResetReturnTime = "RESETRT";
            internal const string Timestamp = "TIMESTAMP";
            internal const string SetReturnTime = "SETRETURNTIME";
            internal const string IsExcluded = "IS-EXCLUDED";
            internal const string SlotType = "SLOTTYPE";

            internal static bool Recognized(string s)
            {
                return string.Compare(s, "CLEARRETURNTIME", true) == 0 || string.Compare(s, "INC-STUCK", true) == 0 ||
                       string.Compare(s, "CLEAR-STUCK", true) == 0 || string.Compare(s, "RESET", true) == 0 ||
                       string.Compare(s, "GET-INFO", true) == 0 || string.Compare(s, "GETRETURNTIME", true) == 0 ||
                       string.Compare(s, "MARKRETURNTIME", true) == 0 || string.Compare(s, "AT-DRUM-LOC", true) == 0 ||
                       string.Compare(s, "SETINVENTORY", true) == 0 || string.Compare(s, "RESETRT", true) == 0 ||
                       string.Compare(s, "TIMESTAMP", true) == 0 || string.Compare(s, "SETRETURNTIME", true) == 0 ||
                       string.Compare(s, "IS-EXCLUDED", true) == 0 || string.Compare(s, "SLOTTYPE", true) == 0;
            }
        }
    }
}