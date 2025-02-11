using System;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    internal class ThreadExecutionTimer
    {
        private long? m_endTimeStamp;
        private long m_startTimeStamp;

        public void Start()
        {
            this.IsRunning = true;
            this.m_startTimeStamp = ThreadExecutionTimer.GetThreadTimes();
        }

        public void Stop()
        {
            this.IsRunning = false;
            this.m_endTimeStamp = new long?(ThreadExecutionTimer.GetThreadTimes());
        }

        public void Reset()
        {
            this.m_startTimeStamp = 0L;
            this.m_endTimeStamp = new long?();
        }

        public TimeSpan Elapsed
        {
            get
            {
                return TimeSpan.FromMilliseconds((double)(((this.m_endTimeStamp ?? ThreadExecutionTimer.GetThreadTimes()) - this.m_startTimeStamp) / 10000L));
            }
        }

        public bool IsRunning { get; private set; }

        [DllImport("Kernel32.dll")]
        private static extern long GetThreadTimes(
          IntPtr threadHandle,
          out long createionTime,
          out long exitTime,
          out long kernelTime,
          out long userTime);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        private static long GetThreadTimes()
        {
            IntPtr currentThread = ThreadExecutionTimer.GetCurrentThread();
            long num1;
            long num2;
            long num3;
            long threadTimes = ThreadExecutionTimer.GetThreadTimes(currentThread, out num1, out num1, out num2, out num3);
            if (!Convert.ToBoolean(threadTimes))
                throw new Exception(string.Format("failed to get timestamp. error code: {0}", (object)threadTimes));
            return num2 + num3;
        }
    }
}
