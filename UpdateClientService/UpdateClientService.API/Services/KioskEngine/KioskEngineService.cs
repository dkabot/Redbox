using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.KioskEngine
{
    public class KioskEngineService : IKioskEngineService
    {
        private const string KioskEngineUrl = "http://localhost:9002";
        private readonly IHttpService _http;
        private readonly ILogger<KioskEngineService> _logger;

        public KioskEngineService(ILogger<KioskEngineService> logger, IHttpService http)
        {
            _logger = logger;
            _http = http;
        }

        private int? KioskEngineProcessId { get; set; }

        public async Task<KioskEngineStatus> GetStatus()
        {
            if (!KioskEngineProcessId.HasValue)
                KioskEngineProcessId = await GetProcessId();
            return KioskEngineProcessId.HasValue
                ? !TryGetProcessById(KioskEngineProcessId, out var _)
                    ? KioskEngineStatus.Stopped
                    : KioskEngineStatus.Running
                : KioskEngineStatus.Unknown;
        }

        public async Task<PerformShutdownResponse> PerformShutdown(int timeoutMs, int attempts)
        {
            var response = new PerformShutdownResponse();
            try
            {
                var attempt = 0;
                while (true)
                {
                    var flag = attempt++ < attempts;
                    if (flag)
                        flag = await GetStatus() != KioskEngineStatus.Stopped;
                    if (flag)
                    {
                        var num = await PerformShutdown(timeoutMs) ? 1 : 0;
                    }
                    else
                    {
                        break;
                    }
                }

                if (await GetStatus() != KioskEngineStatus.Stopped)
                    HandleTimeout(timeoutMs, attempts, response);
            }
            catch (Exception ex)
            {
                HandleException(response, ex);
            }

            response.ProcessId = KioskEngineProcessId;
            _logger.LogInfoWithSource("Result -> " + response.ToJson(),
                "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs");
            KioskEngineProcessId = new int?();
            return response;
        }

        private async Task<bool> PerformShutdown(int timeoutMs)
        {
            var kioskEngineService = this;
            bool flag;
            using (var cts = new CancellationTokenSource(timeoutMs))
            {
                flag = await Task.Run(async () =>
                {
                    var request = kioskEngineService._http.GenerateRequest("http://localhost:9002",
                        "api/engine/shutdown", null, HttpMethod.Post);
                    var apiResponse = await kioskEngineService._http.SendRequestAsync<PerformShutdownResponse>(request,
                        callerLocation: "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs",
                        logRequest: false, logResponse: false);
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        _logger.LogErrorWithSource(
                            "Encountered an error while requesting shutdown from Kiosk Engine. Response -> " +
                            apiResponse.GetErrors(),
                            "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs");
                        return false;
                    }

                    kioskEngineService.KioskEngineProcessId = apiResponse.Response?.ProcessId;
                    while (true)
                        if (await kioskEngineService.GetStatus() != KioskEngineStatus.Stopped &&
                            !cts.IsCancellationRequested)
                            await Task.Delay(3000);
                        else
                            break;
                    return !cts.IsCancellationRequested;
                }, cts.Token);
            }

            return flag;
        }

        private async Task<int?> GetProcessId()
        {
            var process = Process.GetProcessesByName("kioskengine.exe").FirstOrDefault();
            if (process != null)
                return process.Id;
            var apiResponse = await _http.SendRequestAsync<int?>(
                _http.GenerateRequest("http://localhost:9002", "api/engine/processid", null, HttpMethod.Get), 3000,
                "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs", logRequest: false,
                logResponse: false);
            return apiResponse.IsSuccessStatusCode ? apiResponse.Response : new int?();
        }

        private static bool TryGetProcessById(int? id, out Process process)
        {
            process = null;
            if (!id.HasValue)
                return false;
            try
            {
                process = Process.GetProcessById(id.Value);
                return process != null && !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        private static void HandleTimeout(
            int timeoutMs,
            int attempts,
            PerformShutdownResponse response)
        {
            response.Error =
                string.Format(
                    "Kiosk Engine did not shutdown given the provided parameters. Timeout: {0}ms, Attempts: {1}.",
                    timeoutMs, attempts);
        }

        private static void HandleException(PerformShutdownResponse response, Exception e)
        {
            response.Error = e.Message;
        }
    }
}