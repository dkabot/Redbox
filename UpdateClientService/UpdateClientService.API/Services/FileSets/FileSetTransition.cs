using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.FileSets
{
    public class FileSetTransition : IFileSetTransition
    {
        private const string FileSetChangeSetPath = "staging";
        private const string FileSetJsonFileName = "fileset.json";
        private static bool _rebootRequired;
        private readonly AppSettings _appSettings;
        private readonly ICommandLineService _commandLineService;
        private readonly IFileCacheService _fileCacheService;
        private readonly IHttpService _httpService;
        private readonly ILogger<FileSetTransition> _logger;
        private readonly IWindowsServiceFunctions _winServiceFunctions;

        public FileSetTransition(
            IFileCacheService fileCacheService,
            IHttpService httpService,
            IWindowsServiceFunctions winServiceFunctions,
            ICommandLineService cmd,
            IOptions<AppSettings> appSettings,
            ILogger<FileSetTransition> logger)
        {
            _fileCacheService = fileCacheService;
            _httpService = httpService;
            _winServiceFunctions = winServiceFunctions;
            _commandLineService = cmd;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public void MarkRebootRequired()
        {
            _rebootRequired = true;
        }

        public void ClearRebootRequired()
        {
            _rebootRequired = false;
        }

        public bool RebootRequired => _rebootRequired;

        public bool Stage(ClientFileSetRevision clientFileSetRevision)
        {
            var result = true;
            try
            {
                var fileSetRevisionPath = GetStagingFileSetRevisionPath(clientFileSetRevision);
                if (!Directory.Exists(fileSetRevisionPath))
                    Directory.CreateDirectory(fileSetRevisionPath);
                var fileData = GetFileData(clientFileSetRevision);
                fileData.ForEach(eachFileData =>
                {
                    if (_fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId, eachFileData.FileId,
                            eachFileData.FileRevisionId))
                        return;
                    _logger.LogError(string.Format("Missing file for FileId {0} Revision {1}", eachFileData.FileId,
                        eachFileData.FileRevisionId));
                    result = false;
                });
                if (!result)
                    return result;
                fileData.ForEach(eachFileData =>
                {
                    if (_fileCacheService.CopyFileToPath(clientFileSetRevision.FileSetId, eachFileData.FileId,
                            eachFileData.FileRevisionId, eachFileData.StagePath))
                        return;
                    _logger.LogError(string.Format(
                        "File for FileId {0} Revision {1} couldn't be copied to it's staging directory",
                        eachFileData.FileId, eachFileData.FileRevisionId));
                    result = false;
                });
                if (!result)
                    return result;
                fileData.ForEach(eachFileData =>
                {
                    if (_fileCacheService.IsFileHashValid(eachFileData.StagePath, eachFileData.Hash))
                        return;
                    _logger.LogError(string.Format("Hash check failed for FileId {0} Revision {1}", eachFileData.FileId,
                        eachFileData.FileRevisionId));
                    result = false;
                });
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = clientFileSetRevision;
                var str = "Exception while staging files for revision with " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                var objArray = Array.Empty<object>();
                _logger.LogError(exception, str, objArray);
            }

            return result;
        }

        public bool CheckMeetsDependency(
            ClientFileSetRevision clientFileSetRevision,
            Dictionary<long, FileSetDependencyState> dependencyStates)
        {
            var flag = false;
            try
            {
                bool? nullable;
                if (clientFileSetRevision == null)
                {
                    nullable = new bool?();
                }
                else
                {
                    var dependencies = clientFileSetRevision.Dependencies;
                    nullable = dependencies != null ? dependencies.Any() : new bool?();
                }

                if (!nullable.GetValueOrDefault())
                    return true;
                flag = true;
                foreach (var dependency in clientFileSetRevision.Dependencies)
                    if (!dependencyStates.ContainsKey(dependency.DependsOnFileSetId))
                    {
                        _logger.LogWarningWithSource(
                            string.Format("Missing dependency FileSet with FileSetId {0} for revision {1} Revision {2}",
                                dependency.DependsOnFileSetId, clientFileSetRevision.IdentifyingText(),
                                clientFileSetRevision.RevisionId),
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                        flag = false;
                    }
                    else
                    {
                        var dependencyState = dependencyStates[dependency.DependsOnFileSetId];
                        if (!dependencyState.IsDependencyMet(dependency.DependencyType, dependency.MinimumVersion,
                                dependency.MaximumVersion))
                        {
                            _logger.LogWarningWithSource(
                                "Missing dependency version for " + clientFileSetRevision.IdentifyingText() +
                                ". Expected version " + ExpectedDependencyVersionInfo(dependency, dependencyState) +
                                ". Current version: '" +
                                (dependencyState.InProgressRevisionId > 0L
                                    ? dependencyState.InProgressVersion
                                    : dependencyState.ActiveVersion) + "'",
                                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                            flag = false;
                        }
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception while checking dependencies for revision " + (clientFileSetRevision != null
                        ? clientFileSetRevision.IdentifyingText()
                        : null));
            }

            return flag;
        }

        public bool BeforeActivate(ClientFileSetRevision clientFileSetRevision)
        {
            var flag = true;
            try
            {
                var fileSetJsonModel = GetFileSetJsonModel(clientFileSetRevision, true);
                if (fileSetJsonModel == null)
                {
                    _logger.LogWarningWithSource("FileSetJson was null skipping BeforeActivate.",
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                    return flag;
                }

                if (fileSetJsonModel?.WindowsServiceName == "updateclient$service")
                {
                    Task.Run(async () => await CallUpdateClientInstaller(clientFileSetRevision));
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(fileSetJsonModel?.WindowsServiceName) &&
                    _winServiceFunctions.ServiceExists(fileSetJsonModel.WindowsServiceName))
                    _winServiceFunctions.StopService(fileSetJsonModel.WindowsServiceName);
                var fileData = GetFileData(clientFileSetRevision).Where(f =>
                    string.Equals(Path.GetFileName(f.FileDestination), "beforeactivate.ps1",
                        StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (fileData != null)
                {
                    flag = RunScript(clientFileSetRevision, fileData.StagePath);
                    if (!flag)
                    {
                        var logger = _logger;
                        var revisionChangeSetKey = clientFileSetRevision;
                        var str = "Error running BeforeActivate script for " + (revisionChangeSetKey != null
                            ? revisionChangeSetKey.IdentifyingText()
                            : null);
                        _logger.LogErrorWithSource(str,
                            "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var revisionChangeSetKey = clientFileSetRevision;
                var str = "Exception while processing BeforeActivate for " +
                          (revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                flag = false;
            }

            return flag;
        }

        public bool AfterActivate(ClientFileSetRevision clientFileSetRevision)
        {
            var flag = true;
            try
            {
                var fileSetJsonModel = GetFileSetJsonModel(clientFileSetRevision, false);
                if (fileSetJsonModel == null)
                {
                    _logger.LogWarningWithSource("FileSetJson was null skipping AfterActivate.",
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                    return flag;
                }

                if (!string.IsNullOrWhiteSpace(fileSetJsonModel?.WindowsServiceName))
                    flag = InstallAndStartWindowsService(fileSetJsonModel);
                var fileData = GetFileData(clientFileSetRevision).Where(f =>
                    string.Equals(Path.GetFileName(f.FileDestination), "afteractivate.ps1",
                        StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (fileData != null)
                    if (!RunScript(clientFileSetRevision, fileData.FileDestination))
                    {
                        _logger.LogErrorWithSource(
                            "Error while running afteractivate script for " + (clientFileSetRevision != null
                                ? clientFileSetRevision.IdentifyingText()
                                : null), "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                        flag = false;
                    }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Exception while running afteractivate script for " + (clientFileSetRevision != null
                        ? clientFileSetRevision.IdentifyingText()
                        : null), "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                flag = false;
            }

            return flag;
        }

        public bool Activate(ClientFileSetRevision clientFileSetRevision)
        {
            bool flag;
            try
            {
                var fileData1 = GetFileData(clientFileSetRevision);
                flag = StagedFilesExist(clientFileSetRevision, fileData1);
                if (!flag)
                    return flag;
                var action = (Action<string, string>)((sourceFilePath, destinationfilePath) =>
                {
                    if (File.Exists(destinationfilePath))
                        File.Delete(destinationfilePath);
                    File.Move(sourceFilePath, destinationfilePath);
                });
                foreach (var fileData2 in fileData1)
                {
                    flag = CreateFileDestinationDirectory(fileData2);
                    if (!flag)
                        return flag;
                    try
                    {
                        action(fileData2.StagePath, fileData2.FileDestination);
                    }
                    catch (Exception ex1)
                    {
                        try
                        {
                            _logger.LogInfoWithSource(
                                fileData2.FileDestination + " is locked by anther process attempting to retry.",
                                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                            _logger.LogErrorWithSource(ex1, "Error from activate copy - retrying.",
                                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                            Task.Delay(100).Wait();
                            action(fileData2.StagePath, fileData2.FileDestination);
                        }
                        catch (Exception ex2)
                        {
                            _logger.LogInfoWithSource(
                                fileData2.FileDestination + " is locked by anther process doing a deferred move.",
                                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                            _logger.LogErrorWithSource(ex2, "Error from activate copy.",
                                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                            var tempFileName = Path.GetTempFileName();
                            action(fileData2.StagePath, tempFileName);
                            MoveLockedFileSystemEntry(tempFileName, fileData2.FileDestination);
                            MarkRebootRequired();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Exception while activing revision with " + (clientFileSetRevision != null
                        ? clientFileSetRevision.IdentifyingText()
                        : null), "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                flag = false;
            }

            return flag;
        }

        private static string ExpectedDependencyVersionInfo(
            ClientFileSetRevisionDependency clientFileSetRevisionDependency,
            FileSetDependencyState fileSetDependencyState)
        {
            return new
            {
                fileSetDependencyState.FileSetId,
                DependencyType = clientFileSetRevisionDependency.DependencyType.ToString(),
                clientFileSetRevisionDependency.MinimumVersion,
                clientFileSetRevisionDependency.MaximumVersion
            }.ToJson();
        }

        private async Task CallUpdateClientInstaller(ClientFileSetRevision clientFileSetRevision)
        {
            _logger.LogInfoWithSource(
                "Calling UpdateClientInstaller for revision with " + clientFileSetRevision.ToJsonIndented(),
                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
            var apiResponse = await _httpService.SendRequestAsync<object>(_appSettings.UpdateClientInstallerUrl,
                "api/activation?waitForResponse=false", clientFileSetRevision, HttpMethod.Post,
                callerLocation: "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs",
                logRequest: false);
            if (apiResponse.IsSuccessStatusCode)
                return;
            _logger.LogErrorWithSource(
                "Error while calling UpdateClientInstaller.Error-> " + apiResponse.Errors.ToJson(),
                "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
        }

        private FileSetJsonModel GetFileSetJsonModel(
            ClientFileSetRevision clientFileSetRevision,
            bool beforeActivations)
        {
            var fileSetJsonModel = (FileSetJsonModel)null;
            var fileData = GetFileData(clientFileSetRevision)
                .FirstOrDefault(x => Path.GetFileName(x.FileDestination).ToLower() == "fileset.json");
            if (fileData == null)
            {
                _logger.LogWarningWithSource(
                    "fileset.json does not exist for revision: " + clientFileSetRevision.IdentifyingText(),
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                return null;
            }

            var path = beforeActivations ? fileData.StagePath : fileData.FileDestination;
            try
            {
                fileSetJsonModel = File.ReadAllText(path).ToObject<FileSetJsonModel>();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting fileset.json from " + path + ".",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
            }

            return fileSetJsonModel;
        }

        private bool InstallAndStartWindowsService(FileSetJsonModel fileSetJson)
        {
            var flag = true;
            if (!_winServiceFunctions.ServiceExists(fileSetJson.WindowsServiceName))
            {
                if (!string.IsNullOrWhiteSpace(fileSetJson.WindowsServiceStartFile))
                {
                    var displayName = string.IsNullOrWhiteSpace(fileSetJson.WindowsServiceDisplayName)
                        ? fileSetJson.WindowsServiceName
                        : fileSetJson.WindowsServiceDisplayName;
                    var binPath = Path.Combine(fileSetJson.InstallPath, fileSetJson.WindowsServiceStartFile);
                    _winServiceFunctions.InstallService(fileSetJson.WindowsServiceName, displayName, binPath);
                }
                else
                {
                    _logger.LogErrorWithSource(
                        "windowsServiceStartPath in fileSetJson is null for windowsServiceName " +
                        fileSetJson.WindowsServiceName,
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                    flag = false;
                }
            }

            _winServiceFunctions.StartService(fileSetJson.WindowsServiceName);
            return flag;
        }

        private bool StagedFilesExist(
            ClientFileSetRevision clientFileSetRevision,
            List<FileData> fileDataList)
        {
            var result = true;
            fileDataList?.ForEach(eachFileData =>
            {
                if (File.Exists(eachFileData.StagePath))
                    return;
                var logger = _logger;
                var stagePath = eachFileData.StagePath;
                var fileId = eachFileData.FileId;
                var revisionChangeSetKey = clientFileSetRevision;
                var str1 = revisionChangeSetKey != null ? revisionChangeSetKey.IdentifyingText() : null;
                var str2 = string.Format("Missing file {0} for FileId {1} and revision with {2}", stagePath, fileId,
                    str1);
                _logger.LogErrorWithSource(str2,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                result = false;
            });
            return result;
        }

        private bool CreateFileDestinationDirectory(FileData fileData)
        {
            var destinationDirectory = true;
            try
            {
                var directoryName = Path.GetDirectoryName(fileData.FileDestination);
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while creating directory " + fileData?.FileDestination,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                destinationDirectory = false;
            }

            return destinationDirectory;
        }

        private static void MoveLockedFileSystemEntry(string source, string destination)
        {
            var dwFlags = MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
            if (!Directory.Exists(source) && !Directory.Exists(destination))
                dwFlags |= MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
            MoveFileEx(source, destination, dwFlags);
        }

        [DllImport("kernel32.dll")]
        private static extern bool MoveFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            MoveFileFlags dwFlags);

        private List<FileData> GetFileData(ClientFileSetRevision clientFileSetRevision)
        {
            var fileData1 = new List<FileData>();
            var fileSetRevisionPath = GetStagingFileSetRevisionPath(clientFileSetRevision);
            foreach (var file in clientFileSetRevision.Files)
            {
                var fileData2 = new FileData
                {
                    FileId = file.FileId,
                    FileRevisionId = file.FileRevisionId,
                    Size = file.ContentSize,
                    Hash = file.ContentHash,
                    StagePath = Path.Combine(fileSetRevisionPath,
                        _fileCacheService.GetRevisionFileName(file.FileId, file.FileRevisionId)),
                    FileDestination = file.FileDestination
                };
                fileData1.Add(fileData2);
            }

            return fileData1;
        }

        private bool RunScript(ClientFileSetRevision clientFileSetRevision, string scriptPath)
        {
            try
            {
                if (string.IsNullOrEmpty(scriptPath))
                {
                    _logger.LogWarningWithSource(
                        "Script was empty for " + clientFileSetRevision.IdentifyingText() + ". Skipping...",
                        "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                    return true;
                }

                if (!(Path.GetExtension(scriptPath) == ".file"))
                    return _commandLineService.TryExecutePowerShellScriptFromFile(scriptPath);
                var str = Path.GetTempFileName().Replace(".tmp", ".ps1");
                File.Copy(scriptPath, str);
                var flag = _commandLineService.TryExecutePowerShellScriptFromFile(str);
                File.Delete(str);
                return flag;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while running script " + scriptPath,
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetTransition.cs");
                return false;
            }
        }

        private string GetStagingDirectory()
        {
            return Path.Combine(Constants.FileSetsRoot, "staging");
        }

        private string GetStagingFileSetPath(ClientFileSetRevision clientFileSetRevision)
        {
            return Path.Combine(GetStagingDirectory(), clientFileSetRevision.FileSetId.ToString());
        }

        private string GetStagingFileSetRevisionPath(ClientFileSetRevision clientFileSetRevision)
        {
            return Path.Combine(GetStagingFileSetPath(clientFileSetRevision),
                clientFileSetRevision.RevisionId.ToString());
        }

        [Flags]
        private enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8
        }
    }
}