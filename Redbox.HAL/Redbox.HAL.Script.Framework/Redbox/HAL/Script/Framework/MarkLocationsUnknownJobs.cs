using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "mark-locations-unknown")]
    internal sealed class MarkLocationsUnknownJobs : NativeJobAdapter
    {
        internal MarkLocationsUnknownJobs(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var num = Context.PopTop<int>();
            if (num == 0)
                return;
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            var locationList = new List<ILocation>();
            using (new DisposeableList<ILocation>(locationList))
            {
                for (var index = 0; index < num; ++index)
                {
                    var deck = Context.PopTop<int>();
                    var slot = Context.PopTop<int>();
                    var location = service.Get(deck, slot);
                    if (location != null)
                        location.ID = "UNKNOWN";
                }

                if (service.Save(locationList))
                    return;
                AddError("FAILED TO UPDATE");
            }
        }
    }
}