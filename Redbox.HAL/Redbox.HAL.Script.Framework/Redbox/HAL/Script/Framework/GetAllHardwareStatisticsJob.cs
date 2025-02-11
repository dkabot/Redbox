using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-all-hardware-statistics")]
    internal sealed class GetAllHardwareStatisticsJob : NativeJobAdapter
    {
        internal GetAllHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            using (var statsFormatter =
                   new StatsFormatter(ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>()
                       .GetStats()))
            {
                statsFormatter.Format(Context);
            }
        }
    }
}