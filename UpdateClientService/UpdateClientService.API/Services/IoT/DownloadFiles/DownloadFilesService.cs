using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.IoT.DownloadFiles
{
    public class DownloadFilesService : IDownloadFilesService
    {
        private readonly IPersistentDataCacheService _cache;
        private readonly ICommandLineService _cmdService;
        private readonly IDownloadService _downloadService;
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly ILogger<DownloadFilesService> _logger;
        private readonly TimeSpan _proxiedS3UrlRequestTimeout = TimeSpan.FromSeconds(60.0);
        private readonly AppSettings _settings;
        private readonly IStoreService _store;

        public DownloadFilesService(
            ILogger<DownloadFilesService> logger,
            IStoreService store,
            IPersistentDataCacheService cache,
            IIoTCommandClient iotCommandClient,
            IDownloadService downloadService,
            ICommandLineService cmdService,
            IOptionsMonitor<AppSettings> settings)
        {
            _logger = logger;
            _store = store;
            _cache = cache;
            _iotCommandClient = iotCommandClient;
            _downloadService = downloadService;
            _cmdService = cmdService;
            _settings = settings.CurrentValue;
        }

        public async Task HandleDownloadFileJob(DownloadFileJob job)
        {
            var execution = new DownloadFileJobExecution
            {
                DownloadFileJobId = job.DownloadFileJobId,
                KioskId = _store.KioskId,
                Status = JobStatus.Scheduled
            };
            var persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json", true,
                UpdateClientServiceConstants.DownloadDataFolder);
            if (persistentDataWrapper.Data == null)
                persistentDataWrapper.Data = new DownloadFileJobList();
            var jobList = persistentDataWrapper.Data.JobList;
            if ((jobList != null ? jobList.Count(j => j.DownloadFileJobId == job.DownloadFileJobId) == 0 ? 1 : 0 : 0) !=
                0)
            {
                persistentDataWrapper.Data.JobList.Add(job);
                var num = await _cache.Write(persistentDataWrapper.Data, "downloadFileList.json",
                    UpdateClientServiceConstants.DownloadDataFolder)
                    ? 1
                    : 0;
            }

            var startDateUtc = job.StartDateUtc;
            if (job.StartDateUtc == new DateTimeOffset() || job.StartDateUtc.Add(job.StartTime) < DateTimeOffset.Now)
                await ExecuteDownloadFileJob(job);
            var tcommandResponse = await SendDownloadFileJobExecutionStatusUpdate(execution);
        }

        public async Task CancelDownloadFileJob(DownloadFileJob job)
        {
            _logger.LogInfoWithSource("Canceling job -> " + job.ToJson(),
                "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
            var execution = new DownloadFileJobExecution
            {
                DownloadFileJobId = job.DownloadFileJobId,
                KioskId = _store.KioskId,
                Status = JobStatus.CancelRequested
            };
            var tcommandResponse1 = await SendDownloadFileJobExecutionStatusUpdate(execution);
            foreach (var download in await _downloadService.GetDownloads(job.BitsJobId.ToString()))
                try
                {
                    var downloadResponse = await _downloadService.CancelDownload(download.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "An error occurred while canceling " + download.Key,
                        "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
                }

            try
            {
                var num = await _cache.DeleteLike(job.BitsJobId.ToString(),
                    UpdateClientServiceConstants.DownloadDataFolder)
                    ? 1
                    : 0;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("An error occurred while deleting jobs like {0}", job.BitsJobId),
                    "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
            }

            execution.Status = JobStatus.Canceled;
            var tcommandResponse2 = await SendDownloadFileJobExecutionStatusUpdate(execution);
        }

        public async Task<DownloadDataList> GetDownloadFileJobStatus(string bitsJobId)
        {
            return await _downloadService.GetDownloads(bitsJobId);
        }

        public async Task HandleScheduledJobs()
        {
            try
            {
                var persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json",
                    filePath: UpdateClientServiceConstants.DownloadDataFolder);
                if (persistentDataWrapper.Data == null)
                    persistentDataWrapper.Data = new DownloadFileJobList();
                var data = persistentDataWrapper.Data;
                var jobList1 = persistentDataWrapper.Data.JobList;
                var list = jobList1 != null ? jobList1.Distinct(new DownloadFileJobComparer()).ToList() : null;
                data.JobList = list;
                var logger = _logger;
                var jobList2 = persistentDataWrapper.Data.JobList;
                var str = "Handling scheduled jobs, " + ((jobList2 != null ? jobList2.ToJson() : null) ?? "null");
                _logger.LogInfoWithSource(str,
                    "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
                foreach (var job in persistentDataWrapper.Data.JobList)
                    if (job.StartDateUtc.Add(job.StartTime) < DateTimeOffset.Now)
                        await ExecuteDownloadFileJob(job);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while handling schedule jobs",
                    "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
            }
        }

        private async Task<IoTCommandResponse<ObjectResult>> SendDownloadFileJobExecutionStatusUpdate(
            DownloadFileJobExecution execution)
        {
            return await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
            {
                RequestId = Guid.NewGuid().ToString(),
                Version = 2,
                SourceId = _store.KioskId.ToString(),
                Payload = execution,
                Command = CommandEnum.DownloadFileJobExecutionStatusUpdate
            }, new PerformIoTCommandParameters
            {
                RequestTimeout = TimeSpan.FromSeconds(15.0),
                IoTTopic = "$aws/rules/kioskrestcall",
                WaitForResponse = true
            });
        }

        public async Task ExecuteDownloadFileJob(DownloadFileJob job)
        {
            var downloads = await _downloadService.GetDownloads(job.BitsJobId.ToString());
            if (downloads.Count == 0)
            {
                await AddDownloads(job);
            }
            else
            {
                if (!await HandleExistingDownloads(job, downloads))
                    return;
                await CleanupCache(job);
            }
        }

        private async Task CleanupCache(DownloadFileJob job)
        {
            var persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json",
                filePath: UpdateClientServiceConstants.DownloadDataFolder);
            persistentDataWrapper.Data.JobList.RemoveAll(j => j.DownloadFileJobId == job.DownloadFileJobId);
            var num1 = await _cache.Write(persistentDataWrapper.Data, "downloadFileList.json",
                UpdateClientServiceConstants.DownloadDataFolder)
                ? 1
                : 0;
            var num2 = await _cache.DeleteLike(job.BitsJobId.ToString(),
                UpdateClientServiceConstants.DownloadDataFolder)
                ? 1
                : 0;
        }

        private async Task AddDownloads(DownloadFileJob job)
        {
            var execution = new DownloadFileJobExecution
            {
                DownloadFileJobId = job.DownloadFileJobId,
                KioskId = _store.KioskId,
                EventTime = DateTime.Now,
                Status = JobStatus.InProgress
            };
            try
            {
                if (string.IsNullOrWhiteSpace(job.Metadata?.FileName) &&
                    string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName))
                {
                    _logger.LogErrorWithSource("FileName and ActivationScriptName cannot both be null",
                        "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
                    execution.CompletedOn = DateTime.Now;
                    execution.Status = JobStatus.Error;
                    await CleanupCache(job);
                    var tcommandResponse = await SendDownloadFileJobExecutionStatusUpdate(execution);
                    return;
                }

                SetProxiedUrls(job);
                if (!string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName) &&
                    !string.IsNullOrWhiteSpace(job.PresignedScriptUrl))
                {
                    var num1 = await _downloadService.AddDownload(DownloadKey("SCRIPT", job), null,
                        job.PresignedScriptUrl, DownloadPriority.Normal)
                        ? 1
                        : 0;
                }

                if (!string.IsNullOrWhiteSpace(job.Metadata?.FileName) &&
                    !string.IsNullOrWhiteSpace(job.PresignedFileUrl))
                {
                    var num2 = await _downloadService.AddDownload(DownloadKey("FILE", job), null, job.PresignedFileUrl,
                        DownloadPriority.Normal)
                        ? 1
                        : 0;
                }

                var tcommandResponse1 = await SendDownloadFileJobExecutionStatusUpdate(execution);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "An error occurred while executing the download file job.",
                    "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
                execution.CompletedOn = DateTime.Now;
                execution.Status = JobStatus.Error;
                var tcommandResponse = await SendDownloadFileJobExecutionStatusUpdate(execution);
            }
        }

        private async Task<bool> HandleExistingDownloads(
            DownloadFileJob job,
            DownloadDataList downloads)
        {
            var execution = new DownloadFileJobExecution
            {
                DownloadFileJobId = job.DownloadFileJobId,
                KioskId = _store.KioskId,
                EventTime = DateTime.Now,
                Status = JobStatus.InProgress
            };
            var ioTcommandModel = new IoTCommandModel
            {
                Command = CommandEnum.DownloadFileJobExecutionStatusUpdate,
                Payload = new MqttResponse<DownloadFileJobExecution>
                {
                    Data = execution
                }
            };
            try
            {
                if (downloads.All(d => d.DownloadState == DownloadState.Downloading))
                    return false;
                if (downloads.Any(d => d.DownloadState == DownloadState.Error))
                    execution.Status = JobStatus.Error;
                if (downloads.All(d => d.DownloadState == DownloadState.PostDownload))
                {
                    foreach (var download in downloads)
                        if (execution.Status != JobStatus.Error)
                        {
                            if (download.Key == DownloadKey("FILE", job) &&
                                !string.IsNullOrWhiteSpace(job.Metadata?.FileName))
                                SaveFileToDestination(job, download);
                            if (download.Key == DownloadKey("SCRIPT", job) &&
                                !string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName))
                            {
                                var metadata = job.Metadata;
                                int num;
                                if (metadata == null)
                                {
                                    num = 0;
                                }
                                else
                                {
                                    var activationScriptName = metadata.ActivationScriptName;
                                    var nullable = activationScriptName != null
                                        ? !activationScriptName.EndsWith(".ps1")
                                        : new bool?();
                                    var flag = true;
                                    num = (nullable.GetValueOrDefault() == flag) & nullable.HasValue ? 1 : 0;
                                }

                                if (num != 0)
                                {
                                    _logger.LogErrorWithSource(
                                        "Script " + (job.Metadata.ActivationScriptName ?? "null") +
                                        " does not have a valid PowerShell extension of '.ps1'",
                                        "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
                                    execution.Status = JobStatus.Error;
                                }
                                else
                                {
                                    execution.Status = TryDownloadAndExecuteScript(job, download)
                                        ? execution.Status
                                        : JobStatus.Error;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }

                    if (execution.Status != JobStatus.Error)
                        execution.Status = JobStatus.Complete;
                    execution.CompletedOn = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "An error occurred while executing the download file job.",
                    "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesService.cs");
                execution.CompletedOn = DateTime.Now;
                execution.Status = JobStatus.Error;
            }

            var isComplete = execution.Status == JobStatus.Complete || execution.Status == JobStatus.Error;
            if (isComplete)
            {
                var tcommandResponse = await SendDownloadFileJobExecutionStatusUpdate(execution);
            }

            return isComplete;
        }

        private void SetProxiedUrls(DownloadFileJob job)
        {
            if (!string.IsNullOrWhiteSpace(job.Metadata?.FileName))
                job.PresignedFileUrl = new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl,
                    "api/downloads/s3/proxy", job.FileKey, DownloadPathType.DownloadFile)).ToString();
            if (string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName))
                return;
            job.PresignedScriptUrl = new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl,
                "api/downloads/s3/proxy", job.FileKey, DownloadPathType.ActivationScript)).ToString();
        }

        private void SaveFileToDestination(DownloadFileJob job, DownloadData download)
        {
            var destFileName = Path.Combine(job.Metadata.DestinationPath, job.Metadata.FileName);
            Directory.CreateDirectory(job.Metadata.DestinationPath);
            File.Copy(download.Path, destFileName);
        }

        private bool TryDownloadAndExecuteScript(DownloadFileJob job, DownloadData download)
        {
            var str = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ps1");
            Directory.CreateDirectory(Path.GetTempPath());
            File.Copy(download.Path, str);
            var flag = !_cmdService.TryExecutePowerShellScriptFromFile(str);
            File.Delete(str);
            return flag;
        }

        private string DownloadKey(string prefix, DownloadFileJob job)
        {
            return string.Format("{0}.{1}", prefix, job.BitsJobId);
        }
    }
}