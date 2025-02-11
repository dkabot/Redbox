using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core
{
    internal static class TimeoutHelper
    {
        private const int TimerInterval = 25;
        private static readonly Timer m_timer;
        private static volatile bool m_isProcessing;
        private static readonly object m_lockObject = new object();
        private static readonly List<TimeoutEntry> TimeoutEntries = new List<TimeoutEntry>();

        public static void AddSession(TimeoutEntry session)
        {
            lock (typeof(TimeoutHelper))
                TimeoutHelper.TimeoutEntries.Add(session);
        }

        public static void RemoveSession(ITimeoutSink timeOutSink)
        {
            lock (typeof(TimeoutHelper))
            {
                TimeoutEntry timeoutEntry = TimeoutHelper.TimeoutEntries.Find((Predicate<TimeoutEntry>)(each => each.TimeoutSink == timeOutSink));
                if (timeoutEntry == null)
                    return;
                TimeoutHelper.TimeoutEntries.Remove(timeoutEntry);
            }
        }

        static TimeoutHelper()
        {
            TimeoutHelper.m_timer = new Timer((TimerCallback)(o =>
            {
                if (TimeoutHelper.m_isProcessing)
                    return;
                try
                {
                    TimeoutHelper.m_isProcessing = true;
                    lock (TimeoutHelper.m_lockObject)
                    {
                        DateTime now = DateTime.Now;
                        for (int index = 0; index < TimeoutHelper.TimeoutEntries.Count; ++index)
                        {
                            TimeoutEntry timeoutEntry = TimeoutHelper.TimeoutEntries[index];
                            if (!(now < timeoutEntry.Timeout))
                            {
                                TimeoutHelper.TimeoutEntries.RemoveAt(index);
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
                    TimeoutHelper.m_isProcessing = false;
                }
            }), (object)null, 25, 25);
        }
    }
}
