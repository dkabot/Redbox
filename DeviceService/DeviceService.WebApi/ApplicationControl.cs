using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceService.WebApi
{
    public class ApplicationControl : IApplicationControl
    {
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly IIUC285Notifier _iuc285Notifier;
        private readonly ILogger<ApplicationControl> _logger;
        private AutoResetEvent _canShutDownAutoResetEvent;
        private bool _canShutDownClientResponse;

        public ApplicationControl(
            IApplicationLifetime applicationLifetime,
            ILogger<ApplicationControl> logger,
            IIUC285Notifier iuc285Notifier)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _iuc285Notifier = iuc285Notifier;
        }

        public static bool IsService { get; set; }

        public bool CanShutDown(ShutDownReason shutDownReason)
        {
            var str = (string)null;
            _canShutDownClientResponse = true;
            if (_canShutDownClientResponse)
            {
                _iuc285Notifier?.SendDeviceServiceCanShutDownEvent();
                _canShutDownAutoResetEvent = new AutoResetEvent(false);
                _canShutDownAutoResetEvent.WaitOne(5000);
                if (!_canShutDownClientResponse)
                    str = "Kiosk Engine prevented shutdown";
            }
            else
            {
                str = "Device Service Command(s) are currently processing";
            }

            if (!string.IsNullOrEmpty(str))
                str = "  Reason: " + str;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(string.Format("ApplicationControl.CanShutDown: {0} {1}",
                    _canShutDownClientResponse, str));
            return _canShutDownClientResponse;
        }

        public void SetCanShutDownClientResponse(bool clientAllowsShutDown)
        {
            _canShutDownClientResponse &= clientAllowsShutDown;
            if (clientAllowsShutDown || _canShutDownAutoResetEvent == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(string.Format("ApplicationControl.SetCanShutDownClientResponse value: {0}",
                    clientAllowsShutDown));
            _canShutDownAutoResetEvent.Set();
        }

        public bool ShutDown(bool forceShutdown, ShutDownReason shutDownReason)
        {
            var logger1 = _logger;
            if (logger1 != null)
                logger1.LogInformation(string.Format(
                    "ApplicationControl.ShutDown: forceShutdown: {0}, ShutDown Reason: {1}", forceShutdown,
                    shutDownReason));
            var flag = false;
            if (forceShutdown || CanShutDown(shutDownReason))
                try
                {
                    var logger2 = _logger;
                    if (logger2 != null)
                        logger2.LogInformation("ApplicationControl.ShutDown: starting shut down");
                    Task.Run(async () =>
                    {
                        _iuc285Notifier.SendDeviceServiceShutDownStartingEvent(shutDownReason);
                        await Task.Delay(3000);
                        if (IsService)
                        {
                            var serviceController = new ServiceController("device$service");
                            if (serviceController != null)
                            {
                                var logger3 = _logger;
                                if (logger3 != null)
                                    logger3.LogInformation("Stopping service application.");
                                serviceController?.Stop();
                            }
                            else
                            {
                                var logger4 = _logger;
                                if (logger4 == null)
                                    return;
                                logger4.LogError(
                                    "Unable to find service device$service.  Unable to shut down windows service.");
                            }
                        }
                        else
                        {
                            var logger5 = _logger;
                            if (logger5 != null)
                                logger5.LogInformation("Stopping non-service application.");
                            _applicationLifetime?.StopApplication();
                        }
                    });
                    flag = true;
                }
                catch (Exception ex)
                {
                    var logger6 = _logger;
                    if (logger6 != null)
                        logger6.LogInformation(string.Format("ApplicationControl.ShutDown failed.  {0}", ex));
                }

            return flag;
        }
    }
}