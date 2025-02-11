using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets
{
    public class ChangeSetFileService : IChangeSetFileService
    {
        private readonly IFileCacheService _fileCacheService;
        private readonly IFileSetDownloader _fileSetDownloader;
        private readonly IFileSetTransition _fileSetTransition;
        private readonly ILogger<ChangeSetFileService> _logger;
        private readonly IRevisionChangeSetRepository _revisionChangeSetRepository;
        private readonly IFileSetRevisionDownloader _revisionDownloader;

        public ChangeSetFileService(
            ILogger<ChangeSetFileService> logger,
            IFileSetRevisionDownloader revisionDownloader,
            IFileSetDownloader fileSetDownloader,
            IFileSetTransition fileSetTransition,
            IFileCacheService fileCacheService,
            IRevisionChangeSetRepository revisionChangeSetRepository)
        {
            _logger = logger;
            _revisionDownloader = revisionDownloader;
            _fileSetDownloader = fileSetDownloader;
            _fileSetTransition = fileSetTransition;
            _fileCacheService = fileCacheService;
            _revisionChangeSetRepository = revisionChangeSetRepository;
        }

        public async void CleanUp()
        {
            try
            {
                var num = await _revisionChangeSetRepository.Cleanup(DateTime.Now.AddDays(-10.0)) ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "An unhandled exception occurred",
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
            }
        }

        public async Task<bool> Delete(RevisionChangeSet revisionChangeSet)
        {
            return await _revisionChangeSetRepository.Delete(revisionChangeSet);
        }

        public async Task<bool> CreateRevisionChangeSet(
            ClientFileSetRevisionChangeSet clientFileSetRevisionChangeSet)
        {
            var result = false;
            try
            {
                result = await _revisionChangeSetRepository.Save(
                    RevisionChangeSet.Create(clientFileSetRevisionChangeSet));
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while creating RevisionChangeSet",
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
            }

            return result;
        }

        public async Task<List<RevisionChangeSet>> GetAllRevisionChangeSets()
        {
            return await _revisionChangeSetRepository.GetAll();
        }

        public async Task ProcessChangeSet(
            DownloadDataList allDownloads,
            RevisionChangeSet revisionChangeSet)
        {
            try
            {
                var fileSetDownloads = allDownloads.GetByFileSetId(revisionChangeSet.FileSetId);
                await CheckReceived(revisionChangeSet, fileSetDownloads);
                await CheckDownloadingRevision(revisionChangeSet, fileSetDownloads);
                await CheckDownloadedRevision(revisionChangeSet, fileSetDownloads);
                await CheckDownloadingFileSet(revisionChangeSet, fileSetDownloads);
                await CheckDownloadedFileSet(revisionChangeSet);
                await CheckStagingFileSet(revisionChangeSet);
                await CheckStagedFileSet(revisionChangeSet);
                fileSetDownloads = null;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while processing RevisionChangeSet with " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
            }
        }

        public async Task ProcessActivationDependencyCheck(
            Dictionary<long, FileSetDependencyState> dependencyStates,
            RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.ActivationDependencyCheck ? 1 : 0
                    : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                var flag = _fileSetTransition.CheckMeetsDependency(clientFileSetRevision, dependencyStates);
                if (!flag)
                {
                    await AttemptRetryAfterError(FileSetState.NeedsDependency.ToString(), revisionChangeSet);
                    return;
                }

                if (flag)
                {
                    var num = await SetState(ChangesetState.ActivationPending, revisionChangeSet) ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking activation dependency for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception", revisionChangeSet);
            }
        }

        public async Task ProcessActivationPending(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.ActivationPending ? 1 : 0
                    : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                if (revisionChangeSet.ActiveOn > DateTime.Now)
                {
                    var logger = _logger;
                    var activeOn = revisionChangeSet.ActiveOn;
                    var revisionChangeSetKey = revisionChangeSet;
                    var str1 = revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null;
                    var str2 = string.Format("ActivateOn date {0} is in the future for {1}", activeOn, str1);
                    _logger.LogInfoWithSource(str2,
                        "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                    return;
                }

                if (!IsInActivationTime(revisionChangeSet))
                {
                    _logger.LogInfoWithSource(
                        "Current time is not within Activation time window.  ActivationStartTime: " +
                        revisionChangeSet.ActivateStartTime + ", ActivationEndTime: " +
                        revisionChangeSet.ActivateEndTime,
                        "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                    return;
                }

                var num = await SetState(ChangesetState.ActivationBeforeActions, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking ActivationPending for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception", revisionChangeSet);
            }
        }

        public async Task ProcessActivationBeforeActions(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.ActivationBeforeActions ? 1 : 0
                    : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                var flag = _fileSetTransition.BeforeActivate(clientFileSetRevision);
                if (!flag)
                {
                    await AttemptRetryAfterError("Error while running ActivationBeforeActions.", revisionChangeSet);
                    return;
                }

                if (flag)
                {
                    var num = await SetState(ChangesetState.Activating, revisionChangeSet) ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking ActivationBeforeActions for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception", revisionChangeSet);
            }
        }

        public async Task ProcessActivating(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null ? revisionChangeSet1.State != ChangesetState.Activating ? 1 : 0 : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                if (!_fileSetTransition.Activate(clientFileSetRevision))
                {
                    await AttemptRetryAfterError("Activating revision failed", revisionChangeSet);
                    return;
                }

                var num = await SetState(ChangesetState.ActivationAfterActions, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking Activating state for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception.", revisionChangeSet);
            }
        }

        public async Task ProcessActivationAfterActions(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.ActivationAfterActions ? 1 : 0
                    : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                var flag = _fileSetTransition.AfterActivate(clientFileSetRevision);
                if (!flag)
                {
                    var logger = _logger;
                    var revisionChangeSetKey = revisionChangeSet;
                    var str = "Errors activating RevisionChangeSet for " +
                              (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null) + ": " +
                              flag.ToJson();
                    _logger.LogWarningWithSource(str,
                        "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                }

                var num = await SetState(ChangesetState.Activated, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking ActivationAfterActions for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                var num = await SetState(ChangesetState.Activated, revisionChangeSet) ? 1 : 0;
            }
        }

        private async Task CheckReceived(
            RevisionChangeSet revisionChangeSet,
            DownloadDataList downloadDataList)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null ? revisionChangeSet1.State != 0 ? 1 : 0 : 1) != 0)
                return;
            var revisionChangeSetKey1 = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
            LogRevisionChangeSetState(revisionChangeSet, revisionChangeSetKey1);
            try
            {
                if (_revisionDownloader.DoesRevisionExist(revisionChangeSet))
                {
                    var num = await SetState(ChangesetState.DownloadedRevision, revisionChangeSet) ? 1 : 0;
                    return;
                }

                if (!_revisionDownloader.DoesRevisionExist(revisionChangeSet) && revisionChangeSetKey1 == null)
                {
                    var downloadData = await _revisionDownloader.AddDownload(revisionChangeSet,
                        revisionChangeSet.FileHash, revisionChangeSet.Path, revisionChangeSet.DownloadPriority);
                    if (downloadData == null)
                    {
                        await AttemptRetryAfterError("RevisionDownloader.AddDownloader failed", revisionChangeSet);
                        return;
                    }

                    downloadDataList.Add(downloadData);
                }

                var num1 = await SetState(ChangesetState.DownloadingRevision, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey2 = revisionChangeSet;
                var str = "Exception while checking received state for " +
                          (revisionChangeSetKey2 != null ? revisionChangeSetKey2.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckReceived", revisionChangeSet);
            }
        }

        private void LogRevisionChangeSetState(
            RevisionChangeSet revisionChangeSet,
            DownloadData downloadData = null)
        {
            var str = downloadData != null
                ? string.Format(";  DownloadState: {0}", downloadData?.DownloadState)
                : string.Empty;
            _logger.LogInfoWithSource(
                string.Format("RevisionChangeSet State = {0} for {1}{2}", revisionChangeSet?.State,
                    revisionChangeSet != null ? revisionChangeSet.IdentifyingText() : (object)null, str),
                "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
        }

        private async Task CheckDownloadingRevision(
            RevisionChangeSet revisionChangeSet,
            DownloadDataList downloadDataList)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.DownloadingRevision ? 1 : 0
                    : 1) != 0)
                return;
            var revisionChangeSetKey1 = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
            LogRevisionChangeSetState(revisionChangeSet, revisionChangeSetKey1);
            try
            {
                if (_revisionDownloader.DoesRevisionExist(revisionChangeSet))
                {
                    var num = await SetState(ChangesetState.DownloadedRevision, revisionChangeSet) ? 1 : 0;
                    return;
                }

                if (revisionChangeSetKey1 == null)
                {
                    await AttemptRetryAfterError("CheckDownloadingRevision - Download doesn't exist when it should",
                        revisionChangeSet);
                    return;
                }

                if (_revisionDownloader.IsDownloadError(revisionChangeSetKey1))
                {
                    await AttemptRetryAfterError("CheckDownloadingRevision - Download is in an error state",
                        revisionChangeSet);
                    return;
                }

                if (_revisionDownloader.IsDownloadComplete(revisionChangeSet, revisionChangeSetKey1))
                {
                    if (_revisionDownloader.CompleteDownload(revisionChangeSet, revisionChangeSetKey1))
                    {
                        var num1 = await SetState(ChangesetState.DownloadedRevision, revisionChangeSet) ? 1 : 0;
                    }
                    else
                    {
                        await AttemptRetryAfterError("CheckDownloadingRevision - CompleteDownload failed",
                            revisionChangeSet);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey2 = revisionChangeSet;
                var str = "Exception while checking DownloadingRevision state for " +
                          (revisionChangeSetKey2 != null ? revisionChangeSetKey2.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckDownloadingRevision", revisionChangeSet);
            }
        }

        private async Task<ClientFileSetRevision> GetClientFileSetRevision(
            RevisionChangeSet revisionChangeSet)
        {
            return await _fileCacheService.GetClientFileSetRevision(revisionChangeSet);
        }

        private async Task CheckDownloadedRevision(
            RevisionChangeSet revisionChangeSet,
            DownloadDataList downloadDataList)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.DownloadedRevision ? 1 : 0
                    : 1) != 0)
                return;
            var revisionChangeSetKey1 = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
            LogRevisionChangeSetState(revisionChangeSet, revisionChangeSetKey1);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                if (_fileSetDownloader.IsDownloaded(clientFileSetRevision))
                {
                    var num = await SetState(ChangesetState.DownloadedFileSet, revisionChangeSet) ? 1 : 0;
                    return;
                }

                if (await _fileSetDownloader.DownloadFileSet(clientFileSetRevision, downloadDataList,
                        revisionChangeSet.DownloadPriority))
                {
                    var num1 = await SetState(ChangesetState.DownloadingFileSet, revisionChangeSet) ? 1 : 0;
                }
                else
                {
                    await AttemptRetryAfterError("An error while starting fileset downloads", revisionChangeSet);
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey2 = revisionChangeSet;
                var str = "Exception while checking DownloadedRevsion state for " +
                          (revisionChangeSetKey2 != null ? revisionChangeSetKey2.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckDownloadedRevision", revisionChangeSet);
            }
        }

        private async Task CheckDownloadingFileSet(
            RevisionChangeSet revisionChangeSet,
            DownloadDataList downloadDataList)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.DownloadingFileSet ? 1 : 0
                    : 1) != 0)
                return;
            var revisionChangeSetKey1 = downloadDataList.GetByRevisionChangeSetKey(revisionChangeSet);
            LogRevisionChangeSetState(revisionChangeSet, revisionChangeSetKey1);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                if (downloadDataList == null || downloadDataList.Count == 0)
                {
                    await AttemptRetryAfterError("Download does not exist for FileSet", revisionChangeSet);
                    return;
                }

                if (!_fileSetDownloader.CompleteDownloads(clientFileSetRevision, downloadDataList))
                {
                    await AttemptRetryAfterError("Complete downloads failed", revisionChangeSet);
                    return;
                }

                if (downloadDataList.IsDownloading)
                    return;
                if (_fileSetDownloader.IsDownloaded(clientFileSetRevision))
                {
                    var num = await SetState(ChangesetState.DownloadedFileSet, revisionChangeSet) ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey2 = revisionChangeSet;
                var str = "Exception  while checking DownloadingFileSet state for " +
                          (revisionChangeSetKey2 != null ? revisionChangeSetKey2.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckDownloadingFileSet", revisionChangeSet);
            }
        }

        private async Task CheckDownloadedFileSet(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null
                    ? revisionChangeSet1.State != ChangesetState.DownloadedFileSet ? 1 : 0
                    : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var num = await SetState(ChangesetState.Staging, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking DownloadedFileSet state for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckDownloadedFileSet", revisionChangeSet);
            }
        }

        private async Task CheckStagingFileSet(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null ? revisionChangeSet1.State != ChangesetState.Staging ? 1 : 0 : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var clientFileSetRevision = await GetClientFileSetRevision(revisionChangeSet);
                if (clientFileSetRevision == null)
                {
                    await AttemptRetryAfterError("Revision does not exist", revisionChangeSet);
                    return;
                }

                if (!_fileSetTransition.Stage(clientFileSetRevision))
                {
                    await AttemptRetryAfterError("Staging revision failed.", revisionChangeSet);
                    return;
                }

                var num = await SetState(ChangesetState.Staged, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking Staging state for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckStagingFileSet", revisionChangeSet);
            }
        }

        private async Task CheckStagedFileSet(RevisionChangeSet revisionChangeSet)
        {
            var revisionChangeSet1 = revisionChangeSet;
            if ((revisionChangeSet1 != null ? revisionChangeSet1.State != ChangesetState.Staged ? 1 : 0 : 1) != 0)
                return;
            LogRevisionChangeSetState(revisionChangeSet);
            try
            {
                var num = await SetState(ChangesetState.ActivationDependencyCheck, revisionChangeSet) ? 1 : 0;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revisionChangeSet;
                var str = "Exception while checking Staged state for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                await AttemptRetryAfterError("Unhandled exception in CheckStagedFileSet", revisionChangeSet);
            }
        }

        private async Task<bool> SetState(ChangesetState state, RevisionChangeSet revisionChangeSet)
        {
            revisionChangeSet.State = state;
            return await _revisionChangeSetRepository.Save(revisionChangeSet);
        }

        private bool IsInActivationTime(RevisionChangeSet revisionChangeSet)
        {
            TimeSpan result1;
            TimeSpan result2;
            if (string.IsNullOrEmpty(revisionChangeSet.ActivateStartTime) ||
                string.IsNullOrEmpty(revisionChangeSet.ActivateEndTime) ||
                !TimeSpan.TryParse(revisionChangeSet.ActivateStartTime, out result1) ||
                !TimeSpan.TryParse(revisionChangeSet.ActivateEndTime, out result2) || result1 == result2)
                return true;
            var now = DateTime.Now;
            return result2 < result1
                ? now.TimeOfDay <= result2 || now.TimeOfDay >= result1
                : now.TimeOfDay >= result1 && now.TimeOfDay <= result2;
        }

        private async Task AttemptRetryAfterError(
            string errorMessage,
            RevisionChangeSet revisionChangeSet)
        {
            _logger.LogErrorWithSource(
                "Error for RevisionChangeSet with " + revisionChangeSet.IdentifyingText() + " (" + errorMessage + ")",
                "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
            revisionChangeSet.Message = errorMessage;
            ++revisionChangeSet.RetryCount;
            if (revisionChangeSet.RetryCount > 2)
            {
                _logger.LogInfoWithSource(
                    "Setting Error state for RevsionChangeSet with " + revisionChangeSet.IdentifyingText(),
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                var num = await SetState(ChangesetState.Error, revisionChangeSet) ? 1 : 0;
            }
            else
            {
                _logger.LogInfoWithSource(
                    "Retrying download of RevisionChangeSet with " + revisionChangeSet.IdentifyingText(),
                    "/sln/src/UpdateClientService.API/Services/FileSets/ChangeSetFileService.cs");
                var num = await SetState(ChangesetState.Received, revisionChangeSet) ? 1 : 0;
            }
        }
    }
}