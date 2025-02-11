using System;

namespace Redbox.Core
{
    public class RandomExponentialSleepEvaluator : CustomSleepEvaluator
    {
        private readonly int _minimumDelay;

        public RandomExponentialSleepEvaluator(int minimumDelay)
            : base((retryCount, exception) => SleepMilliseconds(retryCount, minimumDelay))
        {
            _minimumDelay = minimumDelay;
        }

        public RandomExponentialSleepEvaluator()
            : this(0)
        {
        }

        private static int SleepMilliseconds(int retryCount, int minimumDelay)
        {
            return new Random(Guid.NewGuid().GetHashCode()).Next(
                (int)Math.Pow(retryCount, 2.0) + minimumDelay,
                (int)Math.Pow(retryCount + 1, 2.0) + 1 + minimumDelay);
        }
    }
}