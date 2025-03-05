using DeviceService.ComponentModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class Base87CardReadModelExtensions
  {
    public static IList<Error> GetTamperedErrors(this Base87CardReadModel source)
    {
      List<Error> result = new List<Error>();
      source.Errors.Where<DeviceService.ComponentModel.Error>((Func<DeviceService.ComponentModel.Error, bool>) (e => string.Equals(e.Code, "TAMPERED", StringComparison.CurrentCultureIgnoreCase))).ToList<DeviceService.ComponentModel.Error>().ForEach((Action<DeviceService.ComponentModel.Error>) (e => result.Add(Error.NewError(e.Code, e.Message, ""))));
      return (IList<Error>) result;
    }
  }
}
