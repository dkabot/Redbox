using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class DictionaryExtensions
  {
    public static Dictionary<TKey, TValue> ShallowCopy<TKey, TValue>(
      this IDictionary<TKey, TValue> source)
    {
      return source.ToDictionary<KeyValuePair<TKey, TValue>, TKey, TValue>((Func<KeyValuePair<TKey, TValue>, TKey>) (kvp => kvp.Key), (Func<KeyValuePair<TKey, TValue>, TValue>) (kvp => kvp.Value));
    }
  }
}
