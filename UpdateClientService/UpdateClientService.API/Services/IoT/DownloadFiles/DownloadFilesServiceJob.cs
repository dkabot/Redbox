using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT.DownloadFiles
{
    public class DownloadFilesServiceJob : IInvocable
    {
        private readonly IDownloadFilesService _downloadFilesService;
        private readonly ILogger<DownloadFilesServiceJob> _logger;
        private readonly DownloadFilesSettings _settings;

        public DownloadFilesServiceJob(
            ILogger<DownloadFilesServiceJob> logger,
            IDownloadFilesService downloadFilesService,
            IOptionsMonitor<AppSettings> settings)
        {
            _logger = logger;
            _downloadFilesService = downloadFilesService;
            _settings = settings.CurrentValue.DownloadFiles;
        }

        public async Task Invoke()
        {
            var settings = _settings;
            if ((settings != null ? settings.Enabled ? 1 : 0 : 0) == 0)
                return;
            _logger.LogInfoWithSource("HandleScheduledJobs",
                "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesServiceJob.cs");
            await _downloadFilesService.HandleScheduledJobs();
        }
    }
}