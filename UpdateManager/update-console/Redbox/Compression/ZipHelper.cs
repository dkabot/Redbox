using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Redbox.Compression
{
    internal static class ZipHelper
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
            ZipFile zipFile = ZipFile.Create(zipFilename);
            FileSystemScanner fileSystemScanner;
            (fileSystemScanner = new FileSystemScanner(fileFilter, directoryFilter)).ProcessFile += (ProcessFileHandler)((s, e) =>
            {
                string entryName = e.Name.Substring(sourcePath.Length);
                zipFile.Add(e.Name, entryName);
            });
            JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
            if (metadata == null)
                metadata = (IDictionary<string, string>)new Dictionary<string, string>();
            IDictionary<string, string> dictionary = metadata;
            string comment = scriptSerializer.Serialize((object)dictionary);
            zipFile.BeginUpdate();
            zipFile.SetComment(comment);
            string directory = sourcePath;
            int num = recurse ? 1 : 0;
            fileSystemScanner.Scan(directory, num != 0);
            zipFile.CommitUpdate();
            zipFile.Close();
        }

        public static IDictionary<string, string> GetMetadata(string filename)
        {
            string zipFileComment = new ZipFile(filename).ZipFileComment;
            return string.IsNullOrEmpty(zipFileComment) ? (IDictionary<string, string>)new Dictionary<string, string>() : new JavaScriptSerializer().Deserialize<IDictionary<string, string>>(zipFileComment);
        }

        public static void ExtractAllFiles(string fileName, string path)
        {
            new FastZip().ExtractZip(fileName, path, FastZip.Overwrite.Always, (FastZip.ConfirmOverwriteDelegate)null, (string)null, (string)null, true);
        }
    }
}
