using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;

namespace Redbox.Compression
{
    public static class TarHelper
    {
        public static void Archive(string sourceRoot, string target)
        {
            using (var outputStream = File.Create(target))
            {
                using (var outputTarArchive = TarArchive.CreateOutputTarArchive(outputStream))
                {
                    outputTarArchive.AsciiTranslate = false;
                    outputTarArchive.ApplyUserInfoOverrides = false;
                    outputTarArchive.SetKeepOldFiles(false);
                    outputTarArchive.WriteEntry(TarEntry.CreateEntryFromFile(sourceRoot), true);
                }
            }
        }

        public static void Archive(
            string target,
            Dictionary<string, string> files,
            bool shouldStripPaths)
        {
            using (var outputStream = File.Create(target))
            {
                using (var outputTarArchive = TarArchive.CreateOutputTarArchive(outputStream))
                {
                    outputTarArchive.AsciiTranslate = false;
                    outputTarArchive.ApplyUserInfoOverrides = false;
                    outputTarArchive.SetKeepOldFiles(false);
                    foreach (var file in files)
                    {
                        var entryFromFile = TarEntry.CreateEntryFromFile(file.Value);
                        entryFromFile.Name = shouldStripPaths ? Path.GetFileName(file.Key) : file.Key;
                        outputTarArchive.WriteEntry(entryFromFile, false);
                    }
                }
            }
        }

        public static void Unpack(string source, string root)
        {
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException(root);
            using (var inputStream = File.OpenRead(source))
            {
                using (var inputTarArchive = TarArchive.CreateInputTarArchive(inputStream))
                {
                    inputTarArchive.AsciiTranslate = false;
                    inputTarArchive.ApplyUserInfoOverrides = false;
                    inputTarArchive.SetKeepOldFiles(false);
                    inputTarArchive.ExtractContents(root);
                }
            }
        }
    }
}