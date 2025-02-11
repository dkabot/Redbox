using System;
using System.Collections.Generic;
using System.Linq;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.DownloadService
{
    public class DownloadDataList : List<DownloadData>
    {
        public bool IsDownloading
        {
            get
            {
                return this.Any(eachDownloadData => eachDownloadData.DownloadState == DownloadState.Downloading ||
                                                    eachDownloadData.DownloadState == DownloadState.Error);
            }
        }

        public DownloadDataList GetByFileSetId(long fileSetId)
        {
            var byFileSetId = new DownloadDataList();
            foreach (var downloadData in this)
                if (downloadData.ParseFileSetIdFromDownloadDataKey() == fileSetId)
                    byFileSetId.Add(downloadData);
            return byFileSetId;
        }

        public DownloadData GetByRevisionChangeSetKey(RevisionChangeSetKey revisionChangeSetKey)
        {
            var key = DownloadData.GetRevisionKey(revisionChangeSetKey);
            return this.Where(d => d.Key == key).FirstOrDefault();
        }

        public DownloadData GetByBitsGuid(Guid bitsGuid)
        {
            return this.FirstOrDefault(downloadData => downloadData.BitsGuid == bitsGuid);
        }

        public bool ExistsByKey(string key)
        {
            return this.Any(downloadData => downloadData.Key == key);
        }
    }
}