using System;

namespace Redbox.JSONPrettyPrinter
{
    internal sealed class Mask
    {
        public static string NullString(string actual) => actual != null ? actual : string.Empty;

        public static string NullString(string actual, string mask) => actual != null ? actual : mask;

        public static string EmptyString(string actual, string emptyValue)
        {
            return Mask.NullString(actual).Length != 0 ? actual : emptyValue;
        }

        private Mask() => throw new NotSupportedException();
    }
}
