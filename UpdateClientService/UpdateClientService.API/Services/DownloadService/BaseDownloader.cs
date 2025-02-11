using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.DownloadService
{
    public class BaseDownloader : IDownloader, IPersistentData
    {
        private readonly ILogger<BaseDownloader> _logger;
        private readonly IPersistentDataCacheService _persistentDataCacheService;

        public BaseDownloader(IPersistentDataCacheService cache, ILogger<BaseDownloader> logger)
        {
            _logger = logger;
            _persistentDataCacheService = cache;
        }

        public virtual async Task<bool> ProcessDownload(DownloadData downloadData)
        {
            return true;
        }

        public virtual async Task<bool> SaveDownload(DownloadData downloadData)
        {
            bool flag;
            try
            {
                flag = await _persistentDataCacheService.Write(downloadData, downloadData.FileName,
                    UpdateClientServiceConstants.DownloadDataFolder);
                if (!flag)
                {
                    var logger = _logger;
                    var downloadData1 = downloadData;
                    var str = "Unable to save DownloadData " + (downloadData1 != null ? downloadData1.ToJson() : null);
                    _logger.LogErrorWithSource(str,
                        "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
                }
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var downloadData2 = downloadData;
                var str = "Exception while saving DownloadData " +
                          (downloadData2 != null ? downloadData2.ToJson() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
                flag = false;
            }

            return flag;
        }

        public virtual async Task<bool> DeleteDownload(DownloadData downloadData)
        {
            bool flag;
            try
            {
                flag = await _persistentDataCacheService.Delete(downloadData.FileName,
                    UpdateClientServiceConstants.DownloadDataFolder);
                if (!flag)
                    _logger.LogErrorWithSource("Unable to delete DownloadData with key: " + downloadData?.Key,
                        "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
            }
            catch (Exception ex)
            {
                flag = false;
                _logger.LogErrorWithSource(ex, "Exception while deleting DownloadData with key " + downloadData?.Key,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
            }

            return flag;
        }

        public virtual async Task<bool> Complete(DownloadData downloadData)
        {
            var success = true;
            try
            {
                downloadData.DownloadState = DownloadState.Complete;
                _logger.LogInfoWithSource(
                    string.Format("Setting DownloadState = {0} for DownloadData {1}", downloadData.DownloadState,
                        downloadData.FileName),
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
                var num = await SaveDownload(downloadData) ? 1 : 0;
            }
            catch (Exception ex)
            {
                success = false;
                _logger.LogErrorWithSource(ex, "Exception while completing DownloadData with key: " + downloadData.Key,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
            }

            return success;
        }

        public virtual bool Cleanup(DownloadDataList downloadDataList)
        {
            return true;
        }
    }
}