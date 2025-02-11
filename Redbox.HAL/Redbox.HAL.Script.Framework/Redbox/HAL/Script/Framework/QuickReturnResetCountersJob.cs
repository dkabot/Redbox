using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-quick-return-counters")]
    internal sealed class QuickReturnResetCountersJob : NativeJobAdapter
    {
        internal QuickReturnResetCountersJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var errors = 0;
            var pcs = ServiceLocator.Instance.GetService<IPersistentCounterService>();
            new QuickReturnCounters().DoForeach(counter =>
            {
                if (pcs.Reset(counter))
                    return;
                ++errors;
            });
            if (errors <= 0)
                return;
            AddError("Failed to reset counter.");
        }
    }
}