using System.Collections.Generic;
using DeviceService.ComponentModel.FileUpdate;
using Newtonsoft.Json;

namespace DeviceService.Domain.FileUpdate
{
    public class FileUpdateResults : IFileUpdateResults
    {
        public bool Success { get; set; }

        public FileUpdateHighLevelStatus Status { get; set; }

        public string StatusMessage { get; set; }

        public List<IZipFileUpdateResult> ZipFileUpdateResultCollection { get; set; } =
            new List<IZipFileUpdateResult>();

        public override string ToString()
        {
            return "FileUpdateResults: " + JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}