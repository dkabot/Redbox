using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class DumpBinInstruction : Instruction
    {
        public override string Mnemonic => "DUMPBIN";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count >= 1 && IsValidOperand(operandTokens[0].Value))
                return;
            result.AddInvalidOperandError(string.Format("The {0} instruction expects at lest one operand.", Mnemonic));
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<ExpectedOperand>.ParseIgnoringCase(Operands[0].Value, ExpectedOperand.None);
            var service = ServiceLocator.Instance.GetService<IDumpbinService>();
            switch (ignoringCase)
            {
                case ExpectedOperand.None:
                    var str = string.Format("DumpBinInstruction: unrecognized operand '{0}'.", Operands[0].Value);
                    LogHelper.Instance.WithContext(str);
                    result.Errors.Add(Error.NewError("E001", "Unrecognized operand.", str));
                    break;
                case ExpectedOperand.IsConfigured:
                    context.PushTop(ControllerConfiguration.Instance.IsVMZMachine);
                    break;
                case ExpectedOperand.SetCapacity:
                    result.Errors.Add(Error.NewError("E011", "Unsupported operand.",
                        "The SetCapacity operand is no longer supported."));
                    break;
                case ExpectedOperand.GetCapacity:
                    context.PushTop(service.Capacity);
                    break;
                case ExpectedOperand.ResetCounter:
                    service.ClearItems();
                    break;
                case ExpectedOperand.GetCurrentCount:
                    context.PushTop(service.CurrentCount());
                    break;
                case ExpectedOperand.ResetCounterMove:
                    service.ClearItems();
                    var num = (int)VMZHelperFunctions.MoveViewable(context);
                    break;
                case ExpectedOperand.FileDisk:
                    var id = GetID(result, context);
                    if (result.Errors.ContainsError() || string.IsNullOrEmpty(id))
                    {
                        context.PushTop(MerchandizeResult.LookupFailure);
                        break;
                    }

                    var dumpBinThinHelper = new DumpBinThinHelper(result, context);
                    context.PushTop(dumpBinThinHelper.ThinDisk(id, MerchFlags.Thin).ToString().ToUpper());
                    break;
                case ExpectedOperand.DumpInventory:
                    service.DumpContents(context.AppLog);
                    break;
            }
        }

        private bool IsValidOperand(string op)
        {
            return Enum<ExpectedOperand>.ParseIgnoringCase(op, ExpectedOperand.None) != 0;
        }

        private enum ExpectedOperand
        {
            None,
            IsConfigured,
            SetCapacity,
            GetCapacity,
            ResetCounter,
            GetCurrentCount,
            ResetCounterMove,
            FileDisk,
            DumpInventory
        }
    }
}