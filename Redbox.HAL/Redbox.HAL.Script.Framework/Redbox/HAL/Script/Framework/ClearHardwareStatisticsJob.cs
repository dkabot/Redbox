using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "clear-hardware-statistics")]
    internal sealed class ClearHardwareStatisticsJob : NativeJobAdapter
    {
        internal ClearHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
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
            ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().RemoveAll(ignoringCase);
        }
    }
}