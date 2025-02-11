using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class ScanResult : IDisposable
    {
        internal static readonly ScanResult NoReadResult = new ScanResult();

        private static readonly string SecretToken =
            ServiceLocator.Instance.GetService<IEncryptionService>().DecryptFromBase64("2PiCFxFlrO8=");

        internal readonly TimeSpan ExecutionTime;
        private readonly IScanResult RawResult;
        internal readonly bool ReadCode;
        internal readonly int ReadCount;
        internal readonly string ScannedMatrix;
        internal readonly int SecureTokensFound;
        internal readonly bool SnapOk;
        internal int ReadAttempts;

        internal ScanResult(IScanResult sr)
            : this()
        {
            RawResult = sr;
            SnapOk = !sr.DeviceError;
            if (sr.DecodeResults.Count > 2)
            {
                LogHelper.Instance.WithContext(false, LogEntryType.Error,
                    "[ScanResult] There are unexpected {0} matrix entries.", sr.DecodeResults.Count);
            }
            else
            {
                if (sr.DecodeResults.Count == 0)
                    return;
                var decodeResult1 = (IDecodeResult)null;
                foreach (var decodeResult2 in sr.DecodeResults)
                    if (decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
                        SecureTokensFound += decodeResult2.Count;
                    else
                        decodeResult1 = decodeResult2;
                if (decodeResult1 == null)
                    return;
                ScannedMatrix = decodeResult1.Matrix;
                ReadCount = decodeResult1.Count > 4 ? 4 : decodeResult1.Count;
                ReadCode = true;
                ExecutionTime = sr.ExecutionTime;
            }
        }

        private ScanResult()
        {
            SecureTokensFound = 0;
            ScannedMatrix = "UNKNOWN";
            ExecutionTime = new TimeSpan();
            ReadCode = false;
            ReadCount = 0;
            SnapOk = true;
            ReadAttempts = 0;
        }

        private string ImageFile => RawResult != null ? RawResult.ImagePath : string.Empty;

        public void Dispose()
        {
            if (RawResult == null)
                return;
            RawResult.Dispose();
        }

        public override string ToString()
        {
            return string.Format("Scan ok = {0} Matrix = {1} Count = {2} time = {3}", SnapOk, ScannedMatrix, ReadCount,
                ExecutionTime.ToString());
        }

        internal bool CopyToFraudFolder()
        {
            var str = ImageFile;
            if (string.IsNullOrEmpty(str))
            {
                var snapResult = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice()
                    .Snap();
                if (!snapResult.SnapOk)
                    return false;
                str = snapResult.Path;
            }

            var fullPath = Path.Combine(ServiceLocator.Instance.GetService<IFraudService>().FraudImagesPath,
                string.Format("{0}_{1}", ScannedMatrix, Path.GetFileName(str)));
            var num = ServiceLocator.Instance.GetService<IRuntimeService>().SafeMove(str, fullPath) ? 1 : 0;
            var path = num != 0 ? fullPath : str;
            var activeContext = ServiceLocator.Instance.GetService<IExecutionService>()?.GetActiveContext();
            if (activeContext != null && File.Exists(path))
            {
                activeContext.CreateResult("PeeledImageFile", Path.GetFileName(path), ScannedMatrix);
                return num != 0;
            }

            activeContext.ContextLog.WriteFormatted("Failed to create PeeledImageFile Result.");
            return num != 0;
        }

        internal void PushTo(ExecutionContext context)
        {
            context.CreateResult("Read-ID", "Id read", new int?(), new int?(), ScannedMatrix, new DateTime?(), null);
            context.CreateInfoResult("SecureCount", SecureTokensFound.ToString());
            var executionTime = ExecutionTime;
            var seconds = executionTime.Seconds;
            executionTime = ExecutionTime;
            var milliseconds = executionTime.Milliseconds;
            var message = string.Format("{0}.{1} s", seconds, milliseconds);
            context.CreateInfoResult("ReadTime", message);
            context.CreateInfoResult("DeviceStatus", !SnapOk ? "CAPTURE ERROR" : "SUCCESS");
            context.CreateInfoResult("NumBarcodes", ReadCount.ToString());
            context.CreateInfoResult("ReadAttempts", ReadAttempts.ToString());
        }
    }
}