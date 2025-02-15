using System;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    public class ThreadExecutionTimer
    {
        private long? m_endTimeStamp;
        private long m_startTimeStamp;

        public TimeSpan Elapsed =>
            TimeSpan.FromMilliseconds(((m_endTimeStamp ?? GetThreadTimes()) - m_startTimeStamp) / 10000L);

        public bool IsRunning { get; private set; }

        public void Start()
        {
            IsRunning = true;
            m_startTimeStamp = GetThreadTimes();
        }

        public void Stop()
        {
            IsRunning = false;
            m_endTimeStamp = GetThreadTimes();
        }

        public void Reset()
        {
            m_startTimeStamp = 0L;
            m_endTimeStamp = new long?();
        }

        [DllImport("kernel32.dll")]
        private static extern long GetThreadTimes(
            IntPtr threadHandle,
            out long createionTime,
            out long exitTime,
            out long kernelTime,
            out long userTime);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        private static long GetThreadTimes()
        {
            long num;
            long kernelTime;
            long userTime;
            var threadTimes = GetThreadTimes(GetCurrentThread(), out num, out num, out kernelTime, out userTime);
            if (!Convert.ToBoolean(threadTimes))
                throw new Exception(string.Format("failed to get timestamp. error code: {0}", threadTimes));
            return kernelTime + userTime;
        }
    }
}