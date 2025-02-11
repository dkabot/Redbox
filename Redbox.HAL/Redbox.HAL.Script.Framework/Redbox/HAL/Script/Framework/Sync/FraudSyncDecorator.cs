using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Sync
{
    internal sealed class FraudSyncDecorator : SyncDecorator
    {
        internal FraudSyncDecorator(ExecutionContext ctx, ExecutionResult r, ILocation l)
            : base(ctx, r, l)
        {
        }

        internal override SyncResult AfterDiskRead(ScanResult sr)
        {
            using (var fraudValidator = new FraudValidator(Context))
            {
                var num = (int)fraudValidator.Validate(sr);
            }

            return SyncResult.Success;
        }
    }
}