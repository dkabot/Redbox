using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Redbox.NetCore.Middleware.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureAndUseKioskMetrics(
            this IWebHostBuilder builder,
            IMetricsRoot metricsRoot)
        {
            builder.ConfigureMetrics(metricsRoot).UseMetrics(options => options.EndpointOptions = endpointOptions =>
                endpointOptions.MetricsEndpointOutputFormatter = metricsRoot.OutputMetricsFormatters
                    .GetType<MetricsPrometheusTextOutputFormatter>());
            return builder;
        }

        public static IWebHostBuilder UseAppMetrics(this IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                var metricsPort = configuration != null
                    ? configuration.GetSection("AppMetrics:MetricsEndpointPort").Get<int>()
                    : 60642;
                var metricsEnabled = configuration != null &&
                                     configuration.GetSection("AppMetrics:MetricsEnabled").Get<bool>();
                var ignoredRegexPatterns = configuration != null
                    ? configuration.GetSection("AppMetrics:IgnoredRouteRegexPatterns").Get<List<string>>()
                    : null;
                var source = ignoredRegexPatterns;
                ignoredRegexPatterns = (source != null ? source.Select(s => s.Replace("^/", "^")).ToList() : null) ??
                                       new List<string>();
                var metricsBuilder = AppMetrics.CreateDefaultBuilder().Configuration.Configure(options =>
                {
                    options.WithGlobalTags((tags, envInfo) =>
                    {
                        tags["app_version"] = envInfo.EntryAssemblyVersion;
                        tags["env"] = envInfo.RunningEnvironment;
                        tags["server"] = envInfo.MachineName;
                    });
                    options.DefaultContextLabel = "application";
                    options.Enabled = metricsEnabled;
                });
                services.AddMetrics(metricsBuilder);
                services.AddMetricsEndpoints(options =>
                {
                    options.MetricsEndpointEnabled = metricsEnabled;
                    options.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                });
                services.AddMetricsTrackingMiddleware(options =>
                {
                    options.IgnoredHttpStatusCodes = new int[1]
                    {
                        405
                    };
                    options.IgnoredRoutesRegexPatterns = ignoredRegexPatterns;
                    options.ApdexTSeconds = 1.0;
                });
                builder.UseAppMetricsPort(services, metricsPort, options =>
                {
                    options.AllEndpointsPort = metricsPort;
                    options.MetricsEndpoint = "/metrics";
                });
            });
            builder.UseMetricsEndpoints();
            builder.UseMetricsWebTracking();
            return builder;
        }

        private static void UseAppMetricsPort(
            this IWebHostBuilder builder,
            IServiceCollection services,
            int metricsPort,
            Action<MetricsEndpointsHostingOptions> setupAction)
        {
            var setting = builder.GetSetting(WebHostDefaults.ServerUrlsKey);
            builder.UseSetting(WebHostDefaults.ServerUrlsKey, string.Format("{0};http://*:{1}/", setting, metricsPort));
            var endpointsHostingOptions = new MetricsEndpointsHostingOptions();
            setupAction(endpointsHostingOptions);
            services.Configure(setupAction);
        }
    }
}