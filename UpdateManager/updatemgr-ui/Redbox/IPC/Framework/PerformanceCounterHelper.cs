using Redbox.Core;
using System;
using System.Diagnostics;

namespace Redbox.IPC.Framework
{
    internal class PerformanceCounterHelper
    {
        private string m_groupName;
        private volatile bool m_countersEnabled;

        public static PerformanceCounterHelper Instance => Singleton<PerformanceCounterHelper>.Instance;

        public void Initialize(string groupName)
        {
            this.m_countersEnabled = true;
            this.m_groupName = groupName;
        }

        public void IncrementCommandPerSecond(string command)
        {
            if (!this.m_countersEnabled)
                return;
            try
            {
                using (PerformanceCounter performanceCounter = new PerformanceCounter(this.m_groupName, PerformanceCounterHelper.FormatCountName(command), false))
                    performanceCounter.Increment();
                using (PerformanceCounter performanceCounter = new PerformanceCounter(this.m_groupName, "Total Commands/second", false))
                    performanceCounter.Increment();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Exception raised in IncrementCounter: ", ex);
                this.m_countersEnabled = false;
            }
        }

        public void IncrementCommandExecutionTime(string command, long ticks)
        {
            if (!this.m_countersEnabled)
                return;
            try
            {
                using (PerformanceCounter performanceCounter = new PerformanceCounter(this.m_groupName, PerformanceCounterHelper.FormatExecutionTimeName(command), false))
                    performanceCounter.IncrementBy(ticks);
                using (PerformanceCounter performanceCounter = new PerformanceCounter(this.m_groupName, PerformanceCounterHelper.FormatExecutionTimeBaseName(command), false))
                    performanceCounter.IncrementBy(1L);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Exception raised in IncrementCommandExecutionTime: ", ex);
                this.m_countersEnabled = false;
            }
        }

        public static string FormatCountName(string command)
        {
            return string.Format("{0}/second", (object)command);
        }

        public static string FormatCountDescription(string command)
        {
            return string.Format("{0} calls per second", (object)command);
        }

        public static string FormatExecutionTimeName(string command)
        {
            return string.Format("{0} Execution Time (seconds)", (object)command);
        }

        public static string FormatExecutionTimeDescription(string command)
        {
            return string.Format("{0} execution time in seconds", (object)command);
        }

        public static string FormatExecutionTimeBaseName(string command)
        {
            return string.Format("{0} Execution Time Base", (object)command);
        }

        public static string FormatExecutionTimeBaseDescription(string command)
        {
            return string.Format("{0} execution time in seconds base", (object)command);
        }

        private PerformanceCounterHelper()
        {
        }
    }
}
