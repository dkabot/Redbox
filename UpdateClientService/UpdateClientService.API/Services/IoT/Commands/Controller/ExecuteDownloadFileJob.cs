using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class ExecuteDownloadFileJob : ICommandIoTController
    {
        private readonly IDownloadFilesService _downloadFilesService;
        private readonly ILogger<ExecuteDownloadFileJob> _logger;
        private readonly IStoreService _store;

        public ExecuteDownloadFileJob(
            IDownloadFilesService downloadFilesService,
            IStoreService store,
            ILogger<ExecuteDownloadFileJob> logger)
        {
            _downloadFilesService = downloadFilesService;
            _store = store;
            _logger = logger;
        }

        public CommandEnum CommandEnum => CommandEnum.ExecuteDownloadFileJob;

        public int Version => 1;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            try
            {
                var requestModel = JsonConvert.DeserializeObject<DownloadFileJob>(ioTCommand.Payload.ToString());
                if (IsDownloadFileJobValid(requestModel))
                {
                    await _downloadFilesService.HandleDownloadFileJob(requestModel);
                    DownloadFileJobExecutionState.Executions.TryRemove(requestModel.DownloadFileJobId, out var _);
                }

                requestModel = null;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var ioTcommandModel = ioTCommand;
                var str = "Exception while executing command " +
                          (ioTcommandModel != null ? ioTcommandModel.ToJson() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
            }
        }

        private bool IsDownloadFileJobValid(DownloadFileJob job)
        {
            var downloadFileJobId = job.DownloadFileJobId;
            if (job == null)
            {
                _logger.LogErrorWithSource("Job cannot be null",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
                return false;
            }

            if (!job.TargetKiosks.Contains(_store.KioskId))
                _logger.LogErrorWithSource(string.Format("Job's target kiosks does not include {0}", _store.KioskId),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
            if (string.IsNullOrWhiteSpace(downloadFileJobId))
            {
                _logger.LogErrorWithSource("DownloadFileJobId cannot be null.",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
                return false;
            }

            if (DownloadFileJobExecutionState.Executions.TryAdd(downloadFileJobId, true))
                return true;
            _logger.LogInfoWithSource(
                "Another job with DownloadFileJobId " + downloadFileJobId + " is already in progress. Skipping...",
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
            return false;
        }
    }
}