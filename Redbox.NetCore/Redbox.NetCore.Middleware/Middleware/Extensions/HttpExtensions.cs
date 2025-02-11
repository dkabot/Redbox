using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class HttpExtensions
    {
        public static string GetActivityId(this HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("x-redbox-activityid"))
                return context.Request.Headers["x-redbox-activityid"][0];
            if (context.Items.ContainsKey("ActivityId")) return context.Items["ActivityId"].ToString();
            return string.Empty;
        }

        public static string GetKioskId(this HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("x-redbox-kioskid"))
                return context.Request.Headers["x-redbox-kioskid"][0];
            if (context.Items.ContainsKey("KioskId")) return context.Items["KioskId"].ToString();
            return string.Empty;
        }

        public static async Task<string> ReadBodyAsync(this HttpRequest request)
        {
            return await ReadStream(request.Body);
        }

        public static HttpRequestMessage AddActivityIdHeader(this HttpRequestMessage message, Guid activityId)
        {
            if (message != null) message.AddActivityIdHeader(activityId.ToString());
            return message;
        }

        public static HttpRequestMessage AddActivityIdHeader(this HttpRequestMessage message, string activityId)
        {
            if (message != null) message.Headers.Add("x-redbox-activityid", activityId);
            return message;
        }

        public static List<Header> AddActivityIdHeader(this List<Header> headers, Guid activityId)
        {
            if (headers != null) headers.AddActivityIdHeader(activityId.ToString());
            return headers;
        }

        public static List<Header> AddActivityIdHeader(this List<Header> headers, string activityId)
        {
            if (headers != null)
            {
                headers.Add(new Header("x-redbox-activityid", activityId));
                headers.Add(new Header("X-Correlation-ID", activityId));
            }

            return headers;
        }

        public static HttpRequestMessage AddApiKeyHeader(this HttpRequestMessage message, string apikey)
        {
            if (message != null && !string.IsNullOrEmpty(apikey)) message.Headers.Add("x-api-key", apikey);
            return message;
        }

        public static List<Header> AddApiKeyHeader(this List<Header> headers, string apikey)
        {
            if (headers != null && !string.IsNullOrEmpty(apikey)) headers.Add(new Header("x-api-key", apikey));
            return headers;
        }

        public static HttpRequestMessage AddAppNameHeader(this HttpRequestMessage message, string appName)
        {
            if (message != null && appName != null)
            {
                var text = new string(appName.Where(c => char.IsLetter(c)).ToArray()).ToLower();
                message.Headers.Add("app-name", text);
                message.Headers.Add("x-redbox-app-name", text);
            }

            return message;
        }

        public static List<Header> AddAppNameHeader(this List<Header> headers, string appName)
        {
            if (headers != null && appName != null)
            {
                var text = new string(appName.Where(c => char.IsLetter(c)).ToArray()).ToLower();
                headers.Add(new Header("app-name", text));
                headers.Add(new Header("x-redbox-app-name", text));
            }

            return headers;
        }

        public static HttpRequestMessage AddAuthorizationHeader(this HttpRequestMessage message, string token)
        {
            if (message != null && !string.IsNullOrEmpty(token)) message.Headers.Add("authorization", token);
            return message;
        }

        public static List<Header> AddAuthorizationHeader(this List<Header> headers, string token)
        {
            if (headers != null && !string.IsNullOrEmpty(token)) headers.Add(new Header("authorization", token));
            return headers;
        }

        public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, int kioskId)
        {
            return message.AddKioskIdHeader(kioskId.ToString());
        }

        public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, int? kioskId)
        {
            if (message != null && kioskId != null) message.AddKioskIdHeader(kioskId.Value);
            return message;
        }

        public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, long kioskId)
        {
            return message.AddKioskIdHeader(kioskId.ToString());
        }

        public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, long? kioskId)
        {
            if (message != null && kioskId != null) message.AddKioskIdHeader(kioskId.Value);
            return message;
        }

        public static HttpRequestMessage AddKioskIdHeader(this HttpRequestMessage message, string kioskId)
        {
            if (message != null && !string.IsNullOrEmpty(kioskId)) message.Headers.Add("x-redbox-kioskid", kioskId);
            return message;
        }

        public static List<Header> AddKioskIdHeader(this List<Header> headers, int kioskId)
        {
            return headers.AddKioskIdHeader(kioskId.ToString());
        }

        public static List<Header> AddKioskIdHeader(this List<Header> headers, int? kioskId)
        {
            if (headers != null && kioskId != null) headers.AddKioskIdHeader(kioskId.Value);
            return headers;
        }

        public static List<Header> AddKioskIdHeader(this List<Header> headers, long kioskId)
        {
            return headers.AddKioskIdHeader(kioskId.ToString());
        }

        public static List<Header> AddKioskIdHeader(this List<Header> headers, long? kioskId)
        {
            if (headers != null && kioskId != null) headers.AddKioskIdHeader(kioskId.Value);
            return headers;
        }

        public static List<Header> AddKioskIdHeader(this List<Header> headers, string kioskId)
        {
            if (headers != null && !string.IsNullOrEmpty(kioskId)) headers.Add(new Header("x-redbox-kioskid", kioskId));
            return headers;
        }

        public static string ToSecureString(this HttpResponseMessage response)
        {
            IEnumerable<string> enumerable;
            if (response.RequestMessage.Headers.TryGetValues("x-api-key", out enumerable))
            {
                response.RequestMessage.Headers.Remove("x-api-key");
                HttpHeaders headers = response.RequestMessage.Headers;
                var text = "x-api-key";
                var text2 = enumerable.FirstOrDefault();
                headers.Add(text, text2 != null ? text2.MaskLogValue(4) : null);
            }

            enumerable = null;
            if (response.RequestMessage.Headers.TryGetValues("authorization", out enumerable))
            {
                response.RequestMessage.Headers.Remove("authorization");
                HttpHeaders headers2 = response.RequestMessage.Headers;
                var text3 = "authorization";
                var text4 = enumerable.FirstOrDefault();
                headers2.Add(text3, text4 != null ? text4.MaskLogValue(4) : null);
            }

            return response.ToJson();
        }

        private static async Task<string> ReadStream(Stream stream)
        {
            var text = "";
            try
            {
                if (stream != null && stream.CanSeek)
                {
                    stream.Seek(0L, SeekOrigin.Begin);
                    text = await new StreamReader(stream).ReadToEndAsync();
                    stream.Seek(0L, SeekOrigin.Begin);
                }
            }
            catch (Exception ex) when (ex is BadHttpRequestException || ex is ObjectDisposedException)
            {
                text = "";
            }

            return text;
        }
    }
}