using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.KDS;
using DeviceService.ComponentModel.Responses;
using DeviceService.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.WebApi.KDS
{
    public class KioskDataServiceClient : IKioskDataServiceClient
    {
        private const string AUTHHEADER = "Authorization";
        private const string KIOSKIDHEADER = "x-redbox-kioskid";
        private readonly string _apiUrl;
        private readonly List<Header> _headers = new List<Header>();
        private readonly IHttpService _httpService;
        private readonly ILogger _logger;

        public KioskDataServiceClient(
            IHttpService httpService,
            ILogger<KioskDataServiceClient> logger,
            IApplicationSettings settings)
        {
            _httpService = httpService;
            _logger = logger;
            var fileData = DataFileHelper.GetFileData<KioskData>(settings.DataFilePath, "KioskData.json", LogException);
            if (fileData == null)
                return;
            _apiUrl = fileData.ApiUrl;
            _headers.Add(new Header("Authorization", "Bearer " + fileData.ApiKey));
            _headers.Add(new Header("x-redbox-kioskid", fileData.KioskId.ToString()));
        }

        public async Task<StandardResponse> PostDeviceStatus(DeviceStatus deviceStatus)
        {
            StandardResponse standardResponse;
            try
            {
                standardResponse = JsonConvert.DeserializeObject<StandardResponse>(
                    await (await _httpService.SendRequest(
                        _httpService.GenerateRequest("DeviceStatus", deviceStatus, HttpMethod.Post,
                            _apiUrl + "/api/KioskData", _headers), 5000)).Content?.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                standardResponse = new StandardResponse(ex);
            }

            _logger.LogInformation("PostDeviceStatus Result: " + JsonConvert.SerializeObject(standardResponse));
            return standardResponse;
        }

        public async Task<StandardResponse> PostRebootStatus(RebootStatus rebootStatus)
        {
            StandardResponse standardResponse;
            try
            {
                standardResponse = JsonConvert.DeserializeObject<StandardResponse>(
                    await (await _httpService.SendRequest(
                        _httpService.GenerateRequest("RebootStatus", rebootStatus, HttpMethod.Post,
                            _apiUrl + "/api/KioskData", _headers), 5000)).Content?.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                standardResponse = new StandardResponse(ex);
            }

            _logger.LogInformation("PostRebootStatus Result: " + JsonConvert.SerializeObject(standardResponse));
            return standardResponse;
        }

        public async Task<StandardResponse> PostCardStats(CardStats cardStats)
        {
            StandardResponse standardResponse;
            try
            {
                _logger.LogInformation("Preparing to send PostCardStats to Kiosk Data Service");
                standardResponse = JsonConvert.DeserializeObject<StandardResponse>(
                    await (await _httpService.SendRequest(
                        _httpService.GenerateRequest("CardStats", cardStats, HttpMethod.Post,
                            _apiUrl + "/api/KioskData", _headers), 5000)).Content?.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                standardResponse = new StandardResponse(ex);
            }

            _logger.LogInformation("PostCardStats Result: " + JsonConvert.SerializeObject(standardResponse));
            return standardResponse;
        }

        public void LogException(string message, Exception e)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogError(e, "Unhandled Exception in " + message);
        }
    }
}