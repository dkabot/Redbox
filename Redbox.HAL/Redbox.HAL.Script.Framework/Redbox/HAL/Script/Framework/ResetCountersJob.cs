using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "reset-counters-job", Operand = "RESET-COUNTERS-JOB")]
    internal sealed class ResetCountersJob : NativeJobAdapter
    {
        private readonly IPersistentMap DataMap;
        private DateTime? m_lastReset;

        internal ResetCountersJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
            DataMap = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
        }

        private DateTime? LastCounterReset
        {
            get
            {
                if (m_lastReset.HasValue)
                    return m_lastReset.Value;
                var s = DataMap.GetValue(nameof(LastCounterReset), "NONE");
                if (s == "NONE")
                    return m_lastReset = new DateTime?();
                try
                {
                    m_lastReset = DateTime.Parse(s);
                    return m_lastReset;
                }
                catch
                {
                    return m_lastReset = new DateTime?();
                }
            }
            set
            {
                m_lastReset = value;
                DataMap.SetValue(nameof(LastCounterReset),
                    !m_lastReset.HasValue ? "NONE" : m_lastReset.Value.ToString());
            }
        }

        protected override void ExecuteInner()
        {
            var now = DateTime.Now;
            if ((!LastCounterReset.HasValue || LastCounterReset.Value.Date != now.Date) &&
                DateRange.NowIsBetweenHours(0, 1))
            {
                LastCounterReset = now;
                LogHelper.Instance.Log("Watchdog resetting counters.");
                var service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
                service.Reset("EmptyOrStuckCount");
                if (now.DayOfWeek == DayOfWeek.Sunday)
                {
                    LogHelper.Instance.Log("Reset timeout counters.", LogEntryType.Info);
                    service.ResetWeekly();
                }
            }

            LogHelper.Instance.Log("[Watchdog] Kiosk function check cleaned {0} entries",
                ServiceLocator.Instance.GetService<IKioskFunctionCheckService>().CleanOldEntries());
        }
    }
}