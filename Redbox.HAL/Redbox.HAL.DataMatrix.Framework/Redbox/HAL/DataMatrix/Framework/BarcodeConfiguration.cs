using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.Configuration;
using KioskConfiguration = Redbox.HAL.Controller.Framework.KioskConfiguration;

namespace Redbox.HAL.DataMatrix.Framework
{
    public sealed class BarcodeConfiguration : AttributeXmlConfiguration
    {
        private static readonly object InstanceLock = new object();

        private readonly string[] LegacyProperties = new string[15]
        {
            "Mode",
            "VideoFormat",
            nameof(WorkingPath),
            "CaptureFrameRate",
            "CameraDeviceName",
            "CameraDriver",
            "CompressionCodecName",
            "VideoCaptureSpinWaitTime",
            "BitmapSelectionStrategy",
            "CenteredDiskSnapTime",
            "VideocapActiveXSpinTime",
            "CenterDiskBeforeRead",
            nameof(CycleCameraOnUse),
            "DeleteBitmapOnSuccessfulRead",
            nameof(IRHardwareInstallDate)
        };

        internal readonly IDictionary<string, object> Properties;
        internal readonly string WorkingPath;

        private BarcodeConfiguration()
            : base("barcodeConfig", typeof(BarcodeConfiguration))
        {
            WorkingPath = ServiceLocator.Instance.GetService<IRuntimeService>().InstallPath("Video");
            Properties = new Dictionary<string, object>();
            Properties[nameof(WorkingPath)] = WorkingPath;
            Properties["VideoFormat"] = new Size(640, 480);
        }

        [Category("Settings")]
        [DisplayName("Camera Plugin")]
        [Description("Sets the plugin to use in legacy scan mode.")]
        [XmlConfiguration(DefaultValue = "DirectShowFrame")]
        public string CameraPlugin { get; set; }

        [Category("Settings")]
        [DisplayName("Snap Decode Port")]
        [Description("The port the snap/decode device is configured.")]
        [XmlConfiguration(DefaultValue = "NONE")]
        public string SnapDecodePort { get; set; }

        [Category("Settings")]
        [DisplayName("Write Pause")]
        [Description("In ms.")]
        [XmlConfiguration(DefaultValue = "500")]
        public int WritePause { get; set; }

        [Category("Settings")]
        [DisplayName("Cycle Camera On Use")]
        [Description("If true, starts/stops the camera; otherwise, keeps it on.")]
        [XmlConfiguration(DefaultValue = "false")]
        public bool CycleCameraOnUse { get; private set; }

        [Category("Settings")]
        [DisplayName("Use CI On Fail")]
        [Description("If true, uses CI to take a photo & try decode.")]
        [XmlConfiguration(DefaultValue = "false")]
        public bool UseInliteOnFail { get; private set; }

        [Category("Settings")]
        [DisplayName("Scanner Service")]
        [Description("The configured scanner.")]
        [XmlConfiguration(DefaultValue = "Legacy")]
        public ScannerServices ScannerService { get; set; }

        [Category("Settings")]
        [DisplayName("Log Detailed Scan")]
        [Description("The configured scanner.")]
        [XmlConfiguration(DefaultValue = "false")]
        public bool LogDetailedScan { get; private set; }

        [Category("Settings")]
        [DisplayName("Expected Read Codes")]
        [Description("For the scanner, the max number of barcodes.")]
        [XmlConfiguration(DefaultValue = "4")]
        public int ExpectedCodes { get; private set; }

        [Category("Settings")]
        [DisplayName("Scan Timeout")]
        [Description("In ms, the time to wait for a scan to complete.")]
        [XmlConfiguration(DefaultValue = "2000")]
        public int ScanTimeout { get; private set; }

        [Category("Settings")]
        [DisplayName("Scanner Wakeup Pause")]
        [Description("In ms, the time to wait for the scanner after boot.")]
        [XmlConfiguration(DefaultValue = "15000")]
        public int ScannerWakeupPause { get; private set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("Filter Excess Codes")]
        [Description("If true, will trim read code count to 4.")]
        [XmlConfiguration(DefaultValue = "true")]
        public bool FilterExcessReadCodes { get; private set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("Cortex Port Open Wait")]
        [Description("Time to wait for the port after open (in ms).")]
        [XmlConfiguration(DefaultValue = "50")]
        public int CortexPortOpenWait { get; private set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("Cortex Snap On Decode Failure")]
        [Description("If true, will snap an image on decode failure.")]
        [XmlConfiguration(DefaultValue = "false")]
        public bool CortexSnapOnDecodeFailure { get; private set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("Cortex Port Buffer Size")]
        [Description("Size of the Cortex port buffer.")]
        [XmlConfiguration(DefaultValue = "8192")]
        public int CortexPortBufferSize { get; private set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("Use Runtime Path")]
        [Description("If true, uses runtime video path")]
        [XmlConfiguration(DefaultValue = "false")]
        public bool UseRuntimePath { get; set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("Use Cortex HD Field")]
        [Description("If true, uses HD field for images")]
        [XmlConfiguration(DefaultValue = "true")]
        public bool UseCortexHDField { get; set; }

        [Browsable(false)]
        [Category("Settings")]
        [DisplayName("IR HardwareInstallDate")]
        [Description("Timestamp of when the IR hardware was installed.")]
        [XmlConfiguration(DefaultValue = "NONE")]
        public string IRHardwareInstallDate { get; set; }

        [Category("Settings")]
        [DisplayName("Use CI For Fraud Validation")]
        [Description("If true, uses CI to support fraud validation.")]
        [XmlConfiguration(DefaultValue = "false")]
        public bool UseInliteForFraudValidation { get; private set; }

        protected override string FileNodeName => "Camera";

        internal static BarcodeConfiguration Instance { get; private set; }

        public static BarcodeConfiguration NewInstance()
        {
            lock (InstanceLock)
            {
                if (Instance == null)
                {
                    Instance = new BarcodeConfiguration();
                    ServiceLocator.Instance.GetService<IConfigurationService>()
                        .RegisterConfiguration(Configurations.Camera.ToString(), Instance);
                    var scannerDeviceFactory = new ScannerDeviceFactory();
                    ServiceLocator.Instance.AddService<IScannerDeviceService>(scannerDeviceFactory);
                    Instance.AddObserver(scannerDeviceFactory);
                }

                return Instance;
            }
        }

        public static BarcodeConfiguration MakeNewInstance2()
        {
            lock (InstanceLock)
            {
                if (Instance == null)
                    Instance = new BarcodeConfiguration();
                return Instance;
            }
        }

        public string[] GetModes()
        {
            return new string[0];
        }

        protected override void LoadPropertiesInner(XmlDocument document, ErrorList errors)
        {
        }

        protected override void StorePropertiesInner(XmlDocument document, ErrorList errors)
        {
        }

        protected override void ImportInner(ErrorList errors)
        {
        }

        protected override void UpgradeInner(XmlDocument document, ErrorList errors)
        {
            try
            {
                var oldChild = document.DocumentElement.SelectSingleNode("Barcode");
                if (oldChild != null)
                    document.DocumentElement.RemoveChild(oldChild);
                var cameraNode = document.DocumentElement.SelectSingleNode(FileNodeName);
                if (cameraNode == null)
                {
                    var element = document.CreateElement(FileNodeName);
                    document.DocumentElement.AppendChild(element);
                    StoreProperties(document, new ErrorList());
                }
                else
                {
                    var toRemove = new List<XmlNode>();
                    using (new DisposeableList<XmlNode>(toRemove))
                    {
                        Array.ForEach(LegacyProperties, property =>
                        {
                            foreach (XmlNode childNode in cameraNode.ChildNodes)
                                if (childNode.Name == property)
                                    toRemove.Add(childNode);
                        });
                        var xmlNode = toRemove.Find(xmlNode_0 => xmlNode_0.Name == "IRHardwareInstallDate");
                        if (xmlNode != null && !string.IsNullOrEmpty(xmlNode.InnerText))
                            KioskConfiguration.Instance.SetConfig("IRHardwareInstallDate", xmlNode.InnerText, true);
                        toRemove.ForEach(xmlNode_0 => cameraNode.RemoveChild(xmlNode_0));
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(Error.NewError("U002", "Barcode configuration upgrade failure.", ex));
            }
        }
    }
}