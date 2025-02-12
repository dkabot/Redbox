using System;

namespace Redbox.JSONPrettyPrinter
{
    internal sealed class Mask
    {
        private Mask()
        {
            throw new NotSupportedException();
        }

        public static string NullString(string actual)
        {
            return actual != null ? actual : string.Empty;
        }

        public static string NullString(string actual, string mask)
        {
            return actual != null ? actual : mask;
        }

        public static string EmptyString(string actual, string emptyValue)
        {
            return NullString(actual).Length != 0 ? actual : emptyValue;
        }
    }
}