using System;

namespace Redbox.Core
{
    internal class CustomRethrowEvaluator : IRethrowEvaluator
    {
        private readonly Func<int, Exception, bool> m_function;

        public CustomRethrowEvaluator(Func<int, Exception, bool> function)
        {
            this.m_function = function != null ? function : throw new ArgumentNullException(nameof(function));
        }

        public bool Rethrow(int retryCount, Exception exception)
        {
            return this.m_function(retryCount, exception);
        }
    }
}
