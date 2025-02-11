using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class EncryptionHelper
    {
        private static readonly byte[] m_keyValue = new Guid("{02CAD7C4-E0F5-4b1b-BD04-808B840A61D2}").ToByteArray();

        private static readonly byte[] m_initialVector =
            new Guid("{AD186FCD-2ACD-41a8-958D-6CA7DAEB1DE5}").ToByteArray();

        public static string Decrypt(string data)
        {
            var numArray = Convert.FromBase64String(data);
            using (var cryptoServiceProvider = new TripleDESCryptoServiceProvider())
            {
                using (var decryptor = cryptoServiceProvider.CreateDecryptor(m_keyValue, m_initialVector))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(numArray, 0, numArray.Length);
                            cryptoStream.FlushFinalBlock();
                            return Encoding.Default.GetString(memoryStream.ToArray());
                        }
                    }
                }
            }
        }

        public static string Encrypt(string data)
        {
            var bytes = Encoding.Default.GetBytes(data);
            using (var cryptoServiceProvider = new TripleDESCryptoServiceProvider())
            {
                using (var encryptor = cryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(bytes, 0, bytes.Length);
                            cryptoStream.FlushFinalBlock();
                            return Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }
                }
            }
        }
    }
}