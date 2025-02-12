using System;
using System.Reflection;

namespace Redbox.JSONPrettyPrinter
{
    internal static class AttributeQuery<T> where T : Attribute
    {
        public static T Get<P>(P provider) where P : ICustomAttributeProvider
        {
            return Get(provider, false);
        }

        public static T Get<P>(P provider, bool inherit) where P : ICustomAttributeProvider
        {
            return Find(provider, inherit) ??
                   throw new ObjectNotFoundException(string.Format("The attribute {0} was not found.",
                       typeof(T).FullName));
        }

        public static T Find<P>(P provider) where P : ICustomAttributeProvider
        {
            return Find(provider, false);
        }

        public static T Find<P>(P provider, bool inherit) where P : ICustomAttributeProvider
        {
            var all = FindAll(provider, inherit);
            return all.Length == 0 ? default : all[0];
        }

        public static T FindAll<P>(P provider) where P : ICustomAttributeProvider
        {
            return Find(provider, false);
        }

        public static T[] FindAll<P>(P provider, bool inherit) where P : ICustomAttributeProvider
        {
            var attributeType = typeof(T);
            return (T[])provider.GetCustomAttributes(attributeType, inherit);
        }
    }
}