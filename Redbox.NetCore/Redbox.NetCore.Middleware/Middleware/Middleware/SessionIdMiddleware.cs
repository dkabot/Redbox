using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Redbox.NetCore.Middleware.Middleware
{
    internal class SessionIdMiddleware
    {
        private const string RedboxSessionIdHeaderKey = "x-redbox-sessionid";

        private const string SessionIdContextKey = "SessionId";

        private readonly ILogger _logger;

        private readonly RequestDelegate _next;

        public SessionIdMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<SessionIdMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var text = context.Request.Headers.Keys.FirstOrDefault(k =>
                string.Equals(k, "x-redbox-sessionid", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogDebug("Session id header not found or invalid.");
                await _next(context);
            }
            else
            {
                string text2 = context.Request.Headers[text];
                _logger.LogDebug("Session id header found: {SessionId}", text2);
                using (LogContext.PushProperty("SessionId", text2))
                {
                    await _next(context);
                }

                IDisposable disposable = null;
            }
        }
    }
}