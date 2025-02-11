using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.Kernel
{
    public class KernelService : IKernelService
    {
        private readonly ICommandLineService _cmd;
        private readonly ILogger<KernelService> _logger;
        private readonly KernelServiceSettings _settings;
        private readonly bool _updateTimestamp;
        private readonly bool _updateTimezone;

        public KernelService(
            ICommandLineService cmd,
            ILogger<KernelService> logger,
            IOptionsMonitor<AppSettings> settings,
            IOptionsMonitorKioskConfiguration kioskConfiguration)
        {
            _cmd = cmd;
            _logger = logger;
            _settings = settings?.CurrentValue?.KernelService;
            _updateTimestamp = kioskConfiguration.Operations.SyncTimestamp;
            _updateTimezone = kioskConfiguration.Operations.SyncTimezone;
        }

        public bool PerformShutdown(ShutdownType shutdownType)
        {
            _logger.LogInfoWithSource(string.Format("Performing {0}", shutdownType),
                "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
            if (!_settings.ShutdownEnabled)
            {
                _logger.LogWarningWithSource(
                    string.Format("Cannot {0} because ShutdownEnabled is set to {1}", shutdownType,
                        _settings.ShutdownEnabled),
                    "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                return false;
            }

            if (shutdownType == ShutdownType.Reboot)
                return _cmd.TryExecutePowerShellScript("Restart-Computer -Force");
            if (shutdownType == ShutdownType.Shutdown)
                return _cmd.TryExecutePowerShellScript("Stop-Computer -Force");
            _logger.LogWarningWithSource(string.Format("{0} was unsuccessful", shutdownType),
                "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
            return false;
        }

        public void SyncTimeAndTimezone(object data)
        {
            try
            {
                var json = data.ToJson();
                _logger.LogInfoWithSource("sync time data: " + json,
                    "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                var data1 = JsonConvert.DeserializeObject<SyncTimestampAndTimeZoneData>(json);
                if (data1 == null)
                {
                    _logger.LogErrorWithSource("unable to deserialize sync time data!",
                        "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                }
                else
                {
                    UpdateTimezone(data1);
                    UpdateTime(data1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCriticalWithSource(ex, "Exception occured sync'ing kiosk time/timezone!",
                    "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
            }
        }

        private void UpdateTimezone(SyncTimestampAndTimeZoneData data)
        {
            if (_updateTimezone)
            {
                if (string.IsNullOrEmpty(data.Timezone))
                {
                    _logger.LogErrorWithSource("Empty time zone returned from server!",
                        "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                }
                else
                {
                    var systemTimeZoneById = TimeZoneInfo.FindSystemTimeZoneById(data.Timezone);
                    switch (TimeZoneFunctions.SetTimeZone(systemTimeZoneById))
                    {
                        case TimeZoneFunctions.SetTimeZoneResult.Same:
                            _logger.LogInfoWithSource("Client and Server time zones match",
                                "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                            break;
                        case TimeZoneFunctions.SetTimeZoneResult.Changed:
                            _logger.LogInfoWithSource("Client zone changed to " + systemTimeZoneById.DisplayName,
                                "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                            break;
                        case TimeZoneFunctions.SetTimeZoneResult.Errored:
                            _logger.LogErrorWithSource("Failed to set TimeZone for the kiosk!",
                                "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                            break;
                        default:
                            _logger.LogErrorWithSource("Unknown error setting the kiosk time zone!",
                                "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                            break;
                    }
                }
            }
            else
            {
                _logger.LogInfoWithSource("Skipping time zone sync; configuration off",
                    "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
            }
        }

        private void UpdateTime(SyncTimestampAndTimeZoneData data)
        {
            if (_updateTimestamp)
                switch (TimeZoneFunctions.SetTime(data.UtcTimestamp))
                {
                    case TimeZoneFunctions.SetTimeResult.InRange:
                        _logger.LogInfoWithSource("Kiosk time is in range",
                            "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                        break;
                    case TimeZoneFunctions.SetTimeResult.Changed:
                        _logger.LogInfoWithSource("Kiosk time changed to match server time",
                            "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                        break;
                    case TimeZoneFunctions.SetTimeResult.Errored:
                        _logger.LogErrorWithSource("Failed to set date/time for the kiosk!",
                            "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                        break;
                    default:
                        _logger.LogErrorWithSource("Unknown error setting the kiosk time!",
                            "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
                        break;
                }
            else
                _logger.LogInfoWithSource("Skipping time sync; configuration off",
                    "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
        }

        private class SyncTimestampAndTimeZoneData
        {
            public DateTime UtcTimestamp { get; set; }

            public string Timezone { get; set; }
        }
    }
}