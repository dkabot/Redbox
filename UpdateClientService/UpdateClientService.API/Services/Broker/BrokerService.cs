using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Controllers.Models;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.Broker
{
    public class BrokerService : IBrokerService
    {
        private readonly IIoTCommandClient _ioTCommandClient;
        private readonly ILogger<BrokerService> _logger;
        private readonly IPingStatisticsService _pingStatisticsService;

        public BrokerService(
            ILogger<BrokerService> logger,
            IIoTCommandClient ioTCommandClient,
            IPingStatisticsService pingStatisticsService)
        {
            _logger = logger;
            _ioTCommandClient = ioTCommandClient;
            _pingStatisticsService = pingStatisticsService;
        }

        public async Task<IActionResult> Ping(
            string appName,
            string messageId,
            PingRequest pingRequest)
        {
            try
            {
                var parameters = new PerformIoTCommandParameters
                {
                    IoTTopic = "$aws/rules/kioskping",
                    WaitForResponse = false
                };
                var ioTcommandClient = _ioTCommandClient;
                var request = new IoTCommandModel();
                request.Version = 1;
                request.RequestId = messageId;
                request.AppName = appName;
                request.Command = CommandEnum.KioskPing;
                request.Payload = pingRequest;
                request.QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce;
                var parameters1 = parameters;
                var ioTCommandResponse = await ioTcommandClient.PerformIoTCommand<ObjectResult>(request, parameters1);
                var statisticsService = _pingStatisticsService;
                var tcommandResponse = ioTCommandResponse;
                var num1 = tcommandResponse != null ? tcommandResponse.StatusCode == 200 ? 1 : 0 : 0;
                var num2 = await statisticsService.RecordPingStatistic(num1 != 0) ? 1 : 0;
                return ProcessIoTCommandResponse(ioTCommandResponse, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception executing Ping",
                    "/sln/src/UpdateClientService.API/Services/Broker/BrokerService.cs");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> Register(
            string appName,
            string messageId,
            RegisterRequest request)
        {
            try
            {
                var parameters = new PerformIoTCommandParameters
                {
                    IoTTopic = "$aws/rules/kioskrestcall",
                    WaitForResponse = true
                };
                var ioTcommandClient = _ioTCommandClient;
                var request1 = new IoTCommandModel();
                request1.Version = 1;
                request1.RequestId = messageId;
                request1.AppName = appName;
                request1.Command = CommandEnum.BrokerRegister;
                request1.Payload = request;
                request1.QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce;
                var parameters1 = parameters;
                return ProcessIoTCommandResponse(
                    await ioTcommandClient.PerformIoTCommand<ObjectResult>(request1, parameters1), parameters);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception executing Register",
                    "/sln/src/UpdateClientService.API/Services/Broker/BrokerService.cs");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> Unregister(
            string appName,
            string messageId,
            UnRegisterRequest unregisterRequest)
        {
            try
            {
                var parameters = new PerformIoTCommandParameters
                {
                    IoTTopic = "$aws/rules/kioskrestcall",
                    WaitForResponse = true
                };
                var ioTcommandClient = _ioTCommandClient;
                var request = new IoTCommandModel();
                request.Version = 1;
                request.RequestId = messageId;
                request.AppName = appName;
                request.Command = CommandEnum.BrokerUnRegister;
                request.Payload = unregisterRequest;
                request.QualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce;
                var parameters1 = parameters;
                return ProcessIoTCommandResponse(
                    await ioTcommandClient.PerformIoTCommand<ObjectResult>(request, parameters1), parameters);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception executing Unregister",
                    "/sln/src/UpdateClientService.API/Services/Broker/BrokerService.cs");
                return new StatusCodeResult(500);
            }
        }

        private StatusCodeResult ProcessIoTCommandResponse<T>(
            IoTCommandResponse<T> response,
            PerformIoTCommandParameters parameters = null)
            where T : ObjectResult
        {
            if (parameters == null || parameters.WaitForResponse)
                return ToStatusCodeResult(response.Payload);
            return ToStatusCodeResult(new ObjectResult(null)
            {
                StatusCode = response != null ? response.StatusCode : 500
            });
        }

        private StatusCodeResult ToStatusCodeResult(object objectResult)
        {
            return new StatusCodeResult(
                (objectResult is ObjectResult objectResult1 ? objectResult1.StatusCode : new int?()) ?? 500);
        }
    }
}