using System;
using System.Collections.Generic;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

internal sealed class ScanResult
{
    private ScanResult()
    {
        ReadCount = SecureCount = 0;
        ScannedMatrix = "UNKNOWN";
        ExecutionTime = new TimeSpan().ToString();
        SnapOk = false;
    }

    internal int ReadCount { get; private set; }

    internal string ScannedMatrix { get; private set; }

    internal string ExecutionTime { get; private set; }

    internal int SecureCount { get; private set; }

    internal bool SnapOk { get; private set; }

    internal bool IsDuplicate { get; private set; }

    public override string ToString()
    {
        return string.Format("Found {0} barcodes ({1}), time = {2} {3}", ReadCount, ScannedMatrix, ExecutionTime,
            IsDuplicate ? "DUPLICATE" : (object)string.Empty);
    }

    public static ScanResult ReadBarcodeOfDiskInPicker(HardwareService service)
    {
        using (var executor = new ReadDiskJob(service))
        {
            executor.Run();
            return executor.EndStatus == HardwareJobStatus.Errored || executor.Errors.ContainsError()
                ? new ScanResult()
                : From(executor);
        }
    }

    internal static ScanResult From(JobExecutor executor)
    {
        return From(executor.Results);
    }

    internal static ScanResult From(List<ProgramResult> results)
    {
        try
        {
            var sr = new ScanResult();
            results.ForEach(result =>
            {
                switch (result.Code)
                {
                    case "ReadTime":
                        sr.ExecutionTime = result.Message;
                        break;
                    case "SecureCount":
                        sr.SecureCount = Convert.ToInt32(result.Message);
                        break;
                    case "Read-ID":
                        sr.ScannedMatrix = result.ItemID.RawResult;
                        break;
                    case "DeviceStatus":
                        sr.SnapOk = result.Message == "SUCCESS";
                        break;
                    case "NumBarcodes":
                        sr.ReadCount = Convert.ToInt32(result.Message);
                        break;
                    case "DuplicateDetected":
                        sr.IsDuplicate = true;
                        break;
                }
            });
            return sr;
        }
        catch (Exception ex)
        {
            LogHelper.Instance.Log("ScanResult::From caught an exception", ex);
            return new ScanResult();
        }
    }

    internal static ScanResult New()
    {
        return new ScanResult();
    }
}