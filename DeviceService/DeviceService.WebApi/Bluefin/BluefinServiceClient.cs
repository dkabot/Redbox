using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Bluefin;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.WebApi.Bluefin
{
    public class BluefinServiceClient : IBluefinServiceClient
    {
        private const string AUTHHEADER = "x-api-key";
        private readonly IHttpService _httpService;

        public BluefinServiceClient(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<StandardResponse> Activate(
            IBluefinActivationRequest request,
            string mfgSerialNumber,
            string injectedSerialNumber,
            DateTime InstallDate)
        {
            return JsonConvert.DeserializeObject<StandardResponse>(await (await _httpService.SendRequest(
                _httpService.GenerateRequest("device/activate", new ActivateRequest
                {
                    KioskId = request.KioskId,
                    ReaderSerialNumber = mfgSerialNumber,
                    InjectedSerialNumber = injectedSerialNumber,
                    LocalDateTime = InstallDate
                }, HttpMethod.Post, request.BluefinServiceUrl, new List<Header>
                {
                    GetAuthHeader(request.ApiKey)
                }), request.Timeout)).Content?.ReadAsStringAsync());
        }

        public async Task<StandardResponse> Deactivate(
            IBluefinActivationRequest request,
            string mfgSerialNumber)
        {
            return JsonConvert.DeserializeObject<StandardResponse>(await (await _httpService.SendRequest(
                _httpService.GenerateRequest("device/deactivate", new DeactivateRequest
                {
                    KioskId = request.KioskId,
                    ReaderSerialNumber = mfgSerialNumber
                }, HttpMethod.Post, request.BluefinServiceUrl, new List<Header>
                {
                    GetAuthHeader(request.ApiKey)
                }), request.Timeout)).Content?.ReadAsStringAsync());
        }

        private Header GetAuthHeader(string apiKey)
        {
            return new Header("x-api-key", apiKey);
        }
    }
}