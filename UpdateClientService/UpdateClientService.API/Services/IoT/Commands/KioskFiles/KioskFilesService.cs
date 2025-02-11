using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.Files;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.IoT.Commands.KioskFiles
{
    public class KioskFilesService : IKioskFilesService
    {
        private readonly IFileService _fileService;
        private readonly IIoTProcessStatusService _ioTProcessStatusService;
        private readonly ILogger<KioskFilesService> _logger;

        public KioskFilesService(
            ILogger<KioskFilesService> logger,
            IFileService fileService,
            IIoTProcessStatusService ioTProcessStatusService,
            IIoTCommandClient iotCommandClient)
        {
            _logger = logger;
            _fileService = fileService;
            _ioTProcessStatusService = ioTProcessStatusService;
        }

        public async Task<MqttResponse<Dictionary<string, string>>> PeekRequestedFilesAsync(
            KioskFilePeekRequest logRequestModel)
        {
            var mqttResponse1 = new MqttResponse<Dictionary<string, string>>
            {
                Data = new Dictionary<string, string>()
            };
            var fileInfoList = new List<FileInfo>();
            try
            {
                List<FileInfo> files;
                if (!string.IsNullOrWhiteSpace(logRequestModel.FileQuery?.Path))
                {
                    var mqttResponse2 = mqttResponse1;
                    (files, mqttResponse2.Error) = await GetFilesByQuery(logRequestModel);
                    mqttResponse2 = null;
                }
                else
                {
                    var mqttResponse = mqttResponse1;
                    var filesByFileType = GetFilesByFileType(logRequestModel);
                    files = filesByFileType.files;
                    string error;
                    var str = error = filesByFileType.error;
                    mqttResponse.Error = error;
                }

                if (files != null && files.Count > 0)
                    FilterFilesByDate(ref files, logRequestModel.Date, logRequestModel.EndDate);
                files?.ForEach(file =>
                {
                    var str = file.LastWriteTime.ToString();
                    if (!string.IsNullOrWhiteSpace(logRequestModel.FileQuery?.Path))
                    {
                        var fileQuery = logRequestModel.FileQuery;
                        int num;
                        if (fileQuery == null)
                        {
                            num = 0;
                        }
                        else
                        {
                            var recurseSubdirectories = fileQuery.SearchOptions == SearchOption.AllDirectories;
                            var flag = true;
                            num = recurseSubdirectories == flag ? 1 : 0;
                        }

                        if (num != 0)
                        {
                            mqttResponse1.Data.Add(
                                PathHelpers.GetRelativePath(logRequestModel.FileQuery.Path, file.FullName), str);
                            return;
                        }
                    }

                    mqttResponse1.Data.Add(file.Name, str);
                });
                if (!mqttResponse1.Data.Any())
                    mqttResponse1.Error = "There were no files found for given request. Request -> " +
                                          logRequestModel.ToJson() + ", Additional Errors -> " + mqttResponse1.Error;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while gettings FileInfos",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
                mqttResponse1.Error = ex.Message +
                                      ". Logged full exception in UpdateClientService logs, take a look at it if you want to see the full exception details.";
                return mqttResponse1;
            }

            _logger.LogInfoWithSource(string.Format("Returning ${0} files.", mqttResponse1.Data.Count),
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
            return mqttResponse1;
        }

        public async Task<MqttResponse<string>> UploadFilesAsync(KioskUploadFileRequest kioskFileRequest)
        {
            if (_ioTProcessStatusService.UploadFilesStatus == IoTProcessStatusEnum.InProgress)
            {
                _logger.LogWarningWithSource(
                    "There was another upload files request while uploading files for another request.",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
                return new MqttResponse<string>
                {
                    Error =
                        "UpdateClientService: Already uploading files for another request, please try again in 1 or 2 minutes."
                };
            }

            if (kioskFileRequest.FileNames.Count > 30)
                return new MqttResponse<string>
                {
                    Error =
                        "UpdateClientService: Can't upload more than 30 files at one time. Contact Kiosk Devs for questions."
                };
            var mqttResponse = new MqttResponse<string>
            {
                Data = kioskFileRequest.S3PreSignedUrl
            };
            string path;
            if (!string.IsNullOrWhiteSpace(kioskFileRequest?.BasePath))
            {
                path = kioskFileRequest.BasePath;
            }
            else
            {
                string error;
                if (!TryGetRequestedLogPath(kioskFileRequest.FileType, out path, out error))
                {
                    _logger.LogErrorWithSource(
                        "An error occurred while trying to upload requested file to S3. Request Model -> " +
                        kioskFileRequest.ToJson() + ". Error -> " + error,
                        "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
                    mqttResponse.Error =
                        "An error occurred while trying to upload requested file to S3. Take a look at UCS logs for full exception details. Error -> " +
                        error;
                    _ioTProcessStatusService.UploadFilesStatus = IoTProcessStatusEnum.Errored;
                    return mqttResponse;
                }
            }

            try
            {
                _ioTProcessStatusService.UploadFilesStatus = IoTProcessStatusEnum.InProgress;
                if (kioskFileRequest.FileNames.Count > 1 || kioskFileRequest.ZipFiles)
                {
                    var fileToUpload = new List<FileInfo>();
                    foreach (var fileName1 in kioskFileRequest.FileNames)
                    {
                        var fileName2 = Path.Combine(path, fileName1);
                        fileToUpload.Add(new FileInfo(fileName2));
                    }

                    await _fileService.ZipAndUploadToS3(fileToUpload, kioskFileRequest.S3PreSignedUrl);
                }
                else
                {
                    var filePath = Path.Combine(path, kioskFileRequest.FileNames[0]);
                    await _fileService.UploadFileToS3(kioskFileRequest.S3PreSignedUrl, filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "An error occurred while trying to upload requested file to S3. Request Model -> " +
                    kioskFileRequest.ToJson(),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
                mqttResponse.Error =
                    "An error occurred while trying to upload requested file to S3. Take a look at UCS logs for full exception details. Error -> " +
                    ex.Message;
                _ioTProcessStatusService.UploadFilesStatus = IoTProcessStatusEnum.Errored;
            }
            finally
            {
                _ioTProcessStatusService.UploadFilesStatus =
                    _ioTProcessStatusService.UploadFilesStatus == IoTProcessStatusEnum.Errored
                        ? IoTProcessStatusEnum.Errored
                        : IoTProcessStatusEnum.Finished;
            }

            _logger.LogInfoWithSource("Returning mqttresponse: " + mqttResponse.ToJson(),
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
            return mqttResponse;
        }

        private (List<FileInfo> files, string error) GetFilesByFileType(
            KioskFilePeekRequest logRequestModel)
        {
            var fileInfoList = new List<FileInfo>();
            string error;
            try
            {
                string path;
                if (!TryGetRequestedLogPath(logRequestModel.FileType, out path, out error))
                    return (null,
                        "Couldn't get path of " + (logRequestModel.FileType ?? "null") + ". Error -> " + error);
                var files = new DirectoryInfo(path).GetFiles();
                fileInfoList = (files != null ? files.ToList() : null) ?? new List<FileInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting FileInfos",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
                error = ex.Message;
            }

            return (fileInfoList, error);
        }

        private async Task<(List<FileInfo> files, string error)> GetFilesByQuery(
            KioskFilePeekRequest logRequestModel,
            TimeSpan? timeout = null)
        {
            return await Task.Run((Func<(List<FileInfo>, string)>)(() =>
                {
                    var fileInfoList = new List<FileInfo>();
                    string str = null;
                    try
                    {
                        var fileQuery = logRequestModel.FileQuery;
                        fileInfoList = new DirectoryInfo(fileQuery.Path).GetFiles(
                            !string.IsNullOrWhiteSpace(fileQuery.SearchPattern) ? fileQuery.SearchPattern : "*",
                            fileQuery.SearchOptions
                        ).ToList() ?? new List<FileInfo>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex, "Exception while getting FileInfos",
                            "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
                        str = ex.Message;
                    }

                    return (fileInfoList, str);
                }),
                (timeout.HasValue
                    ? new CancellationTokenSource(timeout.Value)
                    : new CancellationTokenSource(TimeSpan.FromSeconds(90.0))).Token);
        }

        private static void FilterFilesByDate(
            ref List<FileInfo> files,
            DateTime? date,
            DateTime? endDate)
        {
            if (files == null || files.Count == 0)
                return;
            if (!date.Equals(null) && !endDate.Equals(null))
                files = files.Where(x =>
                {
                    var lastWriteTime = x.LastWriteTime;
                    var nullable = date;
                    return (nullable.HasValue ? lastWriteTime >= nullable.GetValueOrDefault() ? 1 : 0 : 0) != 0 &&
                           x.LastWriteTime <= endDate.Value.AddDays(1.0);
                }).ToList();
            else if (!date.Equals(null))
                files = files.Where(x =>
                {
                    var dateTime = x.LastWriteTime;
                    var shortDateString = dateTime.ToShortDateString();
                    ref var local = ref date;
                    string str;
                    if (!local.HasValue)
                    {
                        str = null;
                    }
                    else
                    {
                        dateTime = local.GetValueOrDefault();
                        str = dateTime.ToShortDateString();
                    }

                    return shortDateString == str;
                }).ToList();
            else
                files = files.ToList();
        }

        private bool TryGetRequestedLogPath(string logType, out string path, out string error)
        {
            path = null;
            error = null;
            FileTypeEnum result;
            if (Enum.TryParse(logType, out result) && FilePaths.TypeMappings.TryGetValue(result, out path) &&
                !string.IsNullOrWhiteSpace(path))
                return true;
            error = (logType ?? "null") + " was not found in FilePaths";
            _logger.LogErrorWithSource(error,
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/KioskFiles/KioskFilesService.cs");
            return false;
        }
    }
}