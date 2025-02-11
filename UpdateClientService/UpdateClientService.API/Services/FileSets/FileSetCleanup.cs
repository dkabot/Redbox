using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets
{
    public class FileSetCleanup : IFileSetCleanup
    {
        private readonly IFileCacheService _fileCacheService;
        private readonly IFileSetService _fileSetService;
        private readonly ILogger<FileSetCleanup> _logger;
        private readonly IStateFileService _stateFileService;

        public FileSetCleanup(
            IFileCacheService fileCacheService,
            IFileSetService fileSetService,
            IStateFileService stateFileService,
            ILogger<FileSetCleanup> logger)
        {
            _fileCacheService = fileCacheService;
            _fileSetService = fileSetService;
            _stateFileService = stateFileService;
            _logger = logger;
        }

        public async Task Run()
        {
            try
            {
                var fileSetCleanupData = new CleanupData();
                await GetStateFiles(fileSetCleanupData);
                foreach (var fileSetId in _fileCacheService.GetFileSetIds())
                    await CleanupFileSet(fileSetCleanupData, fileSetId);
                fileSetCleanupData = null;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while cleaning up FileSets",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
            }
        }

        private async Task CleanupFileSet(CleanupData cleanupData, long fileSetId)
        {
            if (cleanupData.IsComplete)
                return;
            _logger.LogInfoWithSource(string.Format("FileSet Cleanup for FileSetId {0}", fileSetId),
                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
            var fileSetCleanupData = new FileSetCleanupData
            {
                CleanupData = cleanupData,
                FileSetId = fileSetId
            };
            GetActiveOrInProgressFileSetRevisionIds(fileSetCleanupData);
            await GetUsedClientFileSetRevisions(fileSetCleanupData);
            GetRetentionCriteria(fileSetCleanupData);
            await GetFileSetFileInfos(fileSetCleanupData);
            await CleanupFileSetRevisions(fileSetCleanupData, fileSetId);
            RemoveUnneededRevisionFiles(fileSetCleanupData);
            fileSetCleanupData = null;
        }

        private async Task CleanupFileSetRevisions(
            FileSetCleanupData fileSetCleanupData,
            long fileSetId)
        {
            if (fileSetCleanupData.IsComplete)
                return;
            foreach (var num in _fileCacheService.GetFileSetRevisionIds(fileSetId).OrderByDescending(x => x).ToList())
            {
                var revisionRetained = IsRevisionRetained(fileSetCleanupData, num);
                foreach (var revisionFilePath in _fileCacheService.GetClientFileSetRevisionFilePaths(fileSetId, num))
                {
                    var clientFileSetRevision = await _fileCacheService.GetClientFileSetRevision(revisionFilePath);
                    if (clientFileSetRevision != null)
                    {
                        if (revisionRetained)
                            AddRequiredFiles(fileSetCleanupData, clientFileSetRevision);
                        else
                            _fileCacheService.DeleteRevision(clientFileSetRevision);
                    }
                }

                if (revisionRetained)
                    ++fileSetCleanupData.RetainedRevisions;
            }
        }

        private bool IsRevisionRetained(
            FileSetCleanupData fileSetCleanupData,
            long fileSetRevisionId)
        {
            if (fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Any(x => x == fileSetRevisionId))
                return true;
            var totalDays =
                (DateTime.Now -
                 _fileCacheService.GetRevisionCreationDate(fileSetCleanupData.FileSetId, fileSetRevisionId)).TotalDays;
            if (totalDays <= 1.0)
                return true;
            if (fileSetCleanupData.RetentionDays > 0 && totalDays > fileSetCleanupData.RetentionDays)
            {
                _logger.LogInfoWithSource(
                    string.Format(
                        "FileSetId {0} FileSetRevisionId {1} has exceeded the retention {2} days and will be deleted",
                        fileSetCleanupData.FileSetId, fileSetRevisionId, fileSetCleanupData.RetentionDays),
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
                return false;
            }

            if (fileSetCleanupData.RetentionRevisions <= 0 ||
                fileSetCleanupData.RetainedRevisions < fileSetCleanupData.RetentionRevisions)
                return true;
            _logger.LogInfoWithSource(
                string.Format(
                    "FileSetId {0} FileSetRevisionId {1} has exceeded the retention count {2} and will be deleted",
                    fileSetCleanupData.FileSetId, fileSetRevisionId, fileSetCleanupData.RetentionRevisions),
                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
            return false;
        }

        private async Task GetFileSetFileInfos(
            FileSetCleanupData fileSetCleanupData)
        {
            if (fileSetCleanupData.IsComplete)
                return;
            var fileSetCleanupData1 = fileSetCleanupData;
            fileSetCleanupData1.FileSetFileInfos = await _fileCacheService.GetFileInfos(fileSetCleanupData.FileSetId);
            fileSetCleanupData1 = null;
        }

        private void GetRetentionCriteria(
            FileSetCleanupData fileSetCleanupData)
        {
            if (fileSetCleanupData.IsComplete)
                return;
            var maxFileSetRevisionId = fileSetCleanupData.UsedClientFileSetRevisions.Max(x => x.RevisionId);
            var clientFileSetRevision =
                fileSetCleanupData.UsedClientFileSetRevisions.FirstOrDefault(x => x.RevisionId == maxFileSetRevisionId);
            fileSetCleanupData.RetentionDays = clientFileSetRevision.RetentionDays;
            fileSetCleanupData.RetentionRevisions = clientFileSetRevision.RetentionRevisions;
            if (fileSetCleanupData.RetentionDays != 0 || fileSetCleanupData.RetentionRevisions != 0)
                return;
            fileSetCleanupData.IsComplete = true;
        }

        private async Task GetUsedClientFileSetRevisions(
            FileSetCleanupData fileSetCleanupData)
        {
            if (fileSetCleanupData.IsComplete)
                return;
            foreach (var fileSetRevisionId in fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds)
            {
                var clientFileSetRevision =
                    await _fileCacheService.GetAnyClientFileSetRevision(fileSetCleanupData.FileSetId,
                        fileSetRevisionId);
                if (clientFileSetRevision != null)
                    fileSetCleanupData.UsedClientFileSetRevisions.Add(clientFileSetRevision);
            }

            if (fileSetCleanupData.UsedClientFileSetRevisions.Any())
                return;
            fileSetCleanupData.IsComplete = true;
        }

        private void GetActiveOrInProgressFileSetRevisionIds(
            FileSetCleanupData fileSetCleanupData)
        {
            fileSetCleanupData.StateFile =
                fileSetCleanupData.CleanupData.StateFiles.FirstOrDefault(x =>
                    x.FileSetId == fileSetCleanupData.FileSetId);
            if (fileSetCleanupData.StateFile != null)
            {
                if (fileSetCleanupData.StateFile.HasActiveRevision)
                    fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Add(fileSetCleanupData.StateFile
                        .ActiveRevisionId);
                if (fileSetCleanupData.StateFile.IsRevisionDownloadInProgress)
                    fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Add(fileSetCleanupData.StateFile
                        .InProgressRevisionId);
            }

            fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds =
                fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Distinct().ToList();
            if (fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Any())
                return;
            fileSetCleanupData.IsComplete = true;
        }

        private async Task GetStateFiles(CleanupData fileSetCleanupData)
        {
            var all = await _stateFileService.GetAll();
            if (all.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogWarningWithSource("Unable to get StateFiles",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
                fileSetCleanupData.IsComplete = true;
            }
            else
            {
                fileSetCleanupData.StateFiles = all.StateFiles;
            }
        }

        private void AddRequiredFiles(
            FileSetCleanupData fileSetCleanupData,
            ClientFileSetRevision clientFileSetRevision)
        {
            var list1 = fileSetCleanupData.FileSetFileInfos.Where(x =>
                    clientFileSetRevision.Files.Any(y => y.FileId == x.FileId && y.FileRevisionId == x.FileRevisionId))
                .ToList();
            var list2 = fileSetCleanupData.FileSetFileInfos.Where(x =>
                clientFileSetRevision.PatchFiles.Any(y =>
                    y.FileId == x.FileId && y.PatchFileRevisionId == x.FileRevisionId)).ToList();
            fileSetCleanupData.RequiredFileSetFileInfos.AddRange(list1);
            fileSetCleanupData.RequiredFileSetFileInfos.AddRange(list2);
        }

        private void RemoveUnneededRevisionFiles(
            FileSetCleanupData fileSetCleanupData)
        {
            fileSetCleanupData.FileSetFileInfos.Except(fileSetCleanupData.RequiredFileSetFileInfos.Distinct()).ToList()
                .ForEach(eachFileToRemove =>
                {
                    _logger.LogInfoWithSource(
                        string.Format("Deleting FileSetId: {0} FileId {1} FileRevisionId {2}",
                            fileSetCleanupData.FileSetId, eachFileToRemove.FileId, eachFileToRemove.FileRevisionId),
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
                    _fileCacheService.DeleteFile(fileSetCleanupData.FileSetId, eachFileToRemove.FileId,
                        eachFileToRemove.FileRevisionId);
                });
        }

        private class CleanupData
        {
            public List<StateFile> StateFiles { get; set; } = new List<StateFile>();

            public bool IsComplete { get; set; }
        }

        private class FileSetCleanupData
        {
            public long FileSetId { get; set; }

            public CleanupData CleanupData { get; set; }

            public StateFile StateFile { get; set; }

            public List<long> ActiveOrInProgressFileSetRevisionIds { get; set; } = new List<long>();

            public List<ClientFileSetRevision> UsedClientFileSetRevisions { get; } = new List<ClientFileSetRevision>();

            public List<FileSetFileInfo> FileSetFileInfos { get; set; } = new List<FileSetFileInfo>();

            public List<FileSetFileInfo> RequiredFileSetFileInfos { get; } = new List<FileSetFileInfo>();

            public int RetentionDays { get; set; }

            public int RetentionRevisions { get; set; }

            public int RetainedRevisions { get; set; }

            public bool IsComplete { get; set; }
        }
    }
}