using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.Core
{
    internal static class StringExtensions
    {
        internal const BindingFlags GetObjectFromPathBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty;
        internal static readonly char[] PropertyPathSeparators = new char[1]
        {
      '.'
        };

        public static T ToEnum<T>(this string value) => Enum<T>.Parse(value, default(T));

        public static byte[] ReadToBuffer(this string fileName)
        {
            return fileName != null ? File.ReadAllBytes(fileName) : (byte[])null;
        }

        public static IDictionary<string, string> GetNameValuePairs(this string source)
        {
            IDictionary<string, string> nameValuePairs = (IDictionary<string, string>)new Dictionary<string, string>();
            foreach (Match match in Regex.Matches(source, "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))"))
            {
                Group group1 = match.Groups["name"];
                Group group2 = match.Groups["val"];
                if (group1.Captures.Count == group2.Captures.Count)
                {
                    for (int i = 0; i < group1.Captures.Count; ++i)
                        nameValuePairs[group1.Captures[i].Value] = group2.Captures[i].Value;
                }
            }
            return nameValuePairs;
        }

        public static string RemoveString(this string value, string token)
        {
            int startIndex = value.IndexOf(token);
            return startIndex <= -1 ? value : value.Remove(startIndex, token.Length);
        }

        public static string InnerTrim(this string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = false;
            foreach (char c in value)
            {
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
                    flag = true;
            }
            return stringBuilder.ToString();
        }

        public static string GetOnlyDigits(this string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in value)
            {
                if (char.IsDigit(c))
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }

        public static byte[] HexToBytes(this string source)
        {
            List<byte> byteList = new List<byte>();
            int num = 0;
            int startIndex = 0;
            while (num < source.Length / 2)
            {
                byteList.Add(byte.Parse(source.Substring(startIndex, 2), NumberStyles.HexNumber));
                ++num;
                startIndex += 2;
            }
            return byteList.ToArray();
        }

        public static byte[] Base85ToBytes(this string source) => ASCII85.Decode(source);

        public static byte[] Base64ToBytes(this string source) => Convert.FromBase64String(source);

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
            return StringExtensions.GetObjectFromPath(path, rootObject, new int?());
        }

        public static void SetValueForPath(this string path, object rootObject, object value)
        {
            object objectFromPath = StringExtensions.GetObjectFromPath(path, rootObject, new int?(1));
            if (objectFromPath == null)
                return;
            string[] strArray1 = path.Split(StringExtensions.PropertyPathSeparators);
            Type type = objectFromPath.GetType();
            string[] strArray2 = strArray1;
            string name = strArray2[strArray2.Length - 1];
            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (!(property != (PropertyInfo)null))
                return;
            property.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), (object[])null);
        }

        public static Type ToType(this string typeName)
        {
            return !string.IsNullOrEmpty(typeName) ? Type.GetType(typeName) : (Type)null;
        }

        public static object ExtractIndex(string part)
        {
            if (part == null)
                return (object)null;
            Match match1 = Regex.Match(part, "\\[\"(?<indexer>.*?)\"\\]", RegexOptions.Singleline);
            Match match2 = Regex.Match(part, "\\[(?<indexer>[0-9]*?)\\]", RegexOptions.Singleline);
            object index = (object)null;
            if (match1.Success)
            {
                index = (object)match1.Groups["indexer"].Captures[0].Value;
            }
            else
            {
                int result;
                if (match2.Success && int.TryParse(match2.Groups["indexer"].Captures[0].Value, out result))
                    index = (object)result;
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

        public static string ToLastFour(this string cardNumber) => cardNumber.ToLastFour(true);

        public static string ToLastFour(this string cardNumber, bool pad)
        {
            if (cardNumber == null || cardNumber.Length <= 4)
                return cardNumber;
            int startIndex = cardNumber.Length - 4;
            string lastFour = cardNumber.Substring(startIndex);
            if (!pad)
                return lastFour;
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < startIndex; ++index)
                stringBuilder.Append("X");
            stringBuilder.Append(lastFour);
            return stringBuilder.ToString();
        }

        public static string Escaped(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return (string)null;
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
                return (string)null;
            input = input.Replace("\\", "\\\\");
            input = input.Replace("'", "\\'");
            return input;
        }

        public static void ForEach(this string value, Action<char> action)
        {
            foreach (char ch in value)
                action(ch);
        }

        public static int? ToInt32(this string numberToConvert)
        {
            int result;
            return !int.TryParse(numberToConvert, out result) ? new int?() : new int?(result);
        }

        public static string SafePadLeft(this string theString, int length, char padCharacter)
        {
            return theString?.PadLeft(length, padCharacter);
        }

        private static object GetObjectFromPath(string path, object rootObject, int? depthAdjust)
        {
            if (path == null)
                return (object)null;
            string[] strArray = path.Split(StringExtensions.PropertyPathSeparators);
            if (strArray.Length == 0 || rootObject == null)
                return (object)null;
            int length = strArray.Length;
            if (depthAdjust.HasValue)
                length -= depthAdjust.Value;
            object target = rootObject;
            for (int index1 = 0; index1 < length; ++index1)
            {
                object[] args = (object[])null;
                string str = strArray[index1];
                object index2 = StringExtensions.ExtractIndex(str);
                if (index2 != null)
                {
                    str = str.Substring(0, str.IndexOf("["));
                    args = new object[1] { index2 };
                }
                try
                {
                    target = target.GetType().InvokeMember(str, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty, (Binder)null, target, args);
                    if (target == null)
                        break;
                }
                catch (Exception ex)
                {
                    return (object)null;
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
            internal const string NameValuePairRegex = "(?n:((?<name>[\\w-_]*)=(?<val>[\\w-_\\\\\\/\\(\\)\\,'\\.\\s]*)))";
        }
    }
}
