using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class CancelDownloadFileJob : ICommandIoTController
    {
        private readonly IDownloadFilesService _downloadFilesService;
        private readonly ILogger<CancelDownloadFileJob> _logger;
        private readonly IStoreService _store;

        public CancelDownloadFileJob(
            IDownloadFilesService downloadFilesService,
            IStoreService store,
            ILogger<CancelDownloadFileJob> logger)
        {
            _downloadFilesService = downloadFilesService;
            _store = store;
            _logger = logger;
        }

        public CommandEnum CommandEnum => CommandEnum.CancelDownloadFileJob;

        public int Version => 1;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            try
            {
                var job = JsonConvert.DeserializeObject<DownloadFileJob>(ioTCommand.Payload.ToString());
                if (!IsDownloadFileJobValid(job))
                    return;
                await _downloadFilesService.CancelDownloadFileJob(job);
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var ioTcommandModel = ioTCommand;
                var str = "Exception while executing command " +
                          (ioTcommandModel != null ? ioTcommandModel.ToJson() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
            }
        }

        private bool IsDownloadFileJobValid(DownloadFileJob job)
        {
            var downloadFileJobId = job.DownloadFileJobId;
            if (job == null)
            {
                _logger.LogErrorWithSource("Job cannot be null",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
                return false;
            }

            if (!job.TargetKiosks.Contains(_store.KioskId))
                _logger.LogErrorWithSource(string.Format("Job's target kiosks does not include {0}", _store.KioskId),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
            if (string.IsNullOrWhiteSpace(downloadFileJobId))
            {
                _logger.LogErrorWithSource("DownloadFileJobId cannot be null.",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
                return false;
            }

            DownloadFileJobExecutionState.Executions.TryAdd(downloadFileJobId, true);
            return true;
        }
    }
}