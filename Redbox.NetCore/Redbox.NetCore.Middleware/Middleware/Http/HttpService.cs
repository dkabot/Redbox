using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Abstractions;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;
using Redbox.NetCore.Middleware.Metrics;

namespace Redbox.NetCore.Middleware.Http
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpService> _logger;
        private readonly ICommonMetrics _metrics;

        public HttpService(
            ICommonMetricsFactory metricsFactory,
            ILogger<HttpService> logger,
            HttpClient httpClient)
        {
            _httpClient = httpClient;
            _metrics = metricsFactory.GetImplementation<HttpMetrics>();
            _logger = logger;
        }

        public int Timeout { get; set; } = 20000;

        public HttpRequestMessage GenerateRequest(
            string baseUrl,
            string endpoint,
            object requestObject,
            HttpMethod method,
            List<Header> headers = null,
            bool isForm = false)
        {
            var httpContent = (HttpContent)null;
            if (requestObject != null && !isForm)
                httpContent = new StringContent(JsonConvert.SerializeObject(requestObject, 0, new JsonSerializerSettings
                {
                    NullValueHandling = (NullValueHandling)1
                }), Encoding.UTF8, "application/json");
            else if (isForm)
                httpContent = requestObject as MultipartFormDataContent;
            return GenerateRequest(baseUrl, endpoint, httpContent, method, headers);
        }

        public HttpRequestMessage GenerateRequest(
            string baseUrl,
            string endpoint,
            HttpContent httpContent,
            HttpMethod method,
            List<Header> headers = null)
        {
            Uri requestUri;
            if (string.IsNullOrEmpty(baseUrl))
            {
                requestUri = new Uri(endpoint);
            }
            else
            {
                baseUrl = baseUrl.TrimEnd('/');
                requestUri = new Uri(baseUrl + "/" + endpoint);
            }

            var httpRequest = new HttpRequestMessage(method, requestUri);
            headers?.ForEach(header => httpRequest.Headers.Add(header.Key, header.Value));
            httpRequest.Content = httpContent;
            return httpRequest;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
            string baseUrl,
            string endpoint,
            object requestObject,
            HttpMethod method,
            List<Header> headers = null,
            bool isForm = false,
            int? timeout = null,
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerLocation = "",
            bool logResponse = true)
        {
            HttpResponseMessage httpResponseMessage1;
            using (var genRequest = GenerateRequest(baseUrl, endpoint, requestObject, method, headers, isForm))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout ?? Timeout);
                var sw = new Stopwatch();
                sw.Start();
                HttpResponseMessage result;
                try
                {
                    result = await _httpClient.SendAsync(genRequest, cancellationTokenSource.Token);
                    sw.Stop();
                    var elapsedMilliseconds = sw.ElapsedMilliseconds;
                    if (logResponse)
                    {
                        var logger = _logger;
                        var response = result;
                        var message = "response: " + (response != null ? response.ToSecureString() : null);
                        logger.LogInfoWithSource(message,
                            "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Http/HttpService.cs");
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    result = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.GetFullMessage()
                    };
                }

                if (_metrics != null)
                {
                    var metrics = _metrics;
                    var service = callerLocation.ServiceName();
                    var method1 = callerMethod;
                    var httpResponseMessage2 = result;
                    var statusCode = httpResponseMessage2 != null ? (int)httpResponseMessage2.StatusCode : 500;
                    var metricTags = GetMetricTags(service, method1, (HttpStatusCode)statusCode);
                    var elapsedMilliseconds = sw.ElapsedMilliseconds;
                    await metrics.CaptureMetrics(metricTags, elapsedMilliseconds);
                }

                httpResponseMessage1 = result;
            }

            return httpResponseMessage1;
        }

        public async Task<APIResponse<T>> SendRequestAsync<T>(
            string baseUrl,
            string endpoint,
            object requestObject,
            HttpMethod method,
            List<Header> headers = null,
            bool isForm = false,
            int? timeout = null,
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerLocation = "",
            object originalRequestObject = null,
            bool logRequest = true,
            bool logResponse = true)
        {
            APIResponse<T> apiResponse;
            using (var genRequest = GenerateRequest(baseUrl, endpoint, requestObject, method, headers, isForm))
            {
                apiResponse = await SendRequestAsync<T>(genRequest, timeout, callerMethod, callerLocation,
                    requestObject, logRequest, logResponse);
            }

            return apiResponse;
        }

        public async Task<APIResponse<T>> SendRequestAsync<T>(
            HttpRequestMessage request,
            int? timeout = null,
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerLocation = "",
            object originalRequestObject = null,
            bool logRequest = true,
            bool logResponse = true)
        {
            APIResponse<T> apiResponse1;
            using (request)
            {
                var timeoutTokenSource = new CancellationTokenSource();
                timeoutTokenSource.CancelAfter(timeout ?? Timeout);
                var apiResponse = new APIResponse<T>(request, _logger);
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    string str1;
                    if (request.Content != null)
                        str1 = await request.Content.ReadAsStringAsync();
                    else
                        str1 = null;
                    var str2 = str1;
                    if (originalRequestObject != null && originalRequestObject is IMessageScrub)
                        apiResponse.RequestContent = ((IMessageScrub)originalRequestObject).Scrub()?.ToString();
                    else if (!string.IsNullOrEmpty(str2))
                        apiResponse.RequestContent = JsonConvert.SerializeObject(str2, 0);
                    using (var result = await _httpClient.SendAsync(request, timeoutTokenSource.Token))
                    {
                        sw.Stop();
                        var requestTime = sw.ElapsedMilliseconds;
                        apiResponse.SetAPIResponse(result, await result.Content?.ReadAsStringAsync(), (int)requestTime);
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    apiResponse.SetException(ex, (int)sw.ElapsedMilliseconds);
                }

                apiResponse.Log(logRequest, logResponse);
                if (_metrics != null)
                    await _metrics.CaptureMetrics(
                        GetMetricTags(callerLocation.ServiceName(), callerMethod, apiResponse.StatusCode),
                        sw.ElapsedMilliseconds);
                apiResponse1 = apiResponse;
            }

            return apiResponse1;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
            HttpRequestMessage request,
            int? timeout = null,
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerLocation = "",
            object originalRequestObject = null,
            bool logRequest = true,
            bool logResponse = true)
        {
            HttpResponseMessage httpResponseMessage1;
            using (request)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout ?? Timeout);
                var sw = new Stopwatch();
                sw.Start();
                HttpResponseMessage result;
                try
                {
                    result = await _httpClient.SendAsync(request, cancellationTokenSource.Token);
                    sw.Stop();
                    var elapsedMilliseconds = sw.ElapsedMilliseconds;
                    string str1;
                    if (request.Content != null)
                        str1 = await request.Content.ReadAsStringAsync();
                    else
                        str1 = null;
                    var str2 = str1;
                    if (originalRequestObject != null && originalRequestObject is IMessageScrub)
                        str2 = ((IMessageScrub)originalRequestObject).Scrub()?.ToString();
                    if (logRequest)
                        _logger.LogInfoWithSource("request: " + str2,
                            "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Http/HttpService.cs");
                    if (logResponse)
                    {
                        var logger = _logger;
                        var response = result;
                        var message = "response: " + (response != null ? response.ToSecureString() : null);
                        logger.LogInfoWithSource(message,
                            "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Http/HttpService.cs");
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    result = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = ex.GetFullMessage()
                    };
                }

                if (_metrics != null)
                {
                    var metrics = _metrics;
                    var service = callerLocation.ServiceName();
                    var method = callerMethod;
                    var httpResponseMessage2 = result;
                    var statusCode = httpResponseMessage2 != null ? (int)httpResponseMessage2.StatusCode : 500;
                    var metricTags = GetMetricTags(service, method, (HttpStatusCode)statusCode);
                    var elapsedMilliseconds = sw.ElapsedMilliseconds;
                    await metrics.CaptureMetrics(metricTags, elapsedMilliseconds);
                }

                httpResponseMessage1 = result;
            }

            return httpResponseMessage1;
        }

        public async Task<APIResponse<byte[]>> DownloadFile(
            string url,
            int? timeout,
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerLocation = "")
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(timeout ?? Timeout);
            APIResponse<byte[]> apiResponse1;
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var apiResponse = new APIResponse<byte[]>(request, _logger);
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    sw.Stop();
                    using (var result = await _httpClient.GetAsync(url, cancellationTokenSource.Token))
                    {
                        if (result.IsSuccessStatusCode)
                            apiResponse.SetAPIResponse(result, await result.Content.ReadAsByteArrayAsync(),
                                (int)sw.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    apiResponse.SetException(ex, (int)sw.ElapsedMilliseconds);
                }

                apiResponse.Log(true, false);
                if (_metrics != null)
                    await _metrics.CaptureMetrics(
                        GetMetricTags(callerLocation.ServiceName(), callerMethod, apiResponse.StatusCode),
                        sw.ElapsedMilliseconds);
                apiResponse1 = apiResponse;
            }

            return apiResponse1;
        }

        private static Dictionary<string, string> GetMetricTags(
            string service,
            string method,
            HttpStatusCode statusCode)
        {
            return new Dictionary<string, string>
            {
                [nameof(service)] = service,
                [nameof(method)] = method,
                ["statuscode"] = statusCode.ToString()
            };
        }
    }
}