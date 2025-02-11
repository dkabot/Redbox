using Redbox.Core;
using System.Collections.Generic;
using System.IO;

namespace Redbox.UpdateService.Client
{
    internal class LocalChangeSet
    {
        internal readonly Dictionary<long, Dictionary<string, string>> m_updates = new Dictionary<long, Dictionary<string, string>>();
        internal readonly Dictionary<string, Dictionary<string, string>> m_newFiles = new Dictionary<string, Dictionary<string, string>>();

        public bool HashChanges() => this.m_updates.Count > 0 || this.m_newFiles.Count > 0;

        public bool AddUpdateToFile(long id, string localPath, string displayName)
        {
            if (!File.Exists(localPath))
                return false;
            if (!this.m_updates.ContainsKey(id))
                this.m_updates.Add(id, new Dictionary<string, string>());
            using (FileStream inputStream = File.OpenRead(localPath))
                this.m_updates[id]["Hash"] = inputStream.ToASCIISHA1Hash();
            this.m_updates[id]["Data"] = localPath;
            this.m_updates[id]["DisplayName"] = displayName;
            return true;
        }

        public void UpdateDisplayName(long id, string displayName)
        {
            this.UpdateKey(id, "DisplayName", displayName);
        }

        public bool AddNewFile(
          string targetPath,
          string targetName,
          string displayName,
          string localPath)
        {
            if (!File.Exists(localPath))
                return false;
            Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "Name",
          targetName
        },
        {
          "Path",
          targetPath
        },
        {
          "DisplayName",
          displayName
        },
        {
          "Data",
          localPath
        }
      };
            using (FileStream inputStream = File.OpenRead(localPath))
                dictionary["Hash"] = inputStream.ToASCIISHA1Hash();
            this.m_newFiles.Add(Path.Combine(targetPath, targetName), dictionary);
            return true;
        }

        public void UndoChange(long id, string name)
        {
            if (!this.m_updates.ContainsKey(id) || !this.m_updates[id].ContainsKey(name))
                return;
            this.m_updates[id].Remove(name);
        }

        public void UndoAddFile(string fullPath)
        {
            if (!this.m_newFiles.ContainsKey(fullPath))
                return;
            this.m_newFiles.Remove(fullPath);
        }

        private void UpdateKey(long id, string name, string value)
        {
            if (!this.m_updates.ContainsKey(id))
            {
                this.m_updates.Add(id, new Dictionary<string, string>());
                this.m_updates[id].Add(name, value);
            }
            else if (this.m_updates[id].ContainsKey(name))
                this.m_updates[id][name] = value;
            else
                this.m_updates[id].Add(name, value);
        }
    }
}
