using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Redbox.Core
{
    internal static class CardTypeHelper
    {
        private static bool m_isInitialized;
        private static RedboxGiftCard m_rbGiftCard = new RedboxGiftCard();

        public static void Initialize(List<BinRange> rbGiftCardBinRanges)
        {
            if (CardTypeHelper.m_isInitialized)
                return;
            CardTypeHelper.m_rbGiftCard.AddBinRanges(rbGiftCardBinRanges);
            CardTypeHelper.m_isInitialized = true;
        }

        public static CardType? GetCardType(string cardNumber)
        {
            return CardTypeHelper.GetCardType(cardNumber, true);
        }

        public static CardType? GetCardType(string cardNumber, bool checkGCRanges)
        {
            if (checkGCRanges && CardTypeHelper.m_rbGiftCard.IsRedboxGiftCard(cardNumber))
                return new CardType?(CardType.RedboxGiftCard);
            if (CardTypeHelper.NumberMatches(cardNumber, "^(4)", 16))
                return new CardType?(CardType.Visa);
            if (CardTypeHelper.NumberMatches(cardNumber, "^(34|37)", 15))
                return new CardType?(CardType.AmericanExpress);
            if (CardTypeHelper.NumberMatches(cardNumber, "^(51|52|53|54|55)", 16))
                return new CardType?(CardType.MasterCard);
            return CardTypeHelper.NumberMatches(cardNumber, "^(30|38|39|6011|62|64|65)", 16) || CardTypeHelper.NumberMatches(cardNumber, "^(36)", 14) ? new CardType?(CardType.Discover) : new CardType?();
        }

        private static bool NumberMatches(string cardNumber, string regex, int length)
        {
            return Regex.IsMatch(cardNumber, regex) && cardNumber.Length == length;
        }
    }
}
