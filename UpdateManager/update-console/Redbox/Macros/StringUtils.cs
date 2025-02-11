using System;
using System.Collections.Specialized;

namespace Redbox.Macros
{
    internal static class StringUtils
    {
        public static bool EndsWith(string value, char c)
        {
            int num = value != null ? value.Length : throw new ArgumentNullException(nameof(value));
            return num != 0 && (int)value[num - 1] == (int)c;
        }

        public static string ConvertEmptyToNull(string value)
        {
            return string.IsNullOrEmpty(value) ? (string)null : value;
        }

        public static string ConvertNullToEmpty(string value) => value ?? string.Empty;

        public static string Join(string separator, StringCollection value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (separator == null)
                separator = string.Empty;
            string[] array = new string[value.Count];
            value.CopyTo(array, 0);
            return string.Join(separator, array);
        }

        public static StringCollection Clone(StringCollection stringCollection)
        {
            string[] array = new string[stringCollection.Count];
            stringCollection.CopyTo(array, 0);
            StringCollection stringCollection1 = new StringCollection();
            stringCollection1.AddRange(array);
            return stringCollection1;
        }
    }
}
