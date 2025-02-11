using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class TransferInstruction : Instruction
    {
        private const string SourceDeck = "SRC-DECK";
        private const string SourceSlot = "SRC-SLOT";
        private const string DestDeck = "DEST-DECK";
        private const string DestSlot = "DEST-SLOT";

        public override string Mnemonic => "XFER";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 4)
                return;
            result.AddInvalidOperandError(
                "Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var sensorReadResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadPickerSensors();
            if (!sensorReadResult.Success)
            {
                context.PushTop(ErrorCodes.SensorReadError.ToString().ToUpper());
            }
            else if (sensorReadResult.IsFull)
            {
                context.PushTop("PICKERFULL");
                LogHelper.Instance.Log("XFER: the picker is full - bail.");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IInventoryService>();
                var keyValuePairValue1 =
                    GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("SRC-DECK"), result.Errors, context);
                if (!keyValuePairValue1.HasValue)
                {
                    AddError("E004",
                        "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.",
                        result.Errors);
                }
                else
                {
                    var deck = keyValuePairValue1.Value;
                    var keyValuePairValue2 =
                        GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("SRC-SLOT"), result.Errors, context);
                    if (!keyValuePairValue2.HasValue)
                    {
                        AddError("E004",
                            "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.",
                            result.Errors);
                    }
                    else
                    {
                        var slot = keyValuePairValue2.Value;
                        var keyValuePairValue3 = GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("DEST-DECK"),
                            result.Errors, context);
                        var keyValuePairValue4 = GetKeyValuePairValue<int?>(Operands.GetKeyValuePair("DEST-SLOT"),
                            result.Errors, context);
                        if (result.Errors.Count > 0)
                            return;
                        if (!keyValuePairValue3.HasValue || !keyValuePairValue4.HasValue)
                        {
                            AddError("E004",
                                "Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.",
                                result.Errors);
                        }
                        else
                        {
                            var source = service.Get(deck, slot);
                            var destination = service.Get(keyValuePairValue3.Value, keyValuePairValue4.Value);
                            LogHelper.Instance.Log("XFER: {0} to {1}", source.ToString(), destination.ToString());
                            var appLog = context.AppLog;
                            var transferResult = ServiceLocator.Instance.GetService<IControllerService>()
                                .Transfer(source, destination, false);
                            context.PushTop(transferResult.Transferred
                                ? SuccessMessage
                                : (object)transferResult.TransferError.ToString().ToUpper());
                        }
                    }
                }
            }
        }
    }
}