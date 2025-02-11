using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class GetIoTStatisticsSummary : ICommandIoTController
    {
        private readonly IIoTStatisticsService _ioTStatisticsService;
        private readonly ILogger<GetIoTStatisticsSummary> _logger;
        private readonly IMqttProxy _mqttProxy;
        private readonly IStoreService _store;

        public GetIoTStatisticsSummary(
            ILogger<GetIoTStatisticsSummary> logger,
            IIoTStatisticsService ioTStatisticsService,
            IStoreService store,
            IMqttProxy mqttProxy)
        {
            _logger = logger;
            _ioTStatisticsService = ioTStatisticsService;
            _store = store;
            _mqttProxy = mqttProxy;
        }

        public CommandEnum CommandEnum => CommandEnum.GetIotStatisticsSummary;

        public int Version => 2;

        public async Task Execute(IoTCommandModel ioTCommandRequest)
        {
            try
            {
                _logger.LogInfoWithSource("Getting IoTStatisticsSummary",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetIoTStatisticsSummary.cs");
                var tstatisticsSummaryResponse = await _ioTStatisticsService.GetIoTStatisticsSummaryResponse();
                var mqttResponse = new MqttResponse<List<IoTStatisticsSummary>>();
                if (tstatisticsSummaryResponse != null && tstatisticsSummaryResponse.StatusCode == HttpStatusCode.OK &&
                    tstatisticsSummaryResponse.ioTStatisticsSummaries != null)
                    mqttResponse.Data = tstatisticsSummaryResponse.ioTStatisticsSummaries;
                else
                    mqttResponse.Error = "Error getting IoTStatisticsSummary";
                var json = mqttResponse.ToJson();
                var num = await _mqttProxy.PublishIoTCommandAsync(
                    "redbox/updateservice-instance/" + ioTCommandRequest.SourceId + "/request", new IoTCommandModel
                    {
                        RequestId = ioTCommandRequest.RequestId,
                        Command = CommandEnum,
                        MessageType = MessageTypeEnum.Response,
                        Version = Version,
                        SourceId = _store?.KioskId.ToString(),
                        Payload = json,
                        LogRequest = true
                    })
                    ? 1
                    : 0;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception getting IoT statistics summary",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/GetIoTStatisticsSummary.cs");
            }
        }
    }
}