using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    internal class VMZHelperFunctions
    {
        private VMZHelperFunctions()
        {
        }

        internal static VMZHelperFunctions Instance => Singleton<VMZHelperFunctions>.Instance;

        internal static ErrorCodes MoveViewable(ExecutionContext context)
        {
            var location = ServiceLocator.Instance.GetService<IInventoryService>().Get(3, 86);
            return ServiceLocator.Instance.GetService<IMotionControlService>()
                .MoveTo(location, MoveMode.None, context.AppLog);
        }

        internal MerchandizeResult UnloadDisc(ExecutionResult result, ExecutionContext context)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            var loc = (ILocation)null;
            ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(deck =>
            {
                for (var slot = 1; slot <= deck.NumberOfSlots; ++slot)
                {
                    var location = service.Get(deck.Number, slot);
                    if (location.Flags == MerchFlags.Unload)
                    {
                        loc = location;
                        return false;
                    }
                }

                return true;
            });
            if (loc == null)
                return MerchandizeResult.UnloadsCleared;
            var merchandizeResult = new UnloadHelper(context, result).UnloadDisk(loc.Deck, loc.Slot);
            if (merchandizeResult != MerchandizeResult.Success)
                LogHelper.Instance.WithContext("Unload disk returned status {0}", merchandizeResult.ToString());
            return merchandizeResult;
        }
    }
}