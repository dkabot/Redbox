namespace Redbox.Core
{
    public static class GlobalStringExtensions
    {
        public static string ExtractCodeFromBrackets(this string value, string prefix, string postfix)
        {
            var num1 = 0;
            var startIndex1 = -1;
            var num2 = -1;
            var startIndex2 = 0;
            do
            {
                var str1 = string.Empty;
                if (startIndex2 + prefix.Length < value.Length)
                    str1 = value.Substring(startIndex2, prefix.Length);
                var str2 = string.Empty;
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
            } while (startIndex2 < value.Length);

            return startIndex1 == -1 || num2 == -1
                ? null
                : value.Substring(startIndex1, num2 - startIndex1 + postfix.Length);
        }
    }
}