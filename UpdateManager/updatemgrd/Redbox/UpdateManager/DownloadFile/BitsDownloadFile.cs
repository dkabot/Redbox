using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.DownloadFile
{
    internal class BitsDownloadFile : DownloadFileBase
    {
        private const string DownloadFileTypeName = "Bits download";
        private const string BitsNameFormat = "<:~{0}-{1}~:>";
        private const string BitsNameLeft = "<:~";
        private const string BitsNameRight = "~:>";

        public BitsDownloadFile(IDownloadFileData downloadFileData) : base(downloadFileData)
        {
        }

        public override ErrorList ProcessDownloadFile()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.None)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckNone());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PendingDownload)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckPendingDownload());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Downloading)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckDownloading());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostDownload)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckPostDownload());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PendingInstall)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckPendingInstall());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Installing)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckInstalling());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostInstall)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckPostInstall());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Complete)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckComplete());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Error)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckError());
            return errorList;
        }

        private ErrorList CheckNone()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.None)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            try
            {
                if (this.DownloadFileDestPathExists())
                {
                    bool matched;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DownloadFileDestPathCheckHash(out matched, out string _));
                    if (matched)
                    {
                        this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PendingInstall;
                        DownloadFileFactory.Instance.SaveDownloadFile(this.DownloadFileData);
                        LogHelper.Instance.Log("DownloadFile Name: {0} has been put on the downloadfile queue with a pending status as file is already in it's destination.", (object)this.DownloadFileData.Name);
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile already exists, skipping download.", (string)null));
                        return errorList;
                    }
                }
                ITransferJob job = (ITransferJob)null;
                ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
                string name = string.Format("<:~{0}-{1}~:>", (object)this.DownloadFileData.Name, (object)this.DownloadFileData.Id.ToString());
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service.CreateDownloadJob(name, out job));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetMinimumRetryDelay(60U));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetNoProgressTimeout(0U));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.SetPriority(TransferJobPriority.Low));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.AddItem(this.DownloadFileData.Url, Path.GetTempFileName()));
                if (errorList.ContainsError())
                {
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Cancel());
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile bits error, leaving status at None for repeat. RetryCount: " + this.DownloadFileData.RetryCount.ToString(), errorList[0].Description));
                    if (this.DownloadFileData.RetryCount == 10)
                        this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    else
                        this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.None;
                    this.DownloadFileData.Message = errorList[0].Description;
                    ++this.DownloadFileData.RetryCount;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Resume());
                if (errorList.ContainsError())
                {
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = errorList[0].Description;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                this.DownloadFileData.BitsGuid = job.ID;
                this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Downloading;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                LogHelper.Instance.Log("DownloadFile Name: {0} has been put on the downloadfile queue.", (object)this.DownloadFileData.Name);
                LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", (object)name, (object)job.ID);
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile bits download started", (string)null));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.None", ex));
            }
            return errorList;
        }

        private ErrorList CheckPendingDownload()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PendingDownload)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            return errorList;
        }

        private ErrorList CheckDownloading()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Downloading)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            try
            {
                ServiceLocator.Instance.GetService<ITransferService>();
                List<ITransferJob> jobs = (List<ITransferJob>)null;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)BitsDownloadFile.GetDownloadFileBitsJobs(out jobs));
                if (errorList.ContainsError())
                {
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = errorList[0].Description;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                ITransferJob job = jobs.Where<ITransferJob>((Func<ITransferJob, bool>)(j => j.ID == this.DownloadFileData.BitsGuid)).FirstOrDefault<ITransferJob>();
                if (job == null)
                {
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = "Bits job missing";
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                if (job.Status == TransferStatus.Error)
                {
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendTransferStatus(job, false));
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Cancel());
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = "Bits job error";
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                if (job.Status == TransferStatus.Suspended)
                {
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendTransferStatus(job, false));
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Cancel());
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = "Bits job error";
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                if (job.Status == TransferStatus.Transferred)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.FinishBitsDownload(job));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.CheckDownloading.", ex));
            }
            return errorList;
        }

        private ErrorList FinishBitsDownload(ITransferJob job)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log(string.Format("{0} FinishBitsDownload for {1}", (object)"Bits download", (object)this.DownloadFileData.Name));
                List<ITransferItem> items;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.GetItems(out items));
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Complete());
                if (errorList.ContainsError())
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF999", string.Format("Job {0} get items or completion failed.", (object)job.ID), "This download will be canceled."));
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Cancel());
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = errorList[0].Description;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                ITransferItem transferItem = items[0];
                string str = Path.Combine(transferItem.Path, transferItem.Name);
                bool matched;
                string fileHash;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckHash(str, this.DownloadFileData.FileKey, out matched, out fileHash));
                if (!matched)
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("D996", string.Format("Download file: {0} path: {1} should have hash {2} but it hash {3}", (object)this.DownloadFileData.Name, (object)str, (object)this.DownloadFileData.FileKey, (object)fileHash), "This download failed and will need to be restarted."));
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = errorList[0].Description;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    File.Delete(str);
                    return errorList;
                }
                LogHelper.Instance.Log("Download file: {0} matched the server hash of {1}.", (object)this.DownloadFileData.Name, (object)this.DownloadFileData.FileKey);
                string downloadFileDestPath = this.GetDownloadFileDestPath();
                try
                {
                    if (File.Exists(downloadFileDestPath))
                        File.Delete(downloadFileDestPath);
                    File.Move(str, downloadFileDestPath);
                    LogHelper.Instance.Log("Download File {0} succeeded and copied to {1}.", (object)this.DownloadFileData.Name, (object)downloadFileDestPath);
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PendingInstall;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                }
                catch (Exception ex)
                {
                    try
                    {
                        this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                        this.DownloadFileData.Message = ex.Message;
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                        File.Delete(str);
                    }
                    catch
                    {
                    }
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS995", "An unhandled exception occurred in DownloadFileService.FinishDownload.", ex));
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS990", "An unhandled exception occurred in DownloadFileService.FinishDownload.", ex));
            }
            finally
            {
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendTransferStatus(job, errorList.Count == 0));
            }
            return errorList;
        }

        private ErrorList CheckPostDownload()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostDownload)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            return errorList;
        }

        private ErrorList CheckPendingInstall()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PendingInstall)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            try
            {
                LogHelper.Instance.Log("Executing DownloadFileService.CheckPending");
                ServiceLocator.Instance.GetService<IUpdateService>();
                if (!this.IsValidTime())
                {
                    LogHelper.Instance.Log("BitsDownloadFile: CheckPendingInstall is skipping download file {0} until start: {1} and end time: {2} are valid.", (object)this.DownloadFileData.Name, (object)this.DownloadFileData.StartTime, (object)this.DownloadFileData.EndTime);
                    return errorList;
                }
                if (!string.IsNullOrEmpty(this.DownloadFileData.ActivateScript))
                {
                    bool scriptCompleted;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.RunScript(out scriptCompleted));
                    if (errorList.ContainsError())
                    {
                        this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                        this.DownloadFileData.Message = errorList[0].Description;
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                        return errorList;
                    }
                    if (!scriptCompleted)
                        return errorList;
                }
                this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostInstall;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF999", "An unhandled exception occurred in BitsDownloadFile.CheckPendingInstall.", ex));
            }
            return errorList;
        }

        private ErrorList CheckInstalling()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Installing)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            return errorList;
        }

        private ErrorList CheckPostInstall()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostInstall)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
            return errorList;
        }

        private ErrorList CheckComplete()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Complete)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile success", (string)null));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DeleteDownloadFile());
            return errorList;
        }

        private ErrorList CheckError()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Error)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile error", this.DownloadFileData.Message));
            Guid bitsGuid = this.DownloadFileData.BitsGuid;
            List<ITransferJob> jobs = (List<ITransferJob>)null;
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)BitsDownloadFile.GetDownloadFileBitsJobs(out jobs));
            if (!errorList.ContainsError())
            {
                ITransferJob transferJob = jobs.Where<ITransferJob>((Func<ITransferJob, bool>)(j => j.ID == this.DownloadFileData.BitsGuid)).FirstOrDefault<ITransferJob>();
                if (transferJob != null)
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)transferJob.Cancel());
            }
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DeleteDownloadFile());
            return errorList;
        }

        internal static ErrorList Cleanup()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Executing BitsDownloadFile.Cleanup");
                List<IDownloadFile> downloadFiles = DownloadFileFactory.Instance.GetDownloadFiles();
                List<ITransferJob> jobs = (List<ITransferJob>)null;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)BitsDownloadFile.GetDownloadFileBitsJobs(out jobs));
                if (errorList.ContainsError())
                    return errorList;
                foreach (ITransferJob transferJob in jobs)
                {
                    ITransferJob job = transferJob;
                    if (downloadFiles.Where<IDownloadFile>((Func<IDownloadFile, bool>)(d => d.DownloadFileData.BitsGuid == job.ID)).FirstOrDefault<IDownloadFile>() == null)
                    {
                        LogHelper.Instance.Log("DownloadFileService: Clearing bits job, guid {0} that has not download file changeset.", (object)job.ID);
                        errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)job.Cancel());
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS999", "An unhandled exception occurred in BitsDownloadFile.Cleanup.", ex));
            }
            return errorList;
        }

        private static ErrorList GetDownloadFileBitsJobs(out List<ITransferJob> jobs)
        {
            ErrorList downloadFileBitsJobs = new ErrorList();
            downloadFileBitsJobs.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)ServiceLocator.Instance.GetService<ITransferService>().GetJobs(out jobs, false));
            if (downloadFileBitsJobs.ContainsError())
            {
                jobs = new List<ITransferJob>();
                return downloadFileBitsJobs;
            }
            jobs = jobs.Where<ITransferJob>((Func<ITransferJob, bool>)(j => j.Name.StartsWith("<:~") && j.Name.EndsWith("~:>"))).ToList<ITransferJob>();
            return downloadFileBitsJobs;
        }

        private string GetDownloadFileDestPath()
        {
            return Path.Combine(this.DownloadFileData.DestinationPath, this.DownloadFileData.FileName);
        }

        private bool DownloadFileDestPathExists() => File.Exists(this.GetDownloadFileDestPath());

        private ErrorList DownloadFileDestPathCheckHash(out bool matched, out string fileHash)
        {
            ErrorList errorList = new ErrorList();
            matched = false;
            fileHash = string.Empty;
            try
            {
                string downloadFileDestPath = this.GetDownloadFileDestPath();
                if (!string.IsNullOrEmpty(this.DownloadFileData.FileKey))
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckHash(downloadFileDestPath, this.DownloadFileData.FileKey, out matched, out fileHash));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.ChangeSetDestPathCheckHash.", ex));
            }
            return errorList;
        }

        private ErrorList CheckHash(
          string filePath,
          string hash,
          out bool matched,
          out string fileHash)
        {
            ErrorList errorList = new ErrorList();
            matched = false;
            fileHash = string.Empty;
            try
            {
                using (FileStream inputStream = File.OpenRead(filePath))
                {
                    fileHash = inputStream.ToASCIISHA1Hash();
                    matched = fileHash == hash;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("BDF001", "An unhandled exception occurred in BitsDownloadFile.CheckHash.", ex));
            }
            return errorList;
        }

        private ErrorList SendTransferStatus(ITransferJob job, bool success)
        {
            double num = 0.0;
            if (job.Status == TransferStatus.Transferred)
            {
                DateTime dateTime = job.FinishTime.HasValue ? job.FinishTime.Value : DateTime.UtcNow;
                if ((dateTime - job.StartTime).TotalSeconds > 0.0)
                    num = (double)job.TotalBytesTransfered / (dateTime - job.StartTime).TotalSeconds;
            }
            var data = new
            {
                DownloadFileId = this.DownloadFileData.Id,
                Name = this.DownloadFileData.Name,
                DownloadFileType = this.DownloadFileData.DownloadFileDataType.ToString(),
                Success = success,
                TotalBytesTransfered = job.TotalBytesTransfered,
                AverageSpeedInKPS = num,
                DownloadFileState = this.DownloadFileData.DownloadFileDataState.ToString()
            };
            return this.SendStatus(data.Success ? StatusMessage.StatusMessageType.Info : StatusMessage.StatusMessageType.Error, "DownloadFile transfer status", (object)data);
        }
    }
}
