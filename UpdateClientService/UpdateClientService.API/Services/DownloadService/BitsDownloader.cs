using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.Transfer;

namespace UpdateClientService.API.Services.DownloadService
{
    public class BitsDownloader : BaseDownloader
    {
        private const string DownloadTypeName = "Bits downloader";
        private const string BitsNameFormat = "<UCS::~{0}~::UCS>";
        private const string BitsNameLeft = "<UCS::~";
        private const string BitsNameRight = "~::UCS>";
        private readonly ILogger<BitsDownloader> _logger;
        private readonly ITransferService _transferService;

        public BitsDownloader(
            ILogger<BitsDownloader> logger,
            ITransferService transferService,
            IPersistentDataCacheService cache)
            : base(cache, logger)
        {
            _logger = logger;
            _transferService = transferService;
        }

        public override async Task<bool> ProcessDownload(DownloadData downloadData)
        {
            var bitsDownloader = this;
            var processDownloadDataInfo = new ProcessDownloadDataInfo
            {
                BitsDownloader = bitsDownloader,
                DownloadData = downloadData
            };
            bitsDownloader.SetBitTransferStatus(processDownloadDataInfo.DownloadData);
            await bitsDownloader.CheckNone(processDownloadDataInfo);
            await bitsDownloader.CheckDownloading(processDownloadDataInfo);
            await bitsDownloader.CheckPostDownload(processDownloadDataInfo);
            await bitsDownloader.CheckComplete(processDownloadDataInfo);
            await bitsDownloader.CheckError(processDownloadDataInfo);
            return processDownloadDataInfo.Success;
        }

