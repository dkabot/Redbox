using System;
using System.Reflection;

namespace Redbox.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class CardCodeAttribute : Attribute
    {
        public static CardCodeAttribute GetCode(CardType cardType)
        {
            FieldInfo field = typeof(CardType).GetField(cardType.ToString());
            return field == (FieldInfo)null ? (CardCodeAttribute)null : Attribute.GetCustomAttribute((MemberInfo)field, typeof(CardCodeAttribute)) as CardCodeAttribute;
        }

        public string Code { get; set; }
    }
}
