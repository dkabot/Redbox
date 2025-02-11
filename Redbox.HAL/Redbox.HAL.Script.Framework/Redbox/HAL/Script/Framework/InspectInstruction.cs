using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class InspectInstruction : Instruction
    {
        public override string Mnemonic => "INSPECT";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 2)
                return;
            result.AddError(ExecutionErrors.InvalidOperand,
                "Expected key value pair DECK=y, SLOT=x.  The values of x and y may be numeric literals or symbols.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            int deck;
            int slot;
            if (!GetLocation(result, context, out deck, out slot))
            {
                AddError("E004",
                    "Deck or slot are undefined, reissue the INSPECT instruction specifying both deck and slot values.",
                    result.Errors);
            }
            else
            {
                var byNumber = ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(deck);
                if (byNumber == null || !byNumber.IsSlotValid(slot))
                {
                    context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
                }
                else
                {
                    var location = ServiceLocator.Instance.GetService<IInventoryService>().Get(deck, slot);
                    context.PushTop(location.ID);
                }
            }
        }

        private static class KeyNames
        {
            public const string Deck = "DECK";
            public const string Slot = "SLOT";
        }
    }
}