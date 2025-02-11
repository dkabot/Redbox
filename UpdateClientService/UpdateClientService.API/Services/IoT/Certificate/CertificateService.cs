using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Security.Certificate;

namespace UpdateClientService.API.Services.IoT.Certificate
{
    internal class CertificateService : ICertificateService
    {
        private const string IoTCertificateDataFileName = "iotcertificatedata.json";
        private const string ThingType = "kiosks";
        private readonly IIoTCertificateServiceApiClient _iotCertApiClient;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly int _lockWait = 2000;
        private readonly ILogger<CertificateService> _logger;
        private readonly ISecurityService _securityService;
        private readonly IStoreService _store;
        private string _encryptedKioskId;
        private string _encryptedThingType;
        private string _ioTCertificateDataFilePath;
        private string _ioTCertificateServicePassword;
        private long _kioskId;

        public CertificateService(
            ILogger<CertificateService> logger,
            IIoTCertificateServiceApiClient iotCertApiClient,
            IStoreService store,
            ISecurityService securityService)
        {
            _logger = logger;
            _iotCertApiClient = iotCertApiClient;
            _store = store;
            _securityService = securityService;
        }

        private string IoTCertificateDataFilePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_ioTCertificateDataFilePath))
                    return _ioTCertificateDataFilePath;
                var str = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData,
                        Environment.SpecialFolderOption.Create), "Redbox\\UpdateClient\\IoT");
                Directory.CreateDirectory(str);
                _ioTCertificateDataFilePath = Path.GetFullPath(Path.Combine(str, "iotcertificatedata.json"));
                return _ioTCertificateDataFilePath;
            }
        }

        public async Task<IotCert> GetCertificateAsync(bool forceNew = false)
        {
            _logger.LogInfoWithSource(string.Format("force = {0}", forceNew),
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            try
            {
                if (_store.KioskId <= 0L)
                {
                    _logger.LogErrorWithSource(
                        string.Format("KioskId: {0} is not valid, no cert to return", _store.KioskId),
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return null;
                }

                if (!await UpdateIotData())
                {
                    _logger.LogErrorWithSource("IotData failed so we couldn't get certificate",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return null;
                }

                var flag = IsExecutingAssemblyNewerThanCertificate();
                if (flag)
                    _logger.LogInfoWithSource(
                        "Executing Assembly is newer than IoT Certificate. Forcing creation of new certificate.",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                if (forceNew | flag)
                    return !await WriteIoTCertificateDataFileAsync(
                        await _iotCertApiClient.GenerateNewCertificates(_encryptedKioskId, _encryptedThingType,
                            _ioTCertificateServicePassword))
                        ? null
                        : await LoadCertificate();
                var certificateAsync = await LoadCertificate();
                if (certificateAsync != null)
                    return certificateAsync;
                return !await WriteIoTCertificateDataFileAsync(
                    await _iotCertApiClient.GetCertificates(_encryptedKioskId, _encryptedThingType,
                        _ioTCertificateServicePassword))
                    ? null
                    : await LoadCertificate();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception running GetCertificateAsync",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return null;
            }
        }

        public async Task<bool> Validate()
        {
            _logger.LogInfoWithSource("Attempting to run Validate",
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            try
            {
                if (!await UpdateIotData())
                {
                    _logger.LogErrorWithSource("Validate failed because IotData was not valid",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return false;
                }

                if (await IsCertificateValid() ?? true)
                    return true;
                await DeleteLocalCertificate();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception running validate",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            }

            return false;
        }

        private bool IsExecutingAssemblyNewerThanCertificate()
        {
            var flag = false;
            _logger.LogInfoWithSource("Attempting to check if Executing Assembly is newer than cert",
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            try
            {
                if (File.Exists(IoTCertificateDataFilePath))
                {
                    var lastWriteTimeUtc1 = File.GetLastWriteTimeUtc(IoTCertificateDataFilePath);
                    var lastWriteTimeUtc2 = File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location);
                    flag = lastWriteTimeUtc2 > lastWriteTimeUtc1;
                    if (flag)
                        _logger.LogInfoWithSource(
                            string.Format("Assembly date {0} is newer than certificate date {1}", lastWriteTimeUtc2,
                                lastWriteTimeUtc1),
                            "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while comparing certificate date with assembly date.",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            }

            return flag;
        }

        private async Task<bool> UpdateIotData()
        {
            _logger.LogInfoWithSource("Running UpdateIotData",
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            try
            {
                var kioskId = _store.KioskId;
                if (kioskId <= 0L)
                {
                    _logger.LogErrorWithSource(string.Format("KioskId: {0} is not valid", _store.KioskId),
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    _kioskId = 0L;
                }

                if (kioskId == _kioskId && !string.IsNullOrEmpty(_encryptedKioskId) &&
                    !string.IsNullOrEmpty(_ioTCertificateServicePassword) && !string.IsNullOrEmpty(_encryptedThingType))
                {
                    _logger.LogInfoWithSource("IotData is already loaded",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return true;
                }

                _kioskId = kioskId;
                _encryptedKioskId = await _securityService.Encrypt(_store.KioskId.ToString());
                _encryptedThingType = await _securityService.Encrypt("kiosks");
                _ioTCertificateServicePassword =
                    await _securityService.GetIoTCertServicePassword(_store.KioskId.ToString());
                _logger.LogInfoWithSource("Finished loading Iot data",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception occured loading IotData",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return false;
            }
        }

        private void Clear()
        {
            _encryptedKioskId = null;
            _encryptedThingType = null;
            _ioTCertificateServicePassword = null;
        }

        private async Task<bool?> IsCertificateValid()
        {
            _logger.LogInfoWithSource("Attempting to run IsCertificateValid",
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            try
            {
                var iotCert = await LoadCertificate();
                if (iotCert == null)
                {
                    _logger.LogInfoWithSource("iIotCert is null, cert is not valid",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return false;
                }

                var certificateValidResponse = await _iotCertApiClient.IsCertificateValid(_encryptedKioskId,
                    _encryptedThingType, _ioTCertificateServicePassword,
                    await _securityService.Encrypt(iotCert.CertificateId));
                var isValid = certificateValidResponse.IsValid;
                if (!isValid.HasValue)
                {
                    _logger.LogInfoWithSource(
                        "iotCertApiClient.IsCertificateValid was null, call failed can't tell if valid or not",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return new bool?();
                }

                isValid = certificateValidResponse.IsValid;
                if (isValid.Value)
                {
                    _logger.LogInfoWithSource("iotCertApiClient.IsCertificateValid was true, cert is valid",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return true;
                }

                _logger.LogInfoWithSource("iotCertApiClient.IsCertificateValid was false, cert is not valid",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception occured checking to see if the certificate is valid",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return new bool?();
            }
        }

        private async Task DeleteLocalCertificate()
        {
            _logger.LogInfoWithSource("Deleting the local certificate file: " + IoTCertificateDataFilePath,
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            if (!await _lock.WaitAsync(_lockWait))
                _logger.LogErrorWithSource("Lock failed, certificate not deleted",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            else
                try
                {
                    Clear();
                    File.Delete(IoTCertificateDataFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception occured deleting certificate file",
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                }
                finally
                {
                    _lock.Release();
                }
        }

        private async Task<IotCert> LoadCertificate()
        {
            _logger.LogInfoWithSource("Attempting to load Certificate",
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            if (!await _lock.WaitAsync(_lockWait))
            {
                _logger.LogErrorWithSource("Lock failed, certificate cannot be loaded",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return null;
            }

            try
            {
                if (!File.Exists(IoTCertificateDataFilePath))
                {
                    _logger.LogErrorWithSource("Certificate file does not exist: " + IoTCertificateDataFilePath,
                        "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                    return null;
                }

                var localCerts =
                    JsonConvert.DeserializeObject<IoTCertificateServiceResponse>(
                        File.ReadAllText(IoTCertificateDataFilePath));
                return new IotCert(localCerts.DeviceCertPfxBase64, localCerts.RootCa, localCerts.CertificateId,
                    await _securityService.GetCertificatePassword(_store.KioskId.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception loading certificate",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return null;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<bool> WriteIoTCertificateDataFileAsync(IoTCertificateServiceResponse response)
        {
            if (response == null)
            {
                _logger.LogInfoWithSource("Response was null or not successful, skipping writing certificates",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                await DeleteLocalCertificate();
                return false;
            }

            _logger.LogInfoWithSource("Attempting to write certificate to a file",
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
            if (!await _lock.WaitAsync(_lockWait))
            {
                _logger.LogErrorWithSource("Lock failed, certificate cannot be saved",
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return false;
            }

            try
            {
                File.WriteAllText(IoTCertificateDataFilePath, response.ToJson());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Response: " + response.ToJson(),
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/CertificateService.cs");
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}