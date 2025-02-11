using System;
using System.Collections.Generic;

namespace Redbox.Core
{
    internal static class Enum<T>
    {
        public static T Parse(string value, T defaultValue)
        {
            T obj = defaultValue;
            try
            {
                obj = (T)Enum.Parse(typeof(T), value);
            }
            catch (ArgumentException ex)
            {
            }
            return obj;
        }

        public static T Parse(string value) => (T)Enum.Parse(typeof(T), value, true);

        public static T ParseIgnoringCase(string value, T defaultValue)
        {
            T ignoringCase = defaultValue;
            try
            {
                ignoringCase = (T)Enum.Parse(typeof(T), value, true);
            }
            catch (ArgumentException ex)
            {
            }
            return ignoringCase;
        }

        public static IList<T> GetValues()
        {
            List<T> values = new List<T>();
            foreach (object obj in Enum.GetValues(typeof(T)))
                values.Add((T)obj);
            return (IList<T>)values;
        }

        public static T ToObject(object o) => (T)Enum.ToObject(typeof(T), o);
    }
}
