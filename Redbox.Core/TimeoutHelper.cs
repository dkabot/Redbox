using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core
{
    public static class TimeoutHelper
    {
        private const int TimerInterval = 25;
        private static readonly Timer m_timer;
        private static volatile bool m_isProcessing;
        private static readonly object m_lockObject = new object();
        private static readonly List<TimeoutEntry> TimeoutEntries = new List<TimeoutEntry>();

        static TimeoutHelper()
        {
            m_timer = new Timer(o =>
            {
                if (m_isProcessing)
                    return;
                try
                {
                    m_isProcessing = true;
                    lock (m_lockObject)
                    {
                        var now = DateTime.Now;
                        for (var index = 0; index < TimeoutEntries.Count; ++index)
                        {
                            var timeoutEntry = TimeoutEntries[index];
                            if (!(now < timeoutEntry.Timeout))
                            {
                                TimeoutEntries.RemoveAt(index);
                                timeoutEntry.TimeoutSink.RaiseTimeout();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("Unhandled exception raised in TimeoutHelper.", ex);
                }
                finally
                {
                    m_isProcessing = false;
                }
            }, null, 25, 25);
        }

        public static void AddSession(TimeoutEntry session)
        {
            lock (typeof(TimeoutHelper))
            {
                TimeoutEntries.Add(session);
            }
        }

        public static void RemoveSession(ITimeoutSink timeOutSink)
        {
            lock (typeof(TimeoutHelper))
            {
                var timeoutEntry = TimeoutEntries.Find(each => each.TimeoutSink == timeOutSink);
                if (timeoutEntry == null)
                    return;
                TimeoutEntries.Remove(timeoutEntry);
            }
        }
    }
}