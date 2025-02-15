using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
    internal sealed class ResetHardwareStatisticsExecutor : JobExecutor
    {
        private readonly HardwareCorrectionStatistic Stat;

        internal ResetHardwareStatisticsExecutor(HardwareService service, HardwareCorrectionStatistic s)
            : base(service)
        {
            Stat = s;
        }

        protected override string JobName => "clear-hardware-statistics";

        protected override void SetupJob()
        {
            Job.Push(Stat.ToString());
        }
    }
}