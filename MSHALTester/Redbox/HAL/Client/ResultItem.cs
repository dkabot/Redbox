using System;

namespace Redbox.HAL.Client;

public sealed class ResultItem
{
    private string m_rawResult;

    public ResultItem(string item)
    {
        Parse(item);
    }

    public string RawResult
    {
        get => m_rawResult;
        private set
        {
            if (value == null)
                return;
            m_rawResult = (string)value.Clone();
        }
    }

    public string Barcode { get; private set; }

    public string Metadata { get; private set; }

    public bool IsUnknown()
    {
        if (Barcode != null)
            return false;
        return Metadata.ToUpper().Equals("UNKNOWN") || Metadata.ToUpper().Equals("REDBOX");
    }

    public bool IsEmpty()
    {
        return Barcode == null && Metadata.Equals("EMPTY");
    }

    public override string ToString()
    {
        return RawResult;
    }

    private bool Parse(string item)
    {
        RawResult = item;
        if (RawResult == null)
            return false;
        var strArray = item.Split('(');
        if (strArray[0].Equals("UNKNOWN", StringComparison.CurrentCultureIgnoreCase) ||
            strArray[0].Equals("EMPTY", StringComparison.CurrentCultureIgnoreCase) ||
            strArray[0].Equals("redbox", StringComparison.CurrentCultureIgnoreCase))
        {
            Metadata = strArray[0];
        }
        else
        {
            Barcode = strArray[0].TrimEnd();
            if (strArray.Length > 1)
            {
                var length = strArray[1].IndexOf(')');
                Metadata = length == -1 ? strArray[1] : strArray[1].Substring(0, length);
            }
        }

        return true;
    }
}