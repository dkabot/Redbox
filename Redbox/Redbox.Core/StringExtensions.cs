using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.Core
{
    public static class StringExtensions
    {
        internal const BindingFlags GetObjectFromPathBindingFlags = BindingFlags.Instance | BindingFlags.Public |
                                                                    BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                                                                    BindingFlags.GetField | BindingFlags.GetProperty;

        internal static readonly char[] PropertyPathSeparators = new char[1]
        {
            '.'
        };

        public static T ToEnum<T>(this string value)
        {
            return Enum<T>.Parse(value, default);
        }

        public static byte[] ReadToBuffer(this string fileName)
        {
            return fileName != null ? File.ReadAllBytes(fileName) : null;
        }

        public static IDictionary<string, string> GetNameValuePairs(this string source)
        {
            var nameValuePairs = (IDictionary<string, string>)new Dictionary<string, string>();
            foreach (Match match in Regex.Matches(source,
                         "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))"))
            {
                var group1 = match.Groups["name"];
                var group2 = match.Groups["val"];
                if (group1.Captures.Count == group2.Captures.Count)
                    for (var i = 0; i < group1.Captures.Count; ++i)
                        nameValuePairs[group1.Captures[i].Value] = group2.Captures[i].Value;
            }

            return nameValuePairs;
        }

        public static string RemoveString(this string value, string token)
        {
            var startIndex = value.IndexOf(token);
            return startIndex <= -1 ? value : value.Remove(startIndex, token.Length);
        }

        public static string InnerTrim(this string value)
        {
            var stringBuilder = new StringBuilder();
            var flag = false;
            foreach (var c in value)
                if (!char.IsControl(c))
                {
                    if (flag)
                    {
                        stringBuilder.Append(' ');
                        flag = false;
                    }

                    stringBuilder.Append(c);
                }
                else
                {
                    flag = true;
                }

            return stringBuilder.ToString();
        }

        public static string GetOnlyDigits(this string value)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in value)
                if (char.IsDigit(c))
                    stringBuilder.Append(c);
            return stringBuilder.ToString();
        }

        public static byte[] HexToBytes(this string source)
        {
            var byteList = new List<byte>();
            var num = 0;
            var startIndex = 0;
            while (num < source.Length / 2)
            {
                byteList.Add(byte.Parse(source.Substring(startIndex, 2), NumberStyles.HexNumber));
                ++num;
                startIndex += 2;
            }

            return byteList.ToArray();
        }

        public static byte[] Base85ToBytes(this string source)
        {
            return ASCII85.Decode(source);
        }

        public static byte[] Base64ToBytes(this string source)
        {
            return Convert.FromBase64String(source);
        }

        public static string EncryptToBase64(this string source)
        {
            return Encoding.UTF8.GetBytes(source).Encrypt().ToBase64();
        }

        public static string DecryptFromBase64(this string source)
        {
            return Encoding.UTF8.GetString(source.Base64ToBytes().Decrypt());
        }

        public static object GetValueForPath(this string path, object rootObject)
        {
            return GetObjectFromPath(path, rootObject, new int?());
        }

        public static void SetValueForPath(this string path, object rootObject, object value)
        {
            var objectFromPath = GetObjectFromPath(path, rootObject, 1);
            if (objectFromPath == null)
                return;
            var strArray = path.Split(PropertyPathSeparators);
            var property = objectFromPath.GetType()
                .GetProperty(strArray[strArray.Length - 1], BindingFlags.Instance | BindingFlags.Public);
            if (!(property != null))
                return;
            property.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
        }

        public static Type ToType(this string typeName)
        {
            return !string.IsNullOrEmpty(typeName) ? Type.GetType(typeName) : null;
        }

        public static object ExtractIndex(string part)
        {
            if (part == null)
                return null;
            var match1 = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
            var match2 = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
            var index = (object)null;
            if (match1.Success)
            {
                index = match1.Groups["indexer"].Captures[0].Value;
            }
            else
            {
                int result;
                if (match2.Success && int.TryParse(match2.Groups["indexer"].Captures[0].Value, out result))
                    index = result;
            }

            return index;
        }

        public static Dictionary<string, object> ToDictionary(this string json)
        {
            return json.ToObject<Dictionary<string, object>>();
        }

        public static T ToObject<T>(this string json)
        {
            return JavaScriptConverterRegistry.Instance.GetSerializer().Deserialize<T>(json);
        }

        public static object ToObject(this string json, Type type)
        {
            return JavaScriptConverterRegistry.Instance.GetSerializer().Deserialize(type, json);
        }

        public static string ToLastFour(this string cardNumber)
        {
            return cardNumber.ToLastFour(true);
        }

        public static string ToLastFour(this string cardNumber, bool pad)
        {
            if (cardNumber == null || cardNumber.Length <= 4)
                return cardNumber;
            var startIndex = cardNumber.Length - 4;
            var lastFour = cardNumber.Substring(startIndex);
            if (!pad)
                return lastFour;
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < startIndex; ++index)
                stringBuilder.Append("X");
            stringBuilder.Append(lastFour);
            return stringBuilder.ToString();
        }

        public static string Escaped(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            input = input.Replace("\\", "\\\\");
            input = input.Replace("'", "\\'");
            input = input.Replace("\"", "\\\"");
            input = input.Replace("]", string.Empty);
            input = input.Replace("[", string.Empty);
            input = input.Replace("<", string.Empty);
            input = input.Replace(">", string.Empty);
            input = input.Replace("|", string.Empty);
            return input;
        }

        public static string EscapedJson(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            input = input.Replace("\\", "\\\\");
            input = input.Replace("'", "\\'");
            return input;
        }

        public static void ForEach(this string value, Action<char> action)
        {
            foreach (var ch in value)
                action(ch);
        }

        public static int? ToInt32(this string numberToConvert)
        {
            int result;
            return !int.TryParse(numberToConvert, out result) ? new int?() : result;
        }

        public static string SafePadLeft(this string theString, int length, char padCharacter)
        {
            return theString?.PadLeft(length, padCharacter);
        }

        private static object GetObjectFromPath(string path, object rootObject, int? depthAdjust)
        {
            if (path == null)
                return null;
            var strArray = path.Split(PropertyPathSeparators);
            if (strArray.Length == 0 || rootObject == null)
                return null;
            var length = strArray.Length;
            if (depthAdjust.HasValue)
                length -= depthAdjust.Value;
            var target = rootObject;
            for (var index1 = 0; index1 < length; ++index1)
            {
                var args = (object[])null;
                var str = strArray[index1];
                var index2 = ExtractIndex(str);
                if (index2 != null)
                {
                    str = str.Substring(0, str.IndexOf("["));
                    args = new object[1] { index2 };
                }

                try
                {
                    target = target.GetType().InvokeMember(str,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty, null, target,
                        args);
                    if (target == null)
                        break;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            return target;
        }

        internal static class ExtractIndexConstants
        {
            internal const string IndexerGroup = "indexer";
            internal const string TextIndexerRegex = "\\[\"(?<indexer>.*?)\"\\]";
            internal const string NumericIndexerRegex = "\\[(?<indexer>[0-9]*?)\\]";
        }

        internal static class NameValuePairConstants
        {
            internal const string NameGroup = "name";
            internal const string ValueGroup = "val";

            internal const string NameValuePairRegex =
                "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))";
        }
    }
}