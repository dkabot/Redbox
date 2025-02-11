using System;
using System.Collections.Specialized;

namespace Redbox.Macros
{
    public static class StringUtils
    {
        public static bool EndsWith(string value, char c)
        {
            var num = value != null ? value.Length : throw new ArgumentNullException(nameof(value));
            return num != 0 && value[num - 1] == c;
        }

        public static string ConvertEmptyToNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        public static string ConvertNullToEmpty(string value)
        {
            return value ?? string.Empty;
        }

        public static string Join(string separator, StringCollection value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (separator == null)
                separator = string.Empty;
            var array = new string[value.Count];
            value.CopyTo(array, 0);
            return string.Join(separator, array);
        }

        public static StringCollection Clone(StringCollection stringCollection)
        {
            var array = new string[stringCollection.Count];
            stringCollection.CopyTo(array, 0);
            var stringCollection1 = new StringCollection();
            stringCollection1.AddRange(array);
            return stringCollection1;
        }
    }
}