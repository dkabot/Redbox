using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.UpdateManager.DownloadService
{
    internal class DownloadService : IDownloadService
    {
        private Timer _timer;
        private bool _isRunning;
        private int _inDoWork;
        private string _root;

        public static Redbox.UpdateManager.DownloadService.DownloadService Instance
        {
            get => Singleton<Redbox.UpdateManager.DownloadService.DownloadService>.Instance;
        }

        public ErrorList Initialize(string rootPath)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._root = rootPath;
                this._timer = new Timer((TimerCallback)(o => this.DoWork()));
                this._isRunning = false;
                ServiceLocator.Instance.AddService(typeof(IDownloadService), (object)this);
                LogHelper.Instance.Log("Initialized the download service", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(DownloadService), "Unhandled exception occurred in Initialize.", ex));
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(new TimeSpan(0, 0, 35), new TimeSpan(0, 1, 0));
                this._isRunning = true;
                LogHelper.Instance.Log("Starting the Download service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(DownloadService), "An unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public ErrorList Stop()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(-1, -1);
                this._isRunning = false;
                LogHelper.Instance.Log("Stopping the download service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(DownloadService), "An unhandled exception occurred while stopping the Download service.", ex));
            }
            return errorList;
        }

        private void DoWork()
        {
            try
            {
                if (!this._isRunning)
                    LogHelper.Instance.Log("The DownloadService service is not running.", LogEntryType.Info);
                else if (Interlocked.CompareExchange(ref this._inDoWork, 1, 0) == 1)
                {
                    LogHelper.Instance.Log("Already in DownloadService.DoWork", LogEntryType.Info);
                }
                else
                {
                    try
                    {
                        LogHelper.Instance.Log("Executing DownloadService.DoWork");
                        this.ProcessDownloads();
                    }
                    finally
                    {
                        this._inDoWork = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an exception in DownloadService.DoWork()", ex);
            }
        }

        public List<IDownloader> GetDownloads() => DownloadFactory.Instance.GetDownloads();

        public ErrorList AddDownload(
          string key,
          string hash,
          string url,
          DownloadPriority priority,
          out IDownloader download)
        {
            ErrorList errorList = new ErrorList();
            download = (IDownloader)null;
            LogHelper.Instance.Log("Adding download key: {0} hash: {1} url: {2} priority: {3}", (object)key, (object)hash, (object)url, (object)priority.ToString());
            try
            {
                IDownloader downloaderFromData = DownloadFactory.Instance.GetDownloaderFromData(DownloadFactory.Instance.GetDownloadData(key, hash, url, priority));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)downloaderFromData.SaveDownload());
                if (errorList.ContainsError())
                    return errorList;
                download = downloaderFromData;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(DownloadService), "An unhandled exception occurred in AddDownload.", ex));
            }
            return errorList;
        }

        private ErrorList ProcessDownloads()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Executing DownloadService.ProcessDownloads");
                List<IDownloader> downloads = DownloadFactory.Instance.GetDownloads();
                LogHelper.Instance.Log("DownloadService found {0} download files.", (object)downloads.Count);
                foreach (IDownloader downloader in downloads)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)downloader.ProcessDownload());
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)BitsDownloader.Cleanup());
                LogHelper.Instance.Log("Finished executing DownloadService.ProcessDownloads");
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(DownloadService), "An unhandled exception occurred in ProcessDownloads.", ex));
            }
            return errorList;
        }

        private DownloadService()
        {
        }
    }
}
