using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> ShallowCopy<TKey, TValue>(
            this IDictionary<TKey, TValue> source)
        {
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}