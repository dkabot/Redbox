using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.DownloadFile
{
    internal class DownloadFileFactory
    {
        private IDataStoreService _dataStoreService;
        private IStatusMessageService _statusMessageService;
        private object _lock = new object();
        public const string DownloadFileExtension = ".downloaddat";
        public const string DownloadFileLabel = ".download";

        public static DownloadFileFactory Instance => Singleton<DownloadFileFactory>.Instance;

        internal List<IDownloadFile> GetDownloadFiles()
        {
            lock (this._lock)
            {
                List<IDownloadFile> downloadFiles = new List<IDownloadFile>();
                IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
                foreach (Guid downLoad in this.GetDownLoadList())
                {
                    try
                    {
                        DownloadFileData downloadFileData = service.Get<DownloadFileData>(downLoad.ToString() + ".downloaddat");
                        if (downloadFileData == null)
                        {
                            this.DeleteDownloadFile(downLoad);
                        }
                        else
                        {
                            IDownloadFile downloadFileFromData = this.GetDownloadFileFromData((IDownloadFileData)downloadFileData);
                            if (downloadFileData != null)
                                downloadFiles.Add(downloadFileFromData);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log(string.Format("An unhandled exception occurred in DownloadFileFactory.GetDownloadFiles for Guid: {0}", (object)downLoad), ex);
                    }
                }
                return downloadFiles;
            }
        }

        internal IDownloadFileData ChangeSetToDownloadFile(DownloadFileChangeSetData changeSet)
        {
            DownloadFileDataType downloadFileDataType;
            switch (changeSet.DownloadFileType)
            {
                case DownloadFileType.Bits:
                    downloadFileDataType = DownloadFileDataType.Bits;
                    break;
                case DownloadFileType.WUA:
                    downloadFileDataType = DownloadFileDataType.WUA;
                    break;
                case DownloadFileType.Script:
                    downloadFileDataType = DownloadFileDataType.Script;
                    break;
                default:
                    downloadFileDataType = DownloadFileDataType.Script;
                    break;
            }
            return (IDownloadFileData)new DownloadFileData()
            {
                FileGuid = Guid.NewGuid(),
                ActivateScript = changeSet.ActivateScript,
                DestinationPath = changeSet.DestinationPath,
                DownloadFileDataState = DownloadFileDataState.None,
                DownloadFileDataType = downloadFileDataType,
                EndTime = changeSet.EndTime,
                FileKey = changeSet.FileKey,
                FileName = changeSet.FileName,
                Id = changeSet.Id,
                Name = changeSet.Name,
                StartTime = changeSet.StartTime,
                StatusKey = changeSet.StatusKey,
                Url = changeSet.Url,
                RetryCount = 0
            };
        }

        internal IDownloadFile GetDownloadFileFromChangeSet(DownloadFileChangeSetData changeSet)
        {
            return this.GetDownloadFileFromData(this.ChangeSetToDownloadFile(changeSet));
        }

        internal IDownloadFile GetDownloadFileFromData(IDownloadFileData downloadFileData)
        {
            switch (downloadFileData.DownloadFileDataType)
            {
                case DownloadFileDataType.Bits:
                    return (IDownloadFile)new BitsDownloadFile(downloadFileData);
                case DownloadFileDataType.Script:
                    return (IDownloadFile)new ScriptDownloadFile(downloadFileData);
                default:
                    return (IDownloadFile)null;
            }
        }

        internal void SaveDownloadFile(IDownloadFileData downloadFileData)
        {
            lock (this._lock)
            {
                this.DataStoreService.Set(downloadFileData.FileGuid.ToString() + ".downloaddat", (object)downloadFileData);
                List<Guid> downLoadList = this.GetDownLoadList();
                if (downLoadList.Contains(downloadFileData.FileGuid))
                    return;
                downLoadList.Add(downloadFileData.FileGuid);
                this.DataStoreService.Set(".download", (object)downLoadList);
            }
        }

        internal void DeleteDownloadFile(IDownloadFileData downloadFileData)
        {
            lock (this._lock)
            {
                this.DataStoreService.Delete(downloadFileData.FileGuid.ToString() + ".downloaddat");
                List<Guid> downLoadList = this.GetDownLoadList();
                if (!downLoadList.Contains(downloadFileData.FileGuid))
                    return;
                downLoadList.Remove(downloadFileData.FileGuid);
                if (downLoadList.Count > 0)
                {
                    this.DataStoreService.Set(".download", (object)downLoadList);
                }
                else
                {
                    this.DataStoreService.CleanUp(".downloaddat");
                    this.DataStoreService.Delete(".download");
                }
            }
        }

        internal void DeleteDownloadFile(Guid guid)
        {
            lock (this._lock)
            {
                this.DataStoreService.Delete(guid.ToString() + ".downloaddat");
                List<Guid> downLoadList = this.GetDownLoadList();
                if (!downLoadList.Contains(guid))
                    return;
                downLoadList.Remove(guid);
                if (downLoadList.Count > 0)
                {
                    this.DataStoreService.Set(".download", (object)downLoadList);
                }
                else
                {
                    this.DataStoreService.CleanUp(".downloaddat");
                    this.DataStoreService.Delete(".download");
                }
            }
        }

        internal List<Guid> GetDownLoadList()
        {
            return ServiceLocator.Instance.GetService<IDataStoreService>().Get<List<Guid>>(".download") ?? new List<Guid>();
        }

        internal ErrorList SendStatus(
          StatusMessage.StatusMessageType statusMessageType,
          string statusKey,
          string description,
          object data)
        {
            ErrorList errors = new ErrorList();
            try
            {
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.StatusMessageService.EnqueueMessage(statusMessageType, statusKey, description, data.ToJson()));
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred in DownloadFileService.SendStatus.", ex));
            }
            errors.ToLogHelper();
            return errors;
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

        private IStatusMessageService StatusMessageService
        {
            get
            {
                if (this._statusMessageService == null)
                    this._statusMessageService = ServiceLocator.Instance.GetService<IStatusMessageService>();
                return this._statusMessageService;
            }
        }

        private DownloadFileFactory()
        {
        }
    }
}
