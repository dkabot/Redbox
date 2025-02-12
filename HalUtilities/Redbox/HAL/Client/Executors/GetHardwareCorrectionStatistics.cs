using Redbox.HAL.Component.Model;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors
{
  public sealed class GetHardwareCorrectionStatistics : JobExecutor
  {
    private readonly HardwareCorrectionStatistic Stat;

    public List<IHardwareCorrectionStatistic> Stats { get; private set; }

    public GetHardwareCorrectionStatistics(
      HardwareService s,
      HardwareCorrectionStatistic stat,
      HardwareJobPriority p)
      : base(s, p)
    {
      this.Stat = stat;
      this.Stats = new List<IHardwareCorrectionStatistic>();
    }

    public GetHardwareCorrectionStatistics(HardwareService s, HardwareCorrectionStatistic stat)
      : this(s, stat, HardwareJobPriority.Highest)
    {
    }

    protected override string JobName => "get-hardware-statistics";

    protected override void SetupJob() => this.Job.Push((object) this.Stat.ToString());

    protected override void DisposeInner() => this.Stats.Clear();

    protected override void OnJobCompleted() => HWCorrectionStat.From(this.Job, this.Stats);
  }
}
