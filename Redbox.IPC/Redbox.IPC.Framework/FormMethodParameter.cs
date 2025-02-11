using System;
using System.Reflection;
using Redbox.Compression;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    internal class FormMethodParameter
    {
        public int Index { get; set; }

        public string KeyName { get; set; }

        public ParameterInfo Parameter { get; set; }

        public CommandKeyValueAttribute MetaData { get; set; }

        public bool IsRequired()
        {
            return !Parameter.ParameterType.IsValueType
                ? MetaData != null && MetaData.IsRequired
                : MetaData != null && MetaData.IsRequired;
        }

        public object ConvertValue(string value)
        {
            if (Parameter.ParameterType != typeof(byte[]))
                return ConversionHelper.ChangeType(value, Parameter.ParameterType);
            byte[] source;
            switch (MetaData.BinaryEncoding)
            {
                case BinaryEncoding.Hex:
                    source = value.HexToBytes();
                    break;
                case BinaryEncoding.Base64:
                    source = value.Base64ToBytes();
                    break;
                case BinaryEncoding.Ascii95:
                    throw new NotImplementedException("Ascii95 support is not implemented.");
                default:
                    throw new ArgumentException(
                        "The named paramete accepts binary data and the command form method doesn't specify a valid BinaryEncoding.");
            }

            if (source != null && MetaData.CompressionType != CompressionType.None)
                source = CompressionAlgorithm.GetAlgorithm(MetaData.CompressionType).Decompress(source);
            return source;
        }
    }
}