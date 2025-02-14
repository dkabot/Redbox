using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel.Responses;

namespace Redbox.KioskEngine.ComponentModel
{
    public static class Base87CardReadModelExtensions
    {
        public static IList<Error> GetTamperedErrors(this Base87CardReadModel source)
        {
            var result = new List<Error>();
            source.Errors.Where(e => string.Equals(e.Code, "TAMPERED", StringComparison.CurrentCultureIgnoreCase))
                .ToList().ForEach(e => result.Add(Error.NewError(e.Code, e.Message, "")));
            return result;
        }
    }
}