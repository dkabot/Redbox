using System;

namespace Redbox.Core
{
    internal class CustomSleepEvaluator : ISleepEvaluator
    {
        private readonly Func<int, Exception, int> m_function;

        public CustomSleepEvaluator(Func<int, Exception, int> function)
        {
            this.m_function = function != null ? function : throw new ArgumentNullException(nameof(function));
        }

        public int SleepMilliseconds(int retryCount, Exception exception)
        {
            return this.m_function(retryCount, exception);
        }
    }
}
