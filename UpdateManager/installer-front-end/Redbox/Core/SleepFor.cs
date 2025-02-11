using System;

namespace Redbox.Core
{
    internal static class SleepFor
    {
        public static ISleepEvaluator RandomExponential(int minimumDelay)
        {
            return (ISleepEvaluator)new RandomExponentialSleepEvaluator(minimumDelay);
        }

        public static ISleepEvaluator Milliseconds(int milliseconds)
        {
            return (ISleepEvaluator)new CustomSleepEvaluator((Func<int, Exception, int>)((retryCount, exception) => milliseconds));
        }

        public static ISleepEvaluator Custom(Func<int, Exception, int> function)
        {
            return (ISleepEvaluator)new CustomSleepEvaluator(function);
        }
    }
}
