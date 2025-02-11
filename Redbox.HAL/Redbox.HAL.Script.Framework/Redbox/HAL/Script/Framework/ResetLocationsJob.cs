using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-locations")]
    internal sealed class ResetLocationsJob : NativeJobAdapter
    {
        internal ResetLocationsJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            var num1 = Context.PopTop<int>();
            var locationList = new List<ILocation>();
            using (new DisposeableList<ILocation>(locationList))
            {
                for (var index = 0; index < num1; ++index)
                {
                    var num2 = Context.PopTop<int>();
                    var slot = Context.PopTop<int>();
                    var byNumber = DecksService.GetByNumber(num2);
                    if (byNumber != null && byNumber.IsSlotValid(slot))
                    {
                        var location = service.Get(num2, slot);
                        locationList.Add(location);
                        Context.CreateResult("LocationReset", "The location was reset.", num2, slot, null,
                            new DateTime?(), null);
                    }
                }

                service.Reset(locationList);
                Context.CreateInfoResult("TotalLocationsReset", locationList.Count.ToString());
            }
        }
    }
}