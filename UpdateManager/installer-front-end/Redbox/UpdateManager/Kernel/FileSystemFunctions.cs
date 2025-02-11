using Redbox.Compression;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;

namespace Redbox.UpdateManager.Kernel
{
    internal static class FileSystemFunctions
    {
        [KernelFunction(Name = "kernel.getfileinfo")]
        internal static LuaTable GetFileInfo(string file)
        {
            LuaTable fileInfo1 = new LuaTable(KernelService.Instance.LuaRuntime);
            FileInfo fileInfo2 = new FileInfo(file);
            fileInfo1[(object)"directory"] = (object)fileInfo2.DirectoryName;
            fileInfo1[(object)"length"] = (object)fileInfo2.Length;
            fileInfo1[(object)"name"] = (object)fileInfo2.Name;
            fileInfo1[(object)"exists"] = (object)fileInfo2.Exists;
            fileInfo1[(object)"extension"] = (object)fileInfo2.Extension;
            fileInfo1[(object)"full_name"] = (object)fileInfo2.FullName;
            fileInfo1[(object)"is_read_only"] = (object)fileInfo2.IsReadOnly;
            fileInfo1[(object)"last_write_time"] = (object)fileInfo2.LastWriteTime.ToString("yyyyMMddHHmmss");
            return fileInfo1;
        }

        [KernelFunction(Name = "kernel.getfilelength")]
        internal static long GetFileLength(string file) => new FileInfo(file).Length;

        [KernelFunction(Name = "kernel.tar")]
        internal static bool Tar(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                string sourceRoot = service.ExpandProperties(source);
                string target1 = service.ExpandProperties(target);
                LogHelper.Instance.Log("TAR: source={0}, target={1}", (object)sourceRoot, (object)target1);
                TarHelper.Archive(sourceRoot, target1);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.Tar.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.untar")]
        internal static bool UnTar(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                TarHelper.Unpack(service.ExpandProperties(source), service.ExpandProperties(target));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.UnTar.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.zip")]
        internal static bool Zip(
          string source,
          string target,
          bool recurse,
          string fileFilter,
          string dirFilter)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                string sourcePath = service.ExpandProperties(source);
                string zipFileName = service.ExpandProperties(target);
                string fileFilter1 = service.ExpandProperties(fileFilter);
                string directoryFilter = service.ExpandProperties(dirFilter);
                LogHelper.Instance.Log("ZIP: source={0}, target={1}, recurse={2}, fileFilter={3}, dirFilter={4}", (object)sourcePath, (object)zipFileName, (object)recurse, (object)fileFilter1, (object)directoryFilter);
                if (string.IsNullOrEmpty(dirFilter))
                    ZipHelper.CreateZipFile(zipFileName, sourcePath, recurse, fileFilter1);
                else
                    ZipHelper.CreateZipFile(zipFileName, sourcePath, recurse, fileFilter1, directoryFilter);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.Zip.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.unzip")]
        internal static bool UnZip(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                string fileName = service.ExpandProperties(source);
                string path = service.ExpandProperties(target);
                LogHelper.Instance.Log("UNZIP: source={0}, target={1}", (object)fileName, (object)path);
                ZipHelper.ExtractAllFiles(fileName, path);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.UnZip.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.gzipdecompress")]
        internal static bool GzipDecompress(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                string str = service.ExpandProperties(source);
                string path = service.ExpandProperties(target);
                if (!File.Exists(str))
                    throw new FileNotFoundException("File does not exists and can not be decompressed.", str);
                using (FileStream source1 = File.OpenRead(str))
                {
                    using (FileStream target1 = File.Create(path))
                        CompressionAlgorithm.GetAlgorithm(CompressionType.GZip).Decompress((Stream)source1, (Stream)target1);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GzipDecompress.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.gzipcompress")]
        internal static bool GzipCompress(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                string str = service.ExpandProperties(source);
                string path = service.ExpandProperties(target);
                if (!File.Exists(str))
                    throw new FileNotFoundException("File does not exists and can not be decompressed.", str);
                using (FileStream source1 = File.OpenRead(str))
                {
                    using (FileStream target1 = File.Create(path))
                        CompressionAlgorithm.GetAlgorithm(CompressionType.GZip).Compress((Stream)source1, (Stream)target1);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GzipCompress.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.getdirectoryfrompath")]
        internal static string GetDirectoryNameFromPath(string fullPath)
        {
            return Path.GetDirectoryName(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fullPath));
        }

        [KernelFunction(Name = "kernel.getfilenamefrompath")]
        internal static string GetFileNameFromPath(string fullPath)
        {
            return Path.GetFileName(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fullPath));
        }

        [KernelFunction(Name = "kernel.getfilenamewithoutextension")]
        internal static string GetFileNameWithOutExtension(string fullPath)
        {
            return Path.GetFileNameWithoutExtension(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fullPath));
        }

        [KernelFunction(Name = "kernel.directorycreate")]
        internal static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path));
        }

        [KernelFunction(Name = "kernel.directorycopy")]
        internal static bool DirectoryCopy(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                FileSystemFunctions.DirectoryCopyInternal(new DirectoryInfo(service.ExpandProperties(source)), new DirectoryInfo(service.ExpandProperties(target)));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileCopy.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.filecopy")]
        internal static bool FileCopy(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                File.Copy(service.ExpandProperties(source), service.ExpandProperties(target), true);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileCopy.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.filemove")]
        internal static bool FileMove(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                File.Move(service.ExpandProperties(source), service.ExpandProperties(target));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileMove.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.filemoveandoverwrite")]
        internal static bool FileMoveOverwrite(string source, string target)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                File.Delete(service.ExpandProperties(target));
                File.Move(service.ExpandProperties(source), service.ExpandProperties(target));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileMove.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.fileexists")]
        internal static bool FileExists(string file)
        {
            return File.Exists(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file));
        }

        [KernelFunction(Name = "kernel.directoryexists")]
        internal static bool DirectoryExists(string path)
        {
            return Directory.Exists(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path));
        }

        [KernelFunction(Name = "kernel.directoryremove")]
        internal static bool RemoveDirectory(string path, bool recurse)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                if (!Directory.Exists(service.ExpandProperties(path)))
                    return false;
                Directory.Delete(service.ExpandProperties(path), recurse);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.RemoveDirectory.", ex);
            }
            return true;
        }

