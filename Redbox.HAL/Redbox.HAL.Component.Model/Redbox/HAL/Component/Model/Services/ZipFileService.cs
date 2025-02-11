using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.HAL.Component.Model.Services
{
    public sealed class ZipFileService : IZipFileService
    {
        private readonly IRuntimeService RuntimeService;

        public ZipFileService(IRuntimeService rts)
        {
            RuntimeService = rts;
        }

        public ZipResult Zip(IEnumerable<string> files, string zipPath)
        {
            var zipResult = new ZipResult
            {
                ZipFile = zipPath
            };
            try
            {
                using (var zipOutputStream = new ZipOutputStream(File.Create(zipPath)))
                {
                    zipOutputStream.SetLevel(9);
                    var buffer = new byte[4096];
                    var num = 0;
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        zipOutputStream.PutNextEntry(new ZipEntry(fileInfo.Name)
                        {
                            DateTime = fileInfo.CreationTime
                        });
                        using (var fileStream = File.OpenRead(file))
                        {
                            int count;
                            do
                            {
                                count = fileStream.Read(buffer, 0, buffer.Length);
                                zipOutputStream.Write(buffer, 0, count);
                            } while (count > 0);
                        }

                        ++num;
                    }

                    zipOutputStream.Finish();
                    zipOutputStream.Close();
                    zipResult.EntryCount = num;
                    zipResult.Success = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("!!OnZipImages: exception!!", ex);
                zipResult.Success = false;
            }

            return zipResult;
        }

        public UnzipResult Unzip(string zipFile)
        {
            var unzipPath = RuntimeService.RuntimePath(Path.GetFileNameWithoutExtension(new FileInfo(zipFile).Name));
            return Unzip(zipFile, unzipPath);
        }

        public UnzipResult Unzip(string zipFile, string unzipPath)
        {
            var unzipResult = new UnzipResult
            {
                SourceZip = zipFile,
                OutputPath = unzipPath,
                Success = false
            };
            if (!SafeCreateDirectory(unzipPath))
                return unzipResult;
            try
            {
                using (var zipInputStream = new ZipInputStream(File.OpenRead(zipFile)))
                {
                    ZipEntry nextEntry;
                    while ((nextEntry = zipInputStream.GetNextEntry()) != null)
                        if (Path.GetFileName(nextEntry.Name) != string.Empty)
                        {
                            var str = Path.Combine(unzipPath, nextEntry.Name);
                            using (var fileStream = File.Create(str))
                            {
                                var buffer = new byte[2048];
                                while (true)
                                {
                                    var count = zipInputStream.Read(buffer, 0, buffer.Length);
                                    if (count > 0)
                                        fileStream.Write(buffer, 0, count);
                                    else
                                        break;
                                }
                            }

                            new FileInfo(str).CreationTime = nextEntry.DateTime;
                        }
                }

                unzipResult.Success = true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.WithContext("ZipService UnZip failed with an exception", ex);
                unzipResult.Success = false;
            }

            return unzipResult;
        }

        private UnzipResult OnUnzip(string zipFile, string outputPath)
        {
            var unzipResult = new UnzipResult
            {
                OutputPath = outputPath,
                SourceZip = zipFile,
                Success = false
            };
            if (!SafeCreateDirectory(outputPath))
                return unzipResult;
            var num = 0;
            using (var zipFile1 = new ZipFile(zipFile))
            {
                foreach (ZipEntry entry in zipFile1)
                {
                    ++num;
                    if (entry.IsDirectory)
                    {
                        if (!SafeCreateDirectory(entry.Name))
                            return unzipResult;
                    }
                    else
                    {
                        var buffer = new byte[4096];
                        var inputStream = zipFile1.GetInputStream(entry);
                        var path = Path.Combine(outputPath, entry.Name);
                        var directoryName = Path.GetDirectoryName(path);
                        if (directoryName.Length > 0 && !SafeCreateDirectory(directoryName))
                            return unzipResult;
                        using (var destination = File.Create(path))
                        {
                            StreamUtils.Copy(inputStream, destination, buffer);
                        }
                    }
                }

                unzipResult.EntryCount = num;
                unzipResult.Success = num == zipFile1.Count;
                return unzipResult;
            }
        }

        private bool SafeCreateDirectory(string directory)
        {
            if (Directory.Exists(directory))
                return true;
            try
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}