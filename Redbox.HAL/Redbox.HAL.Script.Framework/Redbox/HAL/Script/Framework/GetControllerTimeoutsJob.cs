using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-controller-timeouts")]
    internal sealed class GetControllerTimeoutsJob : NativeJobAdapter
    {
        internal GetControllerTimeoutsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
            foreach (TimeoutCounters counter in Enum.GetValues(typeof(TimeoutCounters)))
                MakeResult(service.Find(counter));
            MakeResult(service.Find("EmptyOrStuckCount"));
        }

        private void MakeResult(IPersistentCounter counter)
        {
            if (counter == null)
                return;
            Context.CreateInfoResult(counter.Name, counter.Value.ToString());
        }
    }
}