using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.FileCache
{
    public class FileCacheService : IFileCacheService
    {
        private readonly ILogger<FileCacheService> _logger;

        public FileCacheService(ILogger<FileCacheService> logger)
        {
            _logger = logger;
        }

        public bool DoesFileExist(long fileSetId, long fileId, long fileRevisionId)
        {
            try
            {
                return File.Exists(GetRevisionFilePath(fileSetId, fileId, fileRevisionId));
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format(
                        "Exception while checking the existance of File with FileSetId {0}, FileId {1}, FileRevisionId {2}",
                        fileSetId, fileId, fileRevisionId),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return false;
        }

        public bool IsFileHashValid(long fileSetId, long fileId, long fileRevisionId, string fileHash)
        {
            try
            {
                return IsFileHashValid(GetRevisionFilePath(fileSetId, fileId, fileRevisionId), fileHash);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format(
                        "Exception while checking the hash for File with FileSetId {0}, FileId {1}, FileRevisionId {2}",
                        fileSetId, fileId, fileRevisionId),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return false;
        }

        public bool IsFileHashValid(string filePath, string hash)
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    return fileStream.GetSHA1Hash() == hash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking hash for file " + filePath + ".");
            }

            return false;
        }

        public bool DoesRevisionExist(RevisionChangeSetKey revisionChangeSetKey)
        {
            try
            {
                return File.Exists(GetRevisionPath(revisionChangeSetKey));
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Exception while checking the existence of a revsion for " + (revisionChangeSetKey != null
                        ? revisionChangeSetKey.IdentifyingText()
                        : null), "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return false;
        }

        public bool AddFile(
            long fileSetId,
            long fileId,
            long fileRevisionId,
            byte[] data,
            string info)
        {
            var flag = false;
            try
            {
                CheckFilePath(fileSetId, fileId);
                File.WriteAllBytes(GetRevisionFilePath(fileSetId, fileId, fileRevisionId), data);
                File.WriteAllText(GetFileInfoPath(fileSetId, fileId, fileRevisionId), info);
                _logger.LogInformation("FileCacheService.AddFile - Info: " + info);
                flag = true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while adding file FileSetId {0}, FileId {1}, FileRevisionId {2}",
                        fileSetId, fileId, fileRevisionId),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return flag;
        }

        public bool AddFilePatch(
            long fileSetId,
            long fileId,
            long fileRevisionId,
            long filePatchRevisionId,
            byte[] data,
            string info)
        {
            var flag = false;
            try
            {
                CheckFilePath(fileSetId, fileId);
                if (!GetFile(fileSetId, fileId, filePatchRevisionId, out var _))
                {
                    _logger.LogError(string.Format("Patch source file is missing FileId: {0} FileRevisionId {1}",
                        fileId, filePatchRevisionId));
                    return flag;
                }

                var revisionFilePath = GetRevisionFilePath(fileSetId, fileId, filePatchRevisionId);
                var errorList = new List<Error>();
                using (var source = new FileStream(revisionFilePath, (FileMode)3, (FileAccess)1))
                {
                    source.Position = 0L;
                    using (var patch = new MemoryStream(data))
                    {
                        patch.Position = 0L;
                        using (var target = new FileStream(GetRevisionFilePath(fileSetId, fileId, fileRevisionId),
                                   (FileMode)4, (FileAccess)2))
                        {
                            errorList.AddRange(XDeltaHelper.Apply(source, patch, target));
                        }
                    }
                }

                File.WriteAllText(GetFileInfoPath(fileSetId, fileId, fileRevisionId), info);
                _logger.LogInfoWithSource("FileCacheService.AddFilePatch - Info: " + info,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
                if (errorList.Count > 0)
                    _logger.LogErrorWithSource(errorList[0].Message,
                        "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
                else
                    flag = true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format(
                        "Exception while adding File Patch with FileSetId {0}, FileId {1}, FileRevisionId {2}, FilePatchRevisionId {3}",
                        fileSetId, fileId, fileRevisionId, filePatchRevisionId),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return flag;
        }

        public bool AddRevision(RevisionChangeSetKey revisionChangeSetKey, byte[] data, string info)
        {
            var flag = false;
            try
            {
                CheckFileSetPath(revisionChangeSetKey.FileSetId);
                File.WriteAllBytes(GetRevisionPath(revisionChangeSetKey), data);
                File.WriteAllText(GetRevisionInfoPath(revisionChangeSetKey), info);
                _logger.LogInfoWithSource("Adding Revision: " + info,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
                flag = true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Exception while adding revision for " +
                    (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return flag;
        }

        public void DeleteFile(long fileSetId, long fileId, long fileRevisionId, bool force = false)
        {
            try
            {
                var revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
                if (!force && (DateTime.Now - File.GetCreationTime(revisionFilePath)).TotalDays <= 1.0)
                {
                    _logger.LogInfoWithSource(
                        string.Format(
                            "Cannot delete files less than a day old - FileSetId {0}, FileId {1}, FileRevisionId {2}",
                            fileSetId, fileId, fileRevisionId),
                        "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
                }
                else
                {
                    _logger.LogInfoWithSource(
                        string.Format("Deleting file for FileSetId {0}, FileId {1}, FileRevisionId {2}", fileSetId,
                            fileId, fileRevisionId),
                        "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
                    DeleteFileIfExists(revisionFilePath);
                    DeleteFileIfExists(GetFileInfoPath(fileSetId, fileId, fileRevisionId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while deleting File for FileSetId {0}, FileId {1}, FileRevisionId {2}",
                        fileSetId, fileId, fileRevisionId),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }
        }

        public void DeleteRevision(RevisionChangeSetKey revisionChangeSetKey)
        {
            try
            {
                _logger.LogInfoWithSource(
                    "Deleting revision with " +
                    (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
                DeleteFileIfExists(GetRevisionPath(revisionChangeSetKey));
                DeleteFileIfExists(GetRevisionInfoPath(revisionChangeSetKey));
                var stagingDirectory =
                    GetStagingDirectory(revisionChangeSetKey.FileSetId, revisionChangeSetKey.RevisionId);
                if (!Directory.Exists(stagingDirectory))
                    return;
                Directory.Delete(stagingDirectory, true);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Exception while deleting revision with " +
                    (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }
        }

        public bool GetFile(long fileSetId, long fileId, long fileRevisionId, out byte[] data)
        {
            data = null;
            var errorList = new List<Error>();
            try
            {
                var revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
                if (File.Exists(revisionFilePath))
                {
                    data = File.ReadAllBytes(revisionFilePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(new Error
                {
                    Message = string.Format("FileCacheService.GetFile Unhandled exception occurred. {0}", ex)
                });
            }

            return false;
        }

        public async Task<List<FileSetFileInfo>> GetFileInfos(long fileSetId)
        {
            var result = new List<FileSetFileInfo>();
            foreach (var fileInfoPath in GetFileInfoPaths(fileSetId))
            {
                var fileInfo = await GetFileInfo(fileInfoPath);
                if (fileInfo != null)
                    result.Add(fileInfo);
            }

            return result;
        }

        public async Task<FileSetFileInfo> GetFileInfo(string filePath)
        {
            var result = (FileSetFileInfo)null;
            try
            {
                var (flag, numArray) = await ReadFileBytes(filePath);
                if (flag)
                    result = Encoding.ASCII.GetString(numArray).ToObject<FileSetFileInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting FileSetFileInfo from file " + filePath,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return result;
        }

        public bool CopyFileToPath(
            long fileSetId,
            long fileId,
            long fileRevisionId,
            string copyToPath)
        {
            try
            {
                var revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
                if (File.Exists(revisionFilePath))
                {
                    File.Copy(revisionFilePath, copyToPath, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format(
                        "Exception while copying file FileSetId {0}, FileId {1}, FileRevisionId {2}, CopyToPath {3}",
                        fileSetId, fileId, fileRevisionId, copyToPath),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return false;
        }

        public async Task<ClientFileSetRevision> GetAnyClientFileSetRevision(
            long fileSetId,
            long fileSetRevisionId)
        {
            var result = (ClientFileSetRevision)null;
            try
            {
                result = await GetClientFileSetRevision(GetClientFileSetRevisionFilePaths(fileSetId, fileSetRevisionId)
                    .FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format(
                        "Exception while getting ClientFileSetRevision for FileSetId {0} and FileSetRevisionId {1}",
                        fileSetId, fileSetRevisionId),
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return result;
        }

        public async Task<ClientFileSetRevision> GetClientFileSetRevision(
            RevisionChangeSetKey revisionChangeSetKey)
        {
            var result = (ClientFileSetRevision)null;
            try
            {
                result = await GetClientFileSetRevision(GetRevisionPath(revisionChangeSetKey));
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey1 = revisionChangeSetKey;
                var str = "Exception while reading ClienfFileSetRevision for " +
                          (revisionChangeSetKey1 != null ? revisionChangeSetKey1.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return result;
        }

        public async Task<ClientFileSetRevision> GetClientFileSetRevision(string filePath)
        {
            var result = (ClientFileSetRevision)null;
            try
            {
                var (flag, numArray) = await ReadFileBytes(filePath);
                if (flag)
                    result = Encoding.ASCII.GetString(numArray).ToObject<ClientFileSetRevision>();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while reading ClienfFileSetRevision from file " + filePath,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return result;
        }

        public List<string> GetClientFileSetRevisionFilePaths(long fileSetId)
        {
            var revisionFilePaths = new List<string>();
            var fileSetDirectory = GetFileSetDirectory(fileSetId);
            if (Directory.Exists(fileSetDirectory))
                revisionFilePaths = Directory.GetFiles(fileSetDirectory, "*.revision", SearchOption.TopDirectoryOnly)
                    .ToList();
            return revisionFilePaths;
        }

        public List<long> GetFileSetIds()
        {
            var fileSetIds = new List<long>();
            if (!Directory.Exists(Constants.FileCacheRootPath))
                return fileSetIds;
            foreach (var directory in Directory.GetDirectories(Constants.FileCacheRootPath))
            {
                var fileName = Path.GetFileName(directory);
                try
                {
                    fileSetIds.Add(Convert.ToInt64(fileName));
                }
                catch
                {
                }
            }

            return fileSetIds;
        }

        public List<long> GetFileSetRevisionIds(long fileSetId)
        {
            var source = new List<long>();
            foreach (var revisionFilePath in GetClientFileSetRevisionFilePaths(fileSetId))
            {
                var (success, revisionId) = ParseRevisionIdFromRevisionFileName(Path.GetFileName(revisionFilePath));
                if (success)
                    source.Add(revisionId);
            }

            return source.Distinct().ToList();
        }

        public List<string> GetClientFileSetRevisionFilePaths(long fileSetId, long revisionId)
        {
            var revisionFilePaths = new List<string>();
            var fileSetDirectory = GetFileSetDirectory(fileSetId);
            if (Directory.Exists(fileSetDirectory))
                revisionFilePaths = Directory.GetFiles(fileSetDirectory, string.Format("{0}-*.revision", revisionId),
                    SearchOption.TopDirectoryOnly).ToList();
            return revisionFilePaths;
        }

        public DateTime GetRevisionCreationDate(long fileSetId, long revisionId)
        {
            return GetMaxCreationdDate(GetClientFileSetRevisionFilePaths(fileSetId, revisionId));
        }

        public List<string> GetFileInfoPaths(long fileSetId)
        {
            var fileInfoPaths = new List<string>();
            var revisionFileDirectory = GetRevisionFileDirectory(fileSetId);
            if (Directory.Exists(revisionFileDirectory))
                fileInfoPaths = Directory.GetFiles(revisionFileDirectory, "*.fileinfo", SearchOption.TopDirectoryOnly)
                    .ToList();
            return fileInfoPaths;
        }

        public string GetRevisionFileName(long fileId, long fileRevisionId)
        {
            return string.Format("{0}-{1}.file", fileId, fileRevisionId);
        }

        private void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while deleting file " + filePath,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }
        }

        private async Task<(bool, byte[])> ReadFileBytes(string filePath)
        {
            var result = false;
            var fileBytes = (byte[])null;
            try
            {
                if (File.Exists(filePath))
                {
                    fileBytes = File.ReadAllBytes(filePath);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while reading file " + filePath,
                    "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
            }

            return (result, fileBytes);
        }

        private (bool success, long revisionId) ParseRevisionIdFromRevisionFileName(
            string revisionFileName)
        {
            var flag = false;
            long result = 0;
            if (!string.IsNullOrEmpty(revisionFileName) && revisionFileName.Contains("-"))
            {
                var source = revisionFileName.Split('-');
                if (source != null && source.Count() > 0 && long.TryParse(source[0], out result))
                    flag = true;
            }

            return (flag, result);
        }

        private DateTime GetMaxCreationdDate(List<string> filePaths)
        {
            var maxCreationdDate = DateTime.MinValue;
            foreach (var filePath in filePaths)
            {
                var creationTime = File.GetCreationTime(filePath);
                if (creationTime > maxCreationdDate)
                    maxCreationdDate = creationTime;
            }

            return maxCreationdDate;
        }

        private string GetFileSetDirectory(long fileSetId)
        {
            return Path.Combine(Constants.FileCacheRootPath, string.Format("{0}", fileSetId));
        }

        private string GetRevisionPath(RevisionChangeSetKey revisionChangeSetKey)
        {
            return GetRevisionPath(revisionChangeSetKey != null ? revisionChangeSetKey.FileSetId : 0L,
                revisionChangeSetKey != null ? revisionChangeSetKey.RevisionId : 0L,
                revisionChangeSetKey != null ? revisionChangeSetKey.PatchRevisionId : 0L);
        }

        private string GetRevisionPath(long fileSetId, long revisionId, long patchRevisionId)
        {
            return Path.Combine(GetFileSetDirectory(fileSetId), GetRevisionName(revisionId, patchRevisionId));
        }

        private string GetRevisionInfoPath(RevisionChangeSetKey revisionChangeSetKey)
        {
            return GetRevisionInfoPath(revisionChangeSetKey != null ? revisionChangeSetKey.FileSetId : 0L,
                revisionChangeSetKey != null ? revisionChangeSetKey.RevisionId : 0L,
                revisionChangeSetKey != null ? revisionChangeSetKey.PatchRevisionId : 0L);
        }

        private string GetRevisionInfoPath(long fileSetId, long revisionId, long patchRevisionId)
        {
            return Path.Combine(GetFileSetDirectory(fileSetId), GetRevisionInfoName(revisionId, patchRevisionId));
        }

        private string GetStagingDirectory(long fileSetId, long revisionId)
        {
            return Path.Combine(Constants.FileSetsRoot, "staging", string.Format("{0}", fileSetId),
                string.Format("{0}", revisionId));
        }

        private string GetRevisionName(long revisionId, long patchRevisionId)
        {
            return string.Format("{0}-{1}.revision", revisionId, patchRevisionId);
        }

        private string GetRevisionInfoName(long revisionId, long patchRevisionId)
        {
            return string.Format("{0}-{1}.revisioninfo", revisionId, patchRevisionId);
        }

        private string GetRevisionFileDirectory(long fileSetId)
        {
            return Path.Combine(GetFileSetDirectory(fileSetId), "File");
        }

        private string GetRevisionFilePath(long fileSetId, long fileId, long fileRevisionId)
        {
            return Path.Combine(GetRevisionFileDirectory(fileSetId), GetRevisionFileName(fileId, fileRevisionId));
        }

        private string GetFileInfoPath(long fileSetId, long fileId, long fileRevisionId)
        {
            return Path.Combine(GetRevisionFileDirectory(fileSetId), GetFileInfoName(fileId, fileRevisionId));
        }

        private string GetFileInfoName(long fileId, long fileRevisionId)
        {
            return string.Format("{0}-{1}.fileinfo", fileId, fileRevisionId);
        }

        private void CheckFilePath(long fileSetId, long fileId)
        {
            CreateDirectoryIfNeeded(Path.Combine(GetRevisionFileDirectory(fileSetId)));
        }

        private void CheckFileSetPath(long fileSetId)
        {
            CreateDirectoryIfNeeded(Path.Combine(GetFileSetDirectory(fileSetId)));
        }

        private void CreateDirectoryIfNeeded(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                return;
            Directory.CreateDirectory(directoryPath);
        }
    }
}