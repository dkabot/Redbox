using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API.Services
{
    public class ActiveResponsesService : IActiveResponseService
    {
        private readonly ConcurrentDictionary<string, Action<IoTCommandModel>> _activeResponses =
            new ConcurrentDictionary<string, Action<IoTCommandModel>>();

        private readonly ILogger<ActiveResponsesService> _logger;

        public ActiveResponsesService(ILogger<ActiveResponsesService> logger)
        {
            _logger = logger;
        }

        public void AddResponseListener(string requestId, Action<IoTCommandModel> model)
        {
            var flag = _activeResponses.TryAdd(requestId, model);
            _logger.LogInfoWithSource(string.Format("Adding {0} to Active Listeners - success = {1}", requestId, flag),
                "/sln/src/UpdateClientService.API/Services/ActiveResponsesService.cs");
        }

        public Action<IoTCommandModel> GetResponseListenerAction(string requestId)
        {
            Action<IoTCommandModel> responseListenerAction;
            if (_activeResponses.TryGetValue(requestId, out responseListenerAction))
                return responseListenerAction;
            _logger.LogWarningWithSource("Failed To Find RequestId: " + requestId + " in Active Responses.",
                "/sln/src/UpdateClientService.API/Services/ActiveResponsesService.cs");
            return null;
        }

        public void RemoveResponseListener(string requestId)
        {
            var flag = _activeResponses.TryRemove(requestId, out var _);
            _logger.LogInfoWithSource(
                string.Format("Removing {0} from Active Listeners - success = {1}", requestId, flag),
                "/sln/src/UpdateClientService.API/Services/ActiveResponsesService.cs");
        }
    }
}