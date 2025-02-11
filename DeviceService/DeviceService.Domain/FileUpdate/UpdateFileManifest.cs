using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel.FileUpdate;

namespace DeviceService.Domain.FileUpdate
{
    public class UpdateFileManifest : IUpdateFileManifest
    {
        public List<FileModel> FileModels { get; set; }
        public Guid Id { get; set; }

        public int Revision { get; set; }

        public string Name { get; set; }

        public DateTime? StartDate { get; set; }

        public TimeSpan? UpdateRangeStart { get; set; }

        public TimeSpan? UpdateRangeEnd { get; set; }

        public override string ToString()
        {
            var objArray = new object[7]
            {
                Id,
                Name,
                Revision,
                StartDate.HasValue ? StartDate.ToString() : (object)"On Receipt",
                null,
                null,
                null
            };
            var nullable = UpdateRangeStart;
            objArray[4] = nullable ?? TimeSpan.MinValue;
            nullable = UpdateRangeEnd;
            objArray[5] = nullable ?? TimeSpan.FromMilliseconds(86399999.0);
            objArray[6] = FileModels != null ? string.Join(", ", FileModels.Select(x => x.Path)) : (object)string.Empty;
            return string.Format("File {0} - {1} Revision {2} Starts: {3} between {4} and {5} with files: {6}",
                objArray);
        }
    }
}