        [KernelFunction(Name = "kernel.filedelete")]
        internal static bool FileDelete(string file)
        {
            try
            {
                File.Delete(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileDelete.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.filewritealltext")]
        internal static bool FileWriteAllText(string file, string text)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                File.WriteAllText(service.ExpandProperties(file), service.ExpandProperties(text));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileWriteAllText.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.fileappendtext")]
        internal static bool FileAppendText(string file, string text)
        {
            try
            {
                IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
                File.AppendAllText(service.ExpandProperties(file), service.ExpandProperties(text));
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileAppendText.", ex);
            }
            return false;
        }

        [KernelFunction(Name = "kernel.filereadalltext")]
        internal static string FileReadAllText(string file)
        {
            try
            {
                return File.ReadAllText(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileReadAllText.", ex);
            }
            return (string)null;
        }

        [KernelFunction(Name = "kernel.filereadalllines")]
        internal static LuaTable FileReadAllLinesText(string file)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            try
            {
                string[] strArray = File.ReadAllLines(service.ExpandProperties(file));
                int key = 1;
                foreach (string str in strArray)
                {
                    luaTable[(object)key] = (object)str;
                    ++key;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.FileReadAllLinesText.", ex);
            }
            return luaTable;
        }

        [KernelFunction(Name = "kernel.getdirectoryname")]
        internal static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path));
        }

        [KernelFunction(Name = "kernel.pathcombine")]
        internal static string PathCombine(string path1, string path2)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            return Path.Combine(service.ExpandProperties(path1), service.ExpandProperties(path2));
        }