        public override bool Cleanup(DownloadDataList downloadList)
        {
            bool flag;
            try
            {
                List<ITransferJob> jobs;
                if (!GetDownloadFileBitsJobs(out jobs))
                    return false;
                var source = new List<Error>();
                foreach (var transferJob in jobs)
                    if (downloadList.GetByBitsGuid(transferJob.ID) == null)
                    {
                        _logger.LogInfoWithSource(
                            string.Format("Clearing bits job, guid {0} that has no download file changeset.",
                                transferJob.ID),
                            "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                        source.AddRange(transferJob.Cancel());
                    }

                flag = !source.Any();
            }
            catch (Exception ex)
            {
                flag = false;
                _logger.LogErrorWithSource(ex, "Exception while cleaning up DownloadDataList",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            }

            return flag;
        }

        private void SetBitTransferStatus(DownloadData downloadData)
        {
            if (downloadData.BitsGuid == new Guid() || downloadData.DownloadState == DownloadState.Error)
                return;
            if (downloadData.DownloadState == DownloadState.PostDownload)
            {
                downloadData.BitsTransferred = downloadData.BitsTotal;
            }
            else
            {
                ITransferJob job;
                _transferService.GetJob(downloadData.BitsGuid, out job);
                if (job == null)
                    _logger.LogInfoWithSource("Job was null for " + downloadData.ToJson(),
                        "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                else
                    try
                    {
                        downloadData.BitsTotal = job.TotalBytes == ulong.MaxValue
                            ? long.MaxValue
                            : Convert.ToInt64(job.TotalBytes);
                        downloadData.BitsTransferred = Convert.ToInt64(job.TotalBytesTransfered);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex, "Something went wrong while updating transfer status.",
                            "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                    }
            }
        }

        private TransferJobPriority GetJobPriority(DownloadData downloadData)
        {
            switch (downloadData.DownloadPriority)
            {
                case DownloadPriority.Normal:
                    return TransferJobPriority.Normal;
                case DownloadPriority.High:
                    return TransferJobPriority.High;
                case DownloadPriority.Foreground:
                    return TransferJobPriority.Foreground;
                default:
                    return TransferJobPriority.Low;
            }
        }

        private async Task CheckNone(
            ProcessDownloadDataInfo processDownloadDataInfo)
        {
            var bitsDownloader = this;
            var downloadData = processDownloadDataInfo.DownloadData;
            if (downloadData.DownloadState != DownloadState.None)
                return;
            _logger.LogInfoWithSource(
                string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState,
                    downloadData.Key), "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            try
            {
                var source = new List<Error>();
                var name = string.Format("<UCS::~{0}~::UCS>", downloadData.Key);
                ITransferJob job;
                source.AddRange(bitsDownloader._transferService.CreateDownloadJob(name, out job));
                source.AddRange(job.SetMinimumRetryDelay(60U));
                source.AddRange(job.SetNoProgressTimeout(0U));
                source.AddRange(job.SetPriority(bitsDownloader.GetJobPriority(downloadData)));
                source.AddRange(job.AddItem(downloadData.Url, Path.GetTempFileName()));
                if (source.Any())
                {
                    processDownloadDataInfo.Success = false;
                    source.AddRange(job.Cancel());
                    if (downloadData.RetryCount == 10)
                    {
                        downloadData.DownloadState = DownloadState.Error;
                        source.Add(new Error
                        {
                            Message = string.Format("Retry limit exceeded for {0}", job.ID)
                        });
                    }
                    else
                    {
                        downloadData.DownloadState = DownloadState.None;
                    }

                    downloadData.Message = string.Join(",",
                        source.Where(e => !string.IsNullOrWhiteSpace(e.Message)).Select(e => e.Message));
                    ++downloadData.RetryCount;
                    var num = await bitsDownloader.SaveDownload(downloadData) ? 1 : 0;
                }
                else
                {
                    source.AddRange(job.Resume());
                    if (source.Any())
                    {
                        var num1 = await processDownloadDataInfo.Update(DownloadState.Error, source[0].Message) ? 1 : 0;
                    }
                    else
                    {
                        downloadData.BitsGuid = job.ID;
                        var num2 = await processDownloadDataInfo.Update(DownloadState.Downloading) ? 1 : 0;
                        _logger.LogInfoWithSource(
                            string.Format(
                                "Download Name: {0} has been put on the download queue. BITS job started with name: {1} and guid: {2}.",
                                downloadData.Key, name, job.ID),
                            "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                        name = null;
                        job = null;
                    }
                }
            }
            catch (Exception ex)
            {
                processDownloadDataInfo.Success = false;
                var logger = bitsDownloader._logger;
                var exception = ex;
                var downloadDataInfo = processDownloadDataInfo;
                string str1;
                if (downloadDataInfo == null)
                {
                    str1 = null;
                }
                else
                {
                    var downloadData1 = downloadDataInfo.DownloadData;
                    str1 = downloadData1 != null ? downloadData1.ToJson() : null;
                }

                var str2 = "Exception while checking status None for DownloadData " + str1;
                _logger.LogErrorWithSource(exception, str2,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            }
        }

        private async Task<bool> UpdateDownloadData(
            DownloadData downloadData,
            DownloadState? downloadState,
            string errorMessage = null)
        {
            var bitsDownloader = this;
            if (downloadState.HasValue)
            {
                _logger.LogInfoWithSource(
                    string.Format("Setting DownloadState = {0} for DownloadData {1}", downloadState,
                        downloadData.FileName),
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                downloadData.DownloadState = downloadState.Value;
            }

            if (errorMessage != null)
                downloadData.Message = errorMessage;
            return await bitsDownloader.SaveDownload(downloadData);
        }

        private async Task CheckDownloading(
            ProcessDownloadDataInfo processDownloadDataInfo)
        {
            var downloadData = processDownloadDataInfo.DownloadData;
            if (downloadData.DownloadState != DownloadState.Downloading)
                return;
            _logger.LogInfoWithSource(
                string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState,
                    downloadData.Key), "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            try
            {
                List<ITransferJob> jobs;
                if (!GetDownloadFileBitsJobs(out jobs))
                {
                    var num1 = await processDownloadDataInfo.Update(DownloadState.Error, "Error getting Jobs list")
                        ? 1
                        : 0;
                }
                else
                {
                    var job = jobs.Where(j => j.ID == downloadData.BitsGuid).FirstOrDefault();
                    if (job == null)
                    {
                        var num2 = await processDownloadDataInfo.Update(DownloadState.Error,
                            "Bits job missing for key: " + downloadData.Key)
                            ? 1
                            : 0;
                    }
                    else if (job.Status == TransferStatus.Error)
                    {
                        job.Cancel();
                        var num3 = await processDownloadDataInfo.Update(DownloadState.Error,
                            "Bits job error for key: " + downloadData.Key)
                            ? 1
                            : 0;
                    }
                    else if (job.Status == TransferStatus.Suspended)
                    {
                        job.Cancel();
                        var num4 = await processDownloadDataInfo.Update(DownloadState.Error,
                            "Bits job suspended for key: " + downloadData.Key)
                            ? 1
                            : 0;
                    }
                    else
                    {
                        if (job.Status == TransferStatus.TransientError)
                        {
                            _logger.LogInfoWithSource("BITS job status is TransientError for key: " + downloadData.Key,
                                "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                            processDownloadDataInfo.Success = false;
                        }

                        if (job.Status != TransferStatus.Transferred)
                            return;
                        await FinishBitsDownload(job, processDownloadDataInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                processDownloadDataInfo.Success = false;
                var logger = _logger;
                var exception = ex;
                var local = DownloadState.Downloading;
                var downloadDataInfo = processDownloadDataInfo;
                string str1;
                if (downloadDataInfo == null)
                {
                    str1 = null;
                }
                else
                {
                    var downloadData1 = downloadDataInfo.DownloadData;
                    str1 = downloadData1 != null ? downloadData1.ToJson() : null;
                }

                var str2 = string.Format("Exception while checking status {0} for DownloadData {1}", local, str1);
                _logger.LogErrorWithSource(exception, str2,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            }
        }

        private async Task FinishBitsDownload(
            ITransferJob job,
            ProcessDownloadDataInfo processDownloadDataInfo)
        {
            var downloadData = processDownloadDataInfo.DownloadData;
            try
            {
                _logger.LogInfoWithSource("Bits downloader FinishBitsDownload for " + downloadData.Key,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                var source = new List<Error>();
                List<ITransferItem> items;
                source.AddRange(job.GetItems(out items));
                source.AddRange(job.Complete());
                if (source.Any())
                {
                    job.Cancel();
                    var num = await processDownloadDataInfo.Update(DownloadState.Error,
                        string.Format(
                            "BitsDownloader -> Job {0} get items or completion failed. This download will be canceled.",
                            job.ID))
                        ? 1
                        : 0;
                }
                else
                {
                    var transferItem = items[0];
                    var downloadPath = Path.Combine(transferItem.Path, transferItem.Name);
                    string fileHash;
                    var flag = CheckHash(downloadPath, downloadData.Hash, out fileHash);
                    if (!string.IsNullOrWhiteSpace(downloadData.Hash) && !flag)
                    {
                        var num = await processDownloadDataInfo.Update(DownloadState.Error,
                            "BitsDownloader -> Download file: " + downloadData.Key + " path: " + downloadPath +
                            " should have hash " + downloadData.Hash + " but it has hash " + fileHash +
                            ". This download failed and will need to be restarted.")
                            ? 1
                            : 0;
                        File.Delete(downloadPath);
                    }
                    else
                    {
                        downloadData.Path = downloadPath;
                        var num = await processDownloadDataInfo.Update(DownloadState.PostDownload) ? 1 : 0;
                        if (string.IsNullOrWhiteSpace(downloadData.Hash))
                            _logger.LogInfoWithSource(
                                "Download file: " + downloadData.Key +
                                " was not provided with a hash to compare against. Ignoring check.",
                                "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                        downloadPath = null;
                    }
                }
            }
            catch (Exception ex)
            {
                processDownloadDataInfo.Success = false;
                _logger.LogErrorWithSource(ex, "Exception while finishing Bits download for " + downloadData.Key,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            }
        }

        private async Task CheckPostDownload(
            ProcessDownloadDataInfo processDownloadDataInfo)
        {
            var bitsDownloader = this;
            var downloadData = processDownloadDataInfo.DownloadData;
            if (downloadData.DownloadState != DownloadState.PostDownload)
                return;
            _logger.LogInfoWithSource(
                string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState,
                    downloadData.Key), "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            if (!downloadData.CompleteOnFinish)
                return;
            _logger.LogInfoWithSource("CompleteOnFinish for " + downloadData.FileName,
                "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            var downloadDataInfo1 = processDownloadDataInfo;
            var downloadDataInfo = downloadDataInfo1;
            var success = downloadDataInfo1.Success;
            downloadDataInfo.Success = success & await bitsDownloader.Complete(downloadData);
            downloadDataInfo = null;
        }

        private async Task CheckComplete(
            ProcessDownloadDataInfo processDownloadDataInfo)
        {
            var bitsDownloader = this;
            var downloadData = processDownloadDataInfo.DownloadData;
            if (downloadData.DownloadState != DownloadState.Complete)
                return;
            _logger.LogInfoWithSource(
                string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState,
                    downloadData.Key), "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            var downloadDataInfo1 = processDownloadDataInfo;
            var downloadDataInfo = downloadDataInfo1;
            var success = downloadDataInfo1.Success;
            downloadDataInfo.Success = success & await bitsDownloader.DeleteDownload(downloadData);
            downloadDataInfo = null;
        }

        private async Task CheckError(
            ProcessDownloadDataInfo processDownloadDataInfo)
        {
            var bitsDownloader = this;
            var downloadData = processDownloadDataInfo.DownloadData;
            if (downloadData.DownloadState != DownloadState.Error)
                return;
            _logger.LogInfoWithSource(
                string.Format("{0} checking state: {1} for {2}", "Bits downloader", downloadData.DownloadState,
                    downloadData.Key), "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            var bitsGuid = downloadData.BitsGuid;
            var source = new List<Error>();
            List<ITransferJob> jobs;
            if (bitsDownloader.GetDownloadFileBitsJobs(out jobs))
            {
                var transferJob = jobs.Where(j => j.ID == downloadData.BitsGuid).FirstOrDefault();
                if (transferJob != null)
                {
                    source.AddRange(transferJob.GetErrors());
                    var str = string.Join(", ", source.Select(x => x.Message));
                    _logger.LogWarningWithSource(
                        "Bits job error message for DownloadData " + downloadData.Key + ": " + str,
                        "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
                    source.AddRange(transferJob.Cancel());
                }
            }

            processDownloadDataInfo.Success &= !source.Any();
            var downloadDataInfo1 = processDownloadDataInfo;
            var downloadDataInfo = downloadDataInfo1;
            var success = downloadDataInfo1.Success;
            downloadDataInfo.Success = success & await bitsDownloader.DeleteDownload(downloadData);
            downloadDataInfo = null;
        }

        private bool GetDownloadFileBitsJobs(out List<ITransferJob> jobs)
        {
            var source = new List<Error>();
            source.AddRange(_transferService.GetJobs(out jobs, false));
            jobs = !source.Any()
                ? jobs.Where(j => j.Name.StartsWith("<UCS::~") && j.Name.EndsWith("~::UCS>")).ToList()
                : new List<ITransferJob>();
            return !source.Any();
        }

        private bool CheckHash(string filePath, string hash, out string fileHash)
        {
            var flag = false;
            fileHash = string.Empty;
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    fileHash = fileStream.GetSHA1Hash();
                    flag = fileHash == hash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while Checking Hash for " + filePath,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
            }

            return flag;
        }

        private class ProcessDownloadDataInfo
        {
            public bool Success { get; set; } = true;

            public DownloadData DownloadData { get; set; }

            public BitsDownloader BitsDownloader { get; set; }

            public async Task<bool> Update(DownloadState? downloadState = null, string errorMessage = null)
            {
                Success &= string.IsNullOrEmpty(errorMessage);
                var flag = await BitsDownloader.UpdateDownloadData(DownloadData, downloadState, errorMessage);
                Success &= flag;
                return flag;
            }
        }
    }
}