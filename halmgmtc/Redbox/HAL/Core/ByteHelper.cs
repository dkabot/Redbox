using System;
using System.IO;
using System.Security.Cryptography;

namespace Redbox.HAL.Core
{
    public static class ByteHelper
    {
        private static readonly byte[] m_keyValue = new Guid("{776DA6AF-3033-43ee-B379-2D4F28B5F1FC}").ToByteArray();
        private static readonly byte[] m_initialVector = new Guid("{F375D7E0-4572-4518-9C2F-E8F022F42AA7}").ToByteArray();

        public static string ToBase64(byte[] inputArray) => Convert.ToBase64String(inputArray);

        internal static byte[] Encrypt(byte[] inputArray)
        {
            ICryptoTransform encryptor = new TripleDESCryptoServiceProvider().CreateEncryptor(ByteHelper.m_keyValue, ByteHelper.m_initialVector);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(inputArray, 0, inputArray.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        internal static byte[] Decrypt(byte[] inputArray)
        {
            ICryptoTransform decryptor = new TripleDESCryptoServiceProvider().CreateDecryptor(ByteHelper.m_keyValue, ByteHelper.m_initialVector);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Write);
                cryptoStream.Write(inputArray, 0, inputArray.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }
    }
}