using ICSharpCode.SharpZipLib.Tar;
using System.Collections.Generic;
using System.IO;

namespace Redbox.Compression
{
    internal static class TarHelper
    {
        public static void Archive(string sourceRoot, string target)
        {
            using (FileStream outputStream = File.Create(target))
            {
                using (TarArchive outputTarArchive = TarArchive.CreateOutputTarArchive((Stream)outputStream))
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
            using (FileStream outputStream = File.Create(target))
            {
                using (TarArchive outputTarArchive = TarArchive.CreateOutputTarArchive((Stream)outputStream))
                {
                    outputTarArchive.AsciiTranslate = false;
                    outputTarArchive.ApplyUserInfoOverrides = false;
                    outputTarArchive.SetKeepOldFiles(false);
                    foreach (KeyValuePair<string, string> file in files)
                    {
                        TarEntry entryFromFile = TarEntry.CreateEntryFromFile(file.Value);
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
            using (FileStream inputStream = File.OpenRead(source))
            {
                using (TarArchive inputTarArchive = TarArchive.CreateInputTarArchive((Stream)inputStream))
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
