using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public static class ErrorListExtensions
  {
    public static void CopyToLocalCollection(this Redbox.REDS.Framework.ErrorList source, Redbox.KioskEngine.ComponentModel.ErrorList target)
    {
      foreach (Redbox.REDS.Framework.Error error in (List<Redbox.REDS.Framework.Error>) source)
        target.Add(error.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(error.Code, error.Description, error.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(error.Code, error.Description, error.Details));
    }
  }
}
