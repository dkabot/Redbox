using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.CameraTuner
{
    internal sealed class ScanResult
    {
        private static readonly string SecretToken =
            ServiceLocator.Instance.GetService<IEncryptionService>().DecryptFromBase64("2PiCFxFlrO8=");

        private ScanResult()
        {
            ReadCount = 0;
            ScannedMatrix = "UNKNOWN";
            ExecutionTime = new TimeSpan();
            SecureCount = 0;
        }

        internal int ReadCount { get; private set; }

        internal string ScannedMatrix { get; private set; }

        internal TimeSpan ExecutionTime { get; private set; }

        internal bool SnapOk { get; private set; }

        internal int SecureCount { get; private set; }

        internal static ScanResult ErrorResult()
        {
            return new ScanResult();
        }

        internal static ScanResult Scan(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                return ErrorResult();
            var scanResult1 = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>().GetConfiguredReader()
                .Scan(file);
            var scanResult2 = new ScanResult
            {
                SnapOk = true
            };
            if (scanResult1.DecodeResults.Count > 2 || scanResult1.DecodeResults.Count == 0)
            {
                LogHelper.Instance.Log("[ScanResult] There are {0} matrix entries.", scanResult1.DecodeResults.Count);
                return scanResult2;
            }

            var decodeResult1 = (IDecodeResult)null;
            foreach (var decodeResult2 in scanResult1.DecodeResults)
                if (!decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
                    decodeResult1 = decodeResult2;
                else
                    scanResult2.SecureCount = decodeResult2.Count;
            if (decodeResult1 != null)
            {
                scanResult2.ScannedMatrix = decodeResult1.Matrix;
                scanResult2.ReadCount = decodeResult1.Count > 4 ? 4 : decodeResult1.Count;
                scanResult2.ExecutionTime = scanResult1.ExecutionTime;
            }

            return scanResult2;
        }
    }
}