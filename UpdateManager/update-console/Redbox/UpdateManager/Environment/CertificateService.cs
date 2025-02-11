using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Redbox.UpdateManager.Environment
{
    internal class CertificateService
    {
        private Timer _timer;
        private bool _isRunning;
        private int _inDoWork;
        private string _certDataPath = "C:\\Program Files\\Redbox\\reds\\Kiosk Engine\\data\\certs";

        public static CertificateService Instance => Singleton<CertificateService>.Instance;

        public ErrorList Initialize()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer = new Timer((TimerCallback)(o => this.DoWork()));
                this._isRunning = false;
                ServiceLocator.Instance.AddService(typeof(CertificateService), (object)this);
                LogHelper.Instance.Log("Initialized the CertificateService", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(CertificateService), "Unhandled exception occurred in Initialize.", ex));
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(new TimeSpan(0, 2, 0), new TimeSpan(0, 10, 0));
                this._isRunning = true;
                LogHelper.Instance.Log("Starting the CertificateService.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(CertificateService), "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public ErrorList Stop()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(-1, -1);
                this._isRunning = false;
                LogHelper.Instance.Log("Stopping the CertificateService.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(CertificateService), "An unhandled exception occurred while stopping the CertificateService.", ex));
            }
            return errorList;
        }

        private void DoWork()
        {
            try
            {
                if (!this._isRunning)
                    LogHelper.Instance.Log("The CertificateService service is not running.", LogEntryType.Info);
                else if (Interlocked.CompareExchange(ref this._inDoWork, 1, 0) == 1)
                {
                    LogHelper.Instance.Log("Already in CertificateService.DoWork", LogEntryType.Info);
                }
                else
                {
                    try
                    {
                        LogHelper.Instance.Log("Executing CertificateService.DoWork");
                        this.LookForCerts();
                    }
                    finally
                    {
                        this._inDoWork = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an exception in CertificateService.DoWork()", ex);
            }
        }

        private void LookForCerts()
        {
            if (!Directory.Exists(this._certDataPath))
            {
                Directory.CreateDirectory(this._certDataPath);
            }
            else
            {
                foreach (string file in Directory.GetFiles(this._certDataPath, "*.staged"))
                {
                    try
                    {
                        Path.GetFileNameWithoutExtension(file);
                        byte[] data = File.ReadAllBytes(file);
                        if (!CertificateHelper.Exists(StoreName.My, StoreLocation.LocalMachine, data))
                        {
                            LogHelper.Instance.Log("CertificateService.LookForCerts - Adding Cert Name: {0} to the capi store", (object)file);
                            CertificateHelper.Add(StoreName.My, StoreLocation.LocalMachine, data);
                            LogHelper.Instance.Log("CertificateService.LookForCerts - Added Cert Name: {0} to the capi store", (object)file);
                        }
                        else
                            LogHelper.Instance.Log("CertificateService.LookForCerts - Cert Name: {0} already exists in capi store", (object)file);
                        string str = file.Replace(".staged", "");
                        if (File.Exists(str))
                            File.Delete(str);
                        File.Move(file, str);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("There was an exception in CertificateService.LookForCerts()", ex);
                    }
                }
            }
        }

        private CertificateService()
        {
        }
    }
}
