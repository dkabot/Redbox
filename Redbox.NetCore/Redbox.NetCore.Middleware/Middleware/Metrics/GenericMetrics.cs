using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Extensions;

namespace Redbox.NetCore.Middleware.Metrics
{
    public class GenericMetrics : ICommonMetrics
    {
        public const string MetricTypesKeyName = "MetricType";
        public const string ValueKeyName = "Value";
        public const string NameKeyName = "Name";
        private readonly ILogger<GenericMetrics> _logger;
        private readonly IMetrics _metrics;

        public GenericMetrics(IMetrics metrics, ILogger<GenericMetrics> logger)
        {
            _metrics = metrics;
            _logger = logger;
        }

        public async Task CaptureMetrics(
            Dictionary<string, string> metricTagCollection,
            long elapsedMilliseconds)
        {
            await Task.Run(() =>
            {
                if (_metrics == null)
                    _logger.LogDebugWithSource("No implementation of IMetrics found. Skipping capture.",
                        "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
                else
                    try
                    {
                        var typeFromMetricTags = GetMetricTypeFromMetricTags(metricTagCollection);
                        if (typeFromMetricTags.Found)
                            switch (typeFromMetricTags.MetricType)
                            {
                                case MetricType.Gauge:
                                    CaptureGaugeMetric(metricTagCollection);
                                    break;
                                case MetricType.Counter:
                                    CaptureCounterMetric(metricTagCollection);
                                    break;
                                case MetricType.Histogram:
                                    CaptureHistogramMetric(metricTagCollection, elapsedMilliseconds);
                                    break;
                                default:
                                    _logger.LogErrorWithSource(null,
                                        string.Format("Unsupported MetricType {0}", typeFromMetricTags.MetricType),
                                        nameof(CaptureMetrics),
                                        "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
                                    break;
                            }
                        else
                            _logger.LogErrorWithSource(null,
                                string.Format("metricTagCollection missing tag {0}", typeFromMetricTags.MetricType),
                                nameof(CaptureMetrics),
                                "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex, "Unhandled exception while capturing metrics",
                            "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
                    }
            });
        }

        private (bool Found, MetricType MetricType) GetMetricTypeFromMetricTags(
            Dictionary<string, string> metricTagCollection)
        {
            var flag = false;
            var metricType = MetricType.Gauge;
            string str;
            if (metricTagCollection != null && metricTagCollection.TryGetValue("MetricType", out str))
            {
                metricTagCollection.Remove("MetricType");
                MetricType result;
                if (Enum.TryParse(str, out result))
                {
                    metricType = result;
                    flag = true;
                }
            }

            return (flag, metricType);
        }

        private void CaptureGaugeMetric(Dictionary<string, string> metricTagCollection)
        {
            var num = 0.0;
            string s;
            if (metricTagCollection.TryGetValue("Value", out s))
            {
                double result;
                if (double.TryParse(s, out result))
                    num = result;
                metricTagCollection.Remove("Value");
            }

            var tags = new MetricTags(metricTagCollection.Keys.ToArray(), metricTagCollection.Values.ToArray());
            _logger.LogInfoWithSource(string.Format("{0} value: {1}", tags.ToJson(), num),
                "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
            _metrics.Measure.Gauge.SetValue(MetricsRegistry.GenericGauge, tags, num);
        }

        private void CaptureCounterMetric(Dictionary<string, string> metricTagCollection)
        {
            var tags = new MetricTags(metricTagCollection.Keys.ToArray(), metricTagCollection.Values.ToArray());
            _logger.LogInfoWithSource("Incremented counter on " + tags.ToJson(),
                "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
            _metrics.Measure.Counter.Increment(MetricsRegistry.GenericCounter, tags);
        }

        private void CaptureHistogramMetric(
            Dictionary<string, string> metricTagCollection,
            long elapsedMilliseconds)
        {
            var tags = new MetricTags(metricTagCollection.Keys.ToArray(), metricTagCollection.Values.ToArray());
            _logger.LogInfoWithSource(string.Format("{0} took {1} ms", tags.ToJson(), elapsedMilliseconds),
                "/var/lib/jenkins/workspace/a_common_redbox.netcore.middleware/Redbox.NetCore.Middleware/Metrics/GenericMetrics.cs");
            _metrics.Measure.Histogram.Update(MetricsRegistry.GenericHistogram, tags, elapsedMilliseconds);
        }
    }
}