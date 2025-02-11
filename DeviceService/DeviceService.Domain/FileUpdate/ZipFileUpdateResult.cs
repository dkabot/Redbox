using System.Collections.Generic;
using DeviceService.ComponentModel.FileUpdate;
using Newtonsoft.Json;

namespace DeviceService.Domain.FileUpdate
{
    public class ZipFileUpdateResult : IZipFileUpdateResult
    {
        public string FileName { get; set; }

        public ZipFileUpdateStatus Status { get; set; }

        public string StatusMessage { get; set; }

        public IUpdateFileManifestProperties UpdateFileManifestProperties { get; set; }

        public List<IFileUpdateResult> FileUpdateResultCollection { get; set; } = new List<IFileUpdateResult>();

        public override string ToString()
        {
            return "ZipFileUpdateResult: " + JsonConvert.SerializeObject(this);
        }
    }
}