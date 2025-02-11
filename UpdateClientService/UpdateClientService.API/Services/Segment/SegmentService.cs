using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.Segment
{
    public class SegmentService : ISegmentService
    {
        private const string SegmentsFileName = "segments.json";
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly IKioskConfiguration _kioskConfiguration;
        private readonly ILogger<SegmentService> _logger;
        private readonly IPersistentDataCacheService _persistentDataCacheService;
        private int _processingSegments;

        public SegmentService(
            ILogger<SegmentService> logger,
            IIoTCommandClient ioTCommandClient,
            IPersistentDataCacheService persistentDataCacheService,
            IOptionsSnapshotKioskConfiguration kioskConfiguration)
        {
            _logger = logger;
            _iotCommandClient = ioTCommandClient;
            _persistentDataCacheService = persistentDataCacheService;
            _kioskConfiguration = kioskConfiguration;
        }

        public async Task<KioskSegmentsResponse> GetKioskSegments()
        {
            var response = new KioskSegmentsResponse();
            try
            {
                var persistentDataCache = await GetKioskSegmentsFromPersistentDataCache();
                if (persistentDataCache?.Data != null)
                    response.Segments.AddRange(persistentDataCache.Data);
                else
                    response.StatusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource(ex, "Exception while getting kiosk segments",
                    "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
            }

            return response;
        }

        public async Task<ApiBaseResponse> UpdateKioskSegmentsFromUpdateService()
        {
            var response = new ApiBaseResponse();
            try
            {
                if (!_kioskConfiguration.Operations.SegmentUpdateEnabled)
                {
                    response.StatusCode = HttpStatusCode.ServiceUnavailable;
                    return response;
                }

                var tcommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
                {
                    Version = 1,
                    RequestId = Guid.NewGuid().ToString(),
                    Command = CommandEnum.GetKioskSegments,
                    QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
                    LogResponse = false
                }, new PerformIoTCommandParameters
                {
                    IoTTopic = "$aws/rules/kioskrestcall",
                    WaitForResponse = true
                });
                if (tcommandResponse != null && tcommandResponse.StatusCode == 200)
                {
                    var segmentsResponse =
                        JsonConvert.DeserializeObject<KioskSegmentsResponse>(
                            tcommandResponse?.Payload?.Value?.ToString());
                    if (segmentsResponse != null)
                    {
                        var kioskSegmentsData = new KioskSegmentsData();
                        kioskSegmentsData.AddRange(segmentsResponse.Segments);
                        response.StatusCode = !await UpdateKioskSegmentsFile(kioskSegmentsData)
                            ? HttpStatusCode.InternalServerError
                            : HttpStatusCode.OK;
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        _logger.LogErrorWithSource("Unable to deserialize Iot command payload as KioskSegmentsResponse",
                            "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
                    }
                }
                else
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    _logger.LogErrorWithSource(
                        string.Format("Iot command {0} returned statuscode {1}", CommandEnum.GetKioskSegments,
                            tcommandResponse?.StatusCode),
                        "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting kiosk segments.",
                    "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<bool> UpdateKioskSegmentsIfNeeded()
        {
            var response = false;
            try
            {
                var persistentDataCache = await GetKioskSegmentsFromPersistentDataCache();
                if (persistentDataCache != null)
                    if (!IsKioskSegmentDataExpired(persistentDataCache))
                        goto label_9;
                var apiBaseResponse = await UpdateKioskSegmentsFromUpdateService();
                if (apiBaseResponse != null)
                    if (apiBaseResponse.StatusCode == HttpStatusCode.OK)
                        response = true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception updating kiosk segments",
                    "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
            }

            label_9:
            return response;
        }

        private async Task<PersistentDataWrapper<KioskSegmentsData>> GetKioskSegmentsFromPersistentDataCache()
        {
            var result = new PersistentDataWrapper<KioskSegmentsData>();
            try
            {
                var persistentDataWrapper =
                    await _persistentDataCacheService.Read<KioskSegmentsData>("segments.json", true, log: false);
                if (persistentDataWrapper != null)
                    result = persistentDataWrapper;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception getting kiosk segments",
                    "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
            }

            return result;
        }

        private async Task<bool> UpdateKioskSegmentsFile(
            KioskSegmentsData kioskSegmentsData)
        {
            var flag = false;
            if (Interlocked.CompareExchange(ref _processingSegments, 1, 0) == 1)
            {
                _logger.LogInfoWithSource("Prevented attempt to process kiosk segments.",
                    "/sln/src/UpdateClientService.API/Services/Segment/SegmentService.cs");
                return flag;
            }

            try
            {
                flag = await _persistentDataCacheService.Write(kioskSegmentsData, "segments.json");
            }
            finally
            {
                _processingSegments = 0;
            }

            return flag;
        }

        private bool IsKioskSegmentDataExpired(
            PersistentDataWrapper<KioskSegmentsData> persistentDataWrapper)
        {
            return (DateTime.Now - persistentDataWrapper.Modified).TotalHours >
                   _kioskConfiguration.Operations.SegmentUpdateFrequencyHours;
        }

        private class KioskSegmentsData : List<KioskSegmentModel>, IPersistentData
        {
        }
    }
}