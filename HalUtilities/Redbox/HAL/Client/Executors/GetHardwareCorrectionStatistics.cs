using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors
{
    public sealed class GetHardwareCorrectionStatistics : JobExecutor
    {
        private readonly HardwareCorrectionStatistic Stat;

        public GetHardwareCorrectionStatistics(
            HardwareService s,
            HardwareCorrectionStatistic stat,
            HardwareJobPriority p)
            : base(s, p)
        {
            Stat = stat;
            Stats = new List<IHardwareCorrectionStatistic>();
        }

        public GetHardwareCorrectionStatistics(HardwareService s, HardwareCorrectionStatistic stat)
            : this(s, stat, HardwareJobPriority.Highest)
        {
        }

        public List<IHardwareCorrectionStatistic> Stats { get; }

        protected override string JobName => "get-hardware-statistics";

        protected override void SetupJob()
        {
            Job.Push(Stat.ToString());
        }

        protected override void DisposeInner()
        {
            Stats.Clear();
        }

        protected override void OnJobCompleted()
        {
            HWCorrectionStat.From(Job, Stats);
        }
    }
}