using System;
using System.Diagnostics;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public class PerformanceCounterHelper
    {
        private volatile bool m_countersEnabled;
        private string m_groupName;

        private PerformanceCounterHelper()
        {
        }

        public static PerformanceCounterHelper Instance => Singleton<PerformanceCounterHelper>.Instance;

        public void Initialize(string groupName)
        {
            m_countersEnabled = true;
            m_groupName = groupName;
        }

        public void IncrementCommandPerSecond(string command)
        {
            if (!m_countersEnabled)
                return;
            try
            {
                using (var performanceCounter = new PerformanceCounter(m_groupName, FormatCountName(command), false))
                {
                    performanceCounter.Increment();
                }

                using (var performanceCounter = new PerformanceCounter(m_groupName, "Total Commands/second", false))
                {
                    performanceCounter.Increment();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Exception raised in IncrementCounter: ", ex);
                m_countersEnabled = false;
            }
        }

        public void IncrementCommandExecutionTime(string command, long ticks)
        {
            if (!m_countersEnabled)
                return;
            try
            {
                using (var performanceCounter =
                       new PerformanceCounter(m_groupName, FormatExecutionTimeName(command), false))
                {
                    performanceCounter.IncrementBy(ticks);
                }

                using (var performanceCounter =
                       new PerformanceCounter(m_groupName, FormatExecutionTimeBaseName(command), false))
                {
                    performanceCounter.IncrementBy(1L);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Exception raised in IncrementCommandExecutionTime: ", ex);
                m_countersEnabled = false;
            }
        }

        public static string FormatCountName(string command)
        {
            return string.Format("{0}/second", command);
        }

        public static string FormatCountDescription(string command)
        {
            return string.Format("{0} calls per second", command);
        }

        public static string FormatExecutionTimeName(string command)
        {
            return string.Format("{0} Execution Time (seconds)", command);
        }

        public static string FormatExecutionTimeDescription(string command)
        {
            return string.Format("{0} execution time in seconds", command);
        }

        public static string FormatExecutionTimeBaseName(string command)
        {
            return string.Format("{0} Execution Time Base", command);
        }

        public static string FormatExecutionTimeBaseDescription(string command)
        {
            return string.Format("{0} execution time in seconds base", command);
        }
    }
}