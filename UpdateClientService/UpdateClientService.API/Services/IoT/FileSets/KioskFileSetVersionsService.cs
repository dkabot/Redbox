using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.IoT.FileSets
{
    public class KioskFileSetVersionsService : IKioskFileSetVersionsService
    {
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly ILogger<KioskFileSetVersionsService> _logger;
        private readonly IStateFileService _stateFileService;
        private readonly IStoreService _store;

        public KioskFileSetVersionsService(
            IIoTCommandClient iotCommandClient,
            IStoreService store,
            IStateFileService stateFileService,
            ILogger<KioskFileSetVersionsService> logger)
        {
            _iotCommandClient = iotCommandClient;
            _store = store;
            _stateFileService = stateFileService;
            _logger = logger;
        }

        public async Task<ReportFileSetVersionsResponse> ReportFileSetVersion(
            FileSetPollRequest fileSetPollRequest)
        {
            if (fileSetPollRequest != null)
                return await InnerReportFileSetVersions(fileSetPollRequest);
            _logger.LogErrorWithSource("FileSetPollRequest is null.",
                "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
            var versionsResponse = new ReportFileSetVersionsResponse();
            versionsResponse.StatusCode = HttpStatusCode.InternalServerError;
            return versionsResponse;
        }

        public async Task<ReportFileSetVersionsResponse> ReportFileSetVersions()
        {
            return await InnerReportFileSetVersions();
        }

        private async Task<ReportFileSetVersionsResponse> InnerReportFileSetVersions(
            FileSetPollRequest fileSetPollRequest = null)
        {
            var response = new ReportFileSetVersionsResponse();
            var (flag, fileSetPollRequestList) = await GetPollRequest();
            if (!flag)
            {
                _logger.LogErrorWithSource("Couldn't report file set versions",
                    "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
            }
            else
            {
                if (fileSetPollRequest != null)
                {
                    var fileSetPollRequest1 = fileSetPollRequestList.FileSetPollRequests.FirstOrDefault(x =>
                        x.FileSetId == fileSetPollRequest.FileSetId &&
                        x.FileSetRevisionId == fileSetPollRequest.FileSetRevisionId);
                    if (fileSetPollRequest1 != null)
                        fileSetPollRequestList.FileSetPollRequests.Remove(fileSetPollRequest1);
                    fileSetPollRequestList.FileSetPollRequests.Add(fileSetPollRequest);
                }

                response = await ReportFileSetVersions(fileSetPollRequestList);
            }

            return response;
        }

        private async Task<ReportFileSetVersionsResponse> ReportFileSetVersions(
            FileSetPollRequestList fileSetPollRequestList)
        {
            var response = new ReportFileSetVersionsResponse();
            if (fileSetPollRequestList == null)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource("FileSetPollRequestList is null",
                    "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
            }
            else
            {
                var tcommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Version = 3,
                    SourceId = _store.KioskId.ToString(),
                    Payload = fileSetPollRequestList,
                    Command = CommandEnum.ReportFileSetVersions
                }, new PerformIoTCommandParameters
                {
                    RequestTimeout = TimeSpan.FromSeconds(15.0),
                    IoTTopic = "$aws/rules/kioskrestcall",
                    WaitForResponse = true
                });
                if (tcommandResponse != null && tcommandResponse.StatusCode == 200 && tcommandResponse != null)
                {
                    var statusCode = tcommandResponse.Payload?.StatusCode;
                    var num = 200;
                    if ((statusCode.GetValueOrDefault() == num) & statusCode.HasValue)
                        try
                        {
                            response.ClientFileSetRevisionChangeSets =
                                JsonConvert.DeserializeObject<List<ClientFileSetRevisionChangeSet>>(
                                    tcommandResponse.Payload.Value.ToString());
                            goto label_8;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogErrorWithSource(ex, "Exception deserializing response",
                                "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
                            goto label_8;
                        }
                }

                response.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource("Unable to report FileSet revision deployment states.",
                    "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
            }

            label_8:
            return response;
        }

        private async Task<(bool success, FileSetPollRequestList)> GetPollRequest()
        {
            var all = await _stateFileService.GetAll();
            if (all.StatusCode != HttpStatusCode.OK && all.StatusCode != HttpStatusCode.NotFound)
            {
                var logger = _logger;
                if (logger != null)
                    _logger.LogErrorWithSource("Unable to get StateFiles.",
                        "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
                return (false, null);
            }

            var setPollRequestList = new FileSetPollRequestList();
            if (all.StateFiles != null)
                foreach (var stateFile in all.StateFiles)
                    setPollRequestList.FileSetPollRequests.Add(new FileSetPollRequest
                    {
                        FileSetId = stateFile.FileSetId,
                        FileSetRevisionId = stateFile.CurrentRevisionId,
                        FileSetState = stateFile.CurrentFileSetState
                    });
            return (true, setPollRequestList);
        }
    }
}