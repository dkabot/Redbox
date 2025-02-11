using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow
{
    public static class ExposureFixer
    {
        public static void ResetCameraProperties()
        {
            LogHelper.Instance.Log("Checking camera properties...");
            var activeCamera = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(false);
            if (activeCamera == null)
            {
                LogHelper.Instance.Log("Unable to find an active camera.");
            }
            else
            {
                LogHelper.Instance.Log("Found camera device {0} ( {1} )", activeCamera.ToString(),
                    activeCamera.Friendlyname);
                if (!activeCamera.Friendlyname.Equals("4th Gen"))
                    return;
                var filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (filterInfoCollection == null || filterInfoCollection.Count == 0)
                {
                    LogHelper.Instance.Log("Unable to find any devices.");
                }
                else
                {
                    var device = new PlayerDevice(filterInfoCollection[0].MonikerString, 500, false);
                    if (device == null)
                    {
                        LogHelper.Instance.Log("Found no devices.");
                    }
                    else
                    {
                        var properties = GetProperties(device);
                        if (properties == null)
                            return;
                        if (CameraControlFlags.Auto != properties.Flags)
                            LogHelper.Instance.Log("Device is not set to auto.");
                        else if (!device.SetCameraProperty(CameraControlProperty.Exposure, properties.Value,
                                     CameraControlFlags.Manual))
                            LogHelper.Instance.Log(" !!Failed to set camera property to manual!!");
                        else
                            GetProperties(device);
                    }
                }
            }
        }

        private static CameraProperties GetProperties(PlayerDevice device)
        {
            var f = 0;
            CameraControlFlags controlFlags;
            if (!device.GetCameraProperty(CameraControlProperty.Exposure, out f, out controlFlags))
            {
                LogHelper.Instance.Log("Unable to retrieve camera properties.");
                return null;
            }

            LogHelper.Instance.Log(" GetProperties: Value = {0} Flags = {1}", f, controlFlags);
            return new CameraProperties(f, controlFlags);
        }

        private class CameraProperties
        {
            internal CameraProperties(int f, CameraControlFlags ccf)
            {
                Value = f;
                Flags = ccf;
            }

            internal int Value { get; }

            internal CameraControlFlags Flags { get; }
        }
    }
}