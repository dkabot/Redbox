using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    internal class StoreFileCache
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public List<StoreFileDataCache> StoreFileDataCacheList { get; set; }

        public List<StoreFileStateCache> StoreFileStateCacheList { get; set; }

        public Dictionary<string, StoreFileDataStateCache> DataStateCache { get; set; }

        public void UpdateDataStateCache()
        {
            this.DataStateCache = new Dictionary<string, StoreFileDataStateCache>();
            this.StoreFileDataCacheList.ForEach((Action<StoreFileDataCache>)(sfd => this.DataStateCache[sfd.StoreNumber] = new StoreFileDataStateCache()
            {
                DataCache = sfd
            }));
            this.StoreFileStateCacheList.ForEach((Action<StoreFileStateCache>)(sfs =>
            {
                StoreFileDataStateCache fileDataStateCache;
                if (this.DataStateCache.TryGetValue(sfs.StoreNumber, out fileDataStateCache))
                    fileDataStateCache.StateCache = sfs;
                else
                    this.DataStateCache[sfs.StoreNumber] = new StoreFileDataStateCache()
                    {
                        StateCache = sfs
                    };
            }));
        }
    }
}
