using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Redbox.Compression;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public static class ProtocolHelper
    {
        public const string NotApplicable = "N/A";

        public static byte? ParseByte(string data)
        {
            if (data == "N/A")
                return new byte?();
            byte result;
            return byte.TryParse(data, out result) ? result : new byte?();
        }

        public static int? ParseInteger(string data)
        {
            if (data == "N/A")
                return new int?();
            int result;
            return int.TryParse(data, out result) ? result : new int?();
        }

        public static long? ParseLong(string data)
        {
            if (data == "N/A")
                return new long?();
            long result;
            return long.TryParse(data, out result) ? result : new long?();
        }

        public static DateTime? ParseDate(string data)
        {
            if (data == "N/A")
                return new DateTime?();
            DateTime result;
            return DateTime.TryParse(data, out result)
                ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
                : new DateTime?();
        }

        public static TimeSpan? ParseTimeSpan(string data)
        {
            if (data == "N/A")
                return new TimeSpan?();
            TimeSpan result;
            return TimeSpan.TryParse(data, out result) ? result : new TimeSpan?();
        }

        public static ReadOnlyCollection<string> ParseProperties(string data)
        {
            var stringList = new List<string>();
            var stringBuilder = new StringBuilder();
            var num = 0;
            var charStack = new Stack<char>();
            foreach (var ch in data)
                if (ch == '|' && num == 0)
                {
                    stringList.Add(stringBuilder.ToString());
                    stringBuilder.Length = 0;
                }
                else
                {
                    if (ch == '<')
                    {
                        charStack.Push('>');
                        ++num;
                        if (num == 1)
                            continue;
                    }

                    if (ch == '[' && (charStack.Count == 0 || charStack.Peek() != '>'))
                    {
                        charStack.Push(']');
                        ++num;
                        if (num == 1)
                            continue;
                    }

                    if (charStack.Count > 0 && ch == charStack.Peek())
                    {
                        --num;
                        if (num == 0)
                            continue;
                    }

                    stringBuilder.Append(ch);
                }

            stringList.Add(stringBuilder.ToString());
            return stringList.AsReadOnly();
        }

        public static void FormatErrors(ErrorList errors, IList<string> messages)
        {
            foreach (var error in errors)
                messages.Add(string.Format("|*{0}|{1}*|", error, error.Details));
        }

        public static void FormatErrors(ErrorList errors, CommandContext context)
        {
            FormatErrors(errors, context.Messages);
        }

        public static void FormatEventMessages(IEnumerable<string> messages, CommandContext context)
        {
            foreach (var message in messages)
                context.Messages.Add(message);
        }

        public static void FormatFile(string path, CommandContext context)
        {
            context.Messages.Add(File.ReadAllBytes(path).ToBase64());
        }

        public static void FormatCompressedFile(string path, CommandContext context)
        {
            FormatGzipCompressedMessage(
                CompressionAlgorithm.GetAlgorithm(CompressionType.GZip)
                    .Compress(Encoding.ASCII.GetBytes(File.ReadAllBytes(path).ToBase64())), context);
        }

        public static void FormatCompressedString(string value, CommandContext context)
        {
            FormatLzmaCompressedMessage(
                CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(value)),
                context);
        }

        public static void FormatCompressedBytes(byte[] buffer, CommandContext context)
        {
            FormatLzmaCompressedMessage(CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Compress(buffer),
                context);
        }

        public static byte[] DecompressBase64String(string value)
        {
            return CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Decompress(value.Base64ToBytes());
        }

        private static void FormatLzmaCompressedMessage(byte[] buffer, CommandContext context)
        {
            context.Messages.Add(string.Format("LZMA|{0}", buffer.ToBase64()));
        }

        private static void FormatGzipCompressedMessage(byte[] buffer, CommandContext context)
        {
            context.Messages.Add(string.Format("GZIP|{0}", buffer.ToBase64()));
        }
    }
}