using System;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.KDS;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.Domain
{
    public class DeviceStatusService : IDeviceStatusService
    {
        private readonly IKioskDataServiceClient _client;
        private readonly ILogger<DeviceStatusService> _logger;
        private readonly IIUC285Proxy _proxy;
        private readonly IApplicationSettings _settings;

        public DeviceStatusService(
            IIUC285Proxy proxy,
            IApplicationSettings settings,
            IKioskDataServiceClient client,
            ILogger<DeviceStatusService> logger)
        {
            _proxy = proxy;
            _client = client;
            _settings = settings;
            _logger = logger;
        }

        public async Task<StandardResponse> PostDeviceStatus()
        {
            var deviceStatusService = this;
            var fileData = DataFileHelper.GetFileData<KioskData>(deviceStatusService._settings.DataFilePath,
                "KioskData.json", deviceStatusService.LogException);
            StandardResponse standardResponse;
            if (fileData != null)
            {
                var deviceStatus = await deviceStatusService._proxy.CheckDeviceStatus(fileData.KioskId);
                standardResponse = await deviceStatusService._client.PostDeviceStatus(deviceStatus);
            }
            else
            {
                standardResponse = new StandardResponse(new Exception("KioskData.json not found in Data folder"));
                deviceStatusService._logger.LogInformation("PostDeviceStatus Result: " +
                                                           JsonConvert.SerializeObject(standardResponse));
            }

            return standardResponse;
        }

        public async Task<StandardResponse> PostDeviceStatus(DeviceStatusRequest request)
        {
            DataFileHelper.TryUpdateFileData(_settings.DataFilePath, "KioskData.json", new KioskData
            {
                ApiUrl = request.ApiUrl,
                ApiKey = request.ApiKey,
                KioskId = request.KioskId
            });
            return await _client.PostDeviceStatus(await _proxy.CheckDeviceStatus(request.KioskId));
        }

        public async Task<DeviceStatus> GetDeviceStatus()
        {
            var deviceStatusService = this;
            var fileData = DataFileHelper.GetFileData<KioskData>(deviceStatusService._settings.DataFilePath,
                "KioskData.json", deviceStatusService.LogException);
            return fileData != null ? await deviceStatusService._proxy.CheckDeviceStatus(fileData.KioskId) : null;
        }

        private void LogException(string message, Exception e)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogError(e, "Unhandled Exception in " + message);
        }
    }
}