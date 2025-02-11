using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class TriggerGetConfigChanges : ICommandIoTController
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<TriggerGetConfigChanges> _logger;

        public TriggerGetConfigChanges(
            IConfigurationService configurationService,
            ILogger<TriggerGetConfigChanges> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        public CommandEnum CommandEnum { get; } = CommandEnum.TriggerGetConfigChanges;

        public int Version { get; } = 2;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            var triggerGetConfigChangesRequest =
                JsonConvert.DeserializeObject<TriggerGetConfigChangesRequest>(ioTCommand.Payload.ToJson());
            try
            {
                var configurationSettingChanges =
                    await _configurationService.TriggerGetKioskConfigurationSettingChanges(
                        triggerGetConfigChangesRequest);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception trying to trigger GetKioskConfigurationSettingChanges",
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/TriggerGetConfigChanges.cs");
            }
        }
    }
}