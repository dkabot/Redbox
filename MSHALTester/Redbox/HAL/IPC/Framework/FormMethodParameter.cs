using System;
using System.Reflection;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Compression;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.IPC.Framework;

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
                source = StringExtensions.HexToBytes(value);
                break;
            case BinaryEncoding.Base64:
                source = StringExtensions.Base64ToBytes(value);
                break;
            case BinaryEncoding.Ascii95:
                throw new NotImplementedException("Ascii95 support is not implemented.");
            default:
                throw new ArgumentException(
                    "The named paramete accepts binary data and the command form method doesn't specify a valid BinaryEncoding.");
        }

        if (source != null && MetaData.CompressionType != CompressionType.None)
            source = CompressionAlgorithmFactory.GetAlgorithm(MetaData.CompressionType).Decompress(source);
        return source;
    }
}