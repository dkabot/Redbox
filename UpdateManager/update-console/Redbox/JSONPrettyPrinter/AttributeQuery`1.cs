using System;
using System.Reflection;

namespace Redbox.JSONPrettyPrinter
{
    internal static class AttributeQuery<T> where T : Attribute
    {
        public static T Get<P>(P provider) where P : ICustomAttributeProvider
        {
            return AttributeQuery<T>.Get<P>(provider, false);
        }

        public static T Get<P>(P provider, bool inherit) where P : ICustomAttributeProvider
        {
            return AttributeQuery<T>.Find<P>(provider, inherit) ?? throw new ObjectNotFoundException(string.Format("The attribute {0} was not found.", (object)typeof(T).FullName));
        }

        public static T Find<P>(P provider) where P : ICustomAttributeProvider
        {
            return AttributeQuery<T>.Find<P>(provider, false);
        }

        public static T Find<P>(P provider, bool inherit) where P : ICustomAttributeProvider
        {
            T[] all = AttributeQuery<T>.FindAll<P>(provider, inherit);
            return all.Length == 0 ? default(T) : all[0];
        }

        public static T FindAll<P>(P provider) where P : ICustomAttributeProvider
        {
            return AttributeQuery<T>.Find<P>(provider, false);
        }

        public static T[] FindAll<P>(P provider, bool inherit) where P : ICustomAttributeProvider
        {
            return (T[])provider.GetCustomAttributes(typeof(T), inherit);
        }
    }
}
