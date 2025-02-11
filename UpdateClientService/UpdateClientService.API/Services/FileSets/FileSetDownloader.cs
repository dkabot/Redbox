using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets
{
    public class FileSetDownloader : IFileSetDownloader
    {
        private readonly IDownloader _downloader;
        private readonly IDownloadService _downloadService;
        private readonly IFileCacheService _fileCacheService;
        private readonly ILogger<FileSetDownloader> _logger;
        private readonly AppSettings _settings;
        private readonly IZipDownloadHelper _zipHelper;

        public FileSetDownloader(
            IFileCacheService fileCacheService,
            IDownloadService downloadService,
            IZipDownloadHelper zipHelper,
            IDownloader downloaderInterface,
            IOptionsMonitor<AppSettings> settings,
            ILogger<FileSetDownloader> logger)
        {
            _fileCacheService = fileCacheService;
            _downloadService = downloadService;
            _zipHelper = zipHelper;
            _downloader = downloaderInterface;
            _settings = settings.CurrentValue;
            _logger = logger;
        }

        public bool IsDownloaded(ClientFileSetRevision clientFileSetRevision)
        {
            var fileSetInfoList = GetFileSetInfoList(clientFileSetRevision);
            return GetDownloadMethod(clientFileSetRevision, fileSetInfoList) == DownloadMethod.None;
        }

        public async Task<bool> DownloadFileSet(
            ClientFileSetRevision revision,
            DownloadDataList downloadDataList,
            DownloadPriority priority)
        {
            var flag = true;
            try
            {
                var fileSetInfoList = GetFileSetInfoList(revision);
                var downloadMethod = GetDownloadMethod(revision, fileSetInfoList);
                _logger.LogInfoWithSource(
                    string.Format("Download method is {0} for {1}", downloadMethod, revision.IdentifyingText()),
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
                switch (downloadMethod)
                {
                    case DownloadMethod.FileSet:
                        flag = await DownloadByFileSet(revision, downloadDataList, priority);
                        break;
                    case DownloadMethod.PatchSet:
                        flag = await DownloadByPatchSet(revision, fileSetInfoList, downloadDataList, priority);
                        break;
                    case DownloadMethod.Files:
                        flag = await DownloadByFiles(fileSetInfoList, downloadDataList, priority);
                        break;
                    case DownloadMethod.Patches:
                        flag = await DownloadByPatches(fileSetInfoList, downloadDataList, priority);
                        break;
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = revision;
                var str = "Exception while downloading file set " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
                flag = false;
            }

            return flag;
        }

        public bool CompleteDownloads(
            ClientFileSetRevision clientFileSetRevision,
            DownloadDataList downloadDataList)
        {
            var flag = true;
            foreach (var downloadData in downloadDataList.Where(x => x.DownloadState == DownloadState.PostDownload))
                if (_zipHelper.Extract(downloadData.Path, clientFileSetRevision))
                    _downloader.Complete(downloadData);
                else
                    flag = false;
            return flag;
        }

        private List<FileSetFileInfo> GetFileSetInfoList(ClientFileSetRevision clientFileSetRevision)
        {
            var fileSetInfoList = new List<FileSetFileInfo>();
            foreach (var file in clientFileSetRevision.Files)
            {
                var eachClientFileSetFile = file;
                var fileSetFileInfo = new FileSetFileInfo
                {
                    FileId = eachClientFileSetFile.FileId,
                    FileRevisionId = eachClientFileSetFile.FileRevisionId,
                    Key = GetFileKey(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId,
                        eachClientFileSetFile.FileRevisionId),
                    FileSize = eachClientFileSetFile.FileSize,
                    PatchSize = 0,
                    FileHash = eachClientFileSetFile.FileHash
                };
                fileSetFileInfo.Exists = _fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId,
                    eachClientFileSetFile.FileId, eachClientFileSetFile.FileRevisionId);
                if (fileSetFileInfo.Exists && !_fileCacheService.IsFileHashValid(clientFileSetRevision.FileSetId,
                        eachClientFileSetFile.FileId, eachClientFileSetFile.FileRevisionId,
                        eachClientFileSetFile.ContentHash))
                {
                    _logger.LogWarning(string.Format(
                        "Invalid Hash for file with FileSetId {0}, FileId {1}, FileRevisionId {2}",
                        clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId,
                        eachClientFileSetFile.FileRevisionId));
                    fileSetFileInfo.Exists = false;
                    _fileCacheService.DeleteFile(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId,
                        eachClientFileSetFile.FileRevisionId, true);
                }

                fileSetFileInfo.Url = GetFileUrl(eachClientFileSetFile, clientFileSetRevision.FileSetId,
                    eachClientFileSetFile.FileRevisionId);
                if (!fileSetFileInfo.Exists)
                {
                    var patchFileSetFile =
                        clientFileSetRevision.PatchFiles.FirstOrDefault(pf =>
                            pf.FileId == eachClientFileSetFile.FileId);
                    if (patchFileSetFile != null)
                    {
                        var fileKey = GetFileKey(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId,
                            patchFileSetFile.PatchFileRevisionId);
                        var flag = _fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId,
                            eachClientFileSetFile.FileId, patchFileSetFile.PatchFileRevisionId);
                        if (flag && !_fileCacheService.IsFileHashValid(clientFileSetRevision.FileSetId,
                                eachClientFileSetFile.FileId, patchFileSetFile.PatchFileRevisionId,
                                patchFileSetFile.ContentHash))
                        {
                            _logger.LogWarning(string.Format(
                                "Invalid Hash for file with FileSetId {0}, FileId {1}, FileRevisionId {2}",
                                clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId,
                                patchFileSetFile.PatchFileRevisionId));
                            flag = false;
                            _fileCacheService.DeleteFile(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId,
                                patchFileSetFile.PatchFileRevisionId, true);
                        }

                        if (flag)
                        {
                            fileSetFileInfo.PatchSize = patchFileSetFile.FileSize;
                            fileSetFileInfo.PatchUrl = GetFileUrl(eachClientFileSetFile,
                                clientFileSetRevision.FileSetId, eachClientFileSetFile.FileRevisionId,
                                patchFileSetFile.PatchFileRevisionId);
                            fileSetFileInfo.PatchKey = fileKey;
                            fileSetFileInfo.PatchHash = patchFileSetFile.FileHash;
                        }
                    }
                }

                fileSetInfoList.Add(fileSetFileInfo);
            }

            return fileSetInfoList;
        }

        private DownloadMethod GetDownloadMethod(
            ClientFileSetRevision clientFileSetRevision,
            List<FileSetFileInfo> fileSetInfoList)
        {
            var num1 = fileSetInfoList.Sum(x => !x.Exists ? x.FileSize : 0L);
            if (num1 == 0L)
                return DownloadMethod.None;
            var num2 = fileSetInfoList.Count(x => !x.Exists);
            var source = new List<DownloadMethodSize>();
            source.Add(new DownloadMethodSize
            {
                DownloadMethod = DownloadMethod.Files,
                Size = num1,
                NumberOfDownloads = num2
            });
            source.Add(new DownloadMethodSize
            {
                DownloadMethod = DownloadMethod.FileSet,
                Size = clientFileSetRevision.SetFileSize,
                NumberOfDownloads = 1
            });
            if (clientFileSetRevision.PatchSetFileSize > 0L)
            {
                var num3 = fileSetInfoList.Sum(x =>
                {
                    if (x.Exists)
                        return 0;
                    return x.PatchSize <= 0L ? x.FileSize : x.PatchSize;
                });
                source.Add(new DownloadMethodSize
                {
                    DownloadMethod = DownloadMethod.Patches,
                    Size = num3,
                    NumberOfDownloads = num2
                });
                var num4 = fileSetInfoList.Count(x => !x.Exists || x.PatchSize == 0L);
                var num5 = fileSetInfoList.Sum(x => !x.Exists && x.PatchSize <= 0L ? x.FileSize : 0L) +
                           clientFileSetRevision.PatchSetFileSize;
                source.Add(new DownloadMethodSize
                {
                    DownloadMethod = DownloadMethod.PatchSet,
                    Size = num5,
                    NumberOfDownloads = num4 + 1
                });
            }

            return source.OrderBy(x => x.Size).ThenBy(x => x.NumberOfDownloads).FirstOrDefault().DownloadMethod;
        }

        private async Task<bool> DownloadByFileSet(
            ClientFileSetRevision clientFileSetRevision,
            DownloadDataList downloadDataList,
            DownloadPriority priority)
        {
            var result = false;
            try
            {
                var key = GetRevisionSetKey(clientFileSetRevision);
                var fileSetUrl = GetFileSetUrl(clientFileSetRevision);
                if (!downloadDataList.ExistsByKey(key))
                {
                    var (flag, downloadData) =
                        await AddDownload(key, clientFileSetRevision.SetFileHash, fileSetUrl, priority);
                    if (flag)
                    {
                        downloadDataList.Add(downloadData);
                        result = true;
                    }
                    else
                    {
                        _logger.LogErrorWithSource("Unable to add DownloadData for " + key,
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
                    }
                }
                else
                {
                    result = true;
                }

                key = null;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = clientFileSetRevision;
                var str = "Excepton while adding DownloadData by FileSet for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
            }

            return result;
        }

        private async Task<bool> DownloadByPatchSet(
            ClientFileSetRevision clientFileSetRevision,
            List<FileSetFileInfo> fileSetInfoList,
            DownloadDataList downloadDataList,
            DownloadPriority priority)
        {
            var result = false;
            try
            {
                var revisionPatchSetKey = GetRevisionPatchSetKey(clientFileSetRevision);
                var patchSetFileUrl = GetPatchSetFileUrl(clientFileSetRevision);
                if (!downloadDataList.ExistsByKey(revisionPatchSetKey))
                {
                    var (flag, downloadData) = await AddDownload(revisionPatchSetKey,
                        clientFileSetRevision.PatchSetFileHash, patchSetFileUrl, priority);
                    if (flag)
                    {
                        downloadDataList.Add(downloadData);
                        result = await AddDownloadsForNewFilesInPatchSet(fileSetInfoList, downloadDataList, priority);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = clientFileSetRevision;
                var str = "Exception while adding DownloadData for Patch Set with " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
            }

            return result;
        }

        private async Task<bool> AddDownloadsForNewFilesInPatchSet(
            List<FileSetFileInfo> fileSetInfoList,
            DownloadDataList downloadDataList,
            DownloadPriority priority)
        {
            var result = true;
            foreach (var fileSetFileInfo in fileSetInfoList.Where(x => !x.Exists && x.PatchSize == 0L))
                if (!downloadDataList.ExistsByKey(fileSetFileInfo.Key))
                {
                    var (flag, downloadData) = await AddDownload(fileSetFileInfo.Key, fileSetFileInfo.FileHash,
                        fileSetFileInfo.Url, priority);
                    if (flag)
                        downloadDataList.Add(downloadData);
                    else
                        result = false;
                }

            return result;
        }

        private async Task<bool> DownloadByPatches(
            List<FileSetFileInfo> fileSetInfoList,
            DownloadDataList downloadDataList,
            DownloadPriority priority)
        {
            var result = true;
            try
            {
                foreach (var fileSetFileInfo in fileSetInfoList.Where(x => !x.Exists))
                {
                    var flag1 = fileSetFileInfo.PatchSize > 0L;
                    var key = flag1 ? fileSetFileInfo.PatchKey : fileSetFileInfo.Key;
                    if (!downloadDataList.ExistsByKey(key))
                    {
                        var hash = flag1 ? fileSetFileInfo.PatchHash : fileSetFileInfo.FileHash;
                        var url = flag1 ? fileSetFileInfo.PatchUrl : fileSetFileInfo.Url;
                        var (flag2, downloadData) = await AddDownload(key, hash, url, priority);
                        if (flag2)
                            downloadDataList.Add(downloadData);
                        else
                            result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while adding DownloadData for Patch files",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
                result = false;
            }

            return result;
        }

        private async Task<bool> DownloadByFiles(
            List<FileSetFileInfo> fileSetInfoList,
            DownloadDataList downloadDataList,
            DownloadPriority downloadPriority)
        {
            var result = true;
            try
            {
                foreach (var fileSetFileInfo in fileSetInfoList.Where(x => !x.Exists))
                    if (!downloadDataList.ExistsByKey(fileSetFileInfo.Key))
                    {
                        var (flag, downloadData) = await AddDownload(fileSetFileInfo.Key, fileSetFileInfo.FileHash,
                            fileSetFileInfo.Url, downloadPriority);
                        if (flag)
                            downloadDataList.Add(downloadData);
                        else
                            result = false;
                    }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while adding DownloadData for files",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
                result = false;
            }

            return result;
        }

        private async Task<(bool success, DownloadData downloadData)> AddDownload(
            string key,
            string hash,
            string url,
            DownloadPriority downloadPriority)
        {
            return await _downloadService.AddRetrieveDownload(key, hash, url, downloadPriority, false);
        }

        private string GetFileKey(long fileSetId, long fileId, long fileRevisionId)
        {
            return string.Format("{0},{1},{2},{3}", 2, fileSetId, fileId, fileRevisionId);
        }

        private string GetFileUrl(
            ClientFileSetFile file,
            long fileSetId,
            long fileRevisionId,
            long patchFileRevisionId = 0)
        {
            return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy",
                Uri.EscapeDataString(string.Format("filesets/{0}/File/{1}-{2}-{3}.zip", fileSetId, file.FileId,
                    fileRevisionId, patchFileRevisionId)), DownloadPathType.None)).ToString();
        }

        private string GetFilePatchKey(
            long fileSetId,
            long fileId,
            long fileRevisionId,
            long patchFileRevisionId)
        {
            return string.Format("{0},{1},{2},{3},{4}", 3, fileSetId, fileId, fileRevisionId, patchFileRevisionId);
        }

        private string GetRevisionSetKey(ClientFileSetRevision revision)
        {
            return string.Format("{0},{1},{2}", 4, revision.FileSetId, revision.RevisionId);
        }

        private string GetRevisionPatchSetKey(ClientFileSetRevision revision)
        {
            return string.Format("{0},{1},{2}", 5, revision.FileSetId, revision.RevisionId);
        }

        private string GetPatchSetFileUrl(ClientFileSetRevision revision)
        {
            return GetProxiedS3Url(revision.PatchSetPath);
        }

        private string GetFileSetUrl(ClientFileSetRevision revision)
        {
            return GetProxiedS3Url(revision.SetPath);
        }

        private string GetProxiedS3Url(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy",
                Uri.EscapeDataString(path), DownloadPathType.None)).ToString();
        }

        private class DownloadMethodSize
        {
            public DownloadMethod DownloadMethod { get; set; }

            public long Size { get; set; }

            public int NumberOfDownloads { get; set; }
        }
    }
}