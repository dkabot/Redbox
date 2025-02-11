using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal struct Scanner : IDisposable
    {
        private readonly IScannerDeviceService ScannerDeviceService;
        private readonly IControlSystem ControlSystem;
        private readonly IScannerDevice Device;
        private bool Disposed;

        private static readonly CenterDiskMethod[] m_smartReadMethods = new CenterDiskMethod[2]
        {
            CenterDiskMethod.VendDoorAndBack,
            CenterDiskMethod.DrumAndBack
        };

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            if (Device != null && Device.RequiresExternalLighting)
                ControlSystem.ToggleRingLight(false,
                    Device.SupportsSecureReads ? ControllerConfiguration.Instance.IRRinglightShutdownPause : 0);
            GC.SuppressFinalize(this);
        }

        internal static ScanResult SmartReadDisk()
        {
            return SmartReadDisk(m_smartReadMethods);
        }

        internal static ScanResult SmartReadDisk(CenterDiskMethod[] methods)
        {
            var service = ServiceLocator.Instance.GetService<IControlSystem>();
            var result =
                ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().SupportsSecureReads
                    ? OnIRScan(service, methods)
                    : OnNonIRScan(service, methods);
            CreateJsonResult(result);
            return result;
        }

        internal static string ReadDiskInPicker(ReadDiskOptions options, ExecutionContext context)
        {
            var str = "UNKNOWN";
            using (var scanner = new Scanner(CenterDiskMethod.VendDoorAndBack,
                       ServiceLocator.Instance.GetService<IControlSystem>()))
            {
                using (var scanResult = scanner.Read())
                {
                    str = scanResult.ScannedMatrix;
                    if (!scanResult.SnapOk)
                    {
                        if ((options & ReadDiskOptions.LeaveCaptureResult) != ReadDiskOptions.None)
                            context.CreateCameraCaptureErrorResult();
                    }
                    else if (scanResult.ReadCode)
                    {
                        if ((options & ReadDiskOptions.CheckForDuplicate) != ReadDiskOptions.None)
                            if (ServiceLocator.Instance.GetService<IInventoryService>()
                                .IsBarcodeDuplicate(scanResult.ScannedMatrix, out _))
                                context.CreateDuplicateItemResult(scanResult.ScannedMatrix);
                    }
                    else if ((options & ReadDiskOptions.LeaveNoReadResult) != ReadDiskOptions.None)
                    {
                        context.CreateNoBarcodeReadResult();
                    }
                }
            }

            return str;
        }

        internal ScanResult Read()
        {
            var scanResult = new ScanResult(Device.Scan());
            var activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
            if (!scanResult.SnapOk)
            {
                LogHelper.Instance.Log(LogEntryType.Error,
                    "[{0}, {1}]: Read disk returned a camera error status CAMERA CAPTURE ERROR",
                    activeContext.ProgramName, activeContext.ID);
                activeContext?.ContextLog.WriteFormatted("No image was captured.");
            }
            else if (!scanResult.ReadCode)
            {
                LogHelper.Instance.WithContext(true, LogEntryType.Error, "No barcodes read on image.");
            }

            return scanResult;
        }

        internal Scanner(IControlSystem cs)
            : this()
        {
            Disposed = false;
            ControlSystem = cs;
            ScannerDeviceService = ServiceLocator.Instance.GetService<IScannerDeviceService>();
            Device = ScannerDeviceService.GetConfiguredDevice();
            Device.Start();
            OnStartRinglight();
        }

        internal Scanner(CenterDiskMethod method, IControlSystem cs)
            : this()
        {
            Disposed = false;
            ControlSystem = cs;
            ScannerDeviceService = ServiceLocator.Instance.GetService<IScannerDeviceService>();
            Device = ScannerDeviceService.GetConfiguredDevice();
            Device.Start();
            if (Device.SupportsSecureReads)
            {
                if (method != CenterDiskMethod.None)
                {
                    var num = (int)ControlSystem.Center(method);
                }

                OnStartRinglight();
            }
            else
            {
                OnStartRinglight();
                if (method == CenterDiskMethod.None)
                    return;
                var num = (int)ControlSystem.Center(method);
            }
        }

        private static ScanResult OnNonIRScan(IControlSystem cs, CenterDiskMethod[] methods)
        {
            var scanResult = (ScanResult)null;
            using (var scanner = new Scanner(cs))
            {
                for (var index = 0; index < methods.Length; ++index)
                {
                    var num = (int)cs.Center(methods[index]);
                    scanResult = scanner.OnReadWithRetry();
                    scanResult.ReadAttempts = index + 1;
                    if (!scanResult.SnapOk || scanResult.ReadCode)
                        return scanResult;
                    if (index != methods.Length - 1)
                        scanResult.Dispose();
                }
            }

            return scanResult;
        }

        private static ScanResult OnIRScan(IControlSystem cs, CenterDiskMethod[] methods)
        {
            var scanResult = (ScanResult)null;
            CheckAndResizeForAdditionalFraudAttempts(ref methods);
            LogHelper.Instance.Log(string.Format(
                "Number Of Center Methods: {0}, RetryEnabled: {1}, AdditionalAttempts: {2}", methods.Length,
                ControllerConfiguration.Instance.RetryReadOnNoMarkersFound,
                ControllerConfiguration.Instance.AdditionalFraudReadAttempts));
            for (var index = 0; index < methods.Length; ++index)
                using (var scanner = new Scanner(methods[index], cs))
                {
                    scanResult = scanner.OnReadWithRetry();
                    scanResult.ReadAttempts = index + 1;
                    if (!scanResult.SnapOk || (scanResult.ReadCode &&
                                               (!ControllerConfiguration.Instance.RetryReadOnNoMarkersFound ||
                                                (ControllerConfiguration.Instance.RetryReadOnNoMarkersFound &&
                                                 scanResult.SecureTokensFound > 0))))
                        return scanResult;
                    if (scanResult.ReadCode && ControllerConfiguration.Instance.RetryReadOnNoMarkersFound &&
                        scanResult.SecureTokensFound <= 0)
                        LogHelper.Instance.Log(
                            string.Format("{0} - OnIRScan Retry Read Image - No Secure Tokens Found.", index));
                    if (index != methods.Length - 1)
                        scanResult.Dispose();
                }

            return scanResult;
        }

        private void OnStartRinglight()
        {
            if (!Device.RequiresExternalLighting)
                return;
            ControlSystem.ToggleRingLight(true,
                Device.SupportsSecureReads
                    ? ControllerConfiguration.Instance.IRRinglightStartupPause
                    : ControllerConfiguration.Instance.RinglightWarmupPause2);
        }

        private ScanResult OnReadWithRetry()
        {
            var scanResult = Read();
            if (scanResult.SnapOk || !ControllerConfiguration.Instance.EnableCameraReset || !Device.Restart())
                return scanResult;
            scanResult = Read();
            return scanResult;
        }

        internal static void CheckAndResizeForAdditionalFraudAttempts(ref CenterDiskMethod[] methods)
        {
            if (!ControllerConfiguration.Instance.RetryReadOnNoMarkersFound ||
                ControllerConfiguration.Instance.AdditionalFraudReadAttempts <= 0)
                return;
            var length = methods.Length;
            var newSize = length + ControllerConfiguration.Instance.AdditionalFraudReadAttempts;
            LogHelper.Instance.Log(string.Format("original length: {0}, new length: {1}", length, newSize));
            Array.Resize(ref methods, newSize);
            for (var index = length; index < newSize; ++index)
                methods[index] = CenterDiskMethod.None;
        }

        internal static void CreateJsonResult(ScanResult result)
        {
            var activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
            var dictionary = new Dictionary<string, object>
            {
                {
                    "BarcodesRead",
                    result.ReadCount
                },
                {
                    "SecureCodesRead",
                    result.SecureTokensFound
                },
                {
                    "ReadAttempts",
                    result.ReadAttempts
                },
                {
                    "ExecutionTime",
                    result.ExecutionTime.ToString()
                }
            };
            var str = result.ScannedMatrix;
            var jsonMessage = dictionary;
            activeContext.CreateJSONResult("CameraStatsJson", str, jsonMessage);
        }
    }
}