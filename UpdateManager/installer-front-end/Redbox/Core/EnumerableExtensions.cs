using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core
{
    internal static class EnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> list, string separator)
        {
            List<string> stringList = new List<string>();
            foreach (T obj in list)
                stringList.Add((object)obj == null ? "(null)" : obj.ToString());
            return string.Join(separator, stringList.ToArray());
        }

        public static string GetRangedList<T>(this IList<T> listItems, int startIndex, int count)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = Math.Min(listItems.Count, startIndex + count);
            for (int index = startIndex; index < num; ++index)
            {
                if (index < listItems.Count)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(",");
                    stringBuilder.Append((object)listItems[index]);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
