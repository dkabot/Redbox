using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.DataMatrix.Framework.Cortex;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class CortexService :
        ScannerAdapter,
        IScannerDevice,
        IDisposable,
        IConfigurationObserver
    {
        private readonly ICommChannelConfiguration Configuration;
        private readonly CommPortReadModes Mode = CommPortReadModes.Callback;
        private string ConfiguredPortName = "NONE";
        private bool LogDetailedScan;
        private ICommPort Port;

        internal CortexService()
        {
            Configuration = ServiceLocator.Instance.GetService<IPortManagerService>().CreateConfiguration();
            var cortexPortBufferSize = BarcodeConfiguration.Instance.CortexPortBufferSize;
            Configuration.ReceiveBufferSize = cortexPortBufferSize;
            LogHelper.Instance.Log("[CortexService] Receive buffer size = {0}", cortexPortBufferSize);
            Configuration.WriteTimeout = 2000;
        }

        public void NotifyConfigurationLoaded()
        {
            UpdateConfiguration();
        }

        public void NotifyConfigurationChangeStart()
        {
        }

        public void NotifyConfigurationChangeEnd()
        {
            UpdateConfiguration();
        }

        public bool Start()
        {
            return Start(false);
        }

        public bool Start(bool unused)
        {
            if (IsConnected)
                return true;
            if (Port != null)
            {
                if (!Port.Open())
                {
                    LogHelper.Instance.Log("[Cortex Service] Unable to open communcation port {0}.", Port.DisplayName);
                    return false;
                }
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IPortManagerService>();
                Port = ConfiguredPortName == "NONE"
                    ? service.Scan(Configuration, TestPort, Mode)
                    : service.Scan(ConfiguredPortName, Configuration, TestPort, Mode);
                if (Port == null)
                {
                    LogHelper.Instance.Log("[Cortex Service] Unable to locate communcation port.");
                    return false;
                }

                ConfiguredPortName = Port.PortName;
            }

            new InfaredLightCommand().Send(Port);
            RuntimeService.Wait(300);
            return IsConnected = !OnScan().DeviceError;
        }

        public bool Stop()
        {
            if (!IsConnected)
                return true;
            if (Port != null)
            {
                ServiceLocator.Instance.GetService<IPortManagerService>().Dispose(Port);
                Port = null;
            }

            IsConnected = false;
            return true;
        }

        public bool Restart()
        {
            Stop();
            RuntimeService.Wait(500);
            return Start();
        }

        public IScanResult Scan()
        {
            if (IsConnected)
                return OnScan();
            LogHelper.Instance.Log("[Cortex Service] Scan is called without initialization.");
            var scanResult = new ScanResult();
            scanResult.ResetOnException();
            scanResult.DeviceError = true;
            return scanResult;
        }

        public ISnapResult Snap()
        {
            var snapFileName = GenerateSnapFileName();
            if (!IsConnected)
            {
                LogHelper.Instance.Log("[Cortex Service] Snap: camera is not connected.");
                return new SnapResult(snapFileName);
            }

            if (BarcodeConfiguration.Instance.UseCortexHDField)
                using (var densityFieldCommand = new SetHighDensityFieldCommand())
                {
                    densityFieldCommand.Send(Port);
                }

            using (var takePictureCommand = new TakePictureCommand())
            {
                takePictureCommand.Send(Port);
                if (takePictureCommand.PortError)
                {
                    LogHelper.Instance.Log("[CortexService]: There was a communication error.");
                    return new SnapResult(snapFileName);
                }

                var imagePacket = takePictureCommand.ImagePacket;
                if (imagePacket == null)
                {
                    LogHelper.Instance.Log("[CortexService]: unable to locate image packet during snap.");
                    return new SnapResult(snapFileName);
                }

                using (var getImageCommand = new GetImageCommand(imagePacket))
                {
                    getImageCommand.Send(Port);
                    if (getImageCommand.PortError)
                    {
                        LogHelper.Instance.Log("[CortexService]: failed to read image data from device.");
                        return new SnapResult(snapFileName);
                    }

                    LogHelper.Instance.Log("[Cortex Service] Snap time: {0}ms",
                        getImageCommand.ExecutionTime.Milliseconds);
                    getImageCommand.CreateImage(snapFileName);
                }

                new DeleteFileCommand(imagePacket).Send(Port);
                return new SnapResult(snapFileName);
            }
        }

        public bool ValidateSettings()
        {
            return ValidateSettings(new ValidatorSink());
        }

        public bool ValidateSettings(IMessageSink sink)
        {
            return new CortexSettingsValidator(this, RuntimeService).Validate(sink);
        }

        public bool RequiresExternalLighting => false;

        public bool IsConnected { get; private set; }

        public bool InError => false;

        public bool IsLegacy => false;

        public bool SupportsSecureReads => true;

        public ScannerServices Service => ScannerServices.Cortex;

        public CameraGeneration CurrentCameraGeneration => CameraGeneration.Fifth;

        protected override void DisposeInner()
        {
            Stop();
        }

        internal ICommPort ConfiguredPort()
        {
            return Port;
        }

        private void UpdateConfiguration()
        {
            Configuration.OpenPause = BarcodeConfiguration.Instance.CortexPortOpenWait;
            Configuration.WritePause = BarcodeConfiguration.Instance.WritePause;
            LogDetailedScan = BarcodeConfiguration.Instance.LogDetailedScan;
            Configuration.EnableDebug = LogDetailedScan;
        }

        private ScanResult OnScan()
        {
            var result = new ScanResult(Snap());
            using (var decodeCommand = new DecodeCommand())
            {
                decodeCommand.Send(Port);
                if (decodeCommand.PortError)
                {
                    LogHelper.Instance.Log("[Cortex Service] Scan: There was a communication error with the device.");
                    result.ResetOnException();
                    result.DeviceError = true;
                    return result;
                }

                decodeCommand.FoundCodes.ForEach(each => result.Add(each));
                result.ExecutionTime = decodeCommand.ExecutionTime;
            }

            return result;
        }

        private bool TestPort(ICommPort port)
        {
            using (var registerValueCommand = new GetRegisterValueCommand(new CortexRegister("21D", "1")))
            {
                registerValueCommand.Send(port);
                var isCorrectlySet = registerValueCommand.IsCorrectlySet;
                LogHelper.Instance.Log("[Cortex Service] Test port returned {0} on port {1}", isCorrectlySet,
                    port.DisplayName);
                return isCorrectlySet;
            }
        }

        private class ValidatorSink : IMessageSink
        {
            public bool Send(string message)
            {
                LogHelper.Instance.Log("[CortexService] <Validate message> {0}", message);
                return true;
            }
        }
    }
}