using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Utilities
{
    public class CleanupService : ICleanupService, IInvocable
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger<CleanupService> _logger;

        public CleanupService(IOptions<AppSettings> appSettings, ILogger<CleanupService> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task Invoke()
        {
            await DoWork();
        }

        private async Task DoWork()
        {
            await _lock.WaitAsync();
            _logger.LogInfoWithSource("Begin Cleanup of Old Files.",
                "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
            var cleanUpFiles = _appSettings.Value.CleanUpFiles;
            if (!cleanUpFiles.Any())
            {
                _logger.LogInfoWithSource("No Files Defined For Cleanup.",
                    "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
            }
            else
            {
                var num1 = 0;
                foreach (var cleanupFiles in cleanUpFiles)
                    try
                    {
                        if (!Directory.Exists(cleanupFiles.Path))
                        {
                            _logger.LogErrorWithSource(
                                string.Format("Skipping files in folder {0} with maxage {1} - Directory doesn't exist.",
                                    cleanupFiles.Path, cleanupFiles.MaxAge),
                                "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                        }
                        else
                        {
                            var files = Directory.GetFiles(cleanupFiles.Path);
                            _logger.LogInfoWithSource(
                                string.Format("Cleanup Folder {0} with {1} files.", cleanupFiles.Path, files.Count()),
                                "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                            var num2 = 0;
                            foreach (var path in files)
                                try
                                {
                                    var lastWriteTime = File.GetLastWriteTime(path);
                                    var date = lastWriteTime.Date;
                                    var dateTime1 = DateTime.Now;
                                    dateTime1 = dateTime1.Date;
                                    var dateTime2 = dateTime1.AddDays(-1 * cleanupFiles.MaxAge);
                                    if (date < dateTime2)
                                    {
                                        var logger = _logger;
                                        var str1 = path;
                                        dateTime1 = DateTime.Now;
                                        var days = (dateTime1.Date - lastWriteTime.Date).Days;
                                        var str2 = string.Format("Deleting File: {0} - it is {1} days old.", str1,
                                            days);
                                        _logger.LogInfoWithSource(str2,
                                            "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                                        File.Delete(path);
                                        ++num2;
                                    }
                                    else
                                    {
                                        var logger = _logger;
                                        var str3 = path;
                                        dateTime1 = DateTime.Now;
                                        var days = (dateTime1.Date - lastWriteTime.Date).Days;
                                        var str4 = string.Format("Skipping File: {0} - it is {1} days old.", str3,
                                            days);
                                        _logger.LogDebugWithSource(str4,
                                            "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogErrorWithSource(ex,
                                        string.Format(
                                            "Unhandled Exception when deleting files in folder {0} with maxage {1}",
                                            cleanupFiles.Path, cleanupFiles.MaxAge),
                                        "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                                }

                            num1 += num2;
                            var num3 = Directory.GetFiles(cleanupFiles.Path).Count();
                            _logger.LogInfoWithSource(
                                string.Format(
                                    "Cleanup Folder {0} Completed with {1} files remaining. {2} files were deleted.",
                                    cleanupFiles.Path, num3, num2),
                                "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex,
                            string.Format("Unhandled Exception when deleting files in folder {0} with maxage {1}",
                                cleanupFiles.Path, cleanupFiles.MaxAge),
                            "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
                    }

                _logger.LogInfoWithSource(string.Format("End Cleanup of Old Files. {0} files were deleted.", num1),
                    "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
            }
        }
    }
}