using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Redbox.UpdateManager.DownloadFile
{
    internal class DownloadFileService : IPollRequestReply, IDownloadFileService
    {
        private bool _rebootRequired;
        private Timer _timer;
        private bool _isRunning;
        private int _inDoWork;

        public static DownloadFileService Instance => Singleton<DownloadFileService>.Instance;

        public ErrorList Initialize()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer = new Timer((TimerCallback)(o => this.DoWork()));
                this._isRunning = false;
                ServiceLocator.Instance.AddService(typeof(IDownloadFileService), (object)this);
                LogHelper.Instance.Log("Initialized the download file service", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred while initializing the download file service.", ex));
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._timer.Change(new TimeSpan(0, 0, 55), new TimeSpan(0, 5, 0));
                this._isRunning = true;
                LogHelper.Instance.Log("Starting the download file service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS998", "An unhandled exception occurred while starting the download file service.", ex));
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
                LogHelper.Instance.Log("Stopping the download file service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS997", "An unhandled exception occurred while stopping the download file service.", ex));
            }
            return errorList;
        }

        public ErrorList GetPollRequest(out List<PollRequest> pollRequests)
        {
            pollRequests = new List<PollRequest>();
            return new ErrorList();
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errors = new ErrorList();
            if (!errors.ContainsError() && pollReplyList != null)
            {
                if (pollReplyList.Any<PollReply>())
                {
                    try
                    {
                        IEnumerable<DownloadFileChangeSetData> changeSets = pollReplyList.Select<PollReply, DownloadFileChangeSetData>((Func<PollReply, DownloadFileChangeSetData>)(pollReply => pollReply.Data.ToObject<DownloadFileChangeSetData>()));
                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CreateDownloads(changeSets));
                    }
                    catch (Exception ex)
                    {
                        errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS0010", "An unhandled exception occurred in DownloadFileService.ProcessPollReply.", ex));
                    }
                    errors.ToLogHelper();
                    return errors;
                }
            }
            return errors;
        }

        private DownloadFileService()
        {
        }

        private void DoWork()
        {
            try
            {
                if (!this._isRunning)
                    LogHelper.Instance.Log("The downloadfile service is not running.", LogEntryType.Info);
                else if (Interlocked.CompareExchange(ref this._inDoWork, 1, 0) == 1)
                {
                    LogHelper.Instance.Log("Already in DownloadFileService.DoWork", LogEntryType.Info);
                }
                else
                {
                    try
                    {
                        LogHelper.Instance.Log("Executing DownloadFileService.DoWork");
                        this.ProcessDownloads().ToLogHelper();
                    }
                    finally
                    {
                        this._inDoWork = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an exception in DownloadFileService.DoWork()", ex);
            }
        }

        private ErrorList CreateDownloads(IEnumerable<DownloadFileChangeSetData> changeSets)
        {
            ErrorList downloads = new ErrorList();
            LogHelper.Instance.Log("Executing DownloadFileService.CreateDownloads");
            try
            {
                foreach (DownloadFileChangeSetData changeSet in changeSets)
                    DownloadFileFactory.Instance.GetDownloadFileFromChangeSet(changeSet).SaveDownloadFile();
            }
            catch (Exception ex)
            {
                downloads.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in DownloadFileService.CreateDownloads.", ex));
            }
            return downloads;
        }

        private ErrorList ProcessDownloads()
        {
            ErrorList errors = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Executing DownloadFileService.ProcessDownloads");
                ServiceLocator.Instance.GetService<IKernelService>().Execute((Action)(() =>
                {
                    List<IDownloadFile> downloadFiles = DownloadFileFactory.Instance.GetDownloadFiles();
                    LogHelper.Instance.Log("Found {0} download files.", (object)downloadFiles.Count);
                    foreach (IDownloadFile downloadFile in downloadFiles)
                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)downloadFile.ProcessDownloadFile());
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)BitsDownloadFile.Cleanup());
                    if (!this.RebootRequired)
                        return;
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.Reboot());
                }));
                LogHelper.Instance.Log("Finished executing DownloadFileService.ProcessDownloads");
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred in DownloadFileService.ProcessDownloads.", ex));
            }
            return errors;
        }

        public void MarkRebootRequired() => this._rebootRequired = true;

        public bool RebootRequired
        {
            get => this._rebootRequired;
            internal set => this._rebootRequired = value;
        }

        private ErrorList Reboot()
        {
            ErrorList errorList = new ErrorList();
            IKernelService service1 = ServiceLocator.Instance.GetService<IKernelService>();
            IKioskEngineService service2 = ServiceLocator.Instance.GetService<IKioskEngineService>();
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service2.Shutdown(15000, 2));
            if (errorList.ContainsError())
            {
                errorList.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e, (object)e.Details)));
                return errorList;
            }
            service1.ShutdownType = ShutdownType.Reboot;
            service1.PerformShutdown();
            return errorList;
        }
    }
}
