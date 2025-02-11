using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-quick-return-counters")]
    internal sealed class QuickReturnCountersJob : NativeJobAdapter
    {
        internal QuickReturnCountersJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var pcs = ServiceLocator.Instance.GetService<IPersistentCounterService>();
            new QuickReturnCounters().DoForeach(counter =>
            {
                var persistentCounter = pcs.Find(counter);
                var message = persistentCounter == null ? "0" : persistentCounter.Value.ToString();
                Context.CreateInfoResult(counter, message);
            });
        }
    }
}