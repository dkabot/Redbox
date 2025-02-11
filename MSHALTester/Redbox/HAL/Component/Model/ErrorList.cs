using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.HAL.Component.Model;

[Serializable]
public sealed class ErrorList : List<Error>, ICloneable
{
    public int ErrorCount => FindAll(each => !each.IsWarning).Count;

    public int WarningCount => FindAll(each => each.IsWarning).Count;

    public object Clone()
    {
        var errorList = new ErrorList();
        errorList.AddRange(this);
        return errorList;
    }

    public static ErrorList NewFromStrings(string[] errors)
    {
        var errorList = new ErrorList();
        if (errors == null)
            return errorList;
        foreach (var error in errors)
            errorList.Add(Error.Parse(error));
        return errorList;
    }

    public bool ContainsCode(string code)
    {
        return Find(each => each.Code == code) != null;
    }

    public bool ContainsError()
    {
        return Find(each => !each.IsWarning) != null;
    }

    public int RemoveCode(string code)
    {
        return RemoveAll(each => each.Code == code);
    }

    public void Dump(TextWriter writer)
    {
        writer.WriteLine(FormatList());
    }

    public void DumpToLog()
    {
        LogHelper.Instance.Log(FormatList());
    }

    private string FormatList()
    {
        var builder = new StringBuilder();
        builder.AppendLine("-- Errors --");
        ForEach(e =>
        {
            builder.AppendLine(string.Format(" Code        : {0}", e.Code));
            builder.AppendLine(string.Format(" Description : {0}", e.Description));
            builder.AppendLine(string.Format(" Details     : {0}", e.Details));
        });
        return builder.ToString();
    }
}