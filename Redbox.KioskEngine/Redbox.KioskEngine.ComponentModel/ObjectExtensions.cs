using System.Collections.Generic;
using System.ComponentModel;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class ObjectExtensions
  {
    public static IDictionary<string, object> ToDictionary(this object source)
    {
      return source.ToDictionary<object>();
    }

    public static IDictionary<string, T> ToDictionary<T>(this object source)
    {
      if (source == null)
        return (IDictionary<string, T>) null;
      Dictionary<string, T> dictionary = new Dictionary<string, T>();
      foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
        ObjectExtensions.AddPropertyToDictionary<T>(property, source, dictionary);
      return (IDictionary<string, T>) dictionary;
    }

    private static void AddPropertyToDictionary<T>(
      PropertyDescriptor property,
      object source,
      Dictionary<string, T> dictionary)
    {
      object obj = property.GetValue(source);
      if (!ObjectExtensions.IsOfType<T>(obj))
        return;
      dictionary.Add(property.Name, (T) obj);
    }

    private static bool IsOfType<T>(object value) => value is T;
  }
}
