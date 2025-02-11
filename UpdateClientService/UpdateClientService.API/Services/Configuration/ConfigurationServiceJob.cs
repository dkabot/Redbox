using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Configuration
{
    public class ConfigurationServiceJob : IConfigurationServiceJob, IInvocable
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<ConfigurationServiceJob> _logger;

        public ConfigurationServiceJob(
            ILogger<ConfigurationServiceJob> logger,
            IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        public async Task Invoke()
        {
            _logger.LogInfoWithSource("Invoking ConfigurationService.GetKioskConfigurationSettingChanges",
                "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationServiceJob.cs");
            var configurationSettingChanges =
                await _configurationService.GetKioskConfigurationSettingChanges(new long?());
        }
    }
}