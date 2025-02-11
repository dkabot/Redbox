using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Configuration
{
    public class ConfigurationFileMissingJob : IConfigurationFileMissingJob, IInvocable
    {
        private readonly IConfigurationService _configurationService;
        private readonly IKioskConfiguration _kioskConfiguration;
        private readonly ILogger<ConfigurationFileMissingJob> _logger;

        public ConfigurationFileMissingJob(
            ILogger<ConfigurationFileMissingJob> logger,
            IConfigurationService configurationService,
            IOptionsSnapshotKioskConfiguration kioskConfiguration)
        {
            _configurationService = configurationService;
            _kioskConfiguration = kioskConfiguration;
            _logger = logger;
        }

        public async Task Invoke()
        {
            if (_kioskConfiguration.ConfigurationVersion != 0L)
                return;
            _logger.LogWarningWithSource("KioskConfiguration is not current.  Getting current configuration version...",
                "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationFileMissingJob.cs");
            var configurationSettingChanges =
                await _configurationService.GetKioskConfigurationSettingChanges(new long?());
        }
    }
}