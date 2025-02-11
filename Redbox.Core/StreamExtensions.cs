using System;
using System.IO;
using System.Security.Cryptography;

namespace Redbox.Core
{
    public static class StreamExtensions
    {
        public static string ToASCIISHA1Hash(this Stream inputStream)
        {
            var asciishA1Hash = BitConverter.ToString(SHA1.Create().ComputeHash(inputStream))
                .Replace("-", string.Empty);
            if (!inputStream.CanSeek)
                return asciishA1Hash;
            inputStream.Seek(0L, SeekOrigin.Begin);
            return asciishA1Hash;
        }

        public static byte[] GetBytes(this Stream stream)
        {
            if (stream == null)
                return null;
            if (stream.CanSeek)
                stream.Seek(0L, SeekOrigin.Begin);
            var buffer = new byte[stream.Length];
            var offset = 0;
            int num;
            while ((num = stream.Read(buffer, offset, buffer.Length - offset)) > 0)
                offset += num;
            return buffer;
        }
    }
}