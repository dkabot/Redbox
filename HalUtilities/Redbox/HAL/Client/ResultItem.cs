using System;

namespace Redbox.HAL.Client
{
  public sealed class ResultItem
  {
    private string m_rawResult;

    public ResultItem(string item) => this.Parse(item);

    public bool IsUnknown()
    {
      if (this.Barcode != null)
        return false;
      return this.Metadata.ToUpper().Equals("UNKNOWN") || this.Metadata.ToUpper().Equals("REDBOX");
    }

    public bool IsEmpty() => this.Barcode == null && this.Metadata.Equals("EMPTY");

    public override string ToString() => this.RawResult;

    public string RawResult
    {
      get => this.m_rawResult;
      private set
      {
        if (value == null)
          return;
        this.m_rawResult = (string) value.Clone();
      }
    }

    public string Barcode { get; private set; }

    public string Metadata { get; private set; }

    private bool Parse(string item)
    {
      this.RawResult = item;
      if (this.RawResult == null)
        return false;
      string[] strArray = item.Split('(');
      if (strArray[0].Equals("UNKNOWN", StringComparison.CurrentCultureIgnoreCase) || strArray[0].Equals("EMPTY", StringComparison.CurrentCultureIgnoreCase) || strArray[0].Equals("redbox", StringComparison.CurrentCultureIgnoreCase))
      {
        this.Metadata = strArray[0];
      }
      else
      {
        this.Barcode = strArray[0].TrimEnd();
        if (strArray.Length > 1)
        {
          int length = strArray[1].IndexOf(')');
          this.Metadata = length == -1 ? strArray[1] : strArray[1].Substring(0, length);
        }
      }
      return true;
    }
  }
}
