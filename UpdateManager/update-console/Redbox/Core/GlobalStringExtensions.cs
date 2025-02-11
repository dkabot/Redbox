namespace Redbox.Core
{
    internal static class GlobalStringExtensions
    {
        public static string ExtractCodeFromBrackets(this string value, string prefix, string postfix)
        {
            int num1 = 0;
            int startIndex1 = -1;
            int num2 = -1;
            int startIndex2 = 0;
            do
            {
                string str1 = string.Empty;
                if (startIndex2 + prefix.Length < value.Length)
                    str1 = value.Substring(startIndex2, prefix.Length);
                string str2 = string.Empty;
                if (startIndex2 + str2.Length < value.Length)
                    str2 = value.Substring(startIndex2, postfix.Length);
                if (str1 == prefix)
                {
                    if (startIndex1 == -1)
                        startIndex1 = startIndex2 + prefix.Length;
                    ++num1;
                }
                else if (str2 == postfix)
                {
                    --num1;
                    if (num1 == 0)
                    {
                        num2 = startIndex2 - postfix.Length;
                        break;
                    }
                }
                ++startIndex2;
            }
            while (startIndex2 < value.Length);
            return startIndex1 == -1 || num2 == -1 ? (string)null : value.Substring(startIndex1, num2 - startIndex1 + postfix.Length);
        }
    }
}
