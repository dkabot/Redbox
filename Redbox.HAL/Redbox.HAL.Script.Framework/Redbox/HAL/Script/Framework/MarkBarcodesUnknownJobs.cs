using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "mark-barcodes-unknown")]
    internal sealed class MarkBarcodesUnknownJobs : NativeJobAdapter
    {
        internal MarkBarcodesUnknownJobs(ExecutionResult result, ExecutionContext ctx)
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
                    var id = Context.PopTop<string>();
                    var location = service.Lookup(id);
                    if (location != null)
                        location.ID = "UNKNOWN";
                }

                if (locationList.Count <= 0 || service.Save(locationList))
                    return;
                AddError("FAILED TO UPDATE");
            }
        }
    }
}