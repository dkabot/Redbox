using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal class DumpBinThinHelper : ThinHelper
    {
        internal DumpBinThinHelper(ExecutionResult result, ExecutionContext context)
            : base(result, context)
        {
        }

        internal override MerchandizeResult ThinDisk(string id, MerchFlags flags)
        {
            var service1 = ServiceLocator.Instance.GetService<IDumpbinService>();
            var service2 = ServiceLocator.Instance.GetService<IInventoryService>();
            if (service1.IsFull())
            {
                Context.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
                return MerchandizeResult.DumpBinFull;
            }

            Context.AppLog.Write(string.Format("Attempt thin of item {0}", id));
            var source1 = service2.Lookup(id);
            if (source1 == null)
            {
                Context.CreateLookupErrorResult("ThinLookupFailed", id);
                return MerchandizeResult.LookupFailure;
            }

            var merchandizeResult = FetchThin(source1, id);
            switch (merchandizeResult)
            {
                case MerchandizeResult.Success:
                    return ThinDiskToLocation(source1, service1.PutLocation, id, MerchFlags.Thin);
                case MerchandizeResult.EmptyStuck:
                    return merchandizeResult;
                default:
                    var source2 = ReturnToSource(source1, id);
                    return source2 != MerchandizeResult.Success ? source2 : merchandizeResult;
            }
        }

        protected override ILocation FindEmptyTarget(MerchFlags flags, out MerchandizeResult result)
        {
            var service = ServiceLocator.Instance.GetService<IDumpbinService>();
            if (service.IsFull())
            {
                result = MerchandizeResult.DumpBinFull;
                return null;
            }

            result = MerchandizeResult.Success;
            return service.PutLocation;
        }
    }
}