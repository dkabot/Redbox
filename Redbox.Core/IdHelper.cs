using System.Security.Cryptography;
using System.Text;

namespace Redbox.Core
{
    public class IdHelper
    {
        private const string HumanReadableChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        private static readonly RNGCryptoServiceProvider m_provider = new RNGCryptoServiceProvider();

        public static string GeneratePseudoUniqueId()
        {
            return GeneratePseudoUniqueId(8, "23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
        }

        public static string GeneratePseudoUniqueId(int codeLength)
        {
            return GeneratePseudoUniqueId(codeLength, "23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
        }

        public static string GeneratePseudoUniqueId(string validChars)
        {
            return GeneratePseudoUniqueId(8, validChars);
        }

        public static string GeneratePseudoUniqueId(int codeLength, string validChars)
        {
            var stringBuilder = new StringBuilder();
            var data = new byte[codeLength];
            m_provider.GetBytes(data);
            for (var index = 0; index < codeLength; ++index)
            {
                var startIndex = data[index] % validChars.Length;
                stringBuilder.Append(validChars.Substring(startIndex, 1));
            }

            return stringBuilder.ToString();
        }
    }
}