using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API
{
    public class ApplicationWebHostService : WebHostService
    {
        private readonly ILogger<ApplicationWebHostService> _logger;

        public ApplicationWebHostService(IWebHost host, ILogger<ApplicationWebHostService> logger)
            : base(host)
        {
            _logger = logger;
        }

        protected override void OnStarting(string[] args)
        {
            var logger = _logger;
            if (logger != null)
                _logger.LogInfoWithSource("WebHost: OnStarting",
                    "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            var logger = _logger;
            if (logger != null)
                _logger.LogInfoWithSource("WebHost: OnStarted",
                    "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            var logger = _logger;
            if (logger != null)
                _logger.LogInfoWithSource("WebHost: OnStopping",
                    "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
            base.OnStopping();
        }

        protected override void OnStopped()
        {
            var logger = _logger;
            if (logger != null)
                _logger.LogInfoWithSource("WebHost: OnStopped",
                    "/sln/src/UpdateClientService.API/ApplicationWebHostService.cs");
            base.OnStopped();
        }
    }
}