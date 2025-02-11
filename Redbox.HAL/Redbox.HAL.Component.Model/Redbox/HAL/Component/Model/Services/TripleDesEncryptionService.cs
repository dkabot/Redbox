using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.HAL.Component.Model.Services
{
    public sealed class TripleDesEncryptionService : IEncryptionService
    {
        private readonly byte[] m_initialVector = new Guid("{F375D7E0-4572-4518-9C2F-E8F022F42AA7}").ToByteArray();
        private readonly byte[] m_keyValue = new Guid("{776DA6AF-3033-43ee-B379-2D4F28B5F1FC}").ToByteArray();

        public byte[] Encrypt(byte[] inputArray)
        {
            using (var cryptoServiceProvider = new TripleDESCryptoServiceProvider())
            {
                return DoTransform(cryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector), inputArray);
            }
        }

        public byte[] Decrypt(byte[] inputArray)
        {
            using (var cryptoServiceProvider = new TripleDESCryptoServiceProvider())
            {
                return DoTransform(cryptoServiceProvider.CreateDecryptor(m_keyValue, m_initialVector), inputArray);
            }
        }

        public string EncryptToBase64(string source)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(source)));
        }

        public string DecryptFromBase64(string source)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(source)));
        }

        public string HashFile(string fullFilePath)
        {
            using (var algorithm = new SHA1CryptoServiceProvider())
            {
                return HashFile(algorithm, fullFilePath);
            }
        }

        public string HashFile(HashAlgorithm algorithm, string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
                throw new ArgumentException("fullFilePath doesn't exist.");
            var numArray = (byte[])null;
            using (var inputStream = new FileStream(fullFilePath, FileMode.Open))
            {
                numArray = algorithm.ComputeHash(inputStream);
            }

            var stringBuilder = new StringBuilder();
            for (var index = 0; index < numArray.Length; ++index)
                stringBuilder.Append(numArray[index].ToString("x2"));
            return stringBuilder.ToString();
        }

        private byte[] DoTransform(ICryptoTransform transform, byte[] inputArray)
        {
            using (var memoryStream = new MemoryStream())
            {
                var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(inputArray, 0, inputArray.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }
    }
}