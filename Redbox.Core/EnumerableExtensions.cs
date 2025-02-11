using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core
{
    public static class EnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> list, string separator)
        {
            var stringList = new List<string>();
            foreach (var obj in list)
                stringList.Add(obj == null ? "(null)" : obj.ToString());
            return string.Join(separator, stringList.ToArray());
        }

        public static string GetRangedList<T>(this IList<T> listItems, int startIndex, int count)
        {
            var stringBuilder = new StringBuilder();
            var num = Math.Min(listItems.Count, startIndex + count);
            for (var index = startIndex; index < num; ++index)
                if (index < listItems.Count)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(",");
                    stringBuilder.Append(listItems[index]);
                }

            return stringBuilder.ToString();
        }
    }
}