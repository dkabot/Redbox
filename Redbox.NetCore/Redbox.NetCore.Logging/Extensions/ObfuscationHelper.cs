namespace Redbox.NetCore.Logging.Extensions
{
    public static class ObfuscationHelper
    {
        public static string ObfuscateMessageData(
            string msgText,
            char newChar,
            int leading,
            int trailing)
        {
            if (msgText == null)
                return null;
            if (leading + trailing > msgText.Length)
            {
                leading = 0;
                trailing = 0;
            }

            var str1 = leading > 0 ? msgText.Substring(0, leading) : string.Empty;
            var str2 = trailing > 0 ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty;
            var str3 = new string(newChar, msgText.Length - (leading + trailing));
            var str4 = str2;
            return str1 + str3 + str4;
        }

        public static string ObfuscateStringData(string value, int leading, int trailing)
        {
            return ObfuscateMessageData(value, 'x', leading, trailing);
        }

        public static string ObfuscateNumericData(string value, int leading, int trailing)
        {
            return ObfuscateMessageData(value, '#', leading, trailing);
        }

        public class Constants
        {
            public const char ReplacementCharacter = 'x';
            public const char ReplacementNumber = '#';
        }
    }
}