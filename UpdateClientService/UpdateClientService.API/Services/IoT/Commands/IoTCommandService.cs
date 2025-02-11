using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands.Controller;

namespace UpdateClientService.API.Services.IoT.Commands
{
    public class IoTCommandService : IIoTCommandService
    {
        private readonly IActiveResponseService _activeResponseService;
        private readonly List<ICommandIoTController> _commandList;
        private readonly ILogger<IoTCommandService> _logger;

        public IoTCommandService(
            IActiveResponseService activeResponseService,
            List<ICommandIoTController> commandList,
            ILogger<IoTCommandService> logger)
        {
            _activeResponseService = activeResponseService;
            _commandList = commandList;
            _logger = logger;
        }

        public async Task Execute(byte[] message, string topic)
        {
            await IoTCommandCallbackMethodAsync(message, topic);
        }

        private async Task IoTCommandCallbackMethodAsync(byte[] mqttMessage, string mqttTopic)
        {
            try
            {
                var str1 = Encoding.UTF8.GetString(mqttMessage);
                var iotCommandModel = JsonConvert.DeserializeObject<IoTCommandModel>(str1);
                if (iotCommandModel == null)
                {
                    _logger.LogErrorWithSource(
                        "Error occured in callback from topic " + mqttTopic + ", message failed deserialization: " +
                        str1, "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
                }
                else
                {
                    string str2;
                    if (!iotCommandModel.LogPayload)
                        str2 = string.Format(", command: {0}, MessageType: {1}, RequestId: {2}, Payload length: {3}",
                            iotCommandModel.Command, iotCommandModel.MessageType, iotCommandModel.RequestId,
                            iotCommandModel.Payload?.ToString()?.Length);
                    else
                        str2 = ", message: " + str1;
                    var str3 = str2;
                    _logger.LogInfoWithSource("message received topic: " + mqttTopic + str3,
                        "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
                    if (iotCommandModel.MessageType == MessageTypeEnum.Request)
                    {
                        await ExecuteRequest(iotCommandModel);
                    }
                    else
                    {
                        if (iotCommandModel.MessageType != MessageTypeEnum.Response)
                            return;
                        await ExecuteResponse(iotCommandModel);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Error occured in callback from topic " + mqttTopic + ", message was: " +
                    Encoding.UTF8.GetString(mqttMessage),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
            }
        }

        private async Task ExecuteRequest(IoTCommandModel iotCommandModel)
        {
            var commandList1 = _commandList;
            var requestedCommand = commandList1 != null
                ? commandList1.FirstOrDefault(x =>
                    x.CommandEnum == iotCommandModel.Command && x.Version == iotCommandModel.Version)
                : null;
            if (requestedCommand == null)
            {
                var logger = _logger;
                var command = iotCommandModel.Command;
                var version = iotCommandModel.Version;
                var commandList2 = _commandList;
                var json = commandList2 != null ? commandList2.ToJson() : null;
                var str = string.Format(
                    "IoT Command: {0}, Version: {1} could not found in registered command list: {2}.", command, version,
                    json);
                _logger.LogErrorWithSource(str,
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
            }
            else
            {
                _logger.LogInfoWithSource(
                    string.Format("Processing command: {0} for RequestId: {1}", requestedCommand.CommandEnum,
                        iotCommandModel.RequestId),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
                await requestedCommand.Execute(iotCommandModel);
                _logger.LogInfoWithSource(
                    string.Format("Finished processing command: {0} for RequestId: {1}. Returning. . .",
                        requestedCommand.CommandEnum, iotCommandModel.RequestId),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
            }
        }

        private async Task ExecuteResponse(IoTCommandModel iotCommandModel)
        {
            var responseListenerAction = _activeResponseService.GetResponseListenerAction(iotCommandModel.RequestId);
            if (responseListenerAction == null)
            {
                _logger.LogInfoWithSource(
                    "ActiveResponseListener could not be found for RequestId: " + iotCommandModel.RequestId +
                    ".  Ignoring response.",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
            }
            else
            {
                _logger.LogInfoWithSource(
                    string.Format("Processing Response Type: {0} for RequestId: {1}", iotCommandModel.Command,
                        iotCommandModel.RequestId),
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/IoTCommandService.cs");
                if (responseListenerAction != null)
                    responseListenerAction(iotCommandModel);
                _activeResponseService.RemoveResponseListener(iotCommandModel.RequestId);
            }
        }
    }
}