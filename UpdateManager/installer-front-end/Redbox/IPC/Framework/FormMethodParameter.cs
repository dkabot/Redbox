using Redbox.Compression;
using Redbox.Core;
using System;
using System.Reflection;

namespace Redbox.IPC.Framework
{
    internal class FormMethodParameter
    {
        public bool IsRequired()
        {
            return !this.Parameter.ParameterType.IsValueType ? this.MetaData != null && this.MetaData.IsRequired : this.MetaData != null && this.MetaData.IsRequired;
        }

        public object ConvertValue(string value)
        {
            if (this.Parameter.ParameterType != typeof(byte[]))
                return ConversionHelper.ChangeType((object)value, this.Parameter.ParameterType);
            byte[] source;
            switch (this.MetaData.BinaryEncoding)
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
                    throw new ArgumentException("The named paramete accepts binary data and the command form method doesn't specify a valid BinaryEncoding.");
            }
            if (source != null && this.MetaData.CompressionType != CompressionType.None)
                source = CompressionAlgorithm.GetAlgorithm(this.MetaData.CompressionType).Decompress(source);
            return (object)source;
        }

        public int Index { get; set; }

        public string KeyName { get; set; }

        public ParameterInfo Parameter { get; set; }

        public CommandKeyValueAttribute MetaData { get; set; }
    }
}
