using System;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Utilities
{
    public class WindowsServiceFunctions : IWindowsServiceFunctions
    {
        private readonly ILogger<WindowsServiceFunctions> _logger;
        private readonly ICommandLineService _powerShellService;

        public WindowsServiceFunctions(
            ICommandLineService powerShellService,
            ILogger<WindowsServiceFunctions> logger)
        {
            _powerShellService = powerShellService;
            _logger = logger;
        }

        public bool ServiceExists(string name)
        {
            foreach (var service in ServiceController.GetServices())
                if (service.ServiceName == name)
                    return true;
            return false;
        }

        public void StartService(string name)
        {
            try
            {
                using (var serviceController = new ServiceController(name))
                {
                    serviceController.Start();
                    serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(5.0));
                    _logger.LogInfoWithSource(
                        string.Format("Exiting from startService. Service name: {0} was {1}", name,
                            serviceController.Status),
                        "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "An unhandled exception was raised while starting service: " + name + ".",
                    "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
            }
        }

        public void StopService(string name)
        {
            try
            {
                using (var serviceController = new ServiceController(name))
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(5.0));
                    _logger.LogInfoWithSource(
                        string.Format("Exiting from stopService. Service name: {0} was {1}", name,
                            serviceController.Status),
                        "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "An unhandled exception was raised while stopping service: " + name + ".",
                    "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
            }
        }

        public void InstallService(
            string name,
            string displayName,
            string binPath,
            string type = null,
            string startType = null,
            string dependencies = null)
        {
            try
            {
                var arguments = string.Format("create {0} displayName= \"{1}\" binPath= \"{2}\" type= {3} start= {4}",
                    name, displayName, binPath, type ?? "own", startType ?? "auto");
                if (!string.IsNullOrEmpty(dependencies))
                    arguments += string.Format(" depend= {0}", dependencies);
                _powerShellService.TryExecuteCommand("SC.exe", arguments);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "An unhandled exception was raised while installing service " + name,
                    "/sln/src/UpdateClientService.API/Services/Utilities/WindowsServiceFunctions.cs");
            }
        }
    }
}