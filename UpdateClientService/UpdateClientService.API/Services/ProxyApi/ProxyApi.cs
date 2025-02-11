using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API.Services.ProxyApi
{
    public class ProxyApi : IProxyApi
    {
        private const string _proxyAuthHeader = "x-api-key";
        private const string ACTIVITY_ID_HEADER_KEY = "x-redbox-activityid";
        internal const string KIOSK_ID_HEADER_KEY = "x-redbox-kioskid";
        private readonly AppSettings _appSettings;
        private readonly List<Header> _headers;
        private readonly IHttpService _httpService;
        private readonly IKioskConfiguration _kioskConfiguration;
        private readonly long _kioskId;
        private readonly ILogger<ProxyApi> _logger;

        public ProxyApi(
            IHttpService httpService,
            ILogger<ProxyApi> logger,
            IOptionsKioskConfiguration kioskConfiguration,
            IStoreService storeService,
            IOptions<AppSettings> appSettings)
        {
            _httpService = httpService;
            _logger = logger;
            _kioskConfiguration = kioskConfiguration;
            _kioskId = storeService.KioskId;
            _appSettings = appSettings.Value;
            _headers = new List<Header>
            {
                new Header("x-redbox-activityid", Guid.NewGuid().ToString()),
                new Header("x-redbox-kioskid", _kioskId.ToString()),
                new Header("x-api-key", kioskConfiguration.KioskClientProxyApi.ProxyApiKey)
            };
        }

        private string ProxyApiUrl => !string.IsNullOrEmpty(_kioskConfiguration.KioskClientProxyApi.ProxyApiUrl)
            ? _kioskConfiguration.KioskClientProxyApi.ProxyApiUrl
            : _appSettings.DefaultProxyServiceUrl;

        public async Task<APIResponse<ApiBaseResponse>> RecordKioskPingFailure(
            KioskPingFailure kioskPingFailure)
        {
            return await _httpService.SendRequestAsync<ApiBaseResponse>(ProxyApiUrl,
                string.Format("api/kiosk/{0}/pingfailure", _kioskId), kioskPingFailure, HttpMethod.Post, _headers,
                callerLocation: "/sln/src/UpdateClientService.API/Services/ProxyApi/ProxyApi.cs");
        }
    }
}