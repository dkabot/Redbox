using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.IoT.Certificate
{
    public class IoTCertificateServiceApiClient : IIoTCertificateServiceApiClient
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpService _httpService;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly IStoreService _store;

        public IoTCertificateServiceApiClient(
            IHttpService httpService,
            IStoreService store,
            IOptions<AppSettings> appSettings)
        {
            _httpService = httpService;
            _store = store;
            _appSettings = appSettings.Value;
            _httpService.Timeout = 90000;
        }

        public async Task<IsCertificateValidResponse> IsCertificateValid(
            string kioskId,
            string thingType,
            string password,
            string certificateId)
        {
            var response = new IsCertificateValidResponse
            {
                IsValid = new bool?()
            };
            var data = new
            {
                type = thingType,
                name = kioskId,
                thingGroupName = _store.Market,
                certificateId
            };
            var httpService = _httpService;
            var tcertificateServiceUrl = _appSettings.IoTCertificateServiceUrl;
            var requestObject = data;
            var post = HttpMethod.Post;
            var headers = new List<Header>();
            headers.Add(new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey));
            headers.Add(new Header(nameof(password), password));
            var timeout = new int?();
            var apiResponse = await httpService.SendRequestAsync<bool>(tcertificateServiceUrl,
                "api/thing-types/certificate-valid", requestObject, post, headers, timeout: timeout,
                callerLocation:
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/IoTCertificateServiceApiClient.cs");
            response.IsValid = !apiResponse.IsSuccessStatusCode ? new bool?() : apiResponse.Response;
            return response;
        }

        public async Task<IoTCertificateServiceResponse> GetCertificates(
            string kioskId,
            string thingType,
            string password)
        {
            await _lock.WaitAsync();
            var data = new
            {
                type = thingType,
                name = kioskId,
                thingGroupName = _store.Market
            };
            var httpService = _httpService;
            var tcertificateServiceUrl = _appSettings.IoTCertificateServiceUrl;
            var requestObject = data;
            var post = HttpMethod.Post;
            var headers = new List<Header>();
            headers.Add(new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey));
            headers.Add(new Header(nameof(password), password));
            var timeout = new int?();
            var apiResponse = await httpService.SendRequestAsync<IoTCertificateServiceResponse>(tcertificateServiceUrl,
                "api/thing-types/get-certificates", requestObject, post, headers, timeout: timeout,
                callerLocation:
                "/sln/src/UpdateClientService.API/Services/IoT/Certificate/IoTCertificateServiceApiClient.cs",
                logResponse: false);
            _lock.Release();
            return apiResponse?.Response;
        }

        public async Task<IoTCertificateServiceResponse> GenerateNewCertificates(
            string kioskId,
            string thingType,
            string password)
        {
            APIResponse<IoTCertificateServiceResponse> apiResponse;
            try
            {
                await _lock.WaitAsync();
                var data = new
                {
                    type = thingType,
                    name = kioskId,
                    thingGroupName = _store.Market
                };
                var httpService = _httpService;
                var tcertificateServiceUrl = _appSettings.IoTCertificateServiceUrl;
                var requestObject = data;
                var post = HttpMethod.Post;
                var headers = new List<Header>();
                headers.Add(new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey));
                headers.Add(new Header(nameof(password), password));
                var timeout = new int?();
                apiResponse = await httpService.SendRequestAsync<IoTCertificateServiceResponse>(tcertificateServiceUrl,
                    "api/thing-types/certificates", requestObject, post, headers, timeout: timeout,
                    callerLocation:
                    "/sln/src/UpdateClientService.API/Services/IoT/Certificate/IoTCertificateServiceApiClient.cs",
                    logResponse: false);
            }
            finally
            {
                _lock.Release();
            }

            return apiResponse?.Response;
        }
    }
}