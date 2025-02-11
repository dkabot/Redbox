using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.FileCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redbox.UpdateManager.FileSets
{
    internal class FileSetRevisionDownloader
    {
        private IFileCacheService _fileCacheService;
        private IDownloadService _downloadService;
        private IDownloader _downloader;
        private long _fileSetId;
        private long _revisionId;
        private long _patchRevisionId;
        private string _hash;
        private string _downloadUrl;
        private string _path;
        private string _contentHash;
        private DownloadPriority _downloadPriority;

        internal FileSetRevisionDownloader(
          List<IDownloader> downloads,
          long fileSetId,
          long revisionId,
          long patchRevisionId,
          string hash,
          string downloadUrl,
          string path,
          string contentHash,
          DownloadPriority priority)
        {
            this._fileSetId = fileSetId;
            this._revisionId = revisionId;
            this._patchRevisionId = patchRevisionId;
            this._hash = hash;
            this._downloadUrl = downloadUrl;
            this._path = path;
            this._contentHash = contentHash;
            this._downloadPriority = priority;
            string key = this.GetRevisionKey();
            this._downloader = downloads.Where<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == key)).FirstOrDefault<IDownloader>();
        }

        internal bool Exists()
        {
            return this.FileCacheService.RevisionExists(this._fileSetId, this._revisionId, this._patchRevisionId);
        }

        internal bool DownloadExists() => this._downloader != null;

        internal bool IsDownloadError()
        {
            return this._downloader != null && this._downloader.DownloadData.DownloadState == DownloadState.Error;
        }

        internal bool IsDownloadComplete()
        {
            if (this.Exists())
                return true;
            return this._downloader != null && this._downloader.DownloadData.DownloadState == DownloadState.PostDownload;
        }

        internal IDownloader AddDownload()
        {
            IDownloader download;
            if (this.AddDownload(this.GetRevisionKey(), this._hash, this.GetRevisionUrl(), out download).ContainsError())
                return (IDownloader)null;
            this._downloader = download;
            return this._downloader;
        }

        private ErrorList AddDownload(string key, string hash, string url, out IDownloader download)
        {
            Redbox.UpdateManager.DownloadService.DownloadPriority downloadPriority = (Redbox.UpdateManager.DownloadService.DownloadPriority)this._downloadPriority;
            return this.DownloadService.AddDownload(key, hash, url, downloadPriority, out download);
        }

        internal bool CompleteDownload()
        {
            ErrorList errorList = new ZipDownloadHelper(this._downloader.DownloadData.Path, this._fileSetId, this._revisionId, this._patchRevisionId).Extract();
            if (!errorList.ContainsError())
                this._downloader.Complete();
            return !errorList.ContainsError();
        }

        internal ClientFileSetRevision GetRevision()
        {
            byte[] data;
            return !this.FileCacheService.GetRevision(this._fileSetId, this._revisionId, this._patchRevisionId, out data) ? (ClientFileSetRevision)null : Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
        }

        internal IFileCacheService FileCacheService
        {
            get
            {
                if (this._fileCacheService == null)
                    this._fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
                return this._fileCacheService;
            }
        }

        public IDownloadService DownloadService
        {
            get
            {
                if (this._downloadService == null)
                    this._downloadService = ServiceLocator.Instance.GetService<IDownloadService>();
                return this._downloadService;
            }
        }

        internal IDownloader Downloader => this._downloader;

        private string GetRevisionKey()
        {
            return string.Format("{0},{1},{2}-{3}", (object)1, (object)this._fileSetId, (object)this._revisionId, (object)this._patchRevisionId);
        }

        private string GetRevisionUrl()
        {
            return string.Format("{0}/{1}", (object)this._downloadUrl, (object)this._path);
        }
    }
}
