using Redbox.UpdateManager.ComponentModel;
using System;

namespace Redbox.UpdateManager.DownloadService
{
    internal class DownloaderBase : IDownloader
    {
        private DownloadData _downloadData;

        public DownloaderBase(DownloadData downloadData) => this._downloadData = downloadData;

        public DownloadData DownloadData
        {
            get => this._downloadData;
            private set => this._downloadData = value;
        }

        public virtual ErrorList ProcessDownload() => new ErrorList();

        public virtual ErrorList DeleteDownload()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                DownloadFactory.Instance.DeleteDownload(this.DownloadData);
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError(nameof(DownloaderBase), "Unhandled exception occurred DeleteDownload", ex));
            }
            return errorList;
        }

        public virtual ErrorList SaveDownload()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                DownloadFactory.Instance.SaveDownload(this.DownloadData);
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError(nameof(DownloaderBase), "Unhandled exception occurred SaveDownload", ex));
            }
            return errorList;
        }

        public ErrorList Complete()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this.DownloadData.DownloadState = DownloadState.Complete;
                this.SaveDownload();
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError(nameof(DownloaderBase), "An unhandled exception occurred in Cleanup.", ex));
            }
            return errorList;
        }
    }
}
