using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Redbox.JSONPrettyPrinter;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class StringExtensionMethods
    {
        public const string NotAvailable = "N/A";

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var length = text.IndexOf(search);
            return length < 0 ? text : text.Substring(0, length) + replace + text.Substring(length + search.Length);
        }

        public static string ToPrettyJson(this string source)
        {
            return source == null ? string.Empty : PrettyPrinter.GetPrettyString(source);
        }

        public static string CompressString(this string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }

            memoryStream.Position = 0L;
            var numArray1 = new byte[memoryStream.Length];
            memoryStream.Read(numArray1, 0, numArray1.Length);
            var numArray2 = new byte[numArray1.Length + 4];
            Buffer.BlockCopy(numArray1, 0, numArray2, 4, numArray1.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, numArray2, 0, 4);
            return Convert.ToBase64String(numArray2);
        }

        public static string DecompressString(this string compressedText)
        {
            var buffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                var int32 = BitConverter.ToInt32(buffer, 0);
                memoryStream.Write(buffer, 4, buffer.Length - 4);
                var numArray = new byte[int32];
                memoryStream.Position = 0L;
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gzipStream.Read(numArray, 0, numArray.Length);
                }

                return Encoding.UTF8.GetString(numArray);
            }
        }

        public static string CheckNotAvailable(this string source)
        {
            return !string.IsNullOrEmpty(source) ? source : "N/A";
        }

        public static string StoreNumberString(this IMachineSettingsStore source)
        {
            return source.GetValue("Store", "ID", "N/A");
        }
    }
}