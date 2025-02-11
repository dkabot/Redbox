using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.IO;

namespace Redbox.UpdateManager.FileSets
{
    internal class Shared
    {
        internal static void SafeDelete(string path)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Deleting file {0}", (object)path);
                if (!File.Exists(path))
                    return;
                File.Delete(path);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetService.SafeDelete", "An unhandled exception occurred.", ex));
            }
        }
    }
}
