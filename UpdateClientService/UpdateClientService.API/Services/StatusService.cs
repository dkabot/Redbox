using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services
{
    public class StatusService : IStatusService
    {
        private const string FILE_NOT_FOUND = "FileNotFound";
        private readonly ILogger<StatusService> _logger;
        private readonly IOptionsMonitor<AppSettings> _settings;

        public StatusService(ILogger<StatusService> logger, IOptionsMonitor<AppSettings> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public FileVersionDataResponse GetFileVersions()
        {
            var fileVersions = new FileVersionDataResponse();
            var fileVersionPaths = _settings?.CurrentValue?.FileVersionPaths;
            if (fileVersionPaths != null && fileVersionPaths.Any())
                return GetFileVersions(fileVersionPaths);
            _logger.LogErrorWithSource("No filepaths found or specified.",
                "/sln/src/UpdateClientService.API/Services/Status/StatusService.cs");
            fileVersions.StatusCode = HttpStatusCode.BadRequest;
            return fileVersions;
        }

        public FileVersionDataResponse GetFileVersions(IEnumerable<string> filepaths)
        {
            var fileVersions = new FileVersionDataResponse();
            try
            {
                var results = new List<FileVersionData>();
                filepaths.AsParallel().ForAll(file =>
                {
                    if (File.Exists(file))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(file);
                        results.Add(new FileVersionData
                        {
                            Name = versionInfo.FileName,
                            Version = versionInfo.FileVersion
                        });
                    }
                    else
                    {
                        results.Add(new FileVersionData
                        {
                            Name = Path.GetFileName(file),
                            Version = "FileNotFound"
                        });
                    }
                });
                fileVersions.Data = results;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Unhandled exception occurred in StatusService.GetFileVersions",
                    "/sln/src/UpdateClientService.API/Services/Status/StatusService.cs");
                fileVersions.StatusCode = HttpStatusCode.InternalServerError;
            }

            return fileVersions;
        }
    }
}