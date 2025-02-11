using System;
using System.Collections.Generic;
using System.IO;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.FileSets
{
    internal class Shared
    {
        internal static void SafeDelete(string path)
        {
            var errorList = new List<Error>();
            try
            {
                if (!File.Exists(path))
                    return;
                File.Delete(path);
            }
            catch (Exception ex)
            {
                errorList.Add(new Error
                {
                    Message = string.Format("FileSetService.SafeDelete An unhandled exception occurred. Exception {0}",
                        ex)
                });
            }
        }
    }
}