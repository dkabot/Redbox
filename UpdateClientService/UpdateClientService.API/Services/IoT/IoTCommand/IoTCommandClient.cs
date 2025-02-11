using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API.Services.IoT.IoTCommand
{
    public class IoTCommandClient : IIoTCommandClient
    {
        private readonly IActiveResponseService _activeResponseService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<IoTCommandClient> _logger;
        private readonly IMqttProxy _mqttRepo;
        private readonly TimeSpan _requestTimeoutDefault = TimeSpan.FromSeconds(20.0);
        private readonly IStoreService _store;

        public IoTCommandClient(
            IMqttProxy mqttRepo,
            IStoreService store,
            IOptions<AppSettings> appSettings,
            ILogger<IoTCommandClient> logger,
            IActiveResponseService activeResponseService)
        {
            _mqttRepo = mqttRepo;
            _store = store;
            _appSettings = appSettings.Value;
            _logger = logger;
            _activeResponseService = activeResponseService;
        }

        public async Task<string> PerformIoTCommandWithStringResult(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters)
        {
            return (await PerformIoTCommand(request, parameters))?.Payload?.ToString();
        }

        public async Task<IoTCommandResponse<T>> PerformIoTCommand<T>(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters = null)
            where T : ObjectResult
        {
            var resultPayload = default(T);
            if (parameters == null)
                parameters = new PerformIoTCommandParameters();
            var ioTcommandModel = await PerformIoTCommand(request, parameters);
            if (parameters.WaitForResponse)
            {
                try
                {
                    resultPayload = JsonConvert.DeserializeObject<T>(ioTcommandModel?.Payload?.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        "Couldn't deserialize the response from UpdateService for request id " + request?.RequestId,
                        "/sln/src/UpdateClientService.API/Services/IoT/IoTCommand/IoTCommandClient.cs");
                }

                return new IoTCommandResponse<T>
                {
                    StatusCode = resultPayload?.StatusCode ?? 500,
                    Payload = resultPayload
                };
            }

            bool result;
            bool.TryParse(ioTcommandModel?.Payload?.ToString(), out result);
            var num = result ? 200 : 500;
            return new IoTCommandResponse<T>
            {
                StatusCode = num
            };
        }

        private async Task<IoTCommandModel> PerformIoTCommand(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(request.SourceId))
                request.SourceId = _store.KioskId.ToString();
            AdjustIoTTopicIfNeeded(request, parameters);
            var tcommandParameters = parameters;
            return (tcommandParameters != null ? tcommandParameters.WaitForResponse ? 1 : 0 : 0) != 0
                ? await PublishIoTCommandAndWaitForResponse(request, parameters)
                : await PublishIoTCommandAndReturn(request, parameters);
        }

        private async Task<IoTCommandModel> PublishIoTCommandAndReturn(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters)
        {
            var flag = await PublishIoTCommandAsync(request, parameters);
            return new IoTCommandModel
            {
                RequestId = request.RequestId,
                MessageType = MessageTypeEnum.Response,
                Command = request.Command,
                Version = request.Version,
                Payload = flag.ToString(),
                SourceId = _store.KioskId.ToString()
            };
        }

        private async Task<IoTCommandModel> PublishIoTCommandAndWaitForResponse(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters)
        {
            var response = (IoTCommandModel)null;
            var mre = new AsyncManualResetEvent(false);
            try
            {
                _activeResponseService.AddResponseListener(request.RequestId, responseIotCommandModel =>
                {
                    response = responseIotCommandModel;
                    mre.Set();
                    _logger.LogInfoWithSource(
                        string.Format(
                            "Received response from update service for request id {0}, IotCommand: {1},  IoT Topic: {2}.",
                            request?.RequestId, request?.Command, parameters?.IoTTopic),
                        "/sln/src/UpdateClientService.API/Services/IoT/IoTCommand/IoTCommandClient.cs");
                });
                var iotCommandTimeout = parameters.RequestTimeout ?? _requestTimeoutDefault;
                if (await PublishIoTCommandAsync(request, parameters))
                {
                    var num = await mre.WaitAsyncAndDisposeOnFinish(iotCommandTimeout, request) ? 1 : 0;
                }

                if (response == null)
                    response = new IoTCommandModel
                    {
                        RequestId = request.RequestId,
                        MessageType = MessageTypeEnum.Response,
                        Command = request.Command,
                        Version = request.Version,
                        Payload = new StatusCodeResult(504).ToJson(),
                        SourceId = _store.KioskId.ToString()
                    };
                iotCommandTimeout = new TimeSpan();
            }
            finally
            {
                _activeResponseService.RemoveResponseListener(request.RequestId);
            }

            return response;
        }

        private async Task<bool> PublishIoTCommandAsync(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters)
        {
            return await _mqttRepo.PublishIoTCommandAsync(parameters?.IoTTopic, request);
        }

        private void AdjustIoTTopicIfNeeded(
            IoTCommandModel request,
            PerformIoTCommandParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters?.IoTTopic))
                return;
            var instanceString = GetInstanceString(request?.ReturnServerId);
            parameters.IoTTopic = "redbox/updateservice-instance/" + instanceString + "/request";
            _logger.LogInfoWithSource(
                "Using IoT Topic: " + parameters.IoTTopic + " for request id: " + request.RequestId,
                "/sln/src/UpdateClientService.API/Services/IoT/IoTCommand/IoTCommandClient.cs");
        }

        private string GetInstanceString(string stringInstanceNumberOverride = null)
        {
            return stringInstanceNumberOverride;
        }
    }
}