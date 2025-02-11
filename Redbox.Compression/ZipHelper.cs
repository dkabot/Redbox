using System.Collections.Generic;
using System.Web.Script.Serialization;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression
{
    public static class ZipHelper
    {
        public static void CreateZipFile(
            string zipFileName,
            string sourcePath,
            bool recurse,
            string fileFilter)
        {
            new FastZip().CreateZip(zipFileName, sourcePath, recurse, fileFilter);
        }

        public static void CreateZipFile(
            string zipFileName,
            string sourcePath,
            bool recurse,
            string fileFilter,
            string directoryFilter)
        {
            new FastZip().CreateZip(zipFileName, sourcePath, recurse, fileFilter, directoryFilter);
        }

        public static void CreateZipFile(
            string zipFilename,
            string sourcePath,
            bool recurse,
            string fileFilter,
            string directoryFilter,
            IDictionary<string, string> metadata)
        {
            var zipFile = ZipFile.Create(zipFilename);
            FileSystemScanner fileSystemScanner;
            (fileSystemScanner = new FileSystemScanner(fileFilter, directoryFilter)).ProcessFile += (s, e) =>
            {
                var entryName = e.Name.Substring(sourcePath.Length);
                zipFile.Add(e.Name, entryName);
            };
            var scriptSerializer = new JavaScriptSerializer();
            if (metadata == null)
                metadata = new Dictionary<string, string>();
            var dictionary = metadata;
            var comment = scriptSerializer.Serialize(dictionary);
            zipFile.BeginUpdate();
            zipFile.SetComment(comment);
            var directory = sourcePath;
            var num = recurse ? 1 : 0;
            fileSystemScanner.Scan(directory, num != 0);
            zipFile.CommitUpdate();
            zipFile.Close();
        }

        public static IDictionary<string, string> GetMetadata(string filename)
        {
            var zipFileComment = new ZipFile(filename).ZipFileComment;
            return string.IsNullOrEmpty(zipFileComment)
                ? new Dictionary<string, string>()
                : new JavaScriptSerializer().Deserialize<IDictionary<string, string>>(zipFileComment);
        }

        public static void ExtractAllFiles(string fileName, string path)
        {
            new FastZip().ExtractZip(fileName, path, FastZip.Overwrite.Always, null, null, null, true);
        }
    }
}