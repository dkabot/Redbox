using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.FileUpdate;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.Domain.FileUpdate
{
    public class FileUpdateService : IFileUpdateService
    {
        private static readonly object _lock = new object();
        private readonly IApplicationControl _applicationControl;
        private readonly ILogger<FileUpdateService> _logger;
        private string _completedFolderPath;
        private string _notProcessedFolderPath;
        private Timer _timer;
        private string _updateFolderPath;

        public FileUpdateService(
            ILogger<FileUpdateService> logger,
            IFileUpdater fileUpdater,
            IApplicationControl applicationControl)
        {
            _logger = logger;
            FileUpdater = fileUpdater;
            _applicationControl = applicationControl;
            StartTimer();
        }

        private string _baseFolder =>
            Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;

        public string UpdateFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_updateFolderPath))
                    _updateFolderPath = Path.Combine(_baseFolder, "Updates");
                return _updateFolderPath;
            }
            set => _updateFolderPath = value;
        }

        public string CompletedFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_completedFolderPath))
                    _completedFolderPath = Path.Combine(_baseFolder, "Updates", "Completed");
                return _completedFolderPath;
            }
            set => _completedFolderPath = value;
        }

        public string NotProcessedFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_notProcessedFolderPath))
                    _notProcessedFolderPath = Path.Combine(_baseFolder, "Updates", "Not Processed");
                return _notProcessedFolderPath;
            }
            set => _notProcessedFolderPath = value;
        }

        public IFileUpdater FileUpdater { get; set; }

        public IFileUpdateResults UpdateFiles()
        {
            lock (_lock)
            {
                var fileUpdateResults1 = (IFileUpdateResults)new FileUpdateResults
                {
                    Success = true
                };
                Log("Starting FileUpdateService.LoadUpdateFiles...");
                FindZipFileUpdates(fileUpdateResults1);
                ReadManifestFiles(fileUpdateResults1);
                ValidateFiles(fileUpdateResults1);
                var fileUpdateResults2 = GetActiveZipFileUpdateResults(fileUpdateResults1);
                ValidateThereAreActiveUpdates(fileUpdateResults1, fileUpdateResults2);
                ValidateFileUpdater(fileUpdateResults1);
                ValidateNotNearExpectedRebootTime(fileUpdateResults1);
                ValidateCanShutDown(fileUpdateResults1, fileUpdateResults2);
                InitializeOutputFolders(fileUpdateResults1);
                ProcessFileUpdates(fileUpdateResults1, fileUpdateResults2);
                Log(fileUpdateResults1.ToString());
                Log("Ending FileUpdateService.LoadUpdateFiles");
                return fileUpdateResults1;
            }
        }

        private void ValidateFileUpdater(IFileUpdateResults fileUpdateResults)
        {
            if (!fileUpdateResults.Success || FileUpdater != null)
                return;
            fileUpdateResults.Success = false;
            fileUpdateResults.Status = FileUpdateHighLevelStatus.MissingFileUpdater;
            Log("Unable to process file updates.  Missing FileUpdater object.");
        }

        private void ValidateCanShutDown(
            IFileUpdateResults fileUpdateResults,
            List<IZipFileUpdateResult> activeZipFileUpdateResults)
        {
            if (!fileUpdateResults.Success)
                return;
            Log("Checking CanShutDown before running file update...");
            if (_applicationControl.CanShutDown(ShutDownReason.SoftwareUpdate))
                return;
            fileUpdateResults.Success = false;
            fileUpdateResults.Status = FileUpdateHighLevelStatus.UnableToShutDown;
            Log("Unable to run file update(s) because CanShutDown returned false. ");
        }

        private void ValidateNotNearExpectedRebootTime(IFileUpdateResults fileUpdateResults)
        {
            if (!fileUpdateResults.Success)
                return;
            var expectedRebootTime = FileUpdater?.GetExpectedRebootTime();
            if (expectedRebootTime.HasValue && DateIsWithinXMinutes(expectedRebootTime.Value, 30))
            {
                fileUpdateResults.Success = false;
                fileUpdateResults.Status = FileUpdateHighLevelStatus.NearExpectedRebootTime;
                Log(string.Format("{0} Expected Reboot Time: {1}",
                    "Unable to run file update(s) because of pending device reboot.", expectedRebootTime));
            }
            else
            {
                Log(string.Format("No immminent device reboot.  Expected Reboot Time: {0}", expectedRebootTime));
            }
        }

        private bool DateIsWithinXMinutes(DateTime dateTime, int minutes)
        {
            return DateTime.Now <= dateTime && DateTime.Now.AddMinutes(minutes) >= dateTime;
        }

        private void ValidateThereAreActiveUpdates(
            IFileUpdateResults fileUpdateResults,
            List<IZipFileUpdateResult> activeZipFileUpdateResults)
        {
            if (!fileUpdateResults.Success ||
                (activeZipFileUpdateResults != null && activeZipFileUpdateResults.Count() != 0))
                return;
            fileUpdateResults.Success = false;
            Log("There are no active updates.");
        }

        private void ValidateFiles(IFileUpdateResults fileUpdateResults)
        {
            if (fileUpdateResults.Success)
                foreach (var fileUpdateResult in fileUpdateResults.ZipFileUpdateResultCollection)
                    if (!ZipFileContainsAllUpdateFiles(fileUpdateResult))
                    {
                        fileUpdateResult.Status = ZipFileUpdateStatus.Errored;
                        fileUpdateResult.StatusMessage = "File missing from zip file.";
                    }

            UpdateFileUpdateResultStatus(fileUpdateResults);
        }

        private void UpdateFileUpdateResultStatus(IFileUpdateResults fileUpdateResults)
        {
            var stringBuilder = new StringBuilder();
            switch (fileUpdateResults.Status)
            {
                case FileUpdateHighLevelStatus.Normal:
                    if (fileUpdateResults.ZipFileUpdateResultCollection.Count == 0)
                        stringBuilder.Append("No files found to update.");
                    break;
                case FileUpdateHighLevelStatus.UnableToShutDown:
                    stringBuilder.Append("Unable to run file update(s) because CanShutDown returned false. ");
                    break;
                case FileUpdateHighLevelStatus.UnableToCreateCompletedFolder:
                    stringBuilder.Append("Unable to create completed updates folder. ");
                    break;
                case FileUpdateHighLevelStatus.MissingFileUpdater:
                    stringBuilder.Append("Unable to process file updates.  Missing FileUpdater object.");
                    break;
                case FileUpdateHighLevelStatus.NearExpectedRebootTime:
                    stringBuilder.Append("Unable to run file update(s) because of pending device reboot.");
                    break;
            }

            var statusGroups = fileUpdateResults.ZipFileUpdateResultCollection
                .GroupBy(x => x.Status)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                });

            foreach (var group in statusGroups) stringBuilder.Append($"{group.Count} zip file(s) {group.Status}; ");

            fileUpdateResults.Success = fileUpdateResults.Status == FileUpdateHighLevelStatus.Normal &&
                                        !statusGroups.Any(x => x.Status == ZipFileUpdateStatus.Errored && x.Count > 0);

            fileUpdateResults.StatusMessage = stringBuilder.ToString();
        }

        private List<IZipFileUpdateResult> GetActiveZipFileUpdateResults(
            IFileUpdateResults fileUpdateResults)
        {
            var fileUpdateResults1 = new List<IZipFileUpdateResult>();
            foreach (var fileUpdateResult in fileUpdateResults.ZipFileUpdateResultCollection)
                if (fileUpdateResult.Status == ZipFileUpdateStatus.NotProcessed)
                {
                    if (IsZipFileUpdatAfterStartDate(fileUpdateResult))
                    {
                        if (IsZipFileInUpdateTimeRange(fileUpdateResult))
                        {
                            fileUpdateResults1.Add(fileUpdateResult);
                        }
                        else
                        {
                            fileUpdateResult.Status = ZipFileUpdateStatus.Skipped;
                            fileUpdateResult.UpdateFileManifestProperties.Status =
                                ManifestPropertiesStatus.OutsideOfTimeRange;
                            fileUpdateResult.StatusMessage =
                                "Skipped because current time is outside update time range";
                            Log(string.Format(
                                "Skipping processing of zip file update {0} because current time is outside update time range {1} - {2}",
                                Path.GetFileName(fileUpdateResult.FileName),
                                fileUpdateResult.UpdateFileManifestProperties.UpdateRangeStart,
                                fileUpdateResult.UpdateFileManifestProperties.UpdateRangeEnd));
                        }
                    }
                    else
                    {
                        fileUpdateResult.Status = ZipFileUpdateStatus.Skipped;
                        fileUpdateResult.UpdateFileManifestProperties.Status =
                            ManifestPropertiesStatus.StartDateIsInTheFuture;
                        fileUpdateResult.StatusMessage = "Skipped because start date is in the future";
                        Log(string.Format(
                            "Skipping processing of zip file update {0} because start date {1} is in the future.",
                            Path.GetFileName(fileUpdateResult.FileName),
                            fileUpdateResult.UpdateFileManifestProperties.StartDate));
                    }
                }

            return fileUpdateResults1;
        }

        private void ProcessFileUpdates(
            IFileUpdateResults fileUpdateResults,
            List<IZipFileUpdateResult> activeZipFileUpdateResults)
        {
            if (fileUpdateResults.Success)
            {
                if (fileUpdateResults?.ZipFileUpdateResultCollection != null)
                {
                    var flag1 = false;
                    Log("Preparing to update files");
                    FileUpdater?.PrepareToUpdateFiles();
                    try
                    {
                        foreach (var fileUpdateResult1 in activeZipFileUpdateResults)
                            if (!flag1)
                            {
                                Log("Processing zip file updates in " + Path.GetFileName(fileUpdateResult1.FileName));
                                using (var zipArchive = ZipFile.OpenRead(fileUpdateResult1.FileName))
                                {
                                    var num = fileUpdateResult1.FileUpdateResultCollection.Max(x =>
                                        x.UpdateFileModel.Order);
                                    foreach (var fileUpdateResult2 in fileUpdateResult1.FileUpdateResultCollection
                                                 .OrderBy(x => x.UpdateFileModel.Order))
                                    {
                                        var flag2 = fileUpdateResult2.UpdateFileModel.Order == num;
                                        var str = flag2
                                            ? "  - Forcing device reboot because this is the last file in the update."
                                            : null;
                                        Log("    Processing file: " + fileUpdateResult2.UpdateFileModel.Path + str);
                                        var flag3 = false;
                                        var entry = zipArchive?.GetEntry(fileUpdateResult2.UpdateFileModel.Path);
                                        if (entry != null)
                                        {
                                            try
                                            {
                                                var stream = entry.Open();
                                                var flag4 = flag2 || fileUpdateResult2.UpdateFileModel.RebootRequired;
                                                var task = FileUpdater.WriteStream(stream,
                                                    fileUpdateResult2.UpdateFileModel.Path, flag4, true);
                                                task.Wait();
                                                flag3 = task.Result;
                                            }
                                            catch (Exception ex)
                                            {
                                                Log(string.Format("Exception while opening/reading stream.  {0}", ex));
                                            }

                                            Log(string.Format("    file update for {0} success = {1}",
                                                fileUpdateResult2.UpdateFileModel.Path, flag3));
                                            if (!flag3)
                                            {
                                                flag1 = true;
                                                fileUpdateResult2.FileUpdateStatus = FileUpdateStatus.Errored;
                                                fileUpdateResult1.Status = ZipFileUpdateStatus.Errored;
                                                fileUpdateResult1.StatusMessage = "Update of " +
                                                    fileUpdateResult2.UpdateFileModel.Path + " failed.";
                                                break;
                                            }

                                            fileUpdateResult2.FileUpdateStatus = FileUpdateStatus.Updated;
                                        }
                                        else
                                        {
                                            flag1 = true;
                                            fileUpdateResult2.FileUpdateStatus = FileUpdateStatus.Errored;
                                            fileUpdateResult1.Status = ZipFileUpdateStatus.Errored;
                                            fileUpdateResult1.StatusMessage = "    Unable to find zip archive entry " +
                                                                              fileUpdateResult2.UpdateFileModel.Path;
                                            break;
                                        }
                                    }
                                }

                                if (!flag1)
                                {
                                    Log(string.Format("Updating IUC285 file update revision number to {0}",
                                        fileUpdateResult1?.UpdateFileManifestProperties?.Revision));
                                    if (fileUpdateResult1?.UpdateFileManifestProperties != null &&
                                        FileUpdater.SetFileUpdateRevisionNumber(fileUpdateResult1
                                            .UpdateFileManifestProperties.Revision.ToString()))
                                    {
                                        CompleteUpdate(fileUpdateResult1);
                                        FileUpdater?.PostDeviceStatus();
                                    }
                                    else
                                    {
                                        fileUpdateResult1.Status = ZipFileUpdateStatus.Errored;
                                        fileUpdateResult1.StatusMessage = "Unable to update revision number on device.";
                                    }
                                }

                                Log("Finished Processing zip file updates in " +
                                    Path.GetFileName(fileUpdateResult1.FileName));
                            }
                            else
                            {
                                fileUpdateResult1.Status = ZipFileUpdateStatus.Skipped;
                                fileUpdateResult1.StatusMessage =
                                    "Skipped processing because prior update file failed.";
                                Log("Skipping processing of zip file updates in " +
                                    Path.GetFileName(fileUpdateResult1.FileName) +
                                    " because prior update file failed.");
                            }
                    }
                    finally
                    {
                        Log("Completing file updates.");
                        FileUpdater?.CompleteFileUpdates();
                    }
                }
            }
            else
            {
                MoveOldRevisionFiles(fileUpdateResults, activeZipFileUpdateResults);
            }

            UpdateFileUpdateResultStatus(fileUpdateResults);
        }

        private void MoveOldRevisionFiles(
            IFileUpdateResults fileUpdateResults,
            List<IZipFileUpdateResult> activeZipFileUpdateResults)
        {
            if (fileUpdateResults?.ZipFileUpdateResultCollection == null)
                return;
            foreach (var fileUpdateResult in fileUpdateResults.ZipFileUpdateResultCollection)
                if (fileUpdateResult.UpdateFileManifestProperties.Status == ManifestPropertiesStatus.RevisionDowngrade)
                {
                    if (!File.Exists(fileUpdateResult.FileName))
                        Log("Unable to move file " + fileUpdateResult.FileName + " because it no longer exists.");
                    else
                        MoveFile(fileUpdateResult, NotProcessedFolderPath);
                }
        }

        private bool IsZipFileUpdatAfterStartDate(IZipFileUpdateResult zipFileUpdateResult)
        {
            DateTime? startDate;
            int num;
            if (zipFileUpdateResult == null)
            {
                num = 1;
            }
            else
            {
                startDate = zipFileUpdateResult.UpdateFileManifestProperties?.StartDate;
                num = !startDate.HasValue ? 1 : 0;
            }

            if (num != 0)
                return true;
            var now = DateTime.Now;
            startDate = zipFileUpdateResult?.UpdateFileManifestProperties?.StartDate;
            return startDate.HasValue && now >= startDate.GetValueOrDefault();
        }

        private bool IsZipFileInUpdateTimeRange(IZipFileUpdateResult zipFileUpdateResult)
        {
            TimeSpan? nullable1;
            int num1;
            if (zipFileUpdateResult == null)
            {
                num1 = 1;
            }
            else
            {
                nullable1 = zipFileUpdateResult.UpdateFileManifestProperties?.UpdateRangeStart;
                num1 = !nullable1.HasValue ? 1 : 0;
            }

            if (num1 == 0)
            {
                int num2;
                if (zipFileUpdateResult == null)
                {
                    num2 = 1;
                }
                else
                {
                    var manifestProperties = zipFileUpdateResult.UpdateFileManifestProperties;
                    TimeSpan? nullable2;
                    if (manifestProperties == null)
                    {
                        nullable1 = new TimeSpan?();
                        nullable2 = nullable1;
                    }
                    else
                    {
                        nullable2 = manifestProperties.UpdateRangeEnd;
                    }

                    nullable1 = nullable2;
                    num2 = !nullable1.HasValue ? 1 : 0;
                }

                if (num2 == 0)
                {
                    var now = DateTime.Now;
                    var timeOfDay1 = now.TimeOfDay;
                    nullable1 = zipFileUpdateResult?.UpdateFileManifestProperties?.UpdateRangeStart;
                    if ((nullable1.HasValue ? timeOfDay1 >= nullable1.GetValueOrDefault() ? 1 : 0 : 0) == 0)
                        return false;
                    now = DateTime.Now;
                    var timeOfDay2 = now.TimeOfDay;
                    nullable1 = zipFileUpdateResult?.UpdateFileManifestProperties?.UpdateRangeEnd;
                    return nullable1.HasValue && timeOfDay2 <= nullable1.GetValueOrDefault();
                }
            }

            return true;
        }

        private bool ZipFileContainsAllUpdateFiles(IZipFileUpdateResult zipFileUpdateResult)
        {
            var flag = true;
            if (zipFileUpdateResult.FileUpdateResultCollection != null &&
                zipFileUpdateResult.FileUpdateResultCollection.Count > 0)
                using (var zipArchive = ZipFile.OpenRead(zipFileUpdateResult.FileName))
                {
                    foreach (var fileUpdateResult in zipFileUpdateResult.FileUpdateResultCollection)
                        if (zipArchive.GetEntry(fileUpdateResult.UpdateFileModel.Path) == null)
                        {
                            fileUpdateResult.FileUpdateStatus = FileUpdateStatus.FileMissing;
                            flag = false;
                        }
                }

            return flag;
        }

        private void FindZipFileUpdates(IFileUpdateResults fileUpdateResults)
        {
            Log("Looking for update zip files in: " + UpdateFolderPath);
            if (Directory.Exists(UpdateFolderPath))
            {
                var flag = false;
                foreach (var enumerateFile in Directory.EnumerateFiles(UpdateFolderPath, "*.zip"))
                {
                    Log("Found update file: " + Path.GetFileName(enumerateFile));
                    flag = true;
                    fileUpdateResults.ZipFileUpdateResultCollection.Add(new ZipFileUpdateResult
                    {
                        FileName = enumerateFile,
                        Status = ZipFileUpdateStatus.NotProcessed,
                        StatusMessage = "Not Processed"
                    });
                }

                if (flag)
                    return;
                fileUpdateResults.StatusMessage = "No files found to update.";
                Log("No files found to update.");
            }
            else
            {
                Log("Update Folder " + UpdateFolderPath + " does not exist. No files found to update.");
            }
        }

        private void ReadManifestFiles(IFileUpdateResults fileUpdateResults)
        {
            foreach (var fileUpdateResult1 in fileUpdateResults.ZipFileUpdateResultCollection)
            {
                List<IFileModel> fileModels;
                IUpdateFileManifestProperties manifestData;
                ReadManifestFile(fileUpdateResult1.FileName, out fileModels, out manifestData);
                fileUpdateResult1.UpdateFileManifestProperties = manifestData;
                if (fileUpdateResult1.UpdateFileManifestProperties.Status != ManifestPropertiesStatus.Normal)
                {
                    fileUpdateResult1.StatusMessage = string.Format("Error in Manifest file.  Manifest Status: {0}",
                        fileUpdateResult1.UpdateFileManifestProperties.Status);
                    fileUpdateResult1.Status = ZipFileUpdateStatus.Errored;
                }

                if (fileModels != null)
                    foreach (var fileModel in fileModels.OrderBy(x => x.Order))
                        if (fileModel != null)
                        {
                            var fileUpdateResult2 = (IFileUpdateResult)new FileUpdateResult
                            {
                                FileUpdateStatus = FileUpdateStatus.NotProcessed,
                                UpdateFileModel = new FileModel
                                {
                                    Order = fileModel.Order,
                                    Path = fileModel.Path,
                                    RebootRequired = fileModel.RebootRequired
                                }
                            };
                            if (Path.GetFileName(fileUpdateResult2?.UpdateFileModel?.Path).Length > 15)
                            {
                                fileUpdateResult2.FileUpdateStatus = FileUpdateStatus.FileNameTooLong;
                                fileUpdateResult1.Status = ZipFileUpdateStatus.Errored;
                                fileUpdateResult1.StatusMessage = "Error in Manifest file.  File Name is too long " +
                                                                  Path.GetFileName(fileUpdateResult2.UpdateFileModel
                                                                      .Path);
                            }

                            fileUpdateResult1.FileUpdateResultCollection.Add(fileUpdateResult2);
                        }
            }

            if (fileUpdateResults.ZipFileUpdateResultCollection.Count > 0)
            {
                fileUpdateResults.ZipFileUpdateResultCollection.Sort((a, b) =>
                {
                    var num = 0;
                    if (a?.UpdateFileManifestProperties != null && b?.UpdateFileManifestProperties != null)
                        num = a.UpdateFileManifestProperties.Revision.CompareTo(b.UpdateFileManifestProperties
                            .Revision);
                    return num;
                });
                Log("Sorted update files by revision: " + string.Join(", ",
                    fileUpdateResults.ZipFileUpdateResultCollection.Select(eachZipFileUpdate =>
                        string.Format("{0} revision: {1}", Path.GetFileName(eachZipFileUpdate?.FileName),
                            eachZipFileUpdate != null
                                ? eachZipFileUpdate.UpdateFileManifestProperties.Revision
                                : (object)-1)).ToArray()));
            }

            UpdateFileUpdateResultStatus(fileUpdateResults);
        }

        private void InitializeOutputFolders(IFileUpdateResults fileUpdateResults)
        {
            if (!Directory.Exists(CompletedFolderPath))
                try
                {
                    Log("Creating FileUpdateService CompletedFolderPath: " + CompletedFolderPath);
                    Directory.CreateDirectory(CompletedFolderPath);
                }
                catch (Exception ex)
                {
                    Log(string.Format("Unable to create FileUpdateService CompletedFolderPath: {0}.  {1}",
                        CompletedFolderPath, ex));
                    fileUpdateResults.Success = false;
                    fileUpdateResults.Status = FileUpdateHighLevelStatus.UnableToCreateCompletedFolder;
                }

            if (Directory.Exists(NotProcessedFolderPath))
                return;
            try
            {
                Log("Creating FileUpdateService NotProcessedFolderPath: " + NotProcessedFolderPath);
                Directory.CreateDirectory(NotProcessedFolderPath);
            }
            catch (Exception ex)
            {
                Log(string.Format("Unable to create FileUpdateService NotProcessedFolderPath: {0}.  {1}",
                    NotProcessedFolderPath, ex));
                fileUpdateResults.Success = false;
                fileUpdateResults.Status = FileUpdateHighLevelStatus.UnableToCreateNotProcessedFolder;
            }
        }

        public void ReadManifestFile(
            string zipFileName,
            out List<IFileModel> fileModels,
            out IUpdateFileManifestProperties manifestData)
        {
            manifestData = new UpdateFileManifestProperties
            {
                Status = ManifestPropertiesStatus.Normal
            };
            fileModels = new List<IFileModel>();
            try
            {
                if (!File.Exists(zipFileName))
                    Log("GetManifestData No File Found at location: " + zipFileName);
                else
                    using (var zipArchive = ZipFile.OpenRead(zipFileName))
                    {
                        var entry = zipArchive.GetEntry("manifest.json");
                        if (entry == null)
                        {
                            manifestData.Status = ManifestPropertiesStatus.MissingManifestFile;
                            Log("GetManifestData No Manifest File Found in zip file update: " + zipFileName);
                        }
                        else
                        {
                            var updateFileManifest = (UpdateFileManifest)null;
                            using (var streamReader = new StreamReader(entry.Open()))
                            {
                                var jsonSerializer = new JsonSerializer();
                                var serializerSettings = new JsonSerializerSettings();
                                try
                                {
                                    updateFileManifest =
                                        (UpdateFileManifest)jsonSerializer.Deserialize(streamReader,
                                            typeof(UpdateFileManifest));
                                    if (updateFileManifest != null)
                                    {
                                        if (updateFileManifest.Revision <= 0)
                                        {
                                            manifestData.Status = ManifestPropertiesStatus.MissingRevisionNumber;
                                            Log("Manifest Revision is missing.");
                                        }
                                        else
                                        {
                                            var fileUpdater = FileUpdater;
                                            var num = fileUpdater != null
                                                ? fileUpdater.GetFileUpdateRevisionNumber()
                                                : -1;
                                            if (updateFileManifest != null &&
                                                num.CompareTo(updateFileManifest.Revision) >= 0)
                                            {
                                                manifestData.Status = ManifestPropertiesStatus.RevisionDowngrade;
                                                Log(string.Format("{0}  Current revision: {1}  Manifest revision: {2}",
                                                    "Current manifest file revision is greater than or equal to the incoming manifest file. Erroring update.",
                                                    num, updateFileManifest.Revision));
                                            }
                                            else
                                            {
                                                Log(string.Format("Device current revision: {0}", num));
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    manifestData.Status = ManifestPropertiesStatus.CorruptedManifestFile;
                                    LogError(string.Format(
                                        "Error deserializing manifest file in zip file: {0}{1} Exception: {2}",
                                        zipFileName, Environment.NewLine, ex));
                                }

                                if (updateFileManifest != null)
                                {
                                    var status = manifestData.Status;
                                    manifestData = new UpdateFileManifestProperties
                                    {
                                        Name = updateFileManifest.Name,
                                        Status = status,
                                        Revision = updateFileManifest.Revision,
                                        Id = updateFileManifest.Id,
                                        StartDate = updateFileManifest.StartDate,
                                        UpdateRangeStart = updateFileManifest.UpdateRangeStart,
                                        UpdateRangeEnd = updateFileManifest.UpdateRangeEnd
                                    };
                                    if (updateFileManifest?.FileModels == null || !updateFileManifest.FileModels.Any())
                                    {
                                        manifestData.Status = ManifestPropertiesStatus.ManifestHasNoUpdateFiles;
                                        Log(string.Format(
                                            "GetManifestData No Files to update with manifest {0} - {1} in file: {2}",
                                            manifestData.Id, manifestData.Name, zipFileName));
                                    }
                                    else
                                    {
                                        foreach (var fileModel in updateFileManifest.FileModels)
                                            fileModels.Add(new FileModel
                                            {
                                                Order = fileModel.Order,
                                                Path = fileModel.Path,
                                                RebootRequired = fileModel.RebootRequired
                                            });
                                    }
                                }
                                else
                                {
                                    if (updateFileManifest != null)
                                        return;
                                    manifestData.Status = ManifestPropertiesStatus.CorruptedManifestFile;
                                    Log("Error: Unable to deserialize manifest file from file update " + zipFileName);
                                }
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                manifestData.Status = ManifestPropertiesStatus.ExceptionWhileReading;
                LogError(string.Format("Error Reading Zip Update File: {0}{1}Exception: {2}", zipFileName,
                    Environment.NewLine, ex));
            }
        }

        private void CompleteUpdate(IZipFileUpdateResult zipFileUpdateResult)
        {
            zipFileUpdateResult.Status = ZipFileUpdateStatus.SuccessfullyProcessed;
            zipFileUpdateResult.StatusMessage = "File successfully updated.";
            if (!File.Exists(zipFileUpdateResult.FileName))
                Log("Unable to move file " + zipFileUpdateResult.FileName + " because it no longer exists.");
            else
                MoveFile(zipFileUpdateResult, CompletedFolderPath);
        }

        private void MoveFile(IZipFileUpdateResult zipFileUpdateResult, string destinationFolderName)
        {
            try
            {
                var str = Path.Combine(destinationFolderName, Path.GetFileName(zipFileUpdateResult.FileName));
                if (File.Exists(str))
                    str = GetUniqueFileName(str);
                File.Move(zipFileUpdateResult.FileName, str);
                Log("Moved file " + Path.GetFileName(zipFileUpdateResult.FileName) + " to folder " + str);
            }
            catch (Exception ex)
            {
                LogError(string.Format("Failed To Move Update file {0}{1}Exception: {2}", zipFileUpdateResult?.FileName,
                    Environment.NewLine, ex));
            }
        }

        private static string GetUniqueFileName(string destPath)
        {
            var withoutExtension = Path.GetFileNameWithoutExtension(destPath);
            var directoryName = Path.GetDirectoryName(destPath);
            var extension = Path.GetExtension(destPath);
            var path2 = withoutExtension + "{0}" + extension;
            var format = Path.Combine(directoryName, path2);
            var num = 1;
            var path = string.Format(format, "");
            while (File.Exists(path))
                path = string.Format(format, "(" + num++ + ")");
            destPath = path;
            return destPath;
        }

        private void Log(string message)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogInformation(message);
        }

        private void LogError(string message)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogError(message);
        }

        private void StartTimer()
        {
            var totalMilliseconds = TimeSpan.FromMinutes(15.0).TotalMilliseconds;
            _timer = new Timer
            {
                Interval = totalMilliseconds,
                AutoReset = true,
                Enabled = false
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            try
            {
                UpdateFiles();
            }
            finally
            {
                _timer.Start();
            }
        }

        public class Constants
        {
            public const string ManifestFileName = "manifest.json";
            public const string UpdateFolderName = "Updates";
            public const string CompletedFolderName = "Completed";
            public const string NotProcessedFolderName = "Not Processed";

            public const string MissingFileUpdaterMessage =
                "Unable to process file updates.  Missing FileUpdater object.";

            public const string FileSuccessfullyUpdated = "File successfully updated.";
            public const string NoFilesToUpdateMessage = "No files found to update.";
            public const string NoActiveUpdatesMessage = "There are no active updates.";

            public const string UnableToShutDownMessage =
                "Unable to run file update(s) because CanShutDown returned false. ";

            public const string UnableToCreateCompletedFolderMessage = "Unable to create completed updates folder. ";

            public const string NearExpectedRebootTimeMessage =
                "Unable to run file update(s) because of pending device reboot.";

            public const string RevisionDowngradeMessage =
                "Current manifest file revision is greater than or equal to the incoming manifest file. Erroring update.";

            public const string UpdateOfRevisionNumberFailedMessage = "Unable to update revision number on device.";
            public const string MissingManifestRevisionMessage = "Manifest Revision is missing.";
        }
    }
}