using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities
{
  internal sealed class DumpHardwareStatisticsExecutor : JobExecutor
  {
    private readonly HardwareCorrectionStatistic Stat;

    protected override string JobName => "dump-hardware-statistics";

    protected override void SetupJob() => this.Job.Push((object) this.Stat.ToString());

    internal DumpHardwareStatisticsExecutor(HardwareService service, HardwareCorrectionStatistic s)
      : base(service)
    {
      this.Stat = s;
    }
  }
}
