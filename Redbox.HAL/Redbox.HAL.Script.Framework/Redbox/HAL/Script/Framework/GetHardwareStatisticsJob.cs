using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "get-hardware-statistics")]
    internal sealed class GetHardwareStatisticsJob : NativeJobAdapter
    {
        internal GetHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var ignoringCase =
                Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(Context.PopTop<string>(),
                    HardwareCorrectionStatistic.None);
            if (ignoringCase == HardwareCorrectionStatistic.None)
                return;
            using (var statsFormatter = new StatsFormatter(ServiceLocator.Instance
                       .GetService<IHardwareCorrectionStatisticService>().GetStats(ignoringCase)))
            {
                statsFormatter.Format(Context);
            }
        }
    }
}