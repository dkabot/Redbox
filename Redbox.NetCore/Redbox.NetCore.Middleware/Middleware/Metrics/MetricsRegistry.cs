using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.ReservoirSampling.SlidingWindow;

namespace Redbox.NetCore.Middleware.Metrics
{
    public static class MetricsRegistry
    {
        public static readonly CounterOptions Errors;
        public static readonly CounterOptions ResponseCodes;
        public static readonly HistogramOptions ResponseTimes;
        public static readonly CounterOptions ExternalErrors;
        public static readonly CounterOptions ExternalRequests;
        public static readonly HistogramOptions ExternalResponseTimes;
        public static readonly CounterOptions InvalidRequests;
        public static readonly CounterOptions DbExecutions;
        public static readonly HistogramOptions DbExecutionTimes;
        public static readonly CounterOptions DbExecutionErrors;
        public static readonly GaugeOptions GenericGauge;
        public static readonly CounterOptions GenericCounter;
        public static readonly HistogramOptions GenericHistogram;

        static MetricsRegistry()
        {
            var counterOptions1 = new CounterOptions();
            counterOptions1.MeasurementUnit = Unit.Errors;
            counterOptions1.ResetOnReporting = true;
            counterOptions1.Name = nameof(Errors);
            Errors = counterOptions1;
            var counterOptions2 = new CounterOptions();
            counterOptions2.MeasurementUnit = Unit.Calls;
            counterOptions2.ResetOnReporting = true;
            counterOptions2.Name = nameof(ResponseCodes);
            ResponseCodes = counterOptions2;
            var histogramOptions1 = new HistogramOptions();
            histogramOptions1.MeasurementUnit = Unit.Requests;
            histogramOptions1.Reservoir = () => new DefaultSlidingWindowReservoir(250);
            histogramOptions1.Name = nameof(ResponseTimes);
            ResponseTimes = histogramOptions1;
            var counterOptions3 = new CounterOptions();
            counterOptions3.MeasurementUnit = Unit.Errors;
            counterOptions3.ResetOnReporting = true;
            counterOptions3.Name = nameof(ExternalErrors);
            ExternalErrors = counterOptions3;
            var counterOptions4 = new CounterOptions();
            counterOptions4.MeasurementUnit = Unit.Requests;
            counterOptions4.ResetOnReporting = true;
            counterOptions4.Name = nameof(ExternalRequests);
            ExternalRequests = counterOptions4;
            var histogramOptions2 = new HistogramOptions();
            histogramOptions2.MeasurementUnit = Unit.Requests;
            histogramOptions2.Reservoir = () => new DefaultSlidingWindowReservoir(250);
            histogramOptions2.Name = nameof(ExternalResponseTimes);
            ExternalResponseTimes = histogramOptions2;
            var counterOptions5 = new CounterOptions();
            counterOptions5.MeasurementUnit = Unit.Requests;
            counterOptions5.ResetOnReporting = true;
            counterOptions5.Name = nameof(InvalidRequests);
            InvalidRequests = counterOptions5;
            var counterOptions6 = new CounterOptions();
            counterOptions6.MeasurementUnit = Unit.Requests;
            counterOptions6.ResetOnReporting = true;
            counterOptions6.Name = nameof(DbExecutions);
            DbExecutions = counterOptions6;
            var histogramOptions3 = new HistogramOptions();
            histogramOptions3.MeasurementUnit = Unit.Requests;
            histogramOptions3.Reservoir = () => new DefaultSlidingWindowReservoir(250);
            histogramOptions3.Name = nameof(DbExecutionTimes);
            DbExecutionTimes = histogramOptions3;
            var counterOptions7 = new CounterOptions();
            counterOptions7.MeasurementUnit = Unit.Requests;
            counterOptions7.ResetOnReporting = true;
            counterOptions7.Name = nameof(DbExecutionErrors);
            DbExecutionErrors = counterOptions7;
            var gaugeOptions = new GaugeOptions();
            gaugeOptions.MeasurementUnit = Unit.Items;
            gaugeOptions.ResetOnReporting = true;
            gaugeOptions.Name = nameof(GenericGauge);
            GenericGauge = gaugeOptions;
            var counterOptions8 = new CounterOptions();
            counterOptions8.MeasurementUnit = Unit.Requests;
            counterOptions8.ResetOnReporting = true;
            counterOptions8.Name = nameof(GenericCounter);
            GenericCounter = counterOptions8;
            var histogramOptions4 = new HistogramOptions();
            histogramOptions4.MeasurementUnit = Unit.Requests;
            histogramOptions4.Reservoir = () => new DefaultSlidingWindowReservoir(250);
            histogramOptions4.Name = nameof(GenericHistogram);
            GenericHistogram = histogramOptions4;
        }
    }
}