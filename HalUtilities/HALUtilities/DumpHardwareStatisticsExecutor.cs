using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class DumpHardwareStatisticsExecutor : JobExecutor
    {
        private readonly HardwareCorrectionStatistic Stat;

        internal DumpHardwareStatisticsExecutor(HardwareService service, HardwareCorrectionStatistic s)
            : base(service)
        {
            Stat = s;
        }

        protected override string JobName => "dump-hardware-statistics";

        protected override void SetupJob()
        {
            Job.Push(Stat.ToString());
        }
    }
}