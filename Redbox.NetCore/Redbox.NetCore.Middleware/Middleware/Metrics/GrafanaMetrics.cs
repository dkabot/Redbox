using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Histogram;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Middleware.Abstractions;

namespace Redbox.NetCore.Middleware.Metrics
{
    internal class GrafanaMetrics : IGrafanaMetrics
    {
        private readonly ILogger _logger;
        private readonly IMetrics _rootMetrics;

        public GrafanaMetrics(IMetrics metrics, ILogger<GrafanaMetrics> logger)
        {
            _rootMetrics = metrics;
            _logger = logger;
        }

        public async Task<T> CaptureExternalServiceAsync<T>(
            string service,
            string method,
            Func<Task<T>> action)
        {
            return await CaptureMetricAsync("service:" + service + ",method:" + method,
                MetricsRegistry.ExternalResponseTimes, MetricsRegistry.ExternalRequests, action);
        }

        public void CaptureResponseCode(string route, string method, string action, string code)
        {
            _rootMetrics.Measure.Counter.Increment(MetricsRegistry.ResponseCodes,
                MetricTags.FromSetItemString("method:" + method + ",action:" + action + ",statusCode:" + code));
        }

        public void CaptureExternalError(string service, string method, Exception ex)
        {
            _rootMetrics.Measure.Counter.Increment(MetricsRegistry.ExternalErrors,
                MetricTags.FromSetItemString("service:" + service + ",method:" + method + ",exception:" +
                                             ex.GetType().Name));
        }

        public void CaptureError(Exception ex)
        {
            _rootMetrics.Measure.Counter.Increment(MetricsRegistry.Errors,
                MetricTags.FromSetItemString("exception:" + ex.GetType().Name));
        }

        public void CaptureResponseTime(
            string route,
            string method,
            string action,
            string code,
            long responseTime)
        {
            _rootMetrics.Measure.Histogram.Update(MetricsRegistry.ResponseTimes,
                MetricTags.FromSetItemString("method:" + method + ",action:" + action + ",statusCode:" + code),
                responseTime);
        }

        private async Task CaptureMetricAsync(
            string tags,
            HistogramOptions histogram,
            CounterOptions counter,
            Func<Task> action)
        {
            _rootMetrics.Measure.Counter.Increment(counter, MetricTags.FromSetItemString(tags));
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                await action();
                sw.Stop();
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError("CaptureMetricAsync error: " + ex.Message);
                throw;
            }
            finally
            {
                _rootMetrics.Measure.Histogram.Update(histogram, MetricTags.FromSetItemString(tags),
                    sw.ElapsedMilliseconds);
                _logger.LogInformation("{@MetricTags} took {@ElapsedMilliseconds} ms", tags, sw.ElapsedMilliseconds);
            }

            sw = null;
        }

        private async Task<T> CaptureMetricAsync<T>(
            string tags,
            HistogramOptions histogram,
            CounterOptions counter,
            Func<Task<T>> action)
        {
            var obj1 = default(T);
            var statusCode = new HttpStatusCode?();
            var message = string.Empty;
            _rootMetrics.Measure.Counter.Increment(counter, MetricTags.FromSetItemString(tags));
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                obj1 = await action();
                if (obj1 is HttpResponseMessage httpResponseMessage)
                {
                    statusCode = httpResponseMessage.StatusCode;
                    message = !httpResponseMessage.IsSuccessStatusCode
                        ? httpResponseMessage.Content.ReadAsStringAsync().Result ?? ""
                        : string.Empty;
                }

                sw.Stop();
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError("CaptureMetricAsync error: " + ex.Message);
                throw;
            }
            finally
            {
                _rootMetrics.Measure.Histogram.Update(histogram, MetricTags.FromSetItemString(tags),
                    sw.ElapsedMilliseconds);
                _logger.LogInformation(string.Format("{0},statuscode={1},elapsed={2},message={3}", tags, statusCode,
                    sw.ElapsedMilliseconds, message));
            }

            var obj2 = obj1;
            message = null;
            sw = null;
            return obj2;
        }

        public async Task CaptureExternalServiceAsync(string service, string method, Func<Task> action)
        {
            await CaptureMetricAsync("service:" + service + ",method:" + method, MetricsRegistry.ExternalResponseTimes,
                MetricsRegistry.ExternalRequests, action);
        }

        public void CaptureInvalidRequests(string ip)
        {
            _rootMetrics.Measure.Counter.Increment(MetricsRegistry.InvalidRequests, new MetricTags(nameof(ip), ip));
        }
    }
}