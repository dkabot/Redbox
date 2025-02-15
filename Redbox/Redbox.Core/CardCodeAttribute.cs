using System;

namespace Redbox.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CardCodeAttribute : Attribute
    {
        public string Code { get; set; }

        public static CardCodeAttribute GetCode(CardType cardType)
        {
            var field = typeof(CardType).GetField(cardType.ToString());
            return field == null ? null : GetCustomAttribute(field, typeof(CardCodeAttribute)) as CardCodeAttribute;
        }
    }
}