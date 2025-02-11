using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;

namespace Redbox.Core
{
    internal static class LinqExtensions
    {
        public static T ToResult<T>(this ISingleResult<T> result)
        {
            List<T> objList = new List<T>((IEnumerable<T>)result);
            return objList.Count <= 0 ? default(T) : objList[0];
        }

        public static T ToResult<T>(this IQueryable<T> result)
        {
            List<T> objList = new List<T>((IEnumerable<T>)result);
            return objList.Count <= 0 ? default(T) : objList[0];
        }

        public static T ToResult<T>(this IEnumerable<T> result)
        {
            List<T> objList = new List<T>(result);
            return objList.Count <= 0 ? default(T) : objList[0];
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T obj in items)
                action(obj);
        }
    }
}
