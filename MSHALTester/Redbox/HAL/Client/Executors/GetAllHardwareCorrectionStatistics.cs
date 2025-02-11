using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class GetAllHardwareCorrectionStatistics : JobExecutor
{
    public GetAllHardwareCorrectionStatistics(HardwareService s, HardwareJobPriority p)
        : base(s, p)
    {
        Stats = new List<IHardwareCorrectionStatistic>();
    }

    public GetAllHardwareCorrectionStatistics(HardwareService s)
        : this(s, HardwareJobPriority.Highest)
    {
    }

    public List<IHardwareCorrectionStatistic> Stats { get; }

    protected override string JobName => "get-all-hardware-statistics";

    protected override void DisposeInner()
    {
        Stats.Clear();
    }

    protected override void OnJobCompleted()
    {
        HWCorrectionStat.From(Job, Stats);
    }
}