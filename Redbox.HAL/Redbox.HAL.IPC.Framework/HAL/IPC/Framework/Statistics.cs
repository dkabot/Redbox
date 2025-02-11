using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework
{
    public class Statistics
    {
        private readonly object m_syncObject = new object();
        public TimeSpan MaximumCommandExecutionTime;
        public TimeSpan MinimumCommandExecutionTime;
        public long NumberOfBytesReceived;
        public long NumberOfBytesSent;
        public long NumberOfCommandsExecuted;
        public DateTime ServerStartTime;
        public TimeSpan TotalCommandExecutionTime;

        private Statistics()
        {
        }

        public static Statistics Instance => Singleton<Statistics>.Instance;

        public string Host => Environment.MachineName;

        public TimeSpan ServerUpTime => DateTime.Now - ServerStartTime;

        public long AverageNumberOfBytesSent
        {
            get
            {
                long numberOfBytesSent = 0;
                if (NumberOfCommandsExecuted > 0L && NumberOfBytesSent > 0L)
                    numberOfBytesSent = NumberOfBytesSent / NumberOfCommandsExecuted;
                return numberOfBytesSent;
            }
        }

        public long AverageNumberOfBytesReceived
        {
            get
            {
                long numberOfBytesReceived = 0;
                if (NumberOfCommandsExecuted > 0L && NumberOfBytesReceived > 0L)
                    numberOfBytesReceived = NumberOfBytesReceived / NumberOfCommandsExecuted;
                return numberOfBytesReceived;
            }
        }

        public TimeSpan AverageCommandExecutionTime
        {
            get
            {
                long ticks = 0;
                if (NumberOfCommandsExecuted > 0L)
                    ticks = TotalCommandExecutionTime.Ticks / NumberOfCommandsExecuted;
                return new TimeSpan(ticks);
            }
        }

        public void TrackCommandStatistics(TimeSpan executionTime)
        {
            lock (m_syncObject)
            {
                ++NumberOfCommandsExecuted;
                TotalCommandExecutionTime = TotalCommandExecutionTime.Add(executionTime);
                if (executionTime < MinimumCommandExecutionTime)
                {
                    MinimumCommandExecutionTime = executionTime;
                }
                else
                {
                    if (!(executionTime > MaximumCommandExecutionTime))
                        return;
                    MaximumCommandExecutionTime = executionTime;
                }
            }
        }
    }
}