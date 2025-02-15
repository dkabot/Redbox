using System;
using System.ComponentModel;

namespace Redbox.Core
{
    public static class ConversionHelper
    {
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetNullableTypeIfWrapped(Type type)
        {
            return !IsNullableType(type) ? type : type.GetGenericArguments()[0];
        }

        public static object ChangeType(object value, Type convertToType)
        {
            if (value == null)
                return null;
            if (convertToType.IsEnum)
                switch (value)
                {
                    case string _:
                        var customAttributes =
                            (FlagsAttribute[])convertToType.GetCustomAttributes(typeof(FlagsAttribute), false);
                        if (customAttributes == null || customAttributes.Length == 0)
                            return Enum.Parse(convertToType, (string)value, true);
                        var num = 0;
                        foreach (var str in ((string)value).Split(",".ToCharArray(),
                                     StringSplitOptions.RemoveEmptyEntries))
                        {
                            var obj = Enum.Parse(convertToType, str.Trim(), true);
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

            if (convertToType == typeof(Type) && value is string)
                return Type.GetType((string)value);
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