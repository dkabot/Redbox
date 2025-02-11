using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.IoT.FileSets;
using UpdateClientService.API.Services.Kernel;
using UpdateClientService.API.Services.KioskEngine;

namespace UpdateClientService.API.Services.FileSets
{
    public class FileSetService : IFileSetService
    {
        private static readonly string _root = Constants.FileSetsRoot;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan _processLockTimeout = TimeSpan.FromSeconds(20.0);
        private static DateTime _lastCallToProcessPendingFileSetVersions = DateTime.MinValue;
        private static readonly SemaphoreSlim _lockPendingFileSetVersions = new SemaphoreSlim(1, 1);
        private readonly IChangeSetFileService _changeSetFileService;
        private readonly IDownloader _downloader;
        private readonly IDownloadService _downloadService;
        private readonly IFileCacheService _fileCacheService;
        private readonly IFileSetTransition _fileSetTransition;
        private readonly IKernelService _kernelService;
        private readonly IKioskEngineService _kioskEngineService;
        private readonly ILogger<FileSetService> _logger;
        private readonly IOptionsMonitorKioskConfiguration _optionsMonitorKioskConfiguration;
        private readonly int _pendingFileSetVersionsLockTimeout = 5000;
        private readonly FileSetSettings _settings;
        private readonly IStateFileService _stateFileService;
        private readonly IKioskFileSetVersionsService _versionsService;

        public FileSetService(
            IFileCacheService fileCacheService,
            IDownloadService downloadService,
            IDownloader downloader,
            IChangeSetFileService changeSetFileService,
            ILogger<FileSetService> logger,
            IKioskFileSetVersionsService versionsService,
            IKernelService kernelService,
            IKioskEngineService kioskEngineService,
            IStateFileService stateFileService,
            IOptionsMonitor<AppSettings> settings,
            IFileSetTransition fileSetTransition,
            IOptionsMonitorKioskConfiguration optionsKioskConfiguration)
        {
            _fileCacheService = fileCacheService;
            _downloadService = downloadService;
            _downloader = downloader;
            _changeSetFileService = changeSetFileService;
            _logger = logger;
            _versionsService = versionsService;
            _kernelService = kernelService;
            _kioskEngineService = kioskEngineService;
            _stateFileService = stateFileService;
            _settings = settings.CurrentValue.FileSet;
            _fileSetTransition = fileSetTransition;
            _optionsMonitorKioskConfiguration = optionsKioskConfiguration;
            Initialize();
        }

        public async Task<ProcessChangeSetResponse> ProcessChangeSet(
            ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
        {
            var fileSetService = this;
            try
            {
                if (clientFileSetRevisionChangeSet == null)
                {
                    _logger.LogErrorWithSource("Changeset is null.",
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                    var changeSetResponse = new ProcessChangeSetResponse();
                    changeSetResponse.StatusCode = HttpStatusCode.InternalServerError;
                    return changeSetResponse;
                }

                var logger1 = fileSetService._logger;
                var revisionChangeSetKey1 = clientFileSetRevisionChangeSet;
                var str1 = "Starting processing for " +
                           (revisionChangeSetKey1 != null ? revisionChangeSetKey1.IdentifyingText() : null);
                _logger.LogInfoWithSource(str1, "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                if (!fileSetService._optionsMonitorKioskConfiguration.FileSets.FileSetDownloadEnabled)
                {
                    var logger2 = fileSetService._logger;
                    var revisionChangeSetKey2 = clientFileSetRevisionChangeSet;
                    var str2 =
                        "FileSet downloads are disabled by config setting FileSets.FileSetDownloadEnabled.  Aborting FileSet download for " +
                        (revisionChangeSetKey2 != null ? revisionChangeSetKey2.IdentifyingText() : null) + " ";
                    _logger.LogWarningWithSource(str2,
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                    var changeSetResponse = new ProcessChangeSetResponse();
                    changeSetResponse.StatusCode = HttpStatusCode.ServiceUnavailable;
                    return changeSetResponse;
                }

                try
                {
                    if (!await _lock.WaitAsync(_processLockTimeout))
                    {
                        var logger3 = fileSetService._logger;
                        var revisionChangeSetKey3 = clientFileSetRevisionChangeSet;
                        var str3 = "Unable to aquire lock for processing " + (revisionChangeSetKey3 != null
                            ? revisionChangeSetKey3.IdentifyingText()
                            : null) + ".";
                        _logger.LogWarningWithSource(str3,
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                        var changeSetResponse = new ProcessChangeSetResponse();
                        changeSetResponse.StatusCode = HttpStatusCode.ServiceUnavailable;
                        return changeSetResponse;
                    }
                }
                catch (Exception ex)
                {
                    var logger4 = fileSetService._logger;
                    var exception = ex;
                    var revisionChangeSetKey4 = clientFileSetRevisionChangeSet;
                    var str4 = "Exception while trying to aquire lock for processing " +
                               (revisionChangeSetKey4 != null ? revisionChangeSetKey4.IdentifyingText() : null) + ".";
                    _logger.LogErrorWithSource(exception, str4,
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                    var changeSetResponse = new ProcessChangeSetResponse();
                    changeSetResponse.StatusCode = HttpStatusCode.InternalServerError;
                    return changeSetResponse;
                }

                try
                {
                    var flag = await fileSetService.StartRevisionChangeSetProcessing(
                        new List<ClientFileSetRevisionChangeSet>
                        {
                            clientFileSetRevisionChangeSet
                        });
                    FileSetPollRequest fileSetPollRequest;
                    if (fileSetService.CreateFileSetPollRequest(clientFileSetRevisionChangeSet,
                            !flag ? FileSetState.Error : FileSetState.InProgress, out fileSetPollRequest))
                    {
                        var versionsResponse =
                            await fileSetService._versionsService.ReportFileSetVersion(fileSetPollRequest);
                    }
                }
                finally
                {
                    var semaphoreSlim = _lock;
                    if ((semaphoreSlim != null ? semaphoreSlim.CurrentCount == 0 ? 1 : 0 : 0) != 0)
                        _lock.Release();
                }
            }
            catch (Exception ex)
            {
                var logger = fileSetService._logger;
                if (logger != null)
                {
                    var exception = ex;
                    var revisionChangeSetKey = clientFileSetRevisionChangeSet;
                    var str = "Exception while processing changeset " +
                              (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                    _logger.LogErrorWithSource(exception, str,
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                }

                var changeSetResponse = new ProcessChangeSetResponse();
                changeSetResponse.StatusCode = HttpStatusCode.InternalServerError;
                return changeSetResponse;
            }

            var changeSetResponse1 = new ProcessChangeSetResponse();
            changeSetResponse1.StatusCode = HttpStatusCode.OK;
            return changeSetResponse1;
        }

        public async Task ProcessInProgressRevisionChangeSets()
        {
            try
            {
                var taskAwaiter = _lock.WaitAsync(_processLockTimeout).GetAwaiter();
                if (!taskAwaiter.IsCompleted)
                {
                    TaskAwaiter<bool> taskAwaiter2;
                    taskAwaiter = taskAwaiter2;
                    taskAwaiter2 = default;
                }

                if (!taskAwaiter.GetResult())
                {
                    _logger.LogWarningWithSource("Unable to aquire lock.",
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                }
                else
                {
                    var revisionChangeSetsProcessingData = new RevisionChangeSetsProcessingData();
                    try
                    {
                        await GetInProgressStateFiles(revisionChangeSetsProcessingData);
                        await GetRevisionChangeSets(revisionChangeSetsProcessingData);
                        await DeleteBadRevisionChangeSets(revisionChangeSetsProcessingData);
                        await ProcessChangeSets(revisionChangeSetsProcessingData);
                        await ProcessNeedsDependencyStates(revisionChangeSetsProcessingData);
                        await ProcessActivationPendingStates(revisionChangeSetsProcessingData);
                        await ProcessBeforeActivationActions(revisionChangeSetsProcessingData);
                        await ProcessActivating(revisionChangeSetsProcessingData);
                        await ProcessAfterActivationActions(revisionChangeSetsProcessingData);
                        await ProcessActivatedStates(revisionChangeSetsProcessingData);
                        await ProcessErrorStates(revisionChangeSetsProcessingData);
                        await RebootIfNeeded(revisionChangeSetsProcessingData);
                        ProcessPostDownloadStates(revisionChangeSetsProcessingData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex, "Exception while processing in progress RevisionChangeSets",
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    revisionChangeSetsProcessingData = null;
                }
            }
            catch (Exception ex2)
            {
                _logger.LogErrorWithSource(ex2, "Exception while processing in progress RevisionChangeSets.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
            }
        }

        public async Task<ReportFileSetVersionsResponse> ProcessPendingFileSetVersions()
        {
            try
            {
                await SetLastCallToProcessPendingFileSetVersions();
                var result = await _versionsService.ReportFileSetVersions();
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var revisionChangeSets = result.ClientFileSetRevisionChangeSets;
                    if ((revisionChangeSets != null ? revisionChangeSets.Any() ? 1 : 0 : 0) != 0)
                        foreach (var revisionChangeSet in result.ClientFileSetRevisionChangeSets)
                        {
                            var changeSetResponse = await ProcessChangeSet(revisionChangeSet);
                        }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while processing pending FileSet versions",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                var versionsResponse = new ReportFileSetVersionsResponse();
                versionsResponse.StatusCode = HttpStatusCode.InternalServerError;
                return versionsResponse;
            }
        }

        public async Task<ReportFileSetVersionsResponse> TriggerProcessPendingFileSetVersions(
            TriggerReportFileSetVersionsRequest triggerReportFileSetVersionsRequest)
        {
            var response = new ReportFileSetVersionsResponse();
            try
            {
                if (triggerReportFileSetVersionsRequest != null)
                {
                    if (triggerReportFileSetVersionsRequest.ExecutionTimeFrameMs.HasValue)
                    {
                        var executionTimeFrameMs = triggerReportFileSetVersionsRequest.ExecutionTimeFrameMs;
                        long num = 0;
                        if (!((executionTimeFrameMs.GetValueOrDefault() == num) & executionTimeFrameMs.HasValue))
                        {
                            await Task.Run(async () =>
                            {
                                var randomMs =
                                    new Random().Next((int)triggerReportFileSetVersionsRequest.ExecutionTimeFrameMs
                                        .Value);
                                _logger.LogInfoWithSource(
                                    string.Format("waiting {0} ms before calling ReportFileSetVersions", randomMs),
                                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                                var lastCall = await GetLastCallToProcessPendingFileSetVersion();
                                await Task.Delay(randomMs);
                                var dateTime = lastCall;
                                if (dateTime != await GetLastCallToProcessPendingFileSetVersion())
                                    return;
                                var versionsResponse = await ProcessPendingFileSetVersions();
                            });
                            return response;
                        }
                    }

                    response = await ProcessPendingFileSetVersions();
                }
                else
                {
                    _logger.LogErrorWithSource("parameter TriggerReportFileSetVersionsRequest must not be null.",
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                    response.StatusCode = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception triggering ProcessPendingFileSetVersions.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public bool Initialize()
        {
            try
            {
                Directory.CreateDirectory(_root);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while initializing FileSetService",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                return false;
            }
        }

        private async Task<bool> StartRevisionChangeSetProcessing(
            IEnumerable<ClientFileSetRevisionChangeSet> clientFileSetRevisionChangeSets)
        {
            var result = true;
            try
            {
                foreach (var eachClientFileSetRevisionChangeSet in clientFileSetRevisionChangeSets)
                    if (eachClientFileSetRevisionChangeSet.Action == FileSetAction.Delete)
                    {
                        var logger = _logger;
                        var revisionChangeSetKey = eachClientFileSetRevisionChangeSet;
                        var str = "Deleting " +
                                  (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                        _logger.LogInfoWithSource(str,
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                        var flag = result;
                        if (flag)
                            flag = await _changeSetFileService.Delete(
                                RevisionChangeSet.Create(eachClientFileSetRevisionChangeSet));
                        result = flag;
                        if (!await _stateFileService.Delete(eachClientFileSetRevisionChangeSet.FileSetId))
                        {
                            result = false;
                            _logger.LogErrorWithSource(
                                string.Format("Unable to delete StateFile for FileSetId {0}",
                                    eachClientFileSetRevisionChangeSet.FileSetId),
                                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                        }
                    }
                    else if (eachClientFileSetRevisionChangeSet.Action == FileSetAction.Update)
                    {
                        var flag = result;
                        if (flag)
                            flag = await StartRevisionChangeSetProcessing(eachClientFileSetRevisionChangeSet);
                        result = flag;
                    }
            }
            catch (Exception ex)
            {
                result = false;
                _logger.LogErrorWithSource(ex, "Exception while starting RevisionChangeSet processing",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
            }

            return result;
        }

        private async Task<bool> StartRevisionChangeSetProcessing(
            ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
        {
            var result = false;
            try
            {
                var logger = _logger;
                var revisionChangeSetKey = clientFileSetRevisionChangeSet;
                var str = "Starting processing for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogInfoWithSource(str, "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                if (await _changeSetFileService.CreateRevisionChangeSet(clientFileSetRevisionChangeSet))
                {
                    if (await CreateStateFileFromChangeSet(clientFileSetRevisionChangeSet))
                        result = true;
                    else
                        _logger.LogErrorWithSource(
                            string.Format("Unable to create StateFile for FileSetId {0}",
                                clientFileSetRevisionChangeSet?.FileSetId),
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while starting processing of ClientFileSetRevisionChangeSet",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
            }

            return result;
        }

        private async Task<bool> CreateStateFileFromChangeSet(
            ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
        {
            var stateFile = (await _stateFileService.Get(clientFileSetRevisionChangeSet.FileSetId))?.StateFile ??
                            new StateFile(clientFileSetRevisionChangeSet.FileSetId, 0L,
                                clientFileSetRevisionChangeSet.RevisionId, FileSetState.InProgress);
            stateFile.InProgressRevisionId = clientFileSetRevisionChangeSet.RevisionId;
            stateFile.InProgressFileSetState = FileSetState.InProgress;
            var stateFileResponse = await _stateFileService.Save(stateFile);
            return stateFileResponse != null && stateFileResponse.StatusCode == HttpStatusCode.OK;
        }

        private void ProcessPostDownloadStates(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var downloadData in revisionChangeSetsProcessingData.AllDownloadData)
                if (downloadData.DownloadState == DownloadState.PostDownload)
                {
                    _logger.LogInfoWithSource("Completing leftover DownloadData " + downloadData.FileName,
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                    _downloader.Complete(downloadData);
                }
        }

        private async Task RebootIfNeeded(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete || !_fileSetTransition.RebootRequired)
                return;
            var num = await Reboot() ? 1 : 0;
        }

        private async Task ProcessErrorStates(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var eachRevisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets.Where(x =>
                         x.State == ChangesetState.Error))
            {
                var stateFile =
                    revisionChangeSetsProcessingData.GetStateFileForRevisionChangeSet(eachRevisionChangeSet);
                if (stateFile != null)
                {
                    if (stateFile.InProgressFileSetState != FileSetState.NeedsDependency)
                    {
                        stateFile.InProgressFileSetState = FileSetState.Error;
                        var stateFileResponse = await _stateFileService.Save(stateFile);
                    }

                    FileSetPollRequest fileSetPollRequest;
                    var flag = CreateFileSetPollRequest(eachRevisionChangeSet, stateFile.InProgressFileSetState,
                        out fileSetPollRequest);
                    if (flag)
                        flag = (await _versionsService.ReportFileSetVersion(fileSetPollRequest)).StatusCode ==
                               HttpStatusCode.OK;
                    if (flag)
                    {
                        var num1 = await _changeSetFileService.Delete(eachRevisionChangeSet) ? 1 : 0;
                        var num2 = await _stateFileService.DeleteInProgress(stateFile.FileSetId) ? 1 : 0;
                    }
                }

                stateFile = null;
            }
        }

        private async Task ProcessActivatedStates(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var eachRevisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets.Where(x =>
                         x.State == ChangesetState.Activated))
            {
                var revisionChangeSet =
                    revisionChangeSetsProcessingData.GetStateFileForRevisionChangeSet(eachRevisionChangeSet);
                if (revisionChangeSet != null)
                {
                    revisionChangeSet.InProgressFileSetState = FileSetState.Active;
                    var stateFileResponse = await _stateFileService.Save(revisionChangeSet);
                }

                var num = await _changeSetFileService.Delete(eachRevisionChangeSet) ? 1 : 0;
                FileSetPollRequest fileSetPollRequest;
                if (CreateFileSetPollRequest(eachRevisionChangeSet, FileSetState.Active, out fileSetPollRequest))
                {
                    var versionsResponse = await _versionsService.ReportFileSetVersion(fileSetPollRequest);
                }
            }
        }

        private async Task ProcessAfterActivationActions(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
                await _changeSetFileService.ProcessActivationAfterActions(revisionChangeSet);
        }

        private async Task ProcessActivating(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
                await _changeSetFileService.ProcessActivating(revisionChangeSet);
        }

        private async Task ProcessBeforeActivationActions(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
                await _changeSetFileService.ProcessActivationBeforeActions(revisionChangeSet);
        }

        private async Task ProcessActivationPendingStates(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            foreach (var revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
                await _changeSetFileService.ProcessActivationPending(revisionChangeSet);
        }

        private async Task ProcessNeedsDependencyStates(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            var dependencyStates = await GetDependencyStates();
            if (dependencyStates != null)
                foreach (var revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
                    await _changeSetFileService.ProcessActivationDependencyCheck(dependencyStates, revisionChangeSet);
            foreach (var revisionChangeSet1 in revisionChangeSetsProcessingData.RevisionChangeSets)
                if (revisionChangeSet1.State == ChangesetState.Error &&
                    revisionChangeSet1.Message == FileSetState.NeedsDependency.ToString())
                {
                    var revisionChangeSet2 =
                        revisionChangeSetsProcessingData.GetStateFileForRevisionChangeSet(revisionChangeSet1);
                    if (revisionChangeSet2 != null)
                    {
                        revisionChangeSet2.InProgressFileSetState = FileSetState.NeedsDependency;
                        var stateFileResponse = await _stateFileService.Save(revisionChangeSet2);
                    }
                }

            dependencyStates = null;
        }

        private async Task ProcessChangeSets(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            var setsProcessingData = revisionChangeSetsProcessingData;
            setsProcessingData.AllDownloadData =
                await _downloadService.GetDownloads(new Regex("^(?!(FILE|SCRIPT|downloadFileList.json)).*"));
            setsProcessingData = null;
            if (revisionChangeSetsProcessingData.RevisionChangeSets.Count() == 0 &&
                revisionChangeSetsProcessingData.AllDownloadData.Count() == 0)
                revisionChangeSetsProcessingData.IsComplete = true;
            else
                foreach (var revisionChangeSet in revisionChangeSetsProcessingData.RevisionChangeSets)
                    await _changeSetFileService.ProcessChangeSet(revisionChangeSetsProcessingData.AllDownloadData,
                        revisionChangeSet);
        }

        private async Task DeleteBadRevisionChangeSets(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            var list = revisionChangeSetsProcessingData.RevisionChangeSets.Where(revisionChangeSet =>
                !revisionChangeSetsProcessingData.InProgressStateFiles.Any(stateFile =>
                    stateFile.FileSetId == revisionChangeSet.FileSetId &&
                    stateFile.InProgressRevisionId == revisionChangeSet.RevisionId)).ToList();
            var badStateFiles = revisionChangeSetsProcessingData.InProgressStateFiles.Where(stateFile =>
                !revisionChangeSetsProcessingData.RevisionChangeSets.Any(revisionChangeSet =>
                    stateFile.FileSetId == revisionChangeSet.FileSetId &&
                    stateFile.InProgressRevisionId == revisionChangeSet.RevisionId)).ToList();
            foreach (var changeset in list)
            {
                revisionChangeSetsProcessingData.RevisionChangeSets.Remove(changeset);
                var num = await _changeSetFileService.Delete(changeset) ? 1 : 0;
            }

            foreach (var stateFile in badStateFiles)
            {
                var num = await _stateFileService.DeleteInProgress(stateFile.FileSetId) ? 1 : 0;
            }

            badStateFiles = null;
        }

        private async Task GetRevisionChangeSets(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            if (revisionChangeSetsProcessingData.IsComplete)
                return;
            var setsProcessingData = revisionChangeSetsProcessingData;
            setsProcessingData.RevisionChangeSets = await _changeSetFileService.GetAllRevisionChangeSets();
            setsProcessingData = null;
        }

        private async Task GetInProgressStateFiles(
            RevisionChangeSetsProcessingData revisionChangeSetsProcessingData)
        {
            revisionChangeSetsProcessingData.InProgressStateFiles =
                (await _stateFileService.GetAllInProgress())?.StateFiles;
            if (revisionChangeSetsProcessingData.InProgressStateFiles == null)
                revisionChangeSetsProcessingData.IsComplete = true;
            if (!revisionChangeSetsProcessingData.InProgressStateFiles.Any())
                return;
            _logger.LogInfoWithSource(
                string.Format("Processing {0} in progress RevisionChangeSet",
                    revisionChangeSetsProcessingData.InProgressStateFiles.Count),
                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
        }

        private bool CreateFileSetPollRequest(
            RevisionChangeSetKey revisionChangeSetKey,
            FileSetState state,
            out FileSetPollRequest fileSetPollRequest)
        {
            fileSetPollRequest = null;
            try
            {
                if (revisionChangeSetKey != null)
                    if (revisionChangeSetKey.FileSetId > 0L)
                        if (revisionChangeSetKey.RevisionId > 0L)
                            fileSetPollRequest = new FileSetPollRequest
                            {
                                FileSetId = revisionChangeSetKey.FileSetId,
                                FileSetRevisionId = revisionChangeSetKey.RevisionId,
                                FileSetState = state
                            };
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "An unhandled exception occurred.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
            }

            return fileSetPollRequest != null;
        }

        private async Task<Dictionary<long, FileSetDependencyState>> GetDependencyStates()
        {
            var dependencies = new Dictionary<long, FileSetDependencyState>();
            try
            {
                var stateFiles = (await _stateFileService.GetAll())?.StateFiles;
                if (stateFiles == null)
                    return null;
                var revisionChangeSets = await _changeSetFileService.GetAllRevisionChangeSets();
                foreach (var stateFile in stateFiles)
                {
                    var eachStateFile = stateFile;
                    var fileSetDependencyState = new FileSetDependencyState
                    {
                        FileSetId = eachStateFile.FileSetId,
                        IsInProgressStaged = false
                    };
                    if (eachStateFile.IsRevisionDownloadInProgress)
                    {
                        var revisionChangeSet = revisionChangeSets.Where(cs => cs.FileSetId == eachStateFile.FileSetId)
                            .FirstOrDefault();
                        if (revisionChangeSet != null)
                        {
                            var clientFileSetRevision =
                                await _fileCacheService.GetClientFileSetRevision(revisionChangeSet);
                            if (clientFileSetRevision != null)
                            {
                                fileSetDependencyState.InProgressRevisionId = revisionChangeSet.RevisionId;
                                fileSetDependencyState.InProgressVersion = clientFileSetRevision.RevisionVersion;
                                fileSetDependencyState.IsInProgressStaged = revisionChangeSet.IsStaged;
                            }
                        }

                        revisionChangeSet = null;
                    }

                    if (eachStateFile.HasActiveRevision)
                    {
                        fileSetDependencyState.ActiveRevisionId = eachStateFile.ActiveRevisionId;
                        var clientFileSetRevision =
                            await _fileCacheService.GetAnyClientFileSetRevision(eachStateFile.FileSetId,
                                eachStateFile.ActiveRevisionId);
                        if (clientFileSetRevision != null)
                            fileSetDependencyState.ActiveVersion = clientFileSetRevision.RevisionVersion;
                    }

                    dependencies[eachStateFile.FileSetId] = fileSetDependencyState;
                    fileSetDependencyState = null;
                }

                return dependencies;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting FileSetDependencyStates",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
                return null;
            }
        }

        private async Task<bool> Reboot()
        {
            _fileSetTransition.ClearRebootRequired();
            var result = false;
            var shutdownResponse = await _kioskEngineService.PerformShutdown(_settings.KioskEngineShutdownTimeoutMs,
                _settings.KioskEngineShutdownTimeoutMs);
            if (!string.IsNullOrWhiteSpace(shutdownResponse?.Error))
                _logger.LogErrorWithSource("Unable to shutdown KioskEngine.  Error Message: " + shutdownResponse?.Error,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
            else if (!_kernelService.PerformShutdown(ShutdownType.Reboot))
                _logger.LogErrorWithSource("Error while trying to perform reboot.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
            else
                result = true;
            return result;
        }

        private async Task<DateTime> GetLastCallToProcessPendingFileSetVersion()
        {
            var result = DateTime.MinValue;
            if (await _lockPendingFileSetVersions.WaitAsync(_pendingFileSetVersionsLockTimeout))
                try
                {
                    result = _lastCallToProcessPendingFileSetVersions;
                }
                finally
                {
                    _lockPendingFileSetVersions.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed. Unable to get last call date for ProcessPendingFileSetVersion",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");

            return result;
        }

        private async Task SetLastCallToProcessPendingFileSetVersions()
        {
            if (await _lockPendingFileSetVersions.WaitAsync(_pendingFileSetVersionsLockTimeout))
                try
                {
                    _lastCallToProcessPendingFileSetVersions = DateTime.Now;
                }
                finally
                {
                    _lockPendingFileSetVersions.Release();
                }
            else
                _logger.LogErrorWithSource("Lock failed. Unable to set last call for ProcessPendingFileSetVersion",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetService.cs");
        }

        private class RevisionChangeSetsProcessingData
        {
            public bool IsComplete { get; set; }

            public List<StateFile> InProgressStateFiles { get; set; }

            public List<RevisionChangeSet> RevisionChangeSets { get; set; }

            public DownloadDataList AllDownloadData { get; set; }

            public StateFile GetStateFileForRevisionChangeSet(RevisionChangeSet revisionChangeSet)
            {
                return InProgressStateFiles.FirstOrDefault(stateFile =>
                    revisionChangeSet.FileSetId == stateFile.FileSetId &&
                    revisionChangeSet.RevisionId == stateFile.InProgressRevisionId);
            }
        }
    }
}