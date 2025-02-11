using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Redbox.UpdateManager.DownloadService
{
    internal class DownloadFactory
    {
        private IDataStoreService _dataStoreService;
        private object _lock = new object();
        public const string DownloadExtension = ".dldat";
        public const string DownloadLabel = ".dl";

        public static DownloadFactory Instance => Singleton<DownloadFactory>.Instance;

        public List<IDownloader> GetDownloads()
        {
            lock (this._lock)
            {
                List<IDownloader> downloads = new List<IDownloader>();
                IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
                foreach (Guid downLoad in this.GetDownLoadList())
                {
                    try
                    {
                        DownloadData downloadData = service.Get<DownloadData>(downLoad.ToString() + ".dldat");
                        if (downloadData == null)
                        {
                            this.DeleteDownload(downLoad);
                        }
                        else
                        {
                            IDownloader downloaderFromData = this.GetDownloaderFromData(downloadData);
                            if (downloadData != null)
                                downloads.Add(downloaderFromData);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("An unhandled exception occurred in DownloadFactory.GetDownloads for Guid: {0}", (object)downLoad), ex);
                    }
                }
                return downloads;
            }
        }

        internal IDownloader GetDownloaderFromData(DownloadData downloadData)
        {
            switch (downloadData.DownloadType)
            {
                case DownloadType.Bits:
                    return (IDownloader)new BitsDownloader(downloadData);
                case DownloadType.WebClient:
                    return (IDownloader)new WebClientDownloader(downloadData);
                default:
                    downloadData.DownloadType = DownloadType.Bits;
                    return (IDownloader)new BitsDownloader(downloadData);
            }
        }

        internal DownloadData GetDownloadData(
          string key,
          string hash,
          string url,
          DownloadPriority downloadPriority)
        {
            return new DownloadData()
            {
                Key = key,
                Url = url,
                DownloadState = DownloadState.None,
                RetryCount = 0,
                FileGuid = Guid.NewGuid(),
                Hash = hash,
                DownloadType = DownloadType.Bits,
                DownloadPriority = downloadPriority
            };
        }

        internal void SaveDownload(DownloadData downloadData)
        {
            lock (this._lock)
            {
                this.DataStoreService.Set(downloadData.FileGuid.ToString() + ".dldat", (object)downloadData);
                List<Guid> downLoadList = this.GetDownLoadList();
                if (downLoadList.Contains(downloadData.FileGuid))
                    return;
                downLoadList.Add(downloadData.FileGuid);
                this.DataStoreService.Set(".dl", (object)downLoadList);
            }
        }

        internal void DeleteDownload(DownloadData downloadData)
        {
            lock (this._lock)
            {
                if (File.Exists(downloadData.Path))
                    File.Delete(downloadData.Path);
                this.DataStoreService.Delete(downloadData.FileGuid.ToString() + ".dldat");
                List<Guid> downLoadList = this.GetDownLoadList();
                if (!downLoadList.Contains(downloadData.FileGuid))
                    return;
                downLoadList.Remove(downloadData.FileGuid);
                if (downLoadList.Count > 0)
                {
                    this.DataStoreService.Set(".dl", (object)downLoadList);
                }
                else
                {
                    this.DataStoreService.CleanUp(".dldat");
                    this.DataStoreService.Delete(".dl");
                }
            }
        }

        internal void DeleteDownload(Guid guid)
        {
            lock (this._lock)
            {
                this.DataStoreService.Delete(guid.ToString() + ".dldat");
                List<Guid> downLoadList = this.GetDownLoadList();
                if (!downLoadList.Contains(guid))
                    return;
                downLoadList.Remove(guid);
                if (downLoadList.Count > 0)
                {
                    this.DataStoreService.Set(".dl", (object)downLoadList);
                }
                else
                {
                    this.DataStoreService.CleanUp(".dldat");
                    this.DataStoreService.Delete(".dl");
                }
            }
        }

        internal List<Guid> GetDownLoadList()
        {
            return ServiceLocator.Instance.GetService<IDataStoreService>().Get<List<Guid>>(".dl") ?? new List<Guid>();
        }

        private IDataStoreService DataStoreService
        {
            get
            {
                if (this._dataStoreService == null)
                    this._dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
                return this._dataStoreService;
            }
        }

        private DownloadFactory()
        {
        }
    }
}
