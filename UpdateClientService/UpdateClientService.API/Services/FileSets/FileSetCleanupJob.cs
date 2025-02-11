using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets
{
    public class FileSetCleanupJob : IFileSetCleanupJob, IInvocable
    {
        private readonly IFileSetCleanup _fileSetCleanup;
        private readonly ILogger<FileSetCleanupJob> _logger;

        public FileSetCleanupJob(IFileSetCleanup fileSetCleanup, ILogger<FileSetCleanupJob> logger)
        {
            _fileSetCleanup = fileSetCleanup;
            _logger = logger;
        }

        public async Task Invoke()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                _logger.LogInfoWithSource("Starting FileSetCleanup",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanupJob.cs");
                await _fileSetCleanup.Run();
                _logger.LogInfoWithSource(string.Format("Finished FileSetCleanup in {0} ms", sw.ElapsedMilliseconds),
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanupJob.cs");
                sw = null;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while running FileSet cleanup.",
                    "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanupJob.cs");
            }
        }
    }
}