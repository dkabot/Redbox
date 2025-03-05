using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.KioskEngine.ComponentModel
{
  public class ErrorList : List<Error>, ICloneable
  {
    public bool ContainsCode(string code)
    {
      return this.Find((Predicate<Error>) (each => each.Code == code)) != null;
    }

    public bool ContainsError() => this.Find((Predicate<Error>) (each => !each.IsWarning)) != null;

    public int RemoveCode(string code)
    {
      return this.RemoveAll((Predicate<Error>) (each => each.Code == code));
    }

    public object Clone()
    {
      ErrorList errorList = new ErrorList();
      errorList.AddRange((IEnumerable<Error>) this);
      return (object) errorList;
    }

    public int ErrorCount => this.FindAll((Predicate<Error>) (each => !each.IsWarning)).Count;

    public int WarningCount => this.FindAll((Predicate<Error>) (each => each.IsWarning)).Count;

    public void ToLogHelper()
    {
      if (this.Count <= 0)
        return;
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Error error in (List<Error>) this)
      {
        stringBuilder.AppendLine(error.ToString());
        stringBuilder.AppendLine(error.Details);
      }
      LogHelper.Instance.Log(stringBuilder.ToString());
    }
  }
}
