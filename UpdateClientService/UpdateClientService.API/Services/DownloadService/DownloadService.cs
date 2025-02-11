using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.DownloadService.Responses;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.DownloadService
{
    public class DownloadService : IDownloadService, IInvocable
    {
        public const string DownloadExtension = ".dldat";
        private readonly IPersistentDataCacheService _cache;
        private readonly IDownloader _downloader;
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly ILogger<DownloadService> _logger;
        private readonly TimeSpan _proxiedS3UrlRequestTimeout = TimeSpan.FromSeconds(60.0);

        public DownloadService(
            ILogger<DownloadService> logger,
            IPersistentDataCacheService cache,
            IDownloader downloader,
            IIoTCommandClient iotCommandClient)
        {
            _logger = logger;
            _cache = cache;
            _downloader = downloader;
            _iotCommandClient = iotCommandClient;
        }

        private static SemaphoreSlim _lock => new SemaphoreSlim(1, 1);

        public async Task Invoke()
        {
            try
            {
                if (!await _lock.WaitAsync(0))
                {
                    _logger.LogWarningWithSource("DownloadService is already being invoked. Skipping...",
                        "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Could not acquire lock",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                return;
            }

            try
            {
                var num = await ProcessDownloads() ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "An unhandled exception occurred.",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
            }
            finally
            {
                var semaphoreSlim = _lock;
                if ((semaphoreSlim != null ? semaphoreSlim.CurrentCount == 0 ? 1 : 0 : 0) != 0)
                    _lock.Release();
            }
        }

        public async Task<GetDownloadStatusesResponse> GetDownloadsResponse(string pattern = null)
        {
            try
            {
                var downloadsResponse = new GetDownloadStatusesResponse();
                var statusesResponse = downloadsResponse;
                statusesResponse.Statuses = await GetDownloads(null, pattern);
                return downloadsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting DownloadDatas for pattern " + pattern + ")",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                var downloadsResponse = new GetDownloadStatusesResponse();
                downloadsResponse.StatusCode = HttpStatusCode.InternalServerError;
                return downloadsResponse;
            }
        }

        public async Task<GetDownloadStatusResponse> GetDownloadResponse(string key)
        {
            try
            {
                var downloads = await GetDownloads();
                var downloadData = downloads != null ? downloads.FirstOrDefault(x => x.Key == key) : null;
                var downloadResponse = new GetDownloadStatusResponse();
                downloadResponse.Status = downloadData;
                downloadResponse.StatusCode = downloadData != null ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                return downloadResponse;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while getting DownloadData for key " + key + ")",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                var downloadResponse = new GetDownloadStatusResponse();
                downloadResponse.StatusCode = HttpStatusCode.InternalServerError;
                return downloadResponse;
            }
        }

        public async Task<DownloadDataList> GetDownloads(string pattern = null)
        {
            return await GetDownloads(null, pattern);
        }

        public async Task<DownloadDataList> GetDownloads(Regex pattern)
        {
            return await GetDownloads(pattern, null);
        }

        public async Task<GetFileResponse> AddDownload(
            string hash,
            string url,
            DownloadPriority priority,
            bool completeOnFinish = false)
        {
            var response = new GetFileResponse();
            try
            {
                if (!await AddDownload(Guid.NewGuid().ToString(), hash, url, priority, completeOnFinish))
                    response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while adding download for hash " + hash + ", url " + url,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
            }

            return response;
        }

        public async Task<bool> AddDownload(
            string key,
            string hash,
            string url,
            DownloadPriority priority,
            bool completeOnFinish = false)
        {
            var (flag, _) = await AddRetrieveDownload(key, hash, url, priority, completeOnFinish);
            return flag;
        }

        public async Task<(bool success, DownloadData downloadData)> AddRetrieveDownload(
            string key,
            string hash,
            string url,
            DownloadPriority priority,
            bool completeOnFinish)
        {
            var downloadData = (DownloadData)null;
            _logger.LogInfoWithSource(
                string.Format("Adding download key: {0} hash: {1} url: {2} priority: {3}", key, hash, url, priority),
                "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
            bool flag;
            try
            {
                downloadData = DownloadData.Initialize(key, hash, url, priority, completeOnFinish);
                flag = await _downloader.SaveDownload(downloadData);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Exception while adding download for {0} hash: {1} url: {2} priority: {3}", key, hash,
                        url, priority), "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                flag = false;
            }

            return (flag, downloadData);
        }

        public async Task<DeleteDownloadResponse> CancelDownload(string key)
        {
            var response = new DeleteDownloadResponse();
            _logger.LogInfoWithSource("Deleting DownloadData with key: " + key,
                "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
            try
            {
                var downloadData = await GetDownloadData(key);
                var flag = downloadData != null;
                if (flag)
                    flag = !await _downloader.DeleteDownload(downloadData);
                if (flag)
                    response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while canceling download with key " + key,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<CompleteDownloadResponse> CompleteDownload(string key)
        {
            var response = new CompleteDownloadResponse();
            _logger.LogInfoWithSource("Completing download matching key: " + key,
                "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
            try
            {
                var downloadData = await GetDownloadData(key);
                if (downloadData != null)
                {
                    if (!await _downloader.Complete(downloadData))
                        response.StatusCode = HttpStatusCode.InternalServerError;
                }
                else
                {
                    _logger.LogErrorWithSource("No download found matching key: " + key,
                        "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                    response.StatusCode = HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while completing Download with key " + key,
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<string> GetProxiedS3Url(string key, DownloadPathType type, bool isHead = false)
        {
            var tcommandParameters = new PerformIoTCommandParameters
            {
                RequestTimeout = _proxiedS3UrlRequestTimeout,
                WaitForResponse = true
            };
            if (!Debugger.IsAttached)
                tcommandParameters.IoTTopic = "$aws/rules/kioskrestcall";
            var iotCommandClient = _iotCommandClient;
            var request = new IoTCommandModel();
            request.Version = 2;
            request.Command = CommandEnum.GetPresignedS3Url;
            request.Payload = new PresignedS3UrlRequest
            {
                Key = key,
                Type = type,
                IsHead = isHead
            };
            var parameters = tcommandParameters;
            var uriString = await iotCommandClient.PerformIoTCommandWithStringResult(request, parameters);
            if (string.IsNullOrEmpty(uriString) || !Uri.TryCreate(uriString, UriKind.Absolute, out var _))
            {
                _logger.LogErrorWithSource("Url '" + uriString + "' is not a valid Url",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                uriString = null;
            }

            return uriString;
        }

        private async Task<DownloadDataList> GetDownloads(Regex regexPattern, string stringPattern)
        {
            var result = new DownloadDataList();
            try
            {
                List<PersistentDataWrapper<DownloadData>> source;
                if (regexPattern != null)
                    source = await _cache.ReadLike<DownloadData>(regexPattern,
                        UpdateClientServiceConstants.DownloadDataFolder, false);
                else
                    source = await _cache.ReadLike<DownloadData>(
                        string.IsNullOrWhiteSpace(stringPattern)
                            ? DownloadDataConstants.DownloadExtension
                            : stringPattern, UpdateClientServiceConstants.DownloadDataFolder, false);
                result.AddRange(source.Where(d => d?.Data != null).Select(d => d.Data).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogCriticalWithSource(ex, "An unhandled exception occurred",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
            }

            return result;
        }

        private async Task<DownloadData> GetDownloadData(string key)
        {
            return (await GetDownloads()).Where(d => d.Key == key).FirstOrDefault();
        }

        private async Task<bool> ProcessDownloads()
        {
            var result = true;
            try
            {
                var downloadDataList = await GetDownloads();
                var downloadDataList1 = downloadDataList;
                if ((downloadDataList1 != null ? downloadDataList1.Count > 0 ? 1 : 0 : 0) != 0)
                    _logger.LogInfoWithSource(string.Format("Start processing {0} downloads", downloadDataList.Count),
                        "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                foreach (var downloadData in downloadDataList)
                {
                    var flag = result;
                    result = flag & await _downloader.ProcessDownload(downloadData);
                }

                result &= _downloader.Cleanup(downloadDataList);
                var downloadDataList2 = downloadDataList;
                if ((downloadDataList2 != null ? downloadDataList2.Count > 0 ? 1 : 0 : 0) != 0)
                    _logger.LogInfoWithSource("Finished executing",
                        "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                downloadDataList = null;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while processing downloads",
                    "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
                result = false;
            }

            return result;
        }
    }
}