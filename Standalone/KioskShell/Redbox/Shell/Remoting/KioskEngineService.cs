using System;
using Redbox.Core;
using Redbox.Shell.ComponentModel;
using Error = Redbox.Shell.ComponentModel.Error;

namespace Redbox.Shell.Remoting
{
    public class KioskEngineService : IKioskEngineService
    {
        private string _kioskEnginePath;

        private KioskEngineService()
        {
        }

        public static KioskEngineService Instance => Singleton<KioskEngineService>.Instance;

        public ErrorList IsEngineRunning(out bool isRunning)
        {
            isRunning = false;
            var errorList = new ErrorList();
            try
            {
                isRunning = KioskEngineProxy.IsEngineRunning();
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("K999",
                    "An unhandled exception was raised in KioskEngineService.IsEngineRunning.", ex));
            }

            return errorList;
        }

        public ErrorList ActivateControlPanel()
        {
            var errorList = new ErrorList();
            try
            {
                if (KioskEngineProxy.IsEngineRunning())
                {
                    var clientCommandResult = KioskEngineProxy.ActiviateControlPanel();
                    if (!clientCommandResult.Success)
                        foreach (var error in clientCommandResult.Errors)
                            errorList.Add(Error.NewError(error.Code, error.Description));
                }
                else
                {
                    errorList.Add(Error.NewError("K999",
                        "Cannot activate control panel bundle; kiosk engine is not running."));
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("K999",
                    "An unhandled exception was raised in KioskEngineService.ActivateControlPanel.", ex));
            }

            return errorList;
        }

        public ErrorList StartEngineWithControlPanel()
        {
            var errorList = new ErrorList();
            if (KioskEngineProxy.IsEngineRunning())
                errorList.Add(Error.NewError("K999", "Cannot start kiosk engine; already running"));
            else
                try
                {
                    LogHelper.Instance.Log("Starting engine at location: {0}", _kioskEnginePath);
                    var num = ShellHelper.StartProcessAsAdministrator(_kioskEnginePath,
                        "--bundle:\"Control Panel Application,*\"");
                    if (num < 0)
                        LogHelper.Instance.Log("Unable to launch Kiosk Engine using TaskScheduler! result={0:X}", num);
                }
                catch (Exception ex)
                {
                    errorList.Add(Error.NewError("K999",
                        "An unhandled exception was raised in KioskEngineService.StartEngineWithControlPanel.", ex));
                }

            return errorList;
        }

        public void Initialize(string kioskEnginePath, string kioskEngineUrl)
        {
            LogHelper.Instance.Log("KioskEngineService.Initialize() - kioskEnginePath: " + kioskEnginePath +
                                   ", kioskEngineUrl: " + kioskEngineUrl);
            _kioskEnginePath = kioskEnginePath;
            ServiceLocator.Instance.AddService(typeof(IKioskEngineService), this);
            KioskEngineProxy.IpcHostUrl = kioskEngineUrl;
        }
    }
}