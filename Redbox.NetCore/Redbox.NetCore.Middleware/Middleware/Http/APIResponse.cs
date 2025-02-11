using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Abstractions;
using Redbox.NetCore.Middleware.Extensions;

namespace Redbox.NetCore.Middleware.Http
{
    public class APIResponse<T>
    {
        private readonly ILogger _logger;

        public APIResponse(HttpRequestMessage request, ILogger logger)
        {
            HttpRequest = request;
            _logger = logger;
        }

        public HttpRequestMessage HttpRequest { get; set; }

        public HttpResponseMessage HttpResponse { get; set; }

        public T Response { get; set; }

        public string RequestContent { get; set; }

        public string ResponseContent { get; set; }

        public int? ResponseTime { get; set; }

        public int ResponseLength { get; set; }

        public string HostName => Environment.MachineName;

        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccessStatusCode => HttpResponse != null && StatusCode >= HttpStatusCode.OK &&
                                           StatusCode <= (HttpStatusCode)299;

        public Exception Exception { get; set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public void SetAPIResponse(
            HttpResponseMessage responseMessage,
            string content,
            int requestTime)
        {
            HttpResponse = responseMessage;
            Response = default;
            var httpResponse = HttpResponse;
            StatusCode = httpResponse != null ? httpResponse.StatusCode : HttpStatusCode.InternalServerError;
            ResponseContent = content;
            ResponseTime = requestTime;
            ResponseLength = content != null ? content.Length : 0;
            try
            {
                if (string.IsNullOrWhiteSpace(content) || !IsSuccessStatusCode)
                    return;
                Response = JsonConvert.DeserializeObject<T>(content);
                ResponseContent = (object)Response is IMessageScrub
                    ? Response is IMessageScrub response ? response.Scrub()?.ToString() : null
                    : Response.ToJson();
            }
            catch (Exception ex)
            {
                SetException(ex, requestTime);
            }
        }

        public void SetAPIResponse(HttpResponseMessage responseMessage, T content, int requestTime)
        {
            HttpResponse = responseMessage;
            Response = default;
            var httpResponse = HttpResponse;
            StatusCode = httpResponse != null ? httpResponse.StatusCode : HttpStatusCode.InternalServerError;
            ResponseTime = requestTime;
            ResponseLength = content is byte[] numArray ? numArray.Length : 0;
            try
            {
                if (ResponseLength == 0)
                    return;
                Response = content;
                ResponseContent = Response.ToJson();
            }
            catch (Exception ex)
            {
                SetException(ex, requestTime);
            }
        }

        public void SetException(Exception exception, int requestTime)
        {
            ResponseTime = requestTime;
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
            var str = string.Format("StatusCode: {0}, Message: {1}", HttpStatusCode.InternalServerError,
                exception.GetFullMessage());
            Errors.Add(new Error
            {
                Code = StatusCode.ToString(),
                Message = str
            });
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogError(exception, str);
        }

        public void Log(bool logRequest, bool logResponse)
        {
            if (HttpRequest == null)
                return;
            var str = logRequest
                ? HttpRequest.RequestUri.ToString()
                : HttpRequest.RequestUri.GetLeftPart((UriPartial)2);
            var logger1 = _logger;
            if (logger1 != null)
                logger1.LogInformation(string.Format("Request: {0}, Uri: {1}, Status: {2}({3}), Time:{4}, Length:{5}",
                    HttpRequest.Method, str, StatusCode, (int)StatusCode, ResponseTime, ResponseLength));
            if (logRequest && RequestContent != null)
            {
                var logger2 = _logger;
                if (logger2 != null)
                    logger2.LogInformation("Request Content: " + RequestContent);
            }

            if (!logResponse || ResponseContent == null)
                return;
            var logger3 = _logger;
            if (logger3 == null)
                return;
            logger3.LogInformation("Response Content: " + ResponseContent);
        }

        public object Scrub()
        {
            if (Response != null)
                return (Response is IMessageScrub response ? response.Scrub() : null) ??
                       JsonConvert.SerializeObject(Response);
            var exception = Exception;
            return exception == null ? null : (object)exception.Message;
        }

        public string GetErrors()
        {
            if (!string.IsNullOrWhiteSpace(ResponseContent) || Errors.Any())
                return "ResponseContent: " + ResponseContent + " . Errors: " +
                       string.Join<string>(",", Errors.Select(x => x.Code + " - " + x.Message));
            return string.Format(
                "ApiResponse.ResponseContent and ApiResponse.Errors are empty. Request: {0},Uri: {1}, Status: {2}({3}), Time:{4}, Length:{5} HttpResponse StatusCode: {6} {7}",
                HttpRequest.Method, HttpRequest.RequestUri, StatusCode, (int)StatusCode, ResponseTime, ResponseLength,
                HttpResponse?.StatusCode, HttpResponse?.ReasonPhrase);
        }
    }
}