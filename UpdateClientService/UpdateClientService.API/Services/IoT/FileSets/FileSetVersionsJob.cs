using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.IoT.FileSets
{
    public class FileSetVersionsJob : IFileSetVersionsJob, IInvocable
    {
        private readonly IFileSetService _fileSetService;
        private readonly ILogger<FileSetVersionsJob> _logger;

        public FileSetVersionsJob(ILogger<FileSetVersionsJob> logger, IFileSetService fileSetService)
        {
            _logger = logger;
            _fileSetService = fileSetService;
        }

        public async Task Invoke()
        {
            _logger.LogInfoWithSource("ProcessPendingFileSetVersions",
                "/sln/src/UpdateClientService.API/Services/IoT/FileSets/FileSetVersionsJob.cs");
            var versionsResponse = await _fileSetService.ProcessPendingFileSetVersions();
        }
    }
}