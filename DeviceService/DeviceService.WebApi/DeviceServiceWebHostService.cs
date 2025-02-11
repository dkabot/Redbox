using DeviceService.ComponentModel.Analytics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi
{
    public class DeviceServiceWebHostService : WebHostService
    {
        private readonly IAnalyticsService _analytics;
        private readonly ILogger _logger;

        public DeviceServiceWebHostService(
            IWebHost host,
            ILogger<DeviceServiceWebHostService> logger,
            IAnalyticsService analytics)
            : base(host)
        {
            _logger = logger;
            _analytics = analytics;
        }

        protected override void OnStarting(string[] args)
        {
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("WebHost: OnStarting");
            _analytics?.ServiceStarting();
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("WebHost: OnStarted");
            _analytics?.ServiceStarted();
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("WebHost: OnStopping");
            _analytics?.ServiceStopping();
            base.OnStopping();
        }

        protected override void OnStopped()
        {
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("WebHost: OnStopped");
            _analytics?.ServiceStopped();
            base.OnStopped();
        }
    }
}