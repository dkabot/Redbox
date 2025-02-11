using System;
using System.Text;

namespace Redbox.HAL.Component.Model;

[Serializable]
public sealed class Error
{
    private Error(string code, string description, string details, bool isWarning)
    {
        Code = code;
        Details = details;
        IsWarning = isWarning;
        Description = description;
    }

    public string Code { get; }

    public string Details { get; }

    public bool IsWarning { get; }

    public string Description { get; }

    public static Error Parse(string error)
    {
        var codeFromBrackets = ExtractCodeFromBrackets(error, "[", "]");
        var startIndex = error.IndexOf("]");
        var strArray = error.Substring(startIndex).Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var str = (string)null;
        if (strArray.Length > 1)
            str = strArray[1];
        var description = strArray[0].Substring(strArray[0].IndexOf(":") + 1);
        var details = str;
        var num = strArray[0].StartsWith("WARNING") ? 1 : 0;
        return new Error(codeFromBrackets, description, details, num != 0);
    }

    public static Error NewError(string code, string description, Exception e)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(e);
        if (e.InnerException != null)
            stringBuilder.AppendFormat("\r\n\r\nInner Exception:\r\n---------------------------\r\n{0}\r\n",
                e.InnerException);
        stringBuilder.AppendFormat("\r\n\r\nStack Trace:\r\n----------------------------\r\n{0}\r\n", e.StackTrace);
        return new Error(code, description, stringBuilder.ToString(), false);
    }

    public static Error NewError(string code, string description, string details)
    {
        return new Error(code, description, details, false);
    }

    public static Error NewWarning(string code, string description, string details)
    {
        return new Error(code, description, details, true);
    }

    public override string ToString()
    {
        return string.Format("[{0}] {1}: {2}", Code, IsWarning ? "WARNING" : (object)"ERROR", Description);
    }

    private static string ExtractCodeFromBrackets(string value, string prefix, string postfix)
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
}