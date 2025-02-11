using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    public class StoreFileCache
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public List<StoreFileDataCache> StoreFileDataCacheList { get; set; }

        public List<StoreFileStateCache> StoreFileStateCacheList { get; set; }

        public Dictionary<string, StoreFileDataStateCache> DataStateCache { get; set; }

        public void UpdateDataStateCache()
        {
            DataStateCache = new Dictionary<string, StoreFileDataStateCache>();
            StoreFileDataCacheList.ForEach(sfd => DataStateCache[sfd.StoreNumber] = new StoreFileDataStateCache
            {
                DataCache = sfd
            });
            StoreFileStateCacheList.ForEach(sfs =>
            {
                StoreFileDataStateCache fileDataStateCache;
                if (DataStateCache.TryGetValue(sfs.StoreNumber, out fileDataStateCache))
                    fileDataStateCache.StateCache = sfs;
                else
                    DataStateCache[sfs.StoreNumber] = new StoreFileDataStateCache
                    {
                        StateCache = sfs
                    };
            });
        }
    }
}