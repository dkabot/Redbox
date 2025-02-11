using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.WebApi.Services
{
    public class HttpService : IHttpService
    {
        public HttpRequestMessage GenerateRequest(
            string endpoint,
            object requestObject,
            HttpMethod method,
            string baseUrl,
            List<Header> headers = null)
        {
            var requestUri = new Uri(baseUrl + "/" + endpoint);
            var httpRequest = new HttpRequestMessage(method, requestUri);
            headers?.ForEach(header => httpRequest.Headers.Add(header.Key, header.Value));
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestObject, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8, "application/json");
            return httpRequest;
        }

        public async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, int timeout)
        {
            using (var httpClient = new HttpClient
                   {
                       Timeout = TimeSpan.FromMilliseconds(timeout)
                   })
            {
                try
                {
                    return await httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    var content = JsonConvert.SerializeObject(new StandardResponse(ex));
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(content)
                    };
                }
            }
        }
    }
}