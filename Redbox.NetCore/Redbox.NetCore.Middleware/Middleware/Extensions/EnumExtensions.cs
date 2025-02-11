using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseEnumByNameAndAttributes<T>(this string source) where T : struct, Enum
        {
            T t;
            if (Enum.TryParse(source, true, out t)) return t;
            var typeFromHandle = typeof(T);
            foreach (var text in Enum.GetNames(typeFromHandle))
            {
                var member = typeFromHandle.GetMember(text);
                if (member.Length != 0)
                {
                    var customAttributes = member[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                    if (customAttributes.Length != 0 &&
                        ((DisplayAttribute)customAttributes[0]).Name.Equals(source.Trim(),
                            StringComparison.OrdinalIgnoreCase)) return (T)Enum.Parse(typeof(T), text);
                    var customAttributes2 = member[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
                    if (customAttributes2.Length != 0 &&
                        ((EnumMemberAttribute)customAttributes2[0]).Value.Equals(source.Trim(),
                            StringComparison.OrdinalIgnoreCase)) return (T)Enum.Parse(typeof(T), text);
                }
            }

            return default;
        }

        public static string GetDescriptionFromEnumValue(this Enum value)
        {
            var enumMemberAttribute = value.GetType().GetField(value.ToString())
                .GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .SingleOrDefault() as EnumMemberAttribute;
            if (enumMemberAttribute != null) return enumMemberAttribute.Value;
            return value.ToString();
        }
    }
}