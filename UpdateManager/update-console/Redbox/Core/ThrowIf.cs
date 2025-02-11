using System;

namespace Redbox.Core
{
    internal static class ThrowIf
    {
        public static IRethrowEvaluator Custom(Func<int, Exception, bool> function)
        {
            return (IRethrowEvaluator)new CustomRethrowEvaluator(function);
        }

        public static IRethrowEvaluator RetryCountIs(int retryCount)
        {
            return (IRethrowEvaluator)new CustomRethrowEvaluator((Func<int, Exception, bool>)((i, e) => i == retryCount));
        }

        public static IRethrowEvaluator Always
        {
            get
            {
                return (IRethrowEvaluator)new CustomRethrowEvaluator((Func<int, Exception, bool>)((i, e) => true));
            }
        }
    }
}
