using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "dump-hardware-statistics")]
    internal sealed class DumpHardwareStatisticsJob : NativeJobAdapter
    {
        internal DumpHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
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
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var path = Path.Combine(
                ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"),
                string.Format("{0}CorrectionStats.txt", ignoringCase.ToString()));
            service.SafeDelete(path);
            using (var writer = new StreamWriter(path))
            {
                var stats = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>()
                    .GetStats(ignoringCase);
                stats.Sort((x, y) => x.CorrectionTime.CompareTo(y.CorrectionTime));
                using (new DisposeableList<IHardwareCorrectionStatistic>(stats))
                {
                    writer.WriteLine("{0} Kiosk:{1}", DateTime.Now, service.KioskId);
                    stats.ForEach(s => writer.WriteLine("{0},{1},{2}", s.CorrectionTime.ToString(), s.ProgramName,
                        s.CorrectionOk ? "SUCCESS" : (object)"FAILURE"));
                }
            }
        }
    }
}