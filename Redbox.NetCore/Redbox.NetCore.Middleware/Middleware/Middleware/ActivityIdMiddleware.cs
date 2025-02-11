using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;
using Serilog.Context;

namespace Redbox.NetCore.Middleware.Middleware
{
    internal class ActivityIdMiddleware
    {
        private const string ActivityIdContextKey = "ActivityId";

        private const string KioskIdContextKey = "KioskId";

        private readonly ILogger _logger;

        private readonly RequestDelegate _next;

        public ActivityIdMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ActivityIdMiddleware>();
        }

        public async Task Invoke(HttpContext context, IMiddlewareData data)
        {
            data.ActivityId = ScrapeHeader(context, "ActivityId", "x-redbox-activityid", Guid.NewGuid().ToString());
            data.KioskId = ScrapeHeader<long?>(context, "KioskId", "x-redbox-kioskid", null);
            using (LogContext.PushProperty("ActivityId", data.ActivityId))
            {
                using (LogContext.PushProperty("KioskId", data.KioskId))
                {
                    await _next(context);
                }

                IDisposable disposable2 = null;
            }

            IDisposable disposable = null;
        }

        private T ScrapeHeader<T>(HttpContext context, string field, string redboxKey, T defaultValue)
        {
            var text = context.Request.Headers.Keys.FirstOrDefault(k =>
                string.Equals(k, redboxKey, StringComparison.OrdinalIgnoreCase));
            T t;
            if (string.IsNullOrWhiteSpace(text))
            {
                t = defaultValue;
                _logger.LogDebug(string.Format("Generated new field: {0}, '{1}'", field, t));
            }
            else
            {
                try
                {
                    t = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(null, CultureInfo.InvariantCulture,
                        context.Request.Headers[text]);
                    _logger.LogDebug(string.Format("Found field: {0}, '{1}'", field, t));
                }
                catch (Exception ex)
                {
                    t = defaultValue;
                    _logger.LogError(ex,
                        string.Format("Exception converting found value for field: {0}, using default value: '{1}'",
                            field, defaultValue));
                }
            }

            return t;
        }
    }
}