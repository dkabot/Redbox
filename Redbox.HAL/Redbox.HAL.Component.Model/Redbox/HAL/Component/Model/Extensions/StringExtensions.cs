using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Redbox.HAL.Component.Model.Extensions
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

        public static byte[] HexToBytes(string source)
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

        public static byte[] Base64ToBytes(string source)
        {
            return Convert.FromBase64String(source);
        }

        public static object GetValueForPath(string path, object rootObject)
        {
            return GetObjectFromPath(path, rootObject, new int?());
        }

        public static void SetValueForPath(string path, object rootObject, object value)
        {
            var objectFromPath = GetObjectFromPath(path, rootObject, 1);
            if (objectFromPath == null)
                return;
            var strArray = path.Split(PropertyPathSeparators);
            var property = objectFromPath.GetType()
                .GetProperty(strArray[strArray.Length - 1], BindingFlags.Instance | BindingFlags.Public);
            property?.SetValue(objectFromPath, ConversionHelper.ChangeType(value, property.PropertyType), null);
        }

        public static Type ToType(string typeName)
        {
            return Type.GetType(typeName);
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

        public static string ExtractCodeFromBrackets(string value, string prefix, string postfix)
        {
            var num1 = 0;
            var startIndex1 = -1;
            var num2 = -1;
            var startIndex2 = 0;
            do
            {
                var str1 = string.Empty;
                if (startIndex2 + prefix.Length < value.Length)
                    str1 = value.Substring(startIndex2, prefix.Length);
                var str2 = string.Empty;
                if (startIndex2 + str2.Length < value.Length)
                    str2 = value.Substring(startIndex2, postfix.Length);
                if (str1 == prefix)
                {
                    if (startIndex1 == -1)
                        startIndex1 = startIndex2 + prefix.Length;
                    ++num1;
                }
                else if (str2 == postfix)
                {
                    --num1;
                    if (num1 == 0)
                    {
                        num2 = startIndex2 - postfix.Length;
                        break;
                    }
                }

                ++startIndex2;
            } while (startIndex2 < value.Length);

            return startIndex1 == -1 || num2 == -1
                ? null
                : value.Substring(startIndex1, num2 - startIndex1 + postfix.Length);
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