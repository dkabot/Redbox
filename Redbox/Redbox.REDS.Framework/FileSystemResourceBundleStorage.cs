using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Redbox.REDS.Framework
{
    public class FileSystemResourceBundleStorage : IResourceBundleStorage, IDisposable
    {
        public void Dispose()
        {
        }

        public void Open(string path)
        {
            BundlePath = path;
        }

        public void Close()
        {
        }

        public void Create(string path, string trimPrefix)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                BundlePath = path;
            }
            catch
            {
            }
        }

        public void AddEntry(string path, string trimPrefix)
        {
        }

        public void BeginUpdate()
        {
        }

        public void CommitUpdate()
        {
        }

        public byte[] GetEntryContent(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var path1 = Path.Combine(BundlePath, path);
            return File.Exists(path1) ? File.ReadAllBytes(path1) : null;
        }

        public IResourceBundleStorageEntry GetEntry(string path)
        {
            var str = Path.Combine(BundlePath, path);
            if (!File.Exists(str))
                return null;
            var fileInfo = new FileInfo(str);
            return new ResourceBundleStorageEntry
            {
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                Path = fileInfo.FullName
            };
        }

        public byte[] GetEntryContent(IResourceBundleStorageEntry entry)
        {
            return GetEntryContent(entry.Path);
        }

        public ReadOnlyCollection<IResourceBundleStorageEntry> GetResourceManifest()
        {
            var bundleStorageEntryList = new List<IResourceBundleStorageEntry>();
            foreach (var file in Directory.GetFiles(BundlePath, "*.*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(file);
                bundleStorageEntryList.Add(new ResourceBundleStorageEntry
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    Path = fileInfo.FullName
                });
            }

            return bundleStorageEntryList.AsReadOnly();
        }

        public bool SupportsSave => true;

        public string BundlePath { get; internal set; }
    }
}