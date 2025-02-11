using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal class QlmThinHelper : ThinHelper
    {
        internal QlmThinHelper(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        protected override MerchandizeResult ThinDiskInner(
            string id,
            ILocation source,
            ILocation notUsed,
            MerchFlags mf)
        {
            var thinSlot = GetThinSlot();
            var service1 = ServiceLocator.Instance.GetService<IDecksService>();
            var service2 = ServiceLocator.Instance.GetService<IInventoryService>();
            try
            {
                while (thinSlot <= service1.QlmDeck.NumberOfSlots)
                {
                    var target = service2.Get(service1.QlmDeck.Number, thinSlot++);
                    var location = ThinDiskToLocation(source, target, id, mf);
                    if (MerchandizeResult.SlotInUse != location)
                        return location;
                }

                return MerchandizeResult.QLMFull;
            }
            finally
            {
                Context.SetSymbolValue("THIN-SLOT", thinSlot);
            }
        }

        protected override ILocation FindEmptyTarget(MerchFlags notUsed, out MerchandizeResult result)
        {
            var thinSlot = GetThinSlot();
            result = MerchandizeResult.Success;
            var qlmDeck = ServiceLocator.Instance.GetService<IDecksService>().QlmDeck;
            if (thinSlot <= qlmDeck.NumberOfSlots)
                return ServiceLocator.Instance.GetService<IInventoryService>().Get(qlmDeck.Number, thinSlot);
            result = MerchandizeResult.QLMFull;
            return null;
        }

        private int GetThinSlot()
        {
            var thinSlot = 1;
            var errors = new ErrorList();
            var symbolValue = Context.GetSymbolValue("THIN-SLOT", errors);
            if (errors.Count == 0)
                thinSlot = (int)symbolValue;
            return thinSlot;
        }
    }
}