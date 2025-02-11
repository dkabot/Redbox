using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-machine-info", Operand = "GET-MACH-INFO")]
    internal sealed class GetMachineInfoJob : NativeJobAdapter
    {
        internal GetMachineInfoJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var qlmDeck = ServiceLocator.Instance.GetService<IDecksService>().QlmDeck;
            if (qlmDeck == null)
                Context.CreateInfoResult("QlmMissingInfo", "There is no QLM deck defined for this machine.");
            else
                Context.CreateResult("QlmInfo",
                    "Designated QLM deck for this machine, slot equals maximum number for this deck.", qlmDeck.Number,
                    qlmDeck.NumberOfSlots, null, new DateTime?(), null);
            DecksService.ForAllReverseDecksDo(deck =>
            {
                Context.CreateResult("DeckInfo", "Slot equals maximum number for this deck.", deck.Number,
                    deck.NumberOfSlots, null, new DateTime?(), null);
                return true;
            });
            Context.CreateResult("MaxDeckInfo", "Deck equals maximum number for this machine.", DecksService.DeckCount,
                1, null, new DateTime?(), null);
            Context.CreateInfoResult("BufferSlot", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
            Context.CreateInfoResult("MerchType", qlmDeck == null ? "VMZ2" : "QLM");
        }
    }
}