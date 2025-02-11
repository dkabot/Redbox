using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Redbox.NetCore.Middleware.Validation
{
    public class NotEmptyEnumerableAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "The {0} field must not be null and have at least one element";

        public NotEmptyEnumerableAttribute()
            : base("The {0} field must not be null and have at least one element")
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null || !(value is IEnumerable enumerable))
                return false;
            var enumerator = enumerable.GetEnumerator();
            try
            {
                if (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    return true;
                }
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                    disposable.Dispose();
            }

            return false;
        }
    }
}