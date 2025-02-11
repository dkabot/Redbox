using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private const string ConfigurationFileName = "configuration.json";
        private const string OldConfigurationFileName = "configuration old.json";
        private const string ConfigurationStatusFileName = "ConfigurationStatus.json";
        private static DateTime _lastCallToGetKioskConfigurationSettingChanges = DateTime.MinValue;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly int _lockWait = 2000;
        private readonly ILogger<IConfigurationService> _logger;
        private readonly IPersistentDataCacheService _persistentDataCacheService;
        private readonly IOptionsMonitor<AppSettings> _settings;
        private readonly IStoreService _storeService;
        private int _processingChanges;

        public ConfigurationService(
            ILogger<IConfigurationService> logger,
            IStoreService storeService,
            IIoTCommandClient ioTCommandClient,
            IOptionsMonitor<AppSettings> settings,
            IPersistentDataCacheService persistentDataCacheService)
        {
            _logger = logger;
            _storeService = storeService;
            _iotCommandClient = ioTCommandClient;
            _settings = settings;
            _persistentDataCacheService = persistentDataCacheService;
        }

        private static string ConfigurationFileDirectory => FilePaths.TypeMappings[FileTypeEnum.Configuration];

        public static string ConfigurationFilePath => Path.Combine(ConfigurationFileDirectory, "configuration.json");

        private string OldConfigurationFilePath => Path.Combine(ConfigurationFileDirectory, "configuration old.json");

        public async Task<bool> UpdateConfigurationStatusIfNeeded()
        {
            var response = false;
            try
            {
                var dataCacheService = await GetConfigurationStatusFromPersistentDataCacheService();
                if (dataCacheService != null)
                    if (!IsMoreThan26HoursAgo(dataCacheService.Modified))
                        goto label_9;
                var configurationStatusResponse = await UpdateConfigurationStatus();
                if (configurationStatusResponse != null)
                    if (configurationStatusResponse.StatusCode == HttpStatusCode.OK)
                        response = true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting config status",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            label_9:
            return response;
        }

        public async Task<ConfigurationStatusResponse> UpdateConfigurationStatus()
        {
            var response = new ConfigurationStatusResponse();
            try
            {
                var tcommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
                {
                    Version = 1,
                    RequestId = Guid.NewGuid().ToString(),
                    Command = CommandEnum.GetConfigStatus,
                    QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
                    LogResponse = false
                }, new PerformIoTCommandParameters
                {
                    IoTTopic = "$aws/rules/kioskrestcall",
                    WaitForResponse = true
                });
                if (tcommandResponse != null && tcommandResponse.StatusCode == 200)
                {
                    var configurationStatusResponse =
                        JsonConvert.DeserializeObject<ConfigurationStatusResponse>(tcommandResponse?.Payload?.Value
                            ?.ToString());
                    if (configurationStatusResponse?.ConfigurationStatus != null &&
                        configurationStatusResponse.StatusCode == HttpStatusCode.OK)
                    {
                        if (await SetConfigurationStatus(configurationStatusResponse.ConfigurationStatus))
                        {
                            response.StatusCode = HttpStatusCode.OK;
                            response.ConfigurationStatus = configurationStatusResponse.ConfigurationStatus;
                        }
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        _logger.LogErrorWithSource("Error updating configuration status.",
                            "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                    }

                    configurationStatusResponse = null;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    _logger.LogErrorWithSource("Error updating configuration status.",
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource(ex, "Exception updating configuration status.",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            return response;
        }

        public async Task<ConfigurationStatusResponse> GetConfigurationStatus()
        {
            var response = new ConfigurationStatusResponse
            {
                ConfigurationStatus = new ConfigurationStatus()
            };
            try
            {
                var dataCacheService = await GetConfigurationStatusFromPersistentDataCacheService();
                if (dataCacheService?.Data != null)
                    response.ConfigurationStatus = dataCacheService.Data;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource(ex, "Exception retrieving configuration status.",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            return response;
        }

        public async Task<ApiBaseResponse> TriggerUpdateConfigurationStatus(
            TriggerUpdateConfigStatusRequest triggerUpdateConfigStatusRequest)
        {
            var apiResponse = new ApiBaseResponse();
            try
            {
                if (triggerUpdateConfigStatusRequest != null)
                {
                    if (triggerUpdateConfigStatusRequest.ExecutionTimeFrameMs.HasValue)
                    {
                        var executionTimeFrameMs = triggerUpdateConfigStatusRequest.ExecutionTimeFrameMs;
                        long num1 = 0;
                        if (!((executionTimeFrameMs.GetValueOrDefault() == num1) & executionTimeFrameMs.HasValue))
                        {
                            await Task.Run(async () =>
                            {
                                var num2 = new Random().Next((int)triggerUpdateConfigStatusRequest.ExecutionTimeFrameMs
                                    .Value);
                                _logger.LogInfoWithSource(
                                    string.Format("waiting {0} ms before calling UpdateConfigurationStatus", num2),
                                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                                await Task.Delay(num2);
                                var configurationStatusResponse = await UpdateConfigurationStatus();
                            });
                            goto label_9;
                        }
                    }

                    apiResponse = await UpdateConfigurationStatus();
                }
                else
                {
                    _logger.LogErrorWithSource("parameter triggerUpdateConfigStatusRequest must not be null.",
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                    apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception triggering UpdateConfigurationStatus.",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            label_9:
            return apiResponse;
        }

        public async Task<ApiBaseResponse> TriggerGetKioskConfigurationSettingChanges(
            TriggerGetConfigChangesRequest triggerGetConfigChangesRequest)
        {
            var configurationService1 = this;
            var apiResponse = new ApiBaseResponse();
            try
            {
                if (triggerGetConfigChangesRequest != null)
                {
                    var configChangesRequest1 = triggerGetConfigChangesRequest;
                    long? nullable1;
                    int num1;
                    if (configChangesRequest1 == null)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        nullable1 = configChangesRequest1.RequestedConfigurationVersionId;
                        long num2 = 0;
                        num1 = (nullable1.GetValueOrDefault() == num2) & nullable1.HasValue ? 1 : 0;
                    }

                    if (num1 != 0)
                    {
                        var configChangesRequest2 = triggerGetConfigChangesRequest;
                        nullable1 = new long?();
                        var nullable2 = nullable1;
                        configChangesRequest2.RequestedConfigurationVersionId = nullable2;
                    }

                    if (triggerGetConfigChangesRequest != null)
                    {
                        nullable1 = triggerGetConfigChangesRequest.ExecutionTimeFrameMs;
                        long num3 = 0;
                        if (!((nullable1.GetValueOrDefault() == num3) & nullable1.HasValue))
                        {
                            await Task.Run(async () =>
                            {
                                var randomMs =
                                    new Random().Next((int)triggerGetConfigChangesRequest.ExecutionTimeFrameMs.Value);
                                _logger.LogInfoWithSource(
                                    string.Format("waiting {0} ms before calling GetKioskConfigurationSettingChanges",
                                        randomMs),
                                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                                var lastCall = await GetLastCallToGetKioskConfigurationSettingChanges();
                                await Task.Delay(randomMs);
                                var dateTime = lastCall;
                                if (dateTime != await GetLastCallToGetKioskConfigurationSettingChanges())
                                    return;
                                var configurationSettingChanges =
                                    await GetKioskConfigurationSettingChanges(triggerGetConfigChangesRequest
                                        ?.RequestedConfigurationVersionId);
                            });
                            return apiResponse;
                        }
                    }

                    var configurationService2 = configurationService1;
                    var configChangesRequest3 = triggerGetConfigChangesRequest;
                    long? requestedConfigurationVersionId;
                    if (configChangesRequest3 == null)
                    {
                        nullable1 = new long?();
                        requestedConfigurationVersionId = nullable1;
                    }
                    else
                    {
                        requestedConfigurationVersionId = configChangesRequest3.RequestedConfigurationVersionId;
                    }

                    apiResponse =
                        await configurationService2.GetKioskConfigurationSettingChanges(
                            requestedConfigurationVersionId);
                }
                else
                {
                    _logger.LogErrorWithSource("parameter triggerGetConfigChangesRequest must not be null.",
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                    apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception triggering GetKioskConfigurationChanges.",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            return apiResponse;
        }

        public async Task<ApiBaseResponse> GetKioskConfigurationSettingChanges(
            long? requestedConfigurationVersionId)
        {
            await SetLastCallToGetKioskConfigurationSettingChanges();
            var response = new ApiBaseResponse();
            try
            {
                var configurationStatus = await GetConfigurationStatus();
                if (configurationStatus == null || !configurationStatus.ConfigurationStatus.EnableUpdates)
                {
                    _logger.LogInfoWithSource(
                        "aborting GetKioskConfigurationSettingChanges because ConfigurationStatus.EnableUpdates = false",
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                    response.StatusCode = HttpStatusCode.ServiceUnavailable;
                    return response;
                }

                var configurationSettingChangesData =
                    await CreateKioskConfigurationSettingChangesRequest(requestedConfigurationVersionId);
                while (!configurationSettingChangesData.AllPagesLoadedOrAborted)
                {
                    var tcommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
                    {
                        Version = 1,
                        RequestId = Guid.NewGuid().ToString(),
                        Command = CommandEnum.GetConfigChanges,
                        Payload = configurationSettingChangesData.KioskConfigurationSettingChangesRequest,
                        QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
                        LogResponse = false
                    }, new PerformIoTCommandParameters
                    {
                        IoTTopic = "$aws/rules/kioskrestcall",
                        WaitForResponse = true
                    });
                    int? nullable1;
                    if (tcommandResponse != null && tcommandResponse.StatusCode == 200)
                    {
                        var kioskSettingChangesResponse =
                            JsonConvert.DeserializeObject<KioskSettingChangesResponse>(tcommandResponse?.Payload?.Value
                                ?.ToString());
                        if (kioskSettingChangesResponse != null)
                        {
                            if (kioskSettingChangesResponse.IsFirstPage)
                            {
                                configurationSettingChangesData.KioskConfigurationChangesResponse =
                                    kioskSettingChangesResponse;
                                if (!kioskSettingChangesResponse.IsLastPage)
                                {
                                    var logger = _logger;
                                    var settingChangesResponse = kioskSettingChangesResponse;
                                    int? nullable2;
                                    if (settingChangesResponse == null)
                                    {
                                        nullable1 = new int?();
                                        nullable2 = nullable1;
                                    }
                                    else
                                    {
                                        nullable2 = settingChangesResponse.PageCount;
                                    }

                                    var str = string.Format("Response has {0} pages", nullable2);
                                    _logger.LogInfoWithSource(str,
                                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                                }
                            }
                            else
                            {
                                await AddNewSettings(
                                    configurationSettingChangesData.KioskConfigurationChangesResponse
                                        ?.NewConfigurationSettingValues,
                                    kioskSettingChangesResponse.NewConfigurationSettingValues);
                            }

                            if (kioskSettingChangesResponse.IsLastPage)
                            {
                                response.StatusCode =
                                    !await ProcessConfigurationSettingChanges(configurationSettingChangesData)
                                        ? HttpStatusCode.InternalServerError
                                        : HttpStatusCode.OK;
                                configurationSettingChangesData.AllPagesLoadedOrAborted = true;
                            }
                            else
                            {
                                var settingChangesRequest = configurationSettingChangesData
                                    .KioskConfigurationSettingChangesRequest;
                                nullable1 = kioskSettingChangesResponse.PageNumber;
                                var nullable3 = nullable1.HasValue ? nullable1.GetValueOrDefault() + 1 : new int?();
                                settingChangesRequest.PageNumber = nullable3;
                            }
                        }
                        else
                        {
                            response.StatusCode = HttpStatusCode.InternalServerError;
                            _logger.LogErrorWithSource(
                                "Unable to deserialize Iot command payload as GetKioskConfigurationSettingChangesResponse",
                                "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                            configurationSettingChangesData.AllPagesLoadedOrAborted = true;
                        }

                        kioskSettingChangesResponse = null;
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        var logger = _logger;
                        var local1 = CommandEnum.GetConfigChanges;
                        int? nullable4;
                        if (tcommandResponse == null)
                        {
                            nullable1 = new int?();
                            nullable4 = nullable1;
                        }
                        else
                        {
                            nullable4 = tcommandResponse.StatusCode;
                        }

                        var str = string.Format("Iot command {0} returned statuscode {1}", local1, nullable4);
                        _logger.LogErrorWithSource(str,
                            "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                        configurationSettingChangesData.AllPagesLoadedOrAborted = true;
                    }
                }

                configurationSettingChangesData = null;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource(ex, "Error in GetKioskConfigurationSettingChanges",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            return response;
        }

        private bool IsMoreThan26HoursAgo(DateTime dateTime)
        {
            return (DateTime.Now - dateTime).TotalHours > 26.0;
        }

        private async Task<PersistentDataWrapper<ConfigurationStatus>>
            GetConfigurationStatusFromPersistentDataCacheService()
        {
            var result = new PersistentDataWrapper<ConfigurationStatus>();
            try
            {
                var persistentDataWrapper =
                    await _persistentDataCacheService.Read<ConfigurationStatus>("ConfigurationStatus.json", true,
                        log: false);
                if (persistentDataWrapper != null)
                    result = persistentDataWrapper;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception getting configuration status",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            return result;
        }

        private async Task<bool> SetConfigurationStatus(ConfigurationStatus configurationStatus)
        {
            var result = false;
            try
            {
                result = await _persistentDataCacheService.Write(configurationStatus, "ConfigurationStatus.json");
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception setting configuration status",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            return result;
        }

        private async Task<DateTime> GetLastCallToGetKioskConfigurationSettingChanges()
        {
            var result = DateTime.MinValue;
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    result = _lastCallToGetKioskConfigurationSettingChanges;
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource(
                    "Lock failed. Unable to get last call to GetKioskConfigurationSettingChanges",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");

            return result;
        }

        private async Task SetLastCallToGetKioskConfigurationSettingChanges()
        {
            if (await _lock.WaitAsync(_lockWait))
                try
                {
                    _lastCallToGetKioskConfigurationSettingChanges = DateTime.Now;
                }
                finally
                {
                    _lock.Release();
                }
            else
                _logger.LogErrorWithSource(
                    "Lock failed. Unable to set last call to GetKioskConfigurationSettingChanges",
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
        }

        private async Task ActivateConfigurationVersion(
            KioskConfigurationVersionActivationRequest kioskConfigurationVersionActivationRequest)
        {
            var str = (await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
            {
                Version = 1,
                RequestId = Guid.NewGuid().ToString(),
                Command = CommandEnum.ActivateConfigVersion,
                Payload = kioskConfigurationVersionActivationRequest,
                QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
                LogResponse = false
            }, new PerformIoTCommandParameters
            {
                IoTTopic = "$aws/rules/kioskrestcall",
                WaitForResponse = true
            })).StatusCode == 200
                ? "successful"
                : "failed";
            _logger.LogInfoWithSource(
                string.Format("Activation of configuration version {0} {1}",
                    kioskConfigurationVersionActivationRequest.ConfigurationVersionId, str),
                "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
        }

        private async Task<ConfigurationSettingChangesData> CreateKioskConfigurationSettingChangesRequest(
            long? requestedConfigurationVersionId)
        {
            var request = new KioskConfigurationSettingChangesRequest
            {
                KioskId = _storeService.KioskId,
                RequestedConfigurationVersionId = requestedConfigurationVersionId,
                SettingChangesRequestId = Guid.NewGuid().ToString()
            };
            var valueTuple = await ReadConfigurationFile(ConfigurationFilePath);
            var flag = valueTuple.Item1;
            var getKioskConfigurationSettingValues = valueTuple.Item2 ?? new GetKioskConfigurationSettingValues();
            request.CurrentConfigurationVersionId = getKioskConfigurationSettingValues.ConfigurationVersion;
            request.ConfigurationVersionHash =
                ComputeConfigurationSettingValuesHashValue(getKioskConfigurationSettingValues);
            return new ConfigurationSettingChangesData
            {
                KioskConfigurationSettingChangesRequest = request,
                KioskConfigurationSettingValues = getKioskConfigurationSettingValues
            };
        }

        private string ComputeConfigurationSettingValuesHashValue(
            GetKioskConfigurationSettingValues getKioskConfigurationSettingValues)
        {
            var settingValuesHashValue = (string)null;
            if (getKioskConfigurationSettingValues != null)
            {
                var stringBuilder1 = new StringBuilder(string.Format("{0},{1}",
                    getKioskConfigurationSettingValues.KioskId,
                    getKioskConfigurationSettingValues.ConfigurationVersion));
                getKioskConfigurationSettingValues.Categories.Sort(sortCategories);
                foreach (var category in getKioskConfigurationSettingValues.Categories)
                {
                    stringBuilder1.Append("," + category.Name);
                    category.Settings.Sort(sortSettings);
                    foreach (var setting in category.Settings)
                    {
                        stringBuilder1.Append(string.Format(",{0},{1},{2}", setting.SettingId, setting.Name,
                            setting.DataType));
                        setting.SettingValues.Sort(sortSettingValues);
                        foreach (var settingValue in setting.SettingValues)
                        {
                            var str1 = settingValue.EncryptionType.HasValue
                                ? string.Format("{0},", settingValue.EncryptionType)
                                : null;
                            var stringBuilder2 = stringBuilder1;
                            var objArray = new object[6]
                            {
                                settingValue.ConfigurationSettingValueId,
                                str1,
                                settingValue.Value,
                                settingValue.Rank,
                                settingValue.EffectiveDateTime.ToString("u"),
                                null
                            };
                            var expireDateTime = settingValue.ExpireDateTime;
                            ref var local = ref expireDateTime;
                            objArray[5] = local.HasValue ? local.GetValueOrDefault().ToString("u") : null;
                            var str2 = string.Format(",{0},{1}{2},{3},{4},{5}", objArray);
                            stringBuilder2.Append(str2);
                        }
                    }
                }

                settingValuesHashValue =
                    Convert.ToBase64String(
                        new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(stringBuilder1.ToString())));
            }

            return settingValuesHashValue;

            int sortCategories(KioskConfigurationCategory a, KioskConfigurationCategory b)
            {
                return a.Name.CompareTo(b.Name);
            }

            int sortSettings(KioskSetting a, KioskSetting b)
            {
                return a.Name.CompareTo(b.Name);
            }

            int sortSettingValues(KioskSettingValue a, KioskSettingValue b)
            {
                return a.ConfigurationSettingValueId.CompareTo(b.ConfigurationSettingValueId);
            }
        }

        private async Task<bool> ProcessConfigurationSettingChanges(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (Interlocked.CompareExchange(ref _processingChanges, 1, 0) == 1)
            {
                _logger.LogInfoWithSource(
                    string.Format("Prevented attempt to process configuration setting changes for version {0}.",
                        configurationSettingChangesData?.NewVersionId),
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                return false;
            }

            try
            {
                if (!configurationSettingChangesData.IsVersionCurrent)
                {
                    await ValidateResponse(configurationSettingChangesData);
                    await InitializePath(configurationSettingChangesData);
                    await ClearOldSettings(configurationSettingChangesData);
                    await AddNewSettings(configurationSettingChangesData);
                    await SaveTempConfigurationFile(configurationSettingChangesData);
                    await RenameCurrentConfigFile(configurationSettingChangesData);
                    await RenameTempFileAsCurrent(configurationSettingChangesData);
                    await ActivateConfigVersion(configurationSettingChangesData);
                    await CleanupFailedAttempt(configurationSettingChangesData);
                }
            }
            finally
            {
                _processingChanges = 0;
            }

            if (configurationSettingChangesData.IsVersionCurrent)
            {
                _logger.LogInfoWithSource(
                    string.Format("No configuration changes to process.  Version {0} is current.",
                        configurationSettingChangesData.CurrentVersionId),
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }
            else
            {
                var str = configurationSettingChangesData.Success ? "Successful" : "Failed";
                _logger.LogInfoWithSource(
                    string.Format("Processing of configuration changes from verion {0} to version {1} {2}",
                        configurationSettingChangesData.CurrentVersionId, configurationSettingChangesData.NewVersionId,
                        str), "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            }

            return configurationSettingChangesData.Success;
        }

        private async Task ActivateConfigVersion(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            var configurationChangesResponse = configurationSettingChangesData.KioskConfigurationChangesResponse;
            if ((configurationChangesResponse != null ? configurationChangesResponse.VersionIsCurrent ? 1 : 0 : 0) != 0)
                return;
            await Task.Run(() =>
            {
                var kioskConfigurationVersionActivationRequest = new KioskConfigurationVersionActivationRequest
                {
                    KioskId = _storeService.KioskId,
                    ModifiedBy = "System",
                    ActivationDateTimeUtc = DateTime.UtcNow,
                    ConfigurationVersionId = configurationSettingChangesData.NewVersionId
                };
                Task.Run(async () => await ActivateConfigurationVersion(kioskConfigurationVersionActivationRequest));
            });
        }

        private async Task ValidateResponse(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            await Task.Run(() =>
            {
                var configurationVersionId = configurationSettingChangesData.KioskConfigurationChangesResponse
                    .OriginalConfigurationVersionId;
                var currentVersionId = configurationSettingChangesData.CurrentVersionId;
                if ((configurationVersionId.GetValueOrDefault() == currentVersionId) & configurationVersionId.HasValue)
                    return;
                configurationSettingChangesData.Success = false;
                _logger.LogErrorWithSource(
                    string.Format(
                        "Error in processing configuration changes.  Current configuration version id {0} doesn't match change results original configuration version id {1}",
                        configurationSettingChangesData?.CurrentVersionId,
                        configurationSettingChangesData?.KioskConfigurationChangesResponse
                            ?.OriginalConfigurationVersionId),
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            });
        }

        private async Task RenameTempFileAsCurrent(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            var settingChangesData = configurationSettingChangesData;
            settingChangesData.Success =
                await RenameFile(configurationSettingChangesData.TempConfigFileName, ConfigurationFilePath);
            settingChangesData = null;
        }

        private async Task RenameCurrentConfigFile(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            var settingChangesData = configurationSettingChangesData;
            settingChangesData.Success = await DeleteFile(OldConfigurationFilePath);
            settingChangesData = null;
            if (!configurationSettingChangesData.Success || !File.Exists(ConfigurationFilePath))
                return;
            settingChangesData = configurationSettingChangesData;
            settingChangesData.Success = await RenameFile(ConfigurationFilePath, OldConfigurationFilePath);
            settingChangesData = null;
        }

        private async Task<bool> RenameFile(string originalFileName, string newFileName)
        {
            var result = false;
            await Task.Run(() =>
            {
                try
                {
                    File.Move(originalFileName, newFileName);
                    if (!File.Exists(newFileName))
                        return;
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Unable to rename file " + originalFileName + " to " + newFileName,
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                }
            });
            return result;
        }

        private async Task SaveTempConfigurationFile(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            var settingChangesData = configurationSettingChangesData;
            settingChangesData.Success = await SaveConfigurationFile(configurationSettingChangesData.TempConfigFileName,
                configurationSettingChangesData.KioskConfigurationSettingValues);
            settingChangesData = null;
        }

        private async Task<bool> SaveConfigurationFile(
            string configurationFileName,
            GetKioskConfigurationSettingValues kioskConfigurationSettingValues)
        {
            var result = false;
            var json = (string)null;
            await Task.Run(() =>
            {
                try
                {
                    json = JsonConvert.SerializeObject(new KioskConfigurationSettingsFile
                    {
                        KioskConfigurationSettings = kioskConfigurationSettingValues
                    }, (Formatting)1);
                    File.WriteAllText(configurationFileName, json);
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        string.Format("Error saving configiration file {0}.  json length: {1}", configurationFileName,
                            json.Length),
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                }
            });
            return result;
        }

        private async Task AddNewSettings(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            var configurationSettingValues1 = configurationSettingChangesData?.KioskConfigurationChangesResponse
                ?.NewConfigurationSettingValues;
            var configurationSettingValues2 = configurationSettingChangesData.KioskConfigurationSettingValues;
            configurationSettingValues2.ConfigurationVersion = configurationSettingValues1.ConfigurationVersion;
            configurationSettingValues2.ConfigurationVersionHash = configurationSettingChangesData
                .KioskConfigurationChangesResponse?.ConfigurationVersionHash;
            configurationSettingValues2.KioskId = configurationSettingValues1.KioskId;
            _logger.LogInfoWithSource(
                string.Format("Updating {0} setting Values",
                    configurationSettingValues1.Categories.Sum(c => c.Settings.Sum(s => s.SettingValues.Count))),
                "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            await AddNewSettings(configurationSettingValues2, configurationSettingValues1);
        }

        private async Task AddNewSettings(
            GetKioskConfigurationSettingValues settingValuesDestination,
            GetKioskConfigurationSettingValues settingValuesToAdd)
        {
            await Task.Run(() =>
            {
                if (settingValuesToAdd == null || settingValuesDestination == null)
                    return;
                foreach (var category in settingValuesToAdd.Categories)
                {
                    var orAddCategory = GetOrAddCategory(category, settingValuesDestination);
                    foreach (var setting in category.Settings)
                    {
                        var orAddSetting = GetOrAddSetting(setting, orAddCategory);
                        foreach (var settingValue in setting.SettingValues)
                            AddSettingValue(settingValue, orAddSetting);
                        orAddSetting.SettingValues.Sort(sortSettingValues);
                    }

                    orAddCategory.Settings.Sort(sortSettings);
                }

                settingValuesDestination.Categories.Sort(sortCategories);
            });

            int sortCategories(KioskConfigurationCategory a, KioskConfigurationCategory b)
            {
                return a.Name.CompareTo(b.Name);
            }

            int sortSettings(KioskSetting a, KioskSetting b)
            {
                return a.Name.CompareTo(b.Name);
            }

            int sortSettingValues(KioskSettingValue a, KioskSettingValue b)
            {
                return a.ConfigurationSettingValueId.CompareTo(b.ConfigurationSettingValueId);
            }
        }

        private void AddSettingValue(KioskSettingValue kioskSettingValue, KioskSetting kioskSetting)
        {
            if (kioskSetting.SettingValues.FirstOrDefault(x =>
                    x.ConfigurationSettingValueId == kioskSettingValue.ConfigurationSettingValueId) != null)
                return;
            var kioskSettingValue1 = new KioskSettingValue
            {
                ConfigurationSettingValueId = kioskSettingValue.ConfigurationSettingValueId,
                EffectiveDateTime = kioskSettingValue.EffectiveDateTime,
                ExpireDateTime = kioskSettingValue.ExpireDateTime,
                Rank = kioskSettingValue.Rank,
                SegmentationName = kioskSettingValue.SegmentationName,
                SegmentId = kioskSettingValue.SegmentId,
                SegmentName = kioskSettingValue.SegmentName,
                EncryptionType = kioskSettingValue.EncryptionType,
                Value = kioskSettingValue.Value
            };
            kioskSetting.SettingValues.Add(kioskSettingValue1);
        }

        private KioskSetting GetOrAddSetting(
            KioskSetting kioskSetting,
            KioskConfigurationCategory kioskConfigurationCategory)
        {
            var orAddSetting =
                kioskConfigurationCategory.Settings.FirstOrDefault(x => x.SettingId == kioskSetting.SettingId);
            if (orAddSetting == null)
            {
                orAddSetting = new KioskSetting
                {
                    Name = kioskSetting.Name,
                    SettingId = kioskSetting.SettingId,
                    DataType = kioskSetting.DataType
                };
                kioskConfigurationCategory.Settings.Add(orAddSetting);
            }

            return orAddSetting;
        }

        private KioskConfigurationCategory GetOrAddCategory(
            KioskConfigurationCategory kioskConfigurationCategory,
            GetKioskConfigurationSettingValues kioskConfigurationSettingValues)
        {
            var orAddCategory =
                kioskConfigurationSettingValues.Categories.FirstOrDefault(x =>
                    x.Name == kioskConfigurationCategory?.Name);
            if (orAddCategory == null)
            {
                orAddCategory = new KioskConfigurationCategory
                {
                    Name = kioskConfigurationCategory.Name
                };
                kioskConfigurationSettingValues.Categories.Add(orAddCategory);
            }

            return orAddCategory;
        }

        private async Task ClearOldSettings(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            await Task.Run(() =>
            {
                var settingChangesData1 = configurationSettingChangesData;
                int num1;
                if (settingChangesData1 == null)
                {
                    num1 = 0;
                }
                else
                {
                    var configurationSettingValues = settingChangesData1.KioskConfigurationChangesResponse
                        ?.RemoveAllExistingConfigurationSettingValues;
                    var flag = true;
                    num1 = (configurationSettingValues.GetValueOrDefault() == flag) &
                           configurationSettingValues.HasValue
                        ? 1
                        : 0;
                }

                if (num1 != 0)
                {
                    configurationSettingChangesData.KioskConfigurationSettingValues.Categories.Clear();
                }
                else
                {
                    var settingChangesData2 = configurationSettingChangesData;
                    int num2;
                    if (settingChangesData2 == null)
                    {
                        num2 = 0;
                    }
                    else
                    {
                        var configurationChangesResponse = settingChangesData2.KioskConfigurationChangesResponse;
                        int? nullable1;
                        if (configurationChangesResponse == null)
                        {
                            nullable1 = new int?();
                        }
                        else
                        {
                            var configurationSettingValueIds =
                                configurationChangesResponse.RemovedConfigurationSettingValueIds;
                            nullable1 = configurationSettingValueIds != null
                                ? configurationSettingValueIds.Count()
                                : new int?();
                        }

                        var nullable2 = nullable1;
                        var num3 = 0;
                        num2 = (nullable2.GetValueOrDefault() > num3) & nullable2.HasValue ? 1 : 0;
                    }

                    if (num2 == 0)
                        return;
                    foreach (var category in configurationSettingChangesData.KioskConfigurationSettingValues.Categories)
                    {
                        var eachCategory = category;
                        foreach (var setting in eachCategory.Settings)
                        {
                            var eachSetting = setting;
                            eachSetting.SettingValues
                                .Where(x => configurationSettingChangesData.KioskConfigurationChangesResponse
                                    .RemovedConfigurationSettingValueIds.Contains(x.ConfigurationSettingValueId))
                                .ToList().ForEach(x => eachSetting.SettingValues.Remove(x));
                        }

                        eachCategory.Settings.Where(x => !x.SettingValues.Any()).ToList()
                            .ForEach(x => eachCategory.Settings.Remove(x));
                    }

                    configurationSettingChangesData.KioskConfigurationSettingValues.Categories
                        .Where(x => !x.Settings.Any()).ToList().ForEach(x =>
                            configurationSettingChangesData.KioskConfigurationSettingValues.Categories.Remove(x));
                }
            });
        }

        private async Task<(bool, GetKioskConfigurationSettingValues)> ReadConfigurationFile(
            string configurationFileName)
        {
            var result = false;
            var getKioskConfigurationSettingValues = (GetKioskConfigurationSettingValues)null;
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(configurationFileName))
                    {
                        getKioskConfigurationSettingValues = JsonConvert
                            .DeserializeObject<KioskConfigurationSettingsFile>(File.ReadAllText(configurationFileName))
                            ?.KioskConfigurationSettings;
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception reading configuration file " + configurationFileName,
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                }
            });
            return (result, getKioskConfigurationSettingValues);
        }

        private async Task CleanupFailedAttempt(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (configurationSettingChangesData.Success)
                return;
            if (!File.Exists(ConfigurationFilePath) && File.Exists(OldConfigurationFilePath))
            {
                var num1 = await RenameFile(OldConfigurationFilePath, ConfigurationFilePath) ? 1 : 0;
            }

            var num2 = await DeleteTempConfigurationFile(configurationSettingChangesData) ? 1 : 0;
        }

        private async Task InitializePath(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            if (!configurationSettingChangesData.Success)
                return;
            var settingChangesData = configurationSettingChangesData;
            settingChangesData.Success = await CreateConfigFilePathIfNeeded(ConfigurationFileDirectory);
            settingChangesData = null;
            if (!configurationSettingChangesData.Success)
                return;
            configurationSettingChangesData.TempConfigFileName = Path.Combine(ConfigurationFileDirectory,
                string.Format("configuration v{0}.json",
                    configurationSettingChangesData?.KioskConfigurationChangesResponse?.NewConfigurationSettingValues
                        ?.ConfigurationVersion));
            settingChangesData = configurationSettingChangesData;
            settingChangesData.Success = await DeleteTempConfigurationFile(configurationSettingChangesData);
            settingChangesData = null;
        }

        private async Task<bool> CreateConfigFilePathIfNeeded(string configFilePath)
        {
            var result = true;
            await Task.Run(() =>
            {
                if (Directory.Exists(configFilePath) || Directory.CreateDirectory(configFilePath).Exists)
                    return;
                result = false;
                _logger.LogErrorWithSource("Unable to create configuration path " + configFilePath,
                    "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
            });
            return result;
        }

        private async Task<bool> DeleteTempConfigurationFile(
            ConfigurationSettingChangesData configurationSettingChangesData)
        {
            return await DeleteFile(configurationSettingChangesData.TempConfigFileName);
        }

        private async Task<bool> DeleteFile(string fileName)
        {
            var result = false;
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                        result = !File.Exists(fileName);
                    }
                    else
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Unable to delete file " + fileName,
                        "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
                }
            });
            return result;
        }

        private class ConfigurationSettingChangesData
        {
            public KioskSettingChangesResponse KioskConfigurationChangesResponse { get; set; }

            public GetKioskConfigurationSettingValues KioskConfigurationSettingValues { get; set; }

            public KioskConfigurationSettingChangesRequest KioskConfigurationSettingChangesRequest { get; set; }

            public string TempConfigFileName { get; set; }

            public bool Success { get; set; } = true;

            public bool AllPagesLoadedOrAborted { get; set; }

            public bool IsVersionCurrent
            {
                get
                {
                    var configurationChangesResponse = KioskConfigurationChangesResponse;
                    return configurationChangesResponse != null && configurationChangesResponse.VersionIsCurrent;
                }
            }

            public long CurrentVersionId
            {
                get
                {
                    var settingChangesRequest = KioskConfigurationSettingChangesRequest;
                    return settingChangesRequest == null ? 0L : settingChangesRequest.CurrentConfigurationVersionId;
                }
            }

            public long NewVersionId =>
                KioskConfigurationChangesResponse?.NewConfigurationSettingValues?.ConfigurationVersion ?? 0;
        }
    }
}