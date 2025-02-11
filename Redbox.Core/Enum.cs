using System;
using System.Collections.Generic;

namespace Redbox.Core
{
    public static class Enum<T>
    {
        public static T Parse(string value, T defaultValue)
        {
            var obj = defaultValue;
            try
            {
                obj = (T)Enum.Parse(typeof(T), value);
            }
            catch (ArgumentException ex)
            {
            }

            return obj;
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ParseIgnoringCase(string value, T defaultValue)
        {
            var ignoringCase = defaultValue;
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
            var values = new List<T>();
            foreach (var obj in Enum.GetValues(typeof(T)))
                values.Add((T)obj);
            return values;
        }

        public static T ToObject(object o)
        {
            return (T)Enum.ToObject(typeof(T), o);
        }
    }
}