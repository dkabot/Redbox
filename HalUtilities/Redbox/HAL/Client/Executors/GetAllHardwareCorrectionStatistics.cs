using Redbox.HAL.Component.Model;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors
{
  public sealed class GetAllHardwareCorrectionStatistics : JobExecutor
  {
    public List<IHardwareCorrectionStatistic> Stats { get; private set; }

    public GetAllHardwareCorrectionStatistics(HardwareService s, HardwareJobPriority p)
      : base(s, p)
    {
      this.Stats = new List<IHardwareCorrectionStatistic>();
    }

    public GetAllHardwareCorrectionStatistics(HardwareService s)
      : this(s, HardwareJobPriority.Highest)
    {
    }

    protected override string JobName => "get-all-hardware-statistics";

    protected override void DisposeInner() => this.Stats.Clear();

    protected override void OnJobCompleted() => HWCorrectionStat.From(this.Job, this.Stats);
  }
}