        [KernelFunction(Name = "kernel.directorygetfiles")]
        internal static LuaTable GetFiles(string path, string pattern, bool recurse)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            LuaTable files1 = new LuaTable(KernelService.Instance.LuaRuntime);
            try
            {
                string[] files2 = Directory.GetFiles(service.ExpandProperties(path), pattern, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                int num = 1;
                foreach (string str in files2)
                    files1[(object)num++] = (object)str;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GetFiles.", ex);
            }
            return files1;
        }

        [KernelFunction(Name = "kernel.directorygetdirectories")]
        internal static LuaTable GetDirectories(string path, string pattern, bool recurse)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            LuaTable directories1 = new LuaTable(KernelService.Instance.LuaRuntime);
            try
            {
                string[] directories2 = Directory.GetDirectories(service.ExpandProperties(path), pattern, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                int num = 1;
                foreach (string str in directories2)
                    directories1[(object)num++] = (object)str;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("An unhandled exception was raised in FileSystemFunctions.GetDirectories.", ex);
            }
            return directories1;
        }

        [KernelFunction(Name = "kernel.editxmlfile")]
        internal static LuaTable EditXmlFile(
          string path,
          bool editAllMatches,
          bool raiseErrors,
          object xmlNamespaces,
          object xmlEdits)
        {
            IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
            LuaTable luaTable1 = new LuaTable(KernelService.Instance.LuaRuntime);
            XmlFileEditor xmlFileEditor = new XmlFileEditor(service.ExpandProperties(path))
            {
                EditAllMatches = editAllMatches,
                RaiseErrors = raiseErrors
            };
            if (xmlNamespaces is LuaTable luaTable3)
            {
                foreach (object key in (IEnumerable)luaTable3.Keys)
                {
                    if (luaTable3[key] is LuaTable luaTable2)
                        xmlFileEditor.Namespaces.Add(new XmlNamespace((string)luaTable2[(object)"prefix"], (string)luaTable2[(object)"uri"]));
                }
            }
            if (xmlEdits is LuaTable luaTable5)
            {
                foreach (object key in (IEnumerable)luaTable5.Keys)
                {
                    if (luaTable5[key] is LuaTable luaTable4)
                    {
                        XmlEdit xmlEdit = new XmlEdit(service.ExpandProperties((string)luaTable4[(object)"xpath"]), service.ExpandProperties((string)luaTable4[(object)"value"]));
                        LogHelper.Instance.Log("xpath = {0}, value = {1}", (object)xmlEdit.XPath, (object)xmlEdit.Value);
                        xmlFileEditor.Edits.Add(xmlEdit);
                    }
                }
            }
            ReadOnlyCollection<XmlEditorError> readOnlyCollection = xmlFileEditor.Apply();
            luaTable1[(object)"success"] = (object)(readOnlyCollection.Count == 0);
            LuaTable luaTable6 = new LuaTable(KernelService.Instance.LuaRuntime);
            int num = 1;
            foreach (XmlEditorError xmlEditorError in readOnlyCollection)
                luaTable6[(object)num++] = (object)new LuaTable(KernelService.Instance.LuaRuntime)
                {
                    [(object)"code"] = (object)xmlEditorError.Code,
                    [(object)"details"] = (object)xmlEditorError.Details,
                    [(object)"description"] = (object)xmlEditorError.Description
                };
            luaTable1[(object)"errors"] = (object)luaTable6;
            return luaTable1;
        }

        [KernelFunction(Name = "kernel.getfilehash")]
        internal static string GetSHA1Hash(string fileName)
        {
            string path = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName);
            if (!File.Exists(path))
                return "0000000000000000000000000000000000000000";
            using (FileStream inputStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                return inputStream.ToASCIISHA1Hash();
        }

        private static void DirectoryCopyInternal(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
                Directory.CreateDirectory(target.FullName);
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.ToString(), file.Name), true);
            foreach (DirectoryInfo directory in source.GetDirectories())
            {
                DirectoryInfo subdirectory = target.CreateSubdirectory(directory.Name);
                FileSystemFunctions.DirectoryCopyInternal(directory, subdirectory);
            }
        }
    }
}
