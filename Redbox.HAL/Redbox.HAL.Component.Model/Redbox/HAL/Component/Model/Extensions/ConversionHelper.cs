using System;
using System.ComponentModel;

namespace Redbox.HAL.Component.Model.Extensions
{
    public static class ConversionHelper
    {
        public static Type GetNullableTypeIfWrapped(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? type.GetGenericArguments()[0]
                : type;
        }

        public static T ChangeType<T>(object value)
        {
            return (T)ChangeType(value, typeof(T));
        }

        public static object ChangeType(object value, Type convertToType)
        {
            if (value == null)
                return null;
            if (convertToType.IsEnum && value is string)
                return Enum.Parse(convertToType, (string)value, true);
            var converter1 = TypeDescriptor.GetConverter(convertToType);
            if (converter1 != null && converter1.CanConvertFrom(typeof(string)) && value is string)
                return converter1.ConvertFrom(value);
            if (convertToType == typeof(string))
            {
                var converter2 = TypeDescriptor.GetConverter(value.GetType());
                if (converter2 != null && converter2.CanConvertTo(typeof(string)))
                    return converter2.ConvertToString(value);
            }

            if (!convertToType.IsInstanceOfType(value))
                try
                {
                    return Convert.ChangeType(value, GetNullableTypeIfWrapped(convertToType));
                }
                catch (InvalidCastException ex)
                {
                }

            return value;
        }
    }
}