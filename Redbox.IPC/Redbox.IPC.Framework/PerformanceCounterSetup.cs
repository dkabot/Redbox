using System.Collections.Generic;
using System.Diagnostics;

namespace Redbox.IPC.Framework
{
    public class PerformanceCounterSetup
    {
        public static void Initialize(string category, List<string> commands)
        {
            RemovePerformanceCounters(category);
            var counterData = new CounterCreationDataCollection();
            foreach (var command in commands)
            {
                counterData.Add(new CounterCreationData(PerformanceCounterHelper.FormatCountName(command),
                    PerformanceCounterHelper.FormatCountDescription(command),
                    PerformanceCounterType.RateOfCountsPerSecond64));
                counterData.Add(new CounterCreationData(PerformanceCounterHelper.FormatExecutionTimeName(command),
                    PerformanceCounterHelper.FormatExecutionTimeDescription(command),
                    PerformanceCounterType.AverageTimer32));
                counterData.Add(new CounterCreationData(PerformanceCounterHelper.FormatExecutionTimeBaseName(command),
                    PerformanceCounterHelper.FormatExecutionTimeBaseDescription(command),
                    PerformanceCounterType.AverageBase));
            }

            counterData.Add(new CounterCreationData("Total Commands/second", "Total Commands executed per second.",
                PerformanceCounterType.RateOfCountsPerSecond64));
            PerformanceCounterCategory.Create(category, "Custom performance counters",
                PerformanceCounterCategoryType.SingleInstance, counterData);
            PerformanceCounterHelper.Instance.Initialize(category);
        }

        private static void RemovePerformanceCounters(string category)
        {
            if (!PerformanceCounterCategory.Exists(category))
                return;
            PerformanceCounterCategory.Delete(category);
        }
    }
}