using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class BackupEntry
    {
        internal readonly DateTime? CreateDate;
        internal readonly string FileName;
        internal readonly string FullPath;

        internal BackupEntry(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(FullPath);
            try
            {
                CreateDate = File.GetCreationTime(FullPath);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[BackupEntry] Unable to obtain creation time.", ex);
                CreateDate = new DateTime?();
            }
        }
    }
}