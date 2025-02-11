using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework.Services
{
    public sealed class FraudValidatorService : IFraudService
    {
        private string m_fraudImagesPath;

        public bool IsConfigured
        {
            get
            {
                var service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
                BarcodeConfiguration.NewInstance();
                var configuredDevice = service.GetConfiguredDevice();
                LogHelper.Instance.Log(string.Format("IsLegacy: {0}, Camera Generation: {1}", configuredDevice.IsLegacy,
                    configuredDevice.CurrentCameraGeneration.ToString()));
                LogHelper.Instance.Log(string.Format(
                    "Device.SupportsSecureReads: {0}, EnablePhotocopyScan: {1} - IsConfigured: {2}",
                    configuredDevice.SupportsSecureReads, ControllerConfiguration.Instance.EnablePhotocopyScan,
                    !configuredDevice.SupportsSecureReads ? 0 :
                    ControllerConfiguration.Instance.EnablePhotocopyScan ? 1 : 0));
                return configuredDevice.SupportsSecureReads && ControllerConfiguration.Instance.EnablePhotocopyScan;
            }
        }

        public string FraudImagesPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_fraudImagesPath))
                {
                    m_fraudImagesPath = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().ImagePath;
                    if (!Directory.Exists(m_fraudImagesPath))
                        try
                        {
                            Directory.CreateDirectory(m_fraudImagesPath);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.WithContext(false, LogEntryType.Error, "Failed to create directory {0}",
                                m_fraudImagesPath);
                            LogHelper.Instance.Log("Exception info ", ex);
                        }
                }

                return m_fraudImagesPath;
            }
        }
    }
}