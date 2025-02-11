using System;
using System.IO;
using System.Xml.Serialization;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework
{
    public sealed class KioskConfiguration
    {
        private const string FILE_NAME = "Kiosk.xml";
        private const string IRHardwareInstallDateName = "IRHardwareInstallDate";
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(KioskConfiguration));
        private bool _loaded;

        private KioskConfiguration()
        {
        }

        public static KioskConfiguration Instance => Singleton<KioskConfiguration>.Instance;

        public DateTime? IRHardwareInstallDate { get; set; }

        private string FilePath
        {
            get
            {
                var service = ServiceLocator.Instance.GetService<IRuntimeService>();
                return service != null
                    ? service.RuntimePath("Kiosk.xml")
                    : Path.Combine(Path.GetDirectoryName(typeof(KioskConfiguration).Assembly.Location), "Kiosk.xml");
            }
        }

        public void Initialize(ErrorList errors)
        {
            if (Load())
                return;
            errors.Add(Error.NewError("KCL001", "KioskConfiguration Initialize failed to load.", ""));
        }

        public void SetConfig(string name, string value, bool saveValue = false)
        {
            LogHelper.Instance.Log(string.Format("KioskConfiguration.SetConfig - name: {0}, value: {1}, save: {2}",
                name, value, saveValue));
            if (name == "IRHardwareInstallDate")
            {
                DateTime result;
                IRHardwareInstallDate = string.IsNullOrEmpty(value) || !DateTime.TryParse(value, out result)
                    ? new DateTime?()
                    : result;
            }
            else
            {
                LogHelper.Instance.Log(string.Format("KioskConfiguration.SetConfig doesn't support: {0} - {1}", name,
                    value));
            }

            if (!saveValue)
                return;
            Save();
        }

        private void Save()
        {
            LogHelper.Instance.Log("KioskConfiguration.Save - " + FilePath);
            try
            {
                using (var streamWriter = new StreamWriter(FilePath))
                {
                    _serializer.Serialize(streamWriter, Instance);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Exception when Saving KioskConfiguration: {0}", ex));
            }
        }

        public bool Load()
        {
            try
            {
                _loaded = false;
                if (File.Exists(FilePath))
                {
                    var kioskConfiguration = (KioskConfiguration)null;
                    using (var streamReader = new StreamReader(FilePath))
                    {
                        kioskConfiguration = _serializer.Deserialize(streamReader) as KioskConfiguration;
                    }

                    if (kioskConfiguration == null)
                        return false;
                    IRHardwareInstallDate = kioskConfiguration.IRHardwareInstallDate;
                    _loaded = true;
                }
                else
                {
                    _loaded = true;
                    Save();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Exception when Loading KioskConfiguration: {0}", ex));
                _loaded = false;
            }
            finally
            {
                LogHelper.Instance.Log(string.Format("KioskConfiguration.Load File: {0} - Success: {1}", FilePath,
                    _loaded.ToString()));
            }

            return _loaded;
        }
    }
}