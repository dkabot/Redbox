using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.IoT.FileSets;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class TriggerReportFileSetVersions : ICommandIoTController
    {
        private readonly IFileSetService _fileSetService;
        private readonly ILogger<TriggerReportFileSetVersions> _logger;

        public TriggerReportFileSetVersions(
            IFileSetService fileSetService,
            ILogger<TriggerReportFileSetVersions> logger)
        {
            _fileSetService = fileSetService;
            _logger = logger;
        }

        public CommandEnum CommandEnum { get; } = CommandEnum.TriggerReportFileSetVersions;

        public int Version { get; } = 1;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            var triggerReportFileSetVersionsRequest =
                JsonConvert.DeserializeObject<TriggerReportFileSetVersionsRequest>(ioTCommand.Payload.ToJson());
            try
            {
                var versionsResponse =
                    await _fileSetService.TriggerProcessPendingFileSetVersions(triggerReportFileSetVersionsRequest);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception trying to trigger ReportFileSetVersions",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/TriggerReportFileSetVersions.cs");
            }
        }
    }
}