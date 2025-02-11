using System;
using System.ComponentModel;

namespace Redbox.Core
{
    internal static class ConversionHelper
    {
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetNullableTypeIfWrapped(Type type)
        {
            return !ConversionHelper.IsNullableType(type) ? type : type.GetGenericArguments()[0];
        }

        public static object ChangeType(object value, Type convertToType)
        {
            if (value == null)
                return (object)null;
            if (convertToType.IsEnum)
            {
                switch (value)
                {
                    case string _:
                        FlagsAttribute[] customAttributes = (FlagsAttribute[])convertToType.GetCustomAttributes(typeof(FlagsAttribute), false);
                        if (customAttributes == null || customAttributes.Length == 0)
                            return Enum.Parse(convertToType, (string)value, true);
                        int num = 0;
                        foreach (string str in ((string)value).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                        {
                            object obj = Enum.Parse(convertToType, str.Trim(), true);
                            num |= Convert.ToInt32(obj);
                        }
                        return Enum.ToObject(convertToType, num);
                    case int _:
                    case byte _:
                    case long _:
                    case short _:
                    case float _:
                    case double _:
                        return Enum.ToObject(convertToType, Convert.ToInt32(value));
                }
            }
            if (convertToType == typeof(Type) && value is string)
                return (object)Type.GetType((string)value);
            TypeConverter converter1 = TypeDescriptor.GetConverter(convertToType);
            if (converter1 != null && converter1.CanConvertFrom(typeof(string)) && value is string)
                return converter1.ConvertFrom(value);
            if (convertToType == typeof(string))
            {
                TypeConverter converter2 = TypeDescriptor.GetConverter(value.GetType());
                if (converter2 != null && converter2.CanConvertTo(typeof(string)))
                    return (object)converter2.ConvertToString(value);
            }
            if (!convertToType.IsInstanceOfType(value))
            {
                try
                {
                    return Convert.ChangeType(value, ConversionHelper.GetNullableTypeIfWrapped(convertToType));
                }
                catch (InvalidCastException ex)
                {
                }
            }
            return value;
        }
    }
}
