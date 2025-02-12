using System;
using System.Collections.Generic;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions
{
    public sealed class ReadBarcodeResult
    {
        private ReadBarcodeResult()
        {
            IsDuplicate = false;
            ScanTime = "0.0s";
            Barcode = "UNKNOWN";
            Count = "0";
        }

        public bool IsDuplicate { get; private set; }

        public string ScanTime { get; private set; }

        public string Barcode { get; private set; }

        public string Count { get; private set; }

        public static ReadBarcodeResult ReadBarcodeOfDiskInPicker(HardwareService service)
        {
            using (var readDiskJob = new ReadDiskJob(service))
            {
                readDiskJob.Run();
                return readDiskJob.EndStatus == HardwareJobStatus.Errored || readDiskJob.Errors.ContainsError()
                    ? CreateErrorResult()
                    : FromResults(readDiskJob.Results);
            }
        }

        public static ReadBarcodeResult FromResults(List<ProgramResult> results)
        {
            if (results.Count == 0)
                return CreateErrorResult();
            var readBarcodeResult = new ReadBarcodeResult();
            foreach (var result in results)
                if (result.Code.Equals("Read-ID", StringComparison.CurrentCultureIgnoreCase))
                    readBarcodeResult.Barcode =
                        result.ItemID.IsUnknown() ? result.ItemID.Metadata : result.ItemID.Barcode;
                else if (result.Code.Equals("ReadTime", StringComparison.CurrentCultureIgnoreCase))
                    readBarcodeResult.ScanTime = result.Message;
                else if (result.Code.Equals("NumBarcodes", StringComparison.CurrentCultureIgnoreCase))
                    readBarcodeResult.Count = result.Message;
                else if (result.Code == "DuplicateDetected")
                    readBarcodeResult.IsDuplicate = true;
            return readBarcodeResult;
        }

        public static ReadBarcodeResult CreateErrorResult()
        {
            return new ReadBarcodeResult();
        }

        private sealed class ReadDiskJob : JobExecutor
        {
            internal ReadDiskJob(HardwareService service)
                : base(service)
            {
            }

            protected override string JobName => "read-barcode";

            protected override string Label => "MS read barcode";
        }
    }
}