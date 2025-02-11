using System;
using System.IO;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Bluefin;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.Domain
{
    public class ActivationService : IActivationService
    {
        private readonly IBluefinServiceClient _bluefinService;
        private readonly IIUC285Proxy _deviceProxy;
        private readonly ILogger<ActivationService> _logger;
        private readonly IApplicationSettings _settings;

        public ActivationService(
            IApplicationSettings settings,
            ILogger<ActivationService> logger,
            IBluefinServiceClient bluefinService,
            IIUC285Proxy deviceProxy)
        {
            _logger = logger;
            _bluefinService = bluefinService;
            _settings = settings;
            _deviceProxy = deviceProxy;
        }

        public async Task<bool> CheckAndActivate(IBluefinActivationRequest request)
        {
            var unitData = _deviceProxy.UnitData;
            return await CheckAndActivate(request, unitData.ManufacturingSerialNumber, unitData.UnitSerialNumber);
        }

        public async Task<bool> CheckAndActivate(
            IBluefinActivationRequest request,
            string mfgSerialNumber,
            string injectedSerialNumber)
        {
            var isSuccess = true;
            try
            {
                _logger.Log(LogLevel.Information,
                    string.Format(
                        "ActivationService.CheckAndActivate called with kioskId {0}, mfgSerialNumber {1}, injectedSerialNumber {2}",
                        request.KioskId, mfgSerialNumber, injectedSerialNumber));
                var activationData = GetActivationData();
                if (activationData == null)
                    _logger.Log(LogLevel.Information, "No previous activation info was found.");
                else
                    _logger.Log(LogLevel.Information,
                        string.Format("Previous activation info was found with kiosk Id {0} serial number {1}.",
                            activationData.KioskId, activationData.MfgSerialNumber));
                if (activationData != null && activationData.KioskId == request.KioskId)
                    if (!(activationData.MfgSerialNumber != mfgSerialNumber))
                        goto label_11;
                _logger.Log(LogLevel.Information, "Calling BluefinService.Activate");
                var response =
                    await _bluefinService.Activate(request, mfgSerialNumber, injectedSerialNumber, DateTime.Now);
                if (response.Success)
                {
                    _logger.Log(LogLevel.Information, "Activation Successful.");
                    UpdateActivationData(request.KioskId, mfgSerialNumber);
                }
                else
                {
                    LogErrors(string.Format("Activation Not Successful. HttpErrorCode: {0}", response.StatusCode),
                        response);
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to activate device due to unhandled exception:");
                isSuccess = false;
            }

            label_11:
            if (!isSuccess)
                FailedRetry(request, mfgSerialNumber, injectedSerialNumber);
            return isSuccess;
        }

        private void FailedRetry(
            IBluefinActivationRequest request,
            string mfgSerialNumber,
            string injectedSerialNumber)
        {
            if (!_settings.ActivationFailureRetry)
                return;
            Task.Run(async () =>
            {
                _logger.LogInformation("ActivationService.FailedRetry - waiting 5 minutes");
                await Task.Delay(300000);
                _logger.LogInformation("ActivationService.FailedRetry - starting");
                if (await CheckAndActivate(request, mfgSerialNumber, injectedSerialNumber))
                    _logger.LogInformation("ActivationService.FailedRetry - worked");
                else
                    _logger.LogInformation("ActivationService.FailedRetry - failed again");
            });
        }

        private DeviceActivationData GetActivationData()
        {
            var path = Path.Combine(_settings.DataFilePath, "BluefinActivationData.json");
            var activationData = (DeviceActivationData)null;
            if (File.Exists(path))
                using (var streamReader = File.OpenText(path))
                {
                    activationData =
                        (DeviceActivationData)new JsonSerializer().Deserialize(streamReader,
                            typeof(DeviceActivationData));
                }

            return activationData;
        }

        private void UpdateActivationData(long kioskId, string mfgSerialNumber)
        {
            try
            {
                if (!Directory.Exists(_settings.DataFilePath))
                    Directory.CreateDirectory(_settings.DataFilePath);
                var deviceActivationData = new DeviceActivationData
                {
                    KioskId = kioskId,
                    MfgSerialNumber = mfgSerialNumber
                };
                File.WriteAllText(Path.Combine(_settings.DataFilePath, "BluefinActivationData.json"),
                    JsonConvert.SerializeObject(deviceActivationData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to write previous activation data:");
            }
        }

        private void LogErrors(string message, StandardResponse response)
        {
            if (response?.Errors?.Count == 0)
                return;
            _logger.Log(LogLevel.Error, message);
            foreach (var error in response.Errors)
                _logger.Log(LogLevel.Error, "Code: " + error.Code + " Message: " + error.Message);
        }
    }
}