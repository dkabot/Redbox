using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.ProxyApi;

namespace UpdateClientService.API.Services.Broker
{
    public class PingStatisticsService : IPingStatisticsService
    {
        private const string PingStatisticsFileName = "PingStatistics.json";
        private const int _initialDelayMinutes = 30;
        private const int _reportFailedPingAttemptWaitingPeriodMinutes = 60;
        private static DateTime _initialReportFailedPingAttempt = DateTime.Now;
        private readonly IKioskConfiguration _kioskConfiguration;
        private readonly ILogger<PingStatisticsService> _logger;
        private readonly IPersistentDataCacheService _persistentDataCacheService;
        private readonly IProxyApi _proxyApi;
        private readonly IStoreService _storeService;
        private PingStatistics _pingStatistics;

        public PingStatisticsService(
            ILogger<PingStatisticsService> logger,
            IPersistentDataCacheService persistentDataCacheService,
            IOptionsKioskConfiguration kioskConfiguration,
            IProxyApi proxyApi,
            IStoreService storeService)
        {
            _logger = logger;
            _persistentDataCacheService = persistentDataCacheService;
            _kioskConfiguration = kioskConfiguration;
            _proxyApi = proxyApi;
            _storeService = storeService;
        }

        public async Task<bool> RecordPingStatistic(bool pingSuccess)
        {
            bool flag;
            try
            {
                var pingStatistics = await GetPingStatistics();
                pingStatistics.Cleanup();
                var pingStatistic = pingStatistics.GetLatest();
                var now = DateTime.Now;
                if (pingStatistic == null || pingStatistic.PingSuccess != pingSuccess)
                {
                    pingStatistic = new PingStatistic
                    {
                        StartInterval = now,
                        StartIntervalUTC = now.ToUniversalTime(),
                        PingSuccess = pingSuccess
                    };
                    pingStatistics.Add(pingStatistic);
                }

                pingStatistic.EndInterval = now;
                pingStatistic.EndIntervalUTC = now.ToUniversalTime();
                ++pingStatistic.PingCount;
                flag = await SavePingStatistics(pingStatistics);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(string.Format("Failed to record ping attempt: {0}", ex),
                    "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
                flag = false;
            }

            return flag;
        }

        public async Task<PingStatisticsResponse> GetPingStatisticsResponse()
        {
            var response = new PingStatisticsResponse();
            try
            {
                var statisticsResponse = response;
                statisticsResponse.PingStatistics = await GetPingStatistics();
                statisticsResponse = null;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception getting Ping Statistics Response",
                    "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<LastSuccessfulPingResponse> GetLastSuccessfulPing()
        {
            var response = new LastSuccessfulPingResponse();
            try
            {
                response.LastSuccessfulPingUTC = (await GetPingStatistics())?.GetLastSuccessful()?.EndIntervalUTC;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception getting last successful ping",
                    "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<bool> ReportFailedPing()
        {
            var result = true;
            try
            {
                if (DateTime.Now > _initialReportFailedPingAttempt.AddMinutes(30.0))
                {
                    var pingStatistics = await GetPingStatistics();
                    var latest = pingStatistics.GetLatest();
                    var endIntervalUtc = pingStatistics?.GetLastSuccessful()?.EndIntervalUTC;
                    if (endIntervalUtc.HasValue)
                        if (endIntervalUtc.Value.AddMinutes(60.0) < DateTime.UtcNow)
                            if (latest != null)
                                if (!latest.PingSuccess)
                                {
                                    if (_kioskConfiguration.KioskHealth.ReportFailedPingsEnabled)
                                    {
                                        var apiResponse = await _proxyApi.RecordKioskPingFailure(new KioskPingFailure
                                        {
                                            KioskId = _storeService.KioskId,
                                            LastSuccessfulPingUTC = endIntervalUtc.Value,
                                            LastUpdateUTC = DateTime.UtcNow
                                        });
                                        result = apiResponse != null && apiResponse.StatusCode == HttpStatusCode.OK;
                                    }
                                    else
                                    {
                                        _logger.LogInfoWithSource(
                                            "Skipping reporting of failed ping because config setting ReportFailedPingsEnabled is turned off",
                                            "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
                                    }
                                }
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception reporting failed ping.",
                    "/sln/src/UpdateClientService.API/Services/Broker/PingStatisticsService.cs");
                result = false;
            }

            return result;
        }

        private async Task<PingStatistics> GetPingStatistics()
        {
            if (_pingStatistics == null)
            {
                var persistentDataWrapper =
                    await _persistentDataCacheService.Read<PingStatistics>("PingStatistics.json", log: false);
                _pingStatistics = persistentDataWrapper?.Data == null
                    ? new PingStatistics()
                    : persistentDataWrapper.Data;
            }

            return _pingStatistics;
        }

        private async Task<bool> SavePingStatistics(PingStatistics pingStatistics)
        {
            return await _persistentDataCacheService.Write(pingStatistics, "PingStatistics.json");
        }
    }
}