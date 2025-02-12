using System;
using System.Threading;

namespace Redbox.HAL.Core
{
    public static class PerfFunctions
    {
        private static WaitHandle m_waitObject = (WaitHandle)new ManualResetEvent(false);

        public static void SpinWait(int milliseconds)
        {
            PerfFunctions.SpinWait(new TimeSpan(0, 0, 0, 0, milliseconds));
        }

        public static void SpinWait(TimeSpan timespan)
        {
            using (ExecutionTimer executionTimer = new ExecutionTimer())
            {
                do
                    ;
                while ((double)executionTimer.ElapsedMilliseconds < timespan.TotalMilliseconds);
            }
        }

        public static void Wait(int ms)
        {
            if (ms < 1000)
                PerfFunctions.SpinWait(ms);
            else
                PerfFunctions.m_waitObject.WaitOne(ms, false);
        }
    }
}