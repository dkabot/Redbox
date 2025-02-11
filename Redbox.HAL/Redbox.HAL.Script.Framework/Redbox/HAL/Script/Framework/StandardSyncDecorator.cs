using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class StandardSyncDecorator : SyncDecorator
    {
        internal StandardSyncDecorator(ExecutionContext ctx, ExecutionResult r, ILocation l)
            : base(ctx, r, l)
        {
        }

        internal override void OnGetFailure(IGetResult notUsed)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            SyncLocation.ID = "UNKNOWN";
            var syncLocation = SyncLocation;
            service.Save(syncLocation);
        }
    }
}