using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.DownloadService
{
    internal class BitsDownloader : DownloaderBase
    {
        private const string DownloadTypeName = "Bits downloader";
        private const string BitsNameFormat = "<::~{0}~::>";
        private const string BitsNameLeft = "<::~";
        private const string BitsNameRight = "~::>";

        public BitsDownloader(DownloadData downloadData) : base(downloadData)
        {
        }

        public override ErrorList ProcessDownload()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadData.DownloadState == DownloadState.None)
                errorList.AddRange(this.CheckNone());
            if (this.DownloadData.DownloadState == DownloadState.Downloading)
                errorList.AddRange(this.CheckDownloading());
            if (this.DownloadData.DownloadState == DownloadState.PostDownload)
                errorList.AddRange(this.CheckPostDownload());
            if (this.DownloadData.DownloadState == DownloadState.Complete)
                errorList.AddRange(this.CheckComplete());
            if (this.DownloadData.DownloadState == DownloadState.Error)
                errorList.AddRange(this.CheckError());
            return errorList;
        }

        private TransferJobPriority GetPriority()
        {
            if (this.DownloadData.DownloadPriority == DownloadPriority.Low)
                return TransferJobPriority.Low;
            if (this.DownloadData.DownloadPriority == DownloadPriority.Normal)
                return TransferJobPriority.Normal;
            if (this.DownloadData.DownloadPriority == DownloadPriority.High)
                return TransferJobPriority.High;
            return this.DownloadData.DownloadPriority == DownloadPriority.Foreground ? TransferJobPriority.Foreground : TransferJobPriority.Low;
        }

        private ErrorList CheckNone()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadData.DownloadState != DownloadState.None)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits downloader", (object)this.DownloadData.DownloadState.ToString(), (object)this.DownloadData.Key));
            try
            {
                ITransferJob job = (ITransferJob)null;
                ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
                string name = string.Format("<::~{0}~::>", (object)this.DownloadData.Key);
                errorList.AddRange(service.CreateDownloadJob(name, out job));
                errorList.AddRange(job.SetMinimumRetryDelay(60U));
                errorList.AddRange(job.SetNoProgressTimeout(0U));
                errorList.AddRange(job.SetPriority(this.GetPriority()));
                errorList.AddRange(job.AddItem(this.DownloadData.Url, Path.GetTempFileName()));
                if (errorList.ContainsError())
                {
                    errorList.AddRange(job.Cancel());
                    if (this.DownloadData.RetryCount == 10)
                        this.DownloadData.DownloadState = DownloadState.Error;
                    else
                        this.DownloadData.DownloadState = DownloadState.None;
                    this.DownloadData.Message = errorList[0].Description;
                    ++this.DownloadData.RetryCount;
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                errorList.AddRange(job.Resume());
                if (errorList.ContainsError())
                {
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = errorList[0].Description;
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                this.DownloadData.BitsGuid = job.ID;
                this.DownloadData.DownloadState = DownloadState.Downloading;
                errorList.AddRange(this.SaveDownload());
                LogHelper.Instance.Log("Download Name: {0} has been put on the download queue.", (object)this.DownloadData.Key);
                LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", (object)name, (object)job.ID);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), "An unhandled exception occurred in CheckNone", ex));
            }
            return errorList;
        }

        private ErrorList CheckDownloading()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadData.DownloadState != DownloadState.Downloading)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits downloader", (object)this.DownloadData.DownloadState.ToString(), (object)this.DownloadData.Key));
            try
            {
                ServiceLocator.Instance.GetService<ITransferService>();
                List<ITransferJob> jobs = (List<ITransferJob>)null;
                errorList.AddRange(BitsDownloader.GetDownloadFileBitsJobs(out jobs));
                if (errorList.ContainsError())
                {
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = errorList[0].Description;
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                ITransferJob job = jobs.Where<ITransferJob>((Func<ITransferJob, bool>)(j => j.ID == this.DownloadData.BitsGuid)).FirstOrDefault<ITransferJob>();
                if (job == null)
                {
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = "Bits job missing";
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                if (job.Status == TransferStatus.Error)
                {
                    errorList.AddRange(job.Cancel());
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = "Bits job error";
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                if (job.Status == TransferStatus.Suspended)
                {
                    errorList.AddRange(job.Cancel());
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = "Bits job error";
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                if (job.Status == TransferStatus.Transferred)
                    errorList.AddRange(this.FinishBitsDownload(job));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), "An unhandled exception occurred in CheckDownloading.", ex));
            }
            return errorList;
        }

        private ErrorList FinishBitsDownload(ITransferJob job)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log(string.Format("{0} FinishBitsDownload for {1}", (object)"Bits downloader", (object)this.DownloadData.Key));
                List<ITransferItem> items;
                errorList.AddRange(job.GetItems(out items));
                errorList.AddRange(job.Complete());
                if (errorList.ContainsError())
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), string.Format("Job {0} get items or completion failed.", (object)job.ID), "This download will be canceled."));
                    errorList.AddRange(job.Cancel());
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = errorList[0].Description;
                    errorList.AddRange(this.SaveDownload());
                    return errorList;
                }
                ITransferItem transferItem = items[0];
                string str = Path.Combine(transferItem.Path, transferItem.Name);
                bool matched;
                string fileHash;
                errorList.AddRange(this.CheckHash(str, this.DownloadData.Hash, out matched, out fileHash));
                if (!matched)
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), string.Format("Download file: {0} path: {1} should have hash {2} but it hash {3}", (object)this.DownloadData.Key, (object)str, (object)this.DownloadData.Hash, (object)fileHash), "This download failed and will need to be restarted."));
                    this.DownloadData.DownloadState = DownloadState.Error;
                    this.DownloadData.Message = errorList[0].Description;
                    errorList.AddRange(this.SaveDownload());
                    File.Delete(str);
                    return errorList;
                }
                this.DownloadData.Path = str;
                this.DownloadData.DownloadState = DownloadState.PostDownload;
                errorList.AddRange(this.SaveDownload());
                LogHelper.Instance.Log("Download file: {0} matched the server hash of {1}.", (object)this.DownloadData.Key, (object)this.DownloadData.Hash);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), "An unhandled exception occurred in FinishBitsDownload.", ex));
            }
            return errorList;
        }

        private ErrorList CheckPostDownload()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadData.DownloadState != DownloadState.PostDownload)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits downloader", (object)this.DownloadData.DownloadState.ToString(), (object)this.DownloadData.Key));
            return errorList;
        }

        private ErrorList CheckComplete()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadData.DownloadState != DownloadState.Complete)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits downloader", (object)this.DownloadData.DownloadState.ToString(), (object)this.DownloadData.Key));
            errorList.AddRange(this.DeleteDownload());
            return errorList;
        }

        private ErrorList CheckError()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadData.DownloadState != DownloadState.Error)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Bits downloader", (object)this.DownloadData.DownloadState.ToString(), (object)this.DownloadData.Key));
            Guid bitsGuid = this.DownloadData.BitsGuid;
            List<ITransferJob> jobs = (List<ITransferJob>)null;
            errorList.AddRange(BitsDownloader.GetDownloadFileBitsJobs(out jobs));
            if (!errorList.ContainsError())
            {
                ITransferJob transferJob = jobs.Where<ITransferJob>((Func<ITransferJob, bool>)(j => j.ID == this.DownloadData.BitsGuid)).FirstOrDefault<ITransferJob>();
                if (transferJob != null)
                    errorList.AddRange(transferJob.Cancel());
            }
            errorList.AddRange(this.DeleteDownload());
            return errorList;
        }

        internal static ErrorList Cleanup()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Executing BitsDownloader.Cleanup");
                List<IDownloader> downloads = DownloadFactory.Instance.GetDownloads();
                List<ITransferJob> jobs = (List<ITransferJob>)null;
                errorList.AddRange(BitsDownloader.GetDownloadFileBitsJobs(out jobs));
                if (errorList.ContainsError())
                    return errorList;
                foreach (ITransferJob transferJob in jobs)
                {
                    ITransferJob job = transferJob;
                    if (downloads.Where<IDownloader>((Func<IDownloader, bool>)(d => d.DownloadData.BitsGuid == job.ID)).FirstOrDefault<IDownloader>() == null)
                    {
                        LogHelper.Instance.Log("BitsDownloader: Clearing bits job, guid {0} that has no download file changeset.", (object)job.ID);
                        errorList.AddRange(job.Cancel());
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), "An unhandled exception occurred in BitsDownloadFile.Cleanup.", ex));
            }
            return errorList;
        }

        private static ErrorList GetDownloadFileBitsJobs(out List<ITransferJob> jobs)
        {
            ErrorList downloadFileBitsJobs = new ErrorList();
            downloadFileBitsJobs.AddRange(ServiceLocator.Instance.GetService<ITransferService>().GetJobs(out jobs, false));
            if (downloadFileBitsJobs.ContainsError())
            {
                jobs = new List<ITransferJob>();
                return downloadFileBitsJobs;
            }
            jobs = jobs.Where<ITransferJob>((Func<ITransferJob, bool>)(j => j.Name.StartsWith("<::~") && j.Name.EndsWith("~::>"))).ToList<ITransferJob>();
            return downloadFileBitsJobs;
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
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(BitsDownloader), "An unhandled exception occurred in CheckHash.", ex));
            }
            return errorList;
        }
    }
}
