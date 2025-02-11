using System.Security.Cryptography;
using System.Text;

namespace Redbox.Core
{
    internal class IdHelper
    {
        private const string HumanReadableChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        private static readonly RNGCryptoServiceProvider m_provider = new RNGCryptoServiceProvider();

        public static string GeneratePseudoUniqueId()
        {
            return IdHelper.GeneratePseudoUniqueId(8, "23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
        }

        public static string GeneratePseudoUniqueId(int codeLength)
        {
            return IdHelper.GeneratePseudoUniqueId(codeLength, "23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
        }

        public static string GeneratePseudoUniqueId(string validChars)
        {
            return IdHelper.GeneratePseudoUniqueId(8, validChars);
        }

        public static string GeneratePseudoUniqueId(int codeLength, string validChars)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte[] data = new byte[codeLength];
            IdHelper.m_provider.GetBytes(data);
            for (int index = 0; index < codeLength; ++index)
            {
                int startIndex = (int)data[index] % validChars.Length;
                stringBuilder.Append(validChars.Substring(startIndex, 1));
            }
            return stringBuilder.ToString();
        }
    }
}
