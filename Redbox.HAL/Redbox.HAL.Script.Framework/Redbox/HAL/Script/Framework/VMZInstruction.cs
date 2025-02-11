using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class VMZInstruction : Instruction
    {
        public override string Mnemonic => "VMZ";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens[0].Type == TokenType.Symbol)
                return;
            result.AddInvalidOperandError("Expected symbol: GET-THIN, GET-UNLOAD or MARK-EMPTY.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            ServiceLocator.Instance.GetService<IMotionControlService>();
            if (string.Compare(Operands[0].Value, "IS-CONFIGURED", true) == 0)
            {
                context.PushTop(ControllerConfiguration.Instance.IsVMZMachine);
            }
            else
            {
                if (string.Compare(Operands[0].Value, "REGISTER-CLEAN", true) == 0)
                    return;
                if (string.Compare(Operands[0].Value, "RANDOMIZE-CLEAN", true) == 0)
                {
                    result.Errors.Add(Error.NewError("E203", "Deprecated operand",
                        "The RANDOMIZE-CLEAN operand is deprecated."));
                }
                else if (string.Compare(Operands[0].Value, "FETCH-LAST-THIN", true) == 0)
                {
                    FetchLast(MerchFlags.Thin, context);
                }
                else if (string.Compare(Operands[0].Value, "FETCH-LAST-REBALANCE", true) == 0)
                {
                    FetchLast(MerchFlags.Rebalance, context);
                }
                else if (string.Compare(Operands[0].Value, "NON-THIN-COUNT", true) == 0)
                {
                    AddError("E101", "NON-THIN-COUNT is not supported.", result.Errors);
                }
                else if (string.Compare(Operands[0].Value, "AGGREGATE-CLEAN", true) == 0)
                {
                    AggregateCleanData(context);
                }
                else if (string.Compare(Operands[0].Value, "CLEAN-SLOT", true) == 0)
                {
                    var result1 = new CleanResult();
                    CleanManager.Instance.CleanSlot(ref result1, context, SegmentManager.Instance);
                    ProcessAnyTransfers(result1, context);
                    if (result1.VisitedAllSlots)
                        context.PushTop("CLEAN");
                    else if (result1.NoEmptySlots)
                        context.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
                    else
                        context.PushTop(result1.TransferFailures > 0 ? "TRANSFERFAILURE" : (object)SuccessMessage);
                }
                else if (string.Compare(Operands[0].Value, "MARK-UNLOAD", true) == 0)
                {
                    var keyValuePairValue =
                        GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("LAST-DECK"), result.Errors, context);
                    if (result.Errors.Count > 0 || !keyValuePairValue.HasValue)
                    {
                        context.PushTop("ERROR");
                    }
                    else
                    {
                        var lowDeck = keyValuePairValue.Value;
                        context.PushTop(VMZ.Instance.ReserveUnload(lowDeck));
                    }
                }
                else if (string.Compare(Operands[0].Value, "MARK-EMPTY", true) == 0)
                {
                    var highest = SegmentManager.Instance.FindHighest();
                    if (highest == null)
                        context.PushTop(0);
                    else
                        context.PushTop(VMZ.Instance.ResetCompressedZone(highest));
                }
                else if (string.Compare(Operands[0].Value, "DUMP-ZONE", true) == 0)
                {
                    AddError("E002", "The operand DUMP-ZONE is no longer supported.", result.Errors);
                }
                else if (string.Compare(Operands[0].Value, "DUMP-ZONE-TO-APPLOG", true) == 0)
                {
                    VMZ.Instance.DumpZone(context.AppLog);
                }
                else if (string.Compare(Operands[0].Value, "THIN-DISC", true) == 0)
                {
                    ThinType(result, context, MerchFlags.Thin);
                }
                else if (string.Compare(Operands[0].Value, "THIN-REBALANCE", true) == 0)
                {
                    ThinType(result, context, MerchFlags.Rebalance);
                }
                else if (string.Compare(Operands[0].Value, "THIN-REDEPLOY", true) == 0)
                {
                    ThinType(result, context, MerchFlags.ThinRedeploy);
                }
                else if (string.Compare(Operands[0].Value, "UNLOAD-DISC", true) == 0)
                {
                    var merchandizeResult = VMZHelperFunctions.Instance.UnloadDisc(result, context);
                    context.PushTop(merchandizeResult.ToString().ToUpper());
                }
                else if (string.Compare(Operands[0].Value, "GET-LAST-THIN", true) == 0)
                {
                    LocateLast(MerchFlags.Thin, context);
                }
                else if (string.Compare(Operands[0].Value, "GET-LAST-REBALANCE", true) == 0)
                {
                    LocateLast(MerchFlags.Rebalance, context);
                }
                else if (string.Compare(Operands[0].Value, "GET-LAST-THIN-REDEPLOY", true) == 0)
                {
                    LocateLast(MerchFlags.ThinRedeploy, context);
                }
                else if (string.Compare(Operands[0].Value, "THIN-ZONE-COUNT", true) == 0)
                {
                    PushCount(MerchFlags.Thin, context);
                }
                else if (string.Compare(Operands[0].Value, "REBALANCE-COUNT", true) == 0)
                {
                    PushCount(MerchFlags.Rebalance, context);
                }
                else if (string.Compare(Operands[0].Value, "THIN-REDEPLOY-COUNT", true) == 0)
                {
                    PushCount(MerchFlags.ThinRedeploy, context);
                }
                else if (string.Compare(Operands[0].Value, "MOVE-VIEWABLE", true) == 0)
                {
                    context.PushTop(VMZHelperFunctions.MoveViewable(context).ToString().ToUpper());
                }
                else
                {
                    if (string.Compare(Operands[0].Value, "ZONE-NON-THIN-COUNT", true) != 0)
                        return;
                    var inCompressedZone = SegmentManager.Instance.GetNonThinItemsInCompressedZone();
                    context.PushTop(inCompressedZone.Count.ToString());
                    inCompressedZone.Clear();
                }
            }
        }

        private void FetchLast(MerchFlags flags, ExecutionContext context)
        {
            var service1 = ServiceLocator.Instance.GetService<IRuntimeService>();
            var service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
            var ms = GetTimeOutValue(new ErrorList(), context) ?? 8000;
            service1.Wait(ms);
            var highestLocation = VMZ.Instance.FindHighestLocation(flags);
            if (highestLocation == null)
            {
                var num = (int)VMZHelperFunctions.MoveViewable(context);
                context.PushTop("NONE");
            }
            else
            {
                var errorCodes = service2.MoveTo(highestLocation, MoveMode.Get, context.AppLog);
                if (errorCodes != ErrorCodes.Success)
                {
                    context.PushTop(errorCodes.ToString().ToUpper());
                }
                else
                {
                    var getResult = ServiceLocator.Instance.GetService<IControllerService>().Get();
                    if (getResult.Success)
                    {
                        var num = (int)VMZHelperFunctions.MoveViewable(context);
                        context.PushTop(SuccessMessage);
                    }
                    else
                    {
                        context.PushTop(getResult.ToString().ToUpper());
                    }
                }
            }
        }

        private void PushCount(MerchFlags flags, ExecutionContext context)
        {
            var merchSegment = MerchSegmentFactory.Get(flags);
            var instance = context != null ? merchSegment.ItemCount() : 0;
            context.PushTop(instance);
        }

        private void ThinType(ExecutionResult result, ExecutionContext context, MerchFlags flags)
        {
            var id = GetID(result, context);
            var merchandizeResult = ThinHelper.MakeInstance(result, context).ThinDisk(id, flags);
            if (merchandizeResult != MerchandizeResult.Success)
                LogHelper.Instance.WithContext("Thin {0} returned status {1}", flags.ToString(), merchandizeResult);
            context.PushTop(merchandizeResult.ToString().ToUpper());
        }

        private void ProcessAnyTransfers(CleanResult result, ExecutionContext context)
        {
            if (result.Transfers == 0)
                return;
            var num1 = 0;
            var errors = new ErrorList();
            var symbolValue = context.GetSymbolValue("TRANSFER-COUNT", errors);
            if (errors.Count == 0)
                num1 = (int)symbolValue;
            var num2 = num1 + result.Transfers;
            context.SetSymbolValue("TRANSFER-COUNT", num2);
        }

        private void LocateLast(MerchFlags flags, ExecutionContext context)
        {
            var highestLocation = VMZ.Instance.FindHighestLocation(flags);
            if (highestLocation == null)
            {
                context.PushTop("NONE");
            }
            else
            {
                context.PushTop(highestLocation.Slot);
                context.PushTop(highestLocation.Deck);
                context.PushTop("FOUND");
            }
        }

        private void AggregateCleanData(ExecutionContext ctx)
        {
            var num = 0;
            var errors = new ErrorList();
            var symbolValue = ctx.GetSymbolValue("TRANSFER-COUNT", errors);
            if (errors.Count == 0)
                num = (int)symbolValue;
            ctx.CreateInfoResult("TransferredDiskCount", num.ToString());
            foreach (var flags in SegmentManager.Instance.DecreasingPriorityOrder)
            {
                var merchSegment = MerchSegmentFactory.Get(flags);
                ctx.CreateInfoResult(string.Format("{0}CountInZone", flags.ToString()),
                    merchSegment.ItemCount().ToString());
                var highestLocation = VMZ.Instance.FindHighestLocation(flags);
                if (highestLocation != null)
                    ctx.CreateInfoResult(string.Format("High{0}Location", flags.ToString()),
                        highestLocation.ToString());
            }
        }
    }
}