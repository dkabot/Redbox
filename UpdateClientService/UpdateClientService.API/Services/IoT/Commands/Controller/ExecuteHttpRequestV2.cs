using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class ExecuteHttpRequestV2 : ICommandIoTController
    {
        private readonly IHttpService _httpService;
        private readonly ILogger<ExecuteHttpRequestV2> _logger;
        private readonly IMqttProxy _mqttProxy;
        private readonly IStoreService _store;

        public ExecuteHttpRequestV2(
            IHttpService httpService,
            ILogger<ExecuteHttpRequestV2> logger,
            IMqttProxy mqttProxy,
            IStoreService store)
        {
            _httpService = httpService;
            _logger = logger;
            _mqttProxy = mqttProxy;
            _store = store;
        }

        public CommandEnum CommandEnum => CommandEnum.ExecuteHttpRequest;

        public int Version => 2;

        public async Task Execute(IoTCommandModel ioTCommandRequest)
        {
            var kioskHttpRequest = JsonConvert.DeserializeObject<KioskHttpRequest>(ioTCommandRequest.Payload.ToJson());
            var stringContent = kioskHttpRequest == null || kioskHttpRequest.JsonBody == null
                ? null
                : new StringContent(kioskHttpRequest.JsonBody, Encoding.UTF8, "application/json");
            var request1 = _httpService.GenerateRequest(null, kioskHttpRequest.Url, stringContent,
                new HttpMethod(kioskHttpRequest.HttpMethod));
            var httpService = _httpService;
            var request2 = request1;
            var timeout = new int?();
            var ioTcommandModel1 = ioTCommandRequest;
            int num1;
            if (ioTcommandModel1 == null)
            {
                num1 = 0;
            }
            else
            {
                var logRequest = ioTcommandModel1.LogRequest;
                var flag = true;
                num1 = (logRequest.GetValueOrDefault() == flag) & logRequest.HasValue ? 1 : 0;
            }

            var ioTcommandModel2 = ioTCommandRequest;
            int num2;
            if (ioTcommandModel2 == null)
            {
                num2 = 0;
            }
            else
            {
                var logResponse = ioTcommandModel2.LogResponse;
                var flag = true;
                num2 = (logResponse.GetValueOrDefault() == flag) & logResponse.HasValue ? 1 : 0;
            }

            var apiResponse = await httpService.SendRequestAsync<object>(request2, timeout,
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteHttpRequestV2.cs",
                logRequest: num1 != 0, logResponse: num2 != 0);
            var mqttResponse1 = new MqttResponse<string>();
            var mqttResponse2 = mqttResponse1;
            string str1;
            if (apiResponse == null)
            {
                str1 = null;
            }
            else
            {
                var response = apiResponse.Response;
                str1 = response != null ? response.ToJson() : null;
            }

            mqttResponse2.Data = str1;
            if (apiResponse == null || apiResponse.Response == null)
            {
                mqttResponse1.Data = apiResponse?.ResponseContent;
                if (apiResponse == null || !apiResponse.IsSuccessStatusCode)
                {
                    if (apiResponse?.Errors?.Count > 0)
                        mqttResponse1.Error =
                            "Errors: " + string.Join<string>(",", apiResponse.Errors.Select(x => x.Message));
                    var ioTcommandModel3 = ioTCommandRequest;
                    int num3;
                    if (ioTcommandModel3 == null)
                    {
                        num3 = 0;
                    }
                    else
                    {
                        var logResponse = ioTcommandModel3.LogResponse;
                        var flag = true;
                        num3 = (logResponse.GetValueOrDefault() == flag) & logResponse.HasValue ? 1 : 0;
                    }

                    string str2;
                    if (num3 == 0)
                    {
                        str2 = string.Empty;
                    }
                    else
                    {
                        string str3;
                        if (apiResponse == null)
                        {
                            str3 = null;
                        }
                        else
                        {
                            var httpResponse = apiResponse.HttpResponse;
                            str3 = httpResponse != null ? httpResponse.ToJson() : null;
                        }

                        str2 = "HttpResponse: " + str3;
                    }

                    var str4 = str2;
                    var ioTcommandModel4 = ioTCommandRequest;
                    int num4;
                    if (ioTcommandModel4 == null)
                    {
                        num4 = 0;
                    }
                    else
                    {
                        var logResponse = ioTcommandModel4.LogResponse;
                        var flag = true;
                        num4 = (logResponse.GetValueOrDefault() == flag) & logResponse.HasValue ? 1 : 0;
                    }

                    string str5;
                    if (num4 != 0)
                    {
                        var ioTcommandModel5 = ioTCommandRequest;
                        int num5;
                        if (ioTcommandModel5 == null)
                        {
                            num5 = 0;
                        }
                        else
                        {
                            var logRequest = ioTcommandModel5.LogRequest;
                            var flag = true;
                            num5 = (logRequest.GetValueOrDefault() == flag) & logRequest.HasValue ? 1 : 0;
                        }

                        if (num5 != 0)
                        {
                            str5 = "Received ioTCommand: " + ioTCommandRequest.ToJson() + ".";
                            goto label_33;
                        }
                    }

                    str5 = string.Empty;
                    label_33:
                    var str6 = str5;
                    var ioTcommandModel6 = ioTCommandRequest;
                    int num6;
                    if (ioTcommandModel6 == null)
                    {
                        num6 = 0;
                    }
                    else
                    {
                        var logResponse = ioTcommandModel6.LogResponse;
                        var flag = true;
                        num6 = (logResponse.GetValueOrDefault() == flag) & logResponse.HasValue ? 1 : 0;
                    }

                    _logger.LogErrorWithSource(
                        "ExecuteHttpRequestV2 has failed. Result: " +
                        (num6 != 0 ? string.Format("{0}", mqttResponse1) : string.Empty) + "." + str4 + str6,
                        "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteHttpRequestV2.cs");
                }
            }

            var ioTcommandModel7 = ioTCommandRequest;
            int num7;
            if (ioTcommandModel7 == null)
            {
                num7 = 0;
            }
            else
            {
                var logResponse = ioTcommandModel7.LogResponse;
                var flag = true;
                num7 = (logResponse.GetValueOrDefault() == flag) & logResponse.HasValue ? 1 : 0;
            }

            _logger.LogInfoWithSource(
                "Executed http v2 request." +
                (num7 != 0 ? string.Format("Received response: {0}", apiResponse?.Response) : string.Empty),
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteHttpRequestV2.cs");
            var num8 = await _mqttProxy.PublishIoTCommandAsync(
                "redbox/updateservice-instance/" + ioTCommandRequest.SourceId + "/request", new IoTCommandModel
                {
                    RequestId = ioTCommandRequest.RequestId,
                    Command = CommandEnum,
                    MessageType = MessageTypeEnum.Response,
                    Version = Version,
                    SourceId = _store?.KioskId.ToString(),
                    Payload = mqttResponse1.ToJson(),
                    LogRequest = true
                })
                ? 1
                : 0;
        }
    }
}