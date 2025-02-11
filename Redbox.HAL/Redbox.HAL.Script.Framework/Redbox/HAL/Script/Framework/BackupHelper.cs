using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class BackupHelper
    {
        private readonly string BackupPath;
        private readonly string BackupPrefix;

        internal BackupHelper(string path, string filePrefix)
        {
            BackupPath = path;
            BackupPrefix = filePrefix;
        }

        internal string MakeNewBackup()
        {
            return Path.Combine(BackupPath,
                string.Format("{0}-{1}", BackupPrefix,
                    ServiceLocator.Instance.GetService<IRuntimeService>().GenerateUniqueFile("xml")));
        }

        internal void Trim()
        {
            var service = ServiceLocator.Instance.GetService<IRuntimeService>();
            var num = 5;
            var sorted = GetSorted();
            using (new DisposeableList<BackupEntry>(sorted))
            {
                if (sorted.Count <= num)
                    return;
                for (var index = 0; index < sorted.Count - num; ++index)
                    service.SafeDelete(sorted[index].FullPath);
            }
        }

        internal BackupEntry GetYoungest()
        {
            var sorted = GetSorted();
            using (new DisposeableList<BackupEntry>(sorted))
            {
                return sorted.Count == 0 ? null : sorted[sorted.Count - 1];
            }
        }

        internal static BackupHelper GetInventoryHelper()
        {
            return new BackupHelper(ServiceLocator.Instance.GetService<IRuntimeService>().DataPath, "inventory");
        }

        private List<BackupEntry> GetSorted()
        {
            var sorted = new List<BackupEntry>();
            foreach (var file in Directory.GetFiles(BackupPath, string.Format("{0}*.xml", BackupPrefix)))
            {
                var backupEntry = new BackupEntry(file);
                if (backupEntry.CreateDate.HasValue)
                    sorted.Add(backupEntry);
            }

            sorted.Sort((x, y) => x.CreateDate.Value.CompareTo(y.CreateDate.Value));
            return sorted;
        }
    }
}