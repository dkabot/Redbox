using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Http
{
    public class ClientMetricsHandler : DelegatingHandler
    {
        private const string _metricHeader = "x-redbox-metrics";
        private readonly ILogger<ClientMetricsHandler> _logger;
        private readonly IApplicationMetrics _metrics;

        public ClientMetricsHandler(IApplicationMetrics metrics, ILoggerFactory loggerFactory)
        {
            _metrics = metrics;
            _logger = loggerFactory.CreateLogger<ClientMetricsHandler>();
        }

        protected virtual async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestTimer = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            requestTimer.Stop();
            var metricName = request.RequestUri.GetLeftPart((UriPartial)1);
            IEnumerable<string> values;
            if (request.Headers.TryGetValues("x-redbox-metrics", out values))
            {
                var str = values.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    metricName = str;
                    _logger.LogInformation("Setting metric name to '" + metricName + "'");
                }
                else
                {
                    _logger.LogInformation("Metrics header not found. Setting metric name to '" + metricName + "'");
                }
            }

            Task.Run(() =>
                _metrics.CaptureUrlExecutionAsync(metricName, requestTimer.ElapsedMilliseconds, response.StatusCode));
            return response;
        }
    }
}