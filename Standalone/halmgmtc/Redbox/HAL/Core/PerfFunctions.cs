using System;
using System.Threading;

namespace Redbox.HAL.Core
{
    public static class PerfFunctions
    {
        private static readonly WaitHandle m_waitObject = new ManualResetEvent(false);

        public static void SpinWait(int milliseconds)
        {
            SpinWait(new TimeSpan(0, 0, 0, 0, milliseconds));
        }

        public static void SpinWait(TimeSpan timespan)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                do
                {
                    ;
                } while (executionTimer.ElapsedMilliseconds < timespan.TotalMilliseconds);
            }
        }

        public static void Wait(int ms)
        {
            if (ms < 1000)
                SpinWait(ms);
            else
                m_waitObject.WaitOne(ms, false);
        }
    }
}