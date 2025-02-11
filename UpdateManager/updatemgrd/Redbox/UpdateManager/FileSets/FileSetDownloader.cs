using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.FileCache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.FileSets
{
    internal class FileSetDownloader
    {
        private ClientFileSetRevision _revision;
        private List<IDownloader> _downloads;
        private string _downloadUrl;
        private DownloadPriority _downloadPriority;
        private List<FileSetFileInfo> _fileSetInfoList;
        private IFileCacheService _fileCacheService;
        private IDownloadService _downloadService;

        internal FileSetDownloader(
          ClientFileSetRevision revision,
          List<IDownloader> downloads,
          string downloadUrl,
          DownloadPriority downloadPriority)
        {
            this._revision = revision;
            this._downloads = downloads;
            this._downloadUrl = downloadUrl;
            this._downloadPriority = downloadPriority;
        }

        public ClientFileSetRevision Revision => this._revision;

        internal bool IsDownloading()
        {
            bool result = false;
            this._downloads.ForEach((Action<IDownloader>)(_downloader =>
            {
                if (_downloader.DownloadData.DownloadState != DownloadState.Downloading && _downloader.DownloadData.DownloadState != DownloadState.Error)
                    return;
                result = true;
            }));
            return result;
        }

        internal bool IsDownloaded() => this.GetDownloadMethod() == DownloadMethod.None;

        internal ErrorList DownloadFileSet()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                switch (this.GetDownloadMethod())
                {
                    case DownloadMethod.None:
                        return errorList;
                    case DownloadMethod.FileSet:
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DownloadByFileSet());
                        break;
                    case DownloadMethod.PatchSet:
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DownloadByPatchSet());
                        break;
                    case DownloadMethod.Files:
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DownloadByFiles());
                        break;
                    case DownloadMethod.Patches:
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DownloadByPatches());
                        break;
                }
                return errorList;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(FileSetDownloader), "An unhandled exception occurred in DownloadFileSet", ex));
            }
            return errorList;
        }

        internal ErrorList CompleteDownloads()
        {
            ErrorList errors = new ErrorList();
            this._downloads.ForEach((Action<IDownloader>)(_downloader =>
            {
                if (_downloader.DownloadData.DownloadState != DownloadState.PostDownload)
                    return;
                ErrorList collection = new ZipDownloadHelper(_downloader.DownloadData.Path, this._revision.FileSetId, this._revision.RevisionId, this._revision.PatchRevisionId).Extract();
                if (!collection.ContainsError())
                    _downloader.Complete();
                errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)collection);
            }));
            return errors;
        }

        internal List<FileSetFileInfo> FileSetInfoList
        {
            get
            {
                if (this._fileSetInfoList == null)
                    this.UpdateFileSetInfoList();
                return this._fileSetInfoList;
            }
        }

        private void UpdateFileSetInfoList()
        {
            List<FileSetFileInfo> fileSetInfoList = new List<FileSetFileInfo>();
            this._revision.Files.ForEach<ClientFileSetFile>((Action<ClientFileSetFile>)(f =>
            {
                FileSetFileInfo fileSetFileInfo = new FileSetFileInfo()
                {
                    FileId = f.FileId,
                    FileRevisionId = f.FileRevisionId,
                    Key = this.GetFileKey(f.FileId, f.FileRevisionId),
                    FileSize = f.FileSize,
                    PatchSize = 0,
                    FileHash = f.FileHash
                };
                fileSetFileInfo.Exists = this.FileCacheService.FileExists(this._revision.FileSetId, f.FileId, f.FileRevisionId);
                fileSetFileInfo.Url = this.GetFileUrl(f);
                if (!fileSetFileInfo.Exists)
                {
                    ClientPatchFileSetFile file = this._revision.PatchFiles.FirstOrDefault<ClientPatchFileSetFile>((Func<ClientPatchFileSetFile, bool>)(pf => pf.FileId == f.FileId));
                    if (file != null)
                    {
                        string fileKey = this.GetFileKey(f.FileId, file.PatchFileRevisionId);
                        if (this.FileCacheService.FileExists(this._revision.FileSetId, f.FileId, file.PatchFileRevisionId))
                        {
                            fileSetFileInfo.PatchSize = file.FileSize;
                            fileSetFileInfo.PatchUrl = this.GetPatchFileUrl(file);
                            fileSetFileInfo.PatchKey = fileKey;
                            fileSetFileInfo.PatchHash = file.FileHash;
                        }
                    }
                }
                fileSetInfoList.Add(fileSetFileInfo);
            }));
            this._fileSetInfoList = fileSetInfoList;
        }

        private DownloadMethod GetDownloadMethod()
        {
            this.UpdateFileSetInfoList();
            long num1 = this.FileSetInfoList.Sum<FileSetFileInfo>((Func<FileSetFileInfo, long>)(item => item.Exists ? 0L : item.FileSize));
            long num2 = 0;
            if (this._revision.PatchSetFileSize > 0L)
                num2 = this.FileSetInfoList.Sum<FileSetFileInfo>((Func<FileSetFileInfo, long>)(item =>
                {
                    if (item.Exists)
                        return 0;
                    return item.PatchSize > 0L ? item.PatchSize : item.FileSize;
                }));
            long num3 = 0;
            if (this._revision.PatchSetFileSize > 0L)
                num3 = this.FileSetInfoList.Sum<FileSetFileInfo>((Func<FileSetFileInfo, long>)(item => item.Exists || item.PatchSize != 0L ? 0L : item.FileSize)) + this._revision.PatchSetFileSize;
            if (num1 == 0L)
                return DownloadMethod.None;
            if (num3 > 0L && (num2 == 0L || num3 <= num2) && num3 < num1 && num3 < this._revision.SetFileSize)
                return DownloadMethod.PatchSet;
            if (num2 > 0L && num2 < num1 && num2 < this._revision.SetFileSize)
                return DownloadMethod.Patches;
            return num1 < this._revision.SetFileSize ? DownloadMethod.Files : DownloadMethod.FileSet;
        }

        private ErrorList DownloadByFileSet()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                string key = this.GetRevisionSetKey();
                string fileSetUrl = this.GetFileSetUrl();
                if (!this._downloads.Any<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == key)))
                {
                    IDownloader download;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.AddDownload(key, this._revision.SetFileHash, fileSetUrl, out download));
                    if (!errorList.ContainsError())
                        this._downloads.Add(download);
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByFileSet", ex));
            }
            return errorList;
        }

        private ErrorList DownloadByPatchSet()
        {
            ErrorList errors = new ErrorList();
            try
            {
                string key = this.GetRevisionPatchSetKey();
                string patchSetFileUrl = this.GetPatchSetFileUrl();
                if (!this._downloads.Any<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == key)))
                {
                    IDownloader download;
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.AddDownload(key, this._revision.PatchSetFileHash, patchSetFileUrl, out download));
                    if (errors.ContainsError())
                        return errors;
                    this._downloads.Add(download);
                }
                this.FileSetInfoList.ForEach((Action<FileSetFileInfo>)(item =>
                {
                    if (item.Exists || item.PatchSize != 0L || this._downloads.Any<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == item.Key)))
                        return;
                    IDownloader download;
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.AddDownload(item.Key, item.FileHash, item.Url, out download));
                    if (errors.ContainsError())
                        return;
                    this._downloads.Add(download);
                }));
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByPatchSet", ex));
            }
            return errors;
        }

        private ErrorList DownloadByPatches()
        {
            ErrorList errors = new ErrorList();
            try
            {
                this.FileSetInfoList.ForEach((Action<FileSetFileInfo>)(item =>
                {
                    if (item.Exists)
                        return;
                    if (item.PatchSize == 0L)
                    {
                        if (this._downloads.Any<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == item.Key)))
                            return;
                        IDownloader download;
                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.AddDownload(item.Key, item.FileHash, item.Url, out download));
                        if (errors.ContainsError())
                            return;
                        this._downloads.Add(download);
                    }
                    else
                    {
                        if (this._downloads.Any<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == item.PatchKey)))
                            return;
                        IDownloader download;
                        errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.AddDownload(item.PatchKey, item.PatchHash, item.PatchUrl, out download));
                        if (errors.ContainsError())
                            return;
                        this._downloads.Add(download);
                    }
                }));
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByPatches", ex));
            }
            return errors;
        }

        private ErrorList DownloadByFiles()
        {
            ErrorList errors = new ErrorList();
            try
            {
                this.FileSetInfoList.ForEach((Action<FileSetFileInfo>)(item =>
                {
                    if (item.Exists || this._downloads.Any<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.Key == item.Key)))
                        return;
                    IDownloader download;
                    errors.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.AddDownload(item.Key, item.FileHash, item.Url, out download));
                    if (errors.ContainsError())
                        return;
                    this._downloads.Add(download);
                }));
            }
            catch (Exception ex)
            {
                errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByFiles", ex));
            }
            return errors;
        }

        private ErrorList AddDownload(string key, string hash, string url, out IDownloader download)
        {
            Redbox.UpdateManager.DownloadService.DownloadPriority downloadPriority = (Redbox.UpdateManager.DownloadService.DownloadPriority)this._downloadPriority;
            return this.DownloadService.AddDownload(key, hash, url, downloadPriority, out download);
        }

        private string GetFileKey(long fileId, long fileRevisionId)
        {
            return string.Format("{0},{1},{2},{3}", (object)2, (object)this._revision.FileSetId, (object)fileId, (object)fileRevisionId);
        }

        private string GetFileUrl(ClientFileSetFile file)
        {
            return string.Format("{0}/{1}", (object)this._downloadUrl, (object)file.Path);
        }

        private string GetPatchFileUrl(ClientPatchFileSetFile file)
        {
            return string.Format("{0}/{1}", (object)this._downloadUrl, (object)file.Path);
        }

        private string GetFilePatchKey(long fileId, long fileRevisionId, long patchFileRevisionId)
        {
            return string.Format("{0},{1},{2},{3},{4}", (object)3, (object)this._revision.FileSetId, (object)fileId, (object)fileRevisionId, (object)patchFileRevisionId);
        }

        private string GetRevisionSetKey()
        {
            return string.Format("{0},{1},{2}", (object)4, (object)this._revision.FileSetId, (object)this._revision.RevisionId);
        }

        private string GetRevisionPatchSetKey()
        {
            return string.Format("{0},{1},{2}", (object)5, (object)this._revision.FileSetId, (object)this._revision.RevisionId);
        }

        private string GetPatchSetFileUrl()
        {
            return string.Format("{0}/{1}", (object)this._downloadUrl, (object)this._revision.PatchSetPath);
        }

        private string GetFileSetUrl()
        {
            return string.Format("{0}/{1}", (object)this._downloadUrl, (object)this._revision.SetPath);
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
    }
}
