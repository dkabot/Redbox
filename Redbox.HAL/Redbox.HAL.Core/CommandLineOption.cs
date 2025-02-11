using System;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Core;

public static class CommandLineOption
{
    public const string ColonDelimiter = ":";
    public const string AssignDelimiter = "=";

    public static string GetOptionValue(string option)
    {
        return GetOptionValue(option, ":");
    }

    public static string GetOptionValue(string option, string delimiter)
    {
        if (!option.Contains(delimiter))
            return string.Empty;
        var strArray = option.Split(new string[1]
        {
            delimiter
        }, StringSplitOptions.RemoveEmptyEntries);
        return strArray.Length != 2 ? string.Empty : strArray[1];
    }

    public static T GetOptionVal<T>(string option, T defVal)
    {
        var optionValue = GetOptionValue(option);
        return string.IsNullOrEmpty(optionValue) ? defVal : ConversionHelper.ChangeType<T>(optionValue);
    }

    public static T GetOptionVal<T>(string option, string delimiter, T defVal)
    {
        var optionValue = GetOptionValue(option, delimiter);
        return string.IsNullOrEmpty(optionValue) ? defVal : ConversionHelper.ChangeType<T>(optionValue);
    }
}