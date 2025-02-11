using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace UpdateClientService.API
{
    public static class StreamExtensions
    {
        public static async Task<string> GetSHA1HashAsync(this Stream stream)
        {
            string base64String;
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                var array = ms.ToArray();
                using (var shA1Managed = new SHA1Managed())
                {
                    base64String = Convert.ToBase64String(shA1Managed.ComputeHash(array));
                }
            }

            return base64String;
        }

        public static string GetSHA1Hash(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                var array = memoryStream.ToArray();
                using (var shA1Managed = new SHA1Managed())
                {
                    return Convert.ToBase64String(shA1Managed.ComputeHash(array));
                }
            }
        }

        public static string ReadToEnd(this Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static byte[] GetBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}