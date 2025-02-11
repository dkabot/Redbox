using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;

namespace Redbox.Core
{
    public static class LinqExtensions
    {
        public static T ToResult<T>(this ISingleResult<T> result)
        {
            var objList = new List<T>(result);
            return objList.Count <= 0 ? default : objList[0];
        }

        public static T ToResult<T>(this IQueryable<T> result)
        {
            var objList = new List<T>(result);
            return objList.Count <= 0 ? default : objList[0];
        }

        public static T ToResult<T>(this IEnumerable<T> result)
        {
            var objList = new List<T>(result);
            return objList.Count <= 0 ? default : objList[0];
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var obj in items)
                action(obj);
        }
    }
}