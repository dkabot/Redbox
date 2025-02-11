using System.Text;

namespace Redbox.Core
{
    internal static class LuhnHelper
    {
        public static bool IsCreditCardValid(string cardNumber)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int startIndex = 0; startIndex < cardNumber.Length; ++startIndex)
            {
                if ("0123456789".IndexOf(cardNumber.Substring(startIndex, 1)) >= 0)
                    stringBuilder.Append(cardNumber.Substring(startIndex, 1));
            }
            if (stringBuilder.Length < 13 || stringBuilder.Length > 16)
                return false;
            for (int index = stringBuilder.Length + 1; index <= 16; ++index)
                stringBuilder.Insert(0, "0");
            int num1 = 0;
            string str = stringBuilder.ToString();
            for (int index = 1; index <= 16; ++index)
            {
                int num2 = 1 + index % 2;
                int num3 = int.Parse(str.Substring(index - 1, 1)) * num2;
                if (num3 > 9)
                    num3 -= 9;
                num1 += num3;
            }
            return num1 % 10 == 0;
        }
    }
}
