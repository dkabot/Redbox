using System;

namespace Redbox.Core
{
    public static class TimeSpanExtensions
    {
        public static void SpinSleep(this TimeSpan timeSpan)
        {
            using (var executionTimer = new ExecutionTimer())
            {
                do
                {
                    ;
                } while (executionTimer.ElapsedMilliseconds < timeSpan.TotalMilliseconds);
            }
        }
    }
}