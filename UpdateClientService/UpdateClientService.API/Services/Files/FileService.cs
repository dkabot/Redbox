using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Files
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public async Task ZipAndUploadToS3(List<FileInfo> fileToUpload, string presignedS3)
        {
            var num = new Random().Next();
            var zipPath = Path.GetTempPath() + string.Format("{0}.zip", num);
            _logger.LogInfoWithSource("Created temporary zip path: " + zipPath,
                "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
            var exception = (Exception)null;
            try
            {
                using (var zipTo = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var fileInfo in fileToUpload)
                    {
                        _logger.LogInfoWithSource("Adding " + fileInfo.Name + " into " + zipPath + " zip file.",
                            "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
                        if (fileInfo.Length > 30000000L)
                            throw new Exception(string.Format(
                                "File: {0} size is {1} byte, exceeds 30 mb limit. Can't zip file that is larger than 30mb. Contact kiosk team for questions.",
                                fileInfo.Name, fileInfo.Length));
                        var entry = zipTo.CreateEntry(fileInfo.Name);
                        using (var fileStream = File.Open(fileInfo.FullName, (FileMode)3, (FileAccess)1, (FileShare)3))
                        {
                            using (var streamTo = entry.Open())
                            {
                                await fileStream.CopyToAsync(streamTo);
                            }
                        }
                    }
                }

                await UploadFileToS3(presignedS3, zipPath);
            }
            catch (Exception ex)
            {
                exception = ex;
                _logger.LogErrorWithSource(ex, "Exception while creating zip file",
                    "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
            }
            finally
            {
                _logger.LogInfoWithSource("Deleting temporary zip file: " + zipPath,
                    "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
                File.Delete(zipPath);
            }

            if (exception != null)
                throw exception;
        }

        public async Task UploadFileToS3(string presignedS3, string filePath)
        {
            _logger.LogInfoWithSource("Uploading file: $" + filePath + " to given s3 presigned Url",
                "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
            var httpRequest = WebRequest.Create(presignedS3) as HttpWebRequest;
            httpRequest.Method = "PUT";
            using (var dataStream = httpRequest.GetRequestStream())
            {
                var buffer = new byte[8000];
                using (var fileStream = File.Open(filePath, (FileMode)3, (FileAccess)1, (FileShare)3))
                {
                    while (true)
                    {
                        int num;
                        if ((num = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            await dataStream.WriteAsync(buffer, 0, num);
                        else
                            break;
                    }
                }

                buffer = null;
            }

            httpRequest.GetResponse();
            _logger.LogInfoWithSource("Successfully uploaded " + filePath + " to the s3",
                "/sln/src/UpdateClientService.API/Services/Files/FileService.cs");
            httpRequest = null;
        }
    }
}