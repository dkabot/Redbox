using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using App.Metrics;
using Microsoft.Extensions.Options;
using MoreLinq;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;
using Serilog;

namespace Redbox.NetCore.Middleware.Metrics
{
    internal class ApplicationMetrics : IApplicationMetrics
    {
        private const string SpecificApiDimension = "SpecificApi";

        private const string ExternalApiDimension = "ExternalApi";

        private const string Status4xx = "4xxError";

        private const string Status5xx = "5xxError";

        private readonly int _bufferSize;

        private readonly bool _isDisabled;

        private readonly string _namespace;

        private readonly RegionEndpoint _regionEndpoint;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private volatile List<MetricDatum> _currentMetrics = new List<MetricDatum>();

        public ApplicationMetrics(IMetrics metrics, IOptions<ApplicationMetricsSettings> metricsSettings)
        {
            _isDisabled = metricsSettings.Value.DisableMetrics;
            _regionEndpoint = RegionEndpoint.GetBySystemName(metricsSettings.Value.MetricsAWSRegion);
            _bufferSize = Math.Min(metricsSettings.Value.BufferSize, 20);
            _namespace = metrics is IMetricsRoot ? ((IMetricsRoot)metrics).Options.DefaultContextLabel : "default";
        }

        public async Task CaptureEndpointExecutionAsync(string controllerName, string actionName,
            long responseMilliseconds, HttpStatusCode statusCode)
        {
            var list = Dimensions("SpecificApi", controllerName + "." + actionName);
            await CaptureAsync(responseMilliseconds, statusCode, list);
        }

        public async Task CaptureUrlExecutionAsync(string url, long responseMilliseconds, HttpStatusCode statusCode)
        {
            var list = Dimensions("ExternalApi", url);
            await CaptureAsync(responseMilliseconds, statusCode, list);
        }

        private async Task CaptureAsync(long responseMilliseconds, HttpStatusCode statusCode,
            List<Dimension> dimensions, List<MetricDatum> data = null)
        {
            await _semaphore.WaitAsync(5000);
            try
            {
                if (!_isDisabled) await SendAsync(Metrics(dimensions, responseMilliseconds, (int)statusCode));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "ApplicationMetrics -> Error while capturing metrics. Clearing current metrics.");
                _currentMetrics = new List<MetricDatum>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SendAsync(List<MetricDatum> data)
        {
            try
            {
                if (_currentMetrics.Count > _bufferSize)
                    _currentMetrics = _currentMetrics.TakeLast(_currentMetrics.Count - _bufferSize).ToList();
                _currentMetrics.AddRange(data);
                if (_currentMetrics.Count >= _bufferSize)
                {
                    var logger = Log.Logger;
                    var text = "ApplicationMetrics -> Buffer limit of {0} reached. Sending first {1} metrics of {2}";
                    object obj = _bufferSize;
                    object obj2 = _bufferSize;
                    var currentMetrics = _currentMetrics;
                    logger.Information(
                        string.Format(text, obj, obj2, currentMetrics != null ? currentMetrics.Count : 0));
                    using (var client = new AmazonCloudWatchClient(_regionEndpoint))
                    {
                        var metricsToSend = _currentMetrics.Take(_bufferSize).ToList();
                        var putMetricDataResponse = await client.PutMetricDataAsync(new PutMetricDataRequest
                        {
                            MetricData = _currentMetrics.Take(_bufferSize).ToList(),
                            Namespace = _namespace
                        });
                        Log.Logger.Debug(string.Format(
                            "ApplicationMetrics -> PutMetricDataAsync -> StatusCode: {0}, Data Sent: {1}",
                            putMetricDataResponse.HttpStatusCode, metricsToSend.ToJson()));
                        metricsToSend = null;
                    }
                }
            }
            catch (Exception ex)
            {
                var logger2 = Log.Logger;
                var text2 = "ApplicationMetrics -> Error while sending metrics -> ";
                string text3;
                if (_currentMetrics == null)
                {
                    text3 = "null";
                }
                else
                {
                    var currentMetrics2 = _currentMetrics;
                    text3 = currentMetrics2 != null ? currentMetrics2.ToJson() : null;
                }

                logger2.Error(ex, text2 + text3);
            }
            finally
            {
                if (_currentMetrics.Count >= _bufferSize)
                {
                    _currentMetrics = _currentMetrics.TakeLast(_currentMetrics.Count - _bufferSize).ToList();
                    Log.Logger.Debug(string.Format("ApplicationMetrics -> {0} Metrics flushed. {1} remaining -> {2}",
                        _bufferSize, _currentMetrics.Count, _currentMetrics.ToJson()));
                }
            }
        }

        private List<MetricDatum> Metrics(List<Dimension> dimensions, long responseTime, int statusCode)
        {
            var utcNow = DateTime.UtcNow;
            var list = new List<MetricDatum>
            {
                new MetricDatum
                {
                    MetricName = "ResponseTime",
                    Dimensions = dimensions,
                    Unit = StandardUnit.Milliseconds,
                    TimestampUtc = utcNow,
                    Value = responseTime
                },
                new MetricDatum
                {
                    MetricName = "CallCount",
                    Dimensions = dimensions,
                    Unit = StandardUnit.Count,
                    Value = 1.0,
                    TimestampUtc = utcNow
                }
            };
            if (statusCode >= 400 && statusCode < 500) list.Add(HttpStatusDatum("4xxError", dimensions));
            if (statusCode >= 500 && statusCode < 600) list.Add(HttpStatusDatum("5xxError", dimensions));
            return list;
        }

        private static List<Dimension> Dimensions(string dimensionName, string dimensionValue)
        {
            return new List<Dimension>
            {
                new Dimension
                {
                    Name = dimensionName,
                    Value = dimensionValue
                }
            };
        }

        private static MetricDatum HttpStatusDatum(string statusName, List<Dimension> dimensions)
        {
            return new MetricDatum
            {
                MetricName = statusName,
                Dimensions = dimensions,
                Unit = StandardUnit.Count,
                Value = 1.0
            };
        }
    }
}