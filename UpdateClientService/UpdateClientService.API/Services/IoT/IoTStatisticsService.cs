using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT
{
    public class IoTStatisticsService : IIoTStatisticsService
    {
        private const string IoTStatisticsFileName = "IoTStatistics.json";
        private readonly ILogger<IoTStatisticsService> _logger;
        private readonly IPersistentDataCacheService _persistentDataCacheService;
        private IoTStatistics _iotStatistics;

        public IoTStatisticsService(
            ILogger<IoTStatisticsService> logger,
            IPersistentDataCacheService persistentDataCacheService)
        {
            _logger = logger;
            _persistentDataCacheService = persistentDataCacheService;
        }

        public async Task<IoTStatisticsSummaryResponse> GetIoTStatisticsSummaryResponse()
        {
            var response = new IoTStatisticsSummaryResponse();
            try
            {
                var ioTstatistics = await GetIoTStatistics();
                bool? nullable;
                if (ioTstatistics == null)
                {
                    nullable = new bool?();
                }
                else
                {
                    var connectionAttempts = ioTstatistics.ConnectionAttempts;
                    nullable = connectionAttempts != null ? connectionAttempts.Any() : new bool?();
                }

                if (nullable.GetValueOrDefault())
                {
                    foreach (var connectionAttempt in ioTstatistics.ConnectionAttempts)
                        if (connectionAttempt.Connected.HasValue && !connectionAttempt.Disconnected.HasValue)
                            connectionAttempt.Disconnected = DateTime.Now;
                    foreach (var source in ioTstatistics.ConnectionAttempts.GroupBy(x =>
                                 x.StartConnectionAttempt.Value.Date))
                    {
                        var tstatisticsSummary = new IoTStatisticsSummary
                        {
                            Date = source.Key.ToShortDateString()
                        };
                        var dictionary = new Dictionary<string, int>();
                        tstatisticsSummary.ConnectionCount = source.Count(x => x.Connected.HasValue);
                        tstatisticsSummary.ConnectionAttemptsCount = source.Sum(x => x.ConnectionAttempts);
                        foreach (var tconnectionAttempt in source)
                        {
                            if (tconnectionAttempt.ConnectionAttemptsDuration.HasValue)
                                tstatisticsSummary.TotalConnectionAttemptsDuration +=
                                    tconnectionAttempt.ConnectionAttemptsDuration.Value;
                            var connectionExceptions = tconnectionAttempt.ConnectionExceptions;
                            if ((connectionExceptions != null ? connectionExceptions.Any() ? 1 : 0 : 0) != 0)
                                foreach (var connectionException in tconnectionAttempt.ConnectionExceptions)
                                {
                                    var num1 = 0;
                                    dictionary.TryGetValue(connectionException.Key, out num1);
                                    var num2 = num1 + connectionException.Value;
                                    dictionary[connectionException.Key] = num2;
                                }
                        }

                        if (dictionary.Count > 0)
                        {
                            tstatisticsSummary.ConnectionExceptions = new List<ConnectionException>();
                            foreach (var keyValuePair in dictionary)
                                tstatisticsSummary.ConnectionExceptions.Add(new ConnectionException
                                {
                                    ExceptionMessage = keyValuePair.Key,
                                    Count = keyValuePair.Value
                                });
                        }

                        response.ioTStatisticsSummaries.Add(tstatisticsSummary);
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource(ex, "Exception getting IotStatisticsSummaryResponse",
                    "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
            }

            return response;
        }

        public async Task<bool> RecordConnectionAttempt()
        {
            bool flag;
            try
            {
                var ioTstatistics = await GetIoTStatistics();
                ioTstatistics.Cleanup();
                ioTstatistics.StartConnectionAttempt();
                flag = await SaveIoTStatistics(ioTstatistics);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(string.Format("Failed to record connection attempt: {0}", ex),
                    "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
                flag = false;
            }

            return flag;
        }

        public async Task<bool> RecordConnectionSuccess()
        {
            bool flag;
            try
            {
                var ioTstatistics = await GetIoTStatistics();
                ioTstatistics.Connected();
                flag = await SaveIoTStatistics(ioTstatistics);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(string.Format("Failed to record connection success: {0}", ex),
                    "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
                flag = false;
            }

            return flag;
        }

        public async Task<bool> RecordDisconnection()
        {
            bool flag;
            try
            {
                var ioTstatistics = await GetIoTStatistics();
                ioTstatistics.Disconnected();
                flag = await SaveIoTStatistics(ioTstatistics);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(string.Format("Failed to record connection disconnect: {0}", ex),
                    "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
                flag = false;
            }

            return flag;
        }

        public async Task<bool> RecordConnectionException(string exceptionMessage)
        {
            bool flag;
            try
            {
                var ioTstatistics = await GetIoTStatistics();
                ioTstatistics.AddException(exceptionMessage);
                flag = await SaveIoTStatistics(ioTstatistics);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(string.Format("Failed to record connection disconnect: {0}", ex),
                    "/sln/src/UpdateClientService.API/Services/IoT/IoTStatisticsService.cs");
                flag = false;
            }

            return flag;
        }

        private async Task<IoTStatistics> GetIoTStatistics()
        {
            if (_iotStatistics == null)
            {
                var persistentDataWrapper =
                    await _persistentDataCacheService.Read<IoTStatistics>("IoTStatistics.json", log: false);
                _iotStatistics = persistentDataWrapper?.Data == null ? new IoTStatistics() : persistentDataWrapper.Data;
            }

            return _iotStatistics;
        }

        private async Task<bool> SaveIoTStatistics(IoTStatistics ioTStatistics)
        {
            return await _persistentDataCacheService.Write(ioTStatistics, "IoTStatistics.json");
        }
    }
}