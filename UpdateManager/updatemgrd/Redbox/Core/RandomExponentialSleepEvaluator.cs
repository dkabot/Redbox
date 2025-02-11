using System;

namespace Redbox.Core
{
    internal class RandomExponentialSleepEvaluator : CustomSleepEvaluator
    {
        public RandomExponentialSleepEvaluator(int minimumDelay) : base((Func<int, Exception, int>)((retryCount, exception) => RandomExponentialSleepEvaluator.SleepMilliseconds(retryCount, minimumDelay)))
        {
        }

        public RandomExponentialSleepEvaluator()
      : this(0)
        {
        }

        private static int SleepMilliseconds(int retryCount, int minimumDelay)
        {
            return new Random(Guid.NewGuid().GetHashCode()).Next((int)Math.Pow((double)retryCount, 2.0) + minimumDelay, (int)Math.Pow((double)(retryCount + 1), 2.0) + 1 + minimumDelay);
        }
    }
}
