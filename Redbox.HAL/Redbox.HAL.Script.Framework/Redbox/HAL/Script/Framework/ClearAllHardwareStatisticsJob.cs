using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "clear-all-hardware-statistics")]
    internal sealed class ClearAllHardwareStatisticsJob : NativeJobAdapter
    {
        internal ClearAllHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().RemoveAll();
        }
    }
}