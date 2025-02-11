using System;

namespace Redbox.Core
{
    public static class SleepFor
    {
        public static ISleepEvaluator RandomExponential(int minimumDelay)
        {
            return new RandomExponentialSleepEvaluator(minimumDelay);
        }

        public static ISleepEvaluator Milliseconds(int milliseconds)
        {
            return new CustomSleepEvaluator((retryCount, exception) => milliseconds);
        }

        public static ISleepEvaluator Custom(Func<int, Exception, int> function)
        {
            return new CustomSleepEvaluator(function);
        }
    }
}