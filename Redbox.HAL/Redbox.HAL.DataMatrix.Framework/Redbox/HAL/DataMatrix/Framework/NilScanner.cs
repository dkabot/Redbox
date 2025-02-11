using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class NilScanner : IScannerDevice, IDisposable
    {
        public void Dispose()
        {
        }

        public bool Start()
        {
            return false;
        }

        public bool Start(bool unused)
        {
            return false;
        }

        public bool Stop()
        {
            return false;
        }

        public bool Restart()
        {
            return false;
        }

        public IScanResult Scan()
        {
            return new NilScanResult("UNCONFIGURED DEVICE");
        }

        public ISnapResult Snap()
        {
            return new SnapResult(null);
        }

        public bool ValidateSettings()
        {
            return this.ValidateSettings(null);
        }

        public bool ValidateSettings(IMessageSink sink)
        {
            return false;
        }

        public bool RequiresExternalLighting => false;

        public bool IsConnected => false;

        public bool InError => true;

        public bool IsLegacy => true;

        public bool SupportsSecureReads => false;

        public ScannerServices Service => ScannerServices.Emulated;

        public CameraGeneration CurrentCameraGeneration => CameraGeneration.Unknown;

        public bool TestConfiguration(IMessageSink sink)
        {
            return false;
        }

        public string FindInstalledPort()
        {
            return "NONE";
        }

        public ISnapResult Snap(string fileName)
        {
            return new SnapResult(fileName);
        }
    }
}