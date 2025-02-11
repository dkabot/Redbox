using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.DataUpdate
{
    public class DataUpdateService : IDataUpdateService
    {
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly ILogger<DataUpdateService> _logger;

        private readonly string DefaultDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Redbox\\Data");

        public DataUpdateService(ILogger<DataUpdateService> logger, IIoTCommandClient iotCommandClient)
        {
            _logger = logger;
            _iotCommandClient = iotCommandClient;
        }

        public async Task<GetRecordChangesResponse> GetRecordChanges(DataUpdateRequest dataUpdateRequest)
        {
            var recordChangesData = new RecordChangesData
            {
                DataUpdateRequest = dataUpdateRequest
            };
            try
            {
                while (!recordChangesData.AllPagesLoadedOrAborted)
                {
                    await SendDataUpdateRequest(recordChangesData);
                    GetDataUpdateResponse(recordChangesData);
                    await ProcessDataUpdateResponse(recordChangesData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    string.Format("Error in GetRecordChanges for Table  {0}.", dataUpdateRequest?.TableName),
                    "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            return recordChangesData.GetRecordChangesResponse;
        }

        private async Task ProcessDataUpdateResponse(
            RecordChangesData recordChangesData)
        {
            if (recordChangesData.AllPagesLoadedOrAborted)
                return;
            if (recordChangesData.CurrentDataUpdateResponse.IsFirstPage)
            {
                recordChangesData.DataUpdateResponse = recordChangesData.CurrentDataUpdateResponse;
                if (!recordChangesData.CurrentDataUpdateResponse.IsLastPage)
                    _logger.LogInfoWithSource(
                        string.Format("Response has {0} pages", recordChangesData.CurrentDataUpdateResponse?.PageCount),
                        "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
            }
            else
            {
                var recordResponses = recordChangesData.CurrentDataUpdateResponse.RecordResponses;
                if ((recordResponses != null ? recordResponses.Any() ? 1 : 0 : 0) != 0)
                    recordChangesData.DataUpdateResponse.RecordResponses.AddRange(recordChangesData
                        .CurrentDataUpdateResponse.RecordResponses);
            }

            if (recordChangesData.CurrentDataUpdateResponse.IsLastPage)
            {
                recordChangesData.DataUpdateResponse.PageCount = new int?();
                recordChangesData.DataUpdateResponse.PageNumber = new int?();
                if (await WriteRecordUpatesToFile(recordChangesData))
                    recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.OK;
                else
                    recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
                recordChangesData.AllPagesLoadedOrAborted = true;
            }
            else
            {
                var dataUpdateRequest = recordChangesData.DataUpdateRequest;
                var pageNumber = recordChangesData.CurrentDataUpdateResponse.PageNumber;
                var nullable = pageNumber.HasValue ? pageNumber.GetValueOrDefault() + 1 : new int?();
                dataUpdateRequest.PageNumber = nullable;
            }
        }

        private void GetDataUpdateResponse(
            RecordChangesData recordChangesData)
        {
            if (recordChangesData.AllPagesLoadedOrAborted)
                return;
            var tcommandResponse = recordChangesData.IoTCommandResponse;
            if ((tcommandResponse != null ? tcommandResponse.StatusCode == 200 ? 1 : 0 : 0) != 0)
            {
                try
                {
                    recordChangesData.CurrentDataUpdateResponse =
                        JsonConvert.DeserializeObject<DataUpdateResponse>(recordChangesData.IoTCommandResponse?.Payload
                            ?.Value?.ToString());
                    if (recordChangesData.CurrentDataUpdateResponse != null)
                        return;
                    recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
                    _logger.LogErrorWithSource(
                        string.Format("Unable to deserialize Iot command payload as {0} for RequestId {1}",
                            "DataUpdateResponse", recordChangesData?.DataUpdateRequest?.RequestId),
                        "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                    recordChangesData.AllPagesLoadedOrAborted = true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        string.Format("Exception while deserializing {0} for RequestId {1}", "DataUpdateResponse",
                            recordChangesData?.DataUpdateRequest?.RequestId),
                        "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                }
            }
            else
            {
                recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
                _logger.LogErrorWithSource(
                    string.Format("Iot command {0} returned statuscode {1}", CommandEnum.GetRecordChanges,
                        recordChangesData.IoTCommandResponse?.StatusCode),
                    "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                recordChangesData.AllPagesLoadedOrAborted = true;
            }
        }

        private async Task SendDataUpdateRequest(
            RecordChangesData recordChangesData)
        {
            var recordChangesData1 = recordChangesData;
            var iotCommandClient = _iotCommandClient;
            recordChangesData1.IoTCommandResponse = await iotCommandClient.PerformIoTCommand<ObjectResult>(
                new IoTCommandModel
                {
                    Version = 1,
                    RequestId = Guid.NewGuid().ToString(),
                    Command = CommandEnum.GetRecordChanges,
                    Payload = recordChangesData.DataUpdateRequest,
                    QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
                    LogResponse = false,
                    LogRequest = false
                }, new PerformIoTCommandParameters
                {
                    IoTTopic = "$aws/rules/kioskrestcall",
                    WaitForResponse = true
                });
            recordChangesData1 = null;
        }

        private async Task<bool> WriteRecordUpatesToFile(
            RecordChangesData recordChangesData)
        {
            var result = false;
            var json = (string)null;
            var filePath = GetDataUpdateResponseFileName(recordChangesData);
            await Task.Run(async () =>
            {
                try
                {
                    var flag = !string.IsNullOrEmpty(filePath);
                    if (flag)
                        flag = await CreateFilePathIfNeeded(filePath);
                    if (!flag || recordChangesData?.DataUpdateResponse == null)
                        return;
                    json = recordChangesData.DataUpdateResponse.ToJsonIndented();
                    File.WriteAllText(filePath, json);
                    recordChangesData.GetRecordChangesResponse.DataUpdateResponseFileName = filePath;
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex,
                        string.Format("Error saving configiration file {0}.  json length: {1}", filePath, json.Length),
                        "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                }
            });
            return result;
        }

        private async Task<bool> CreateFilePathIfNeeded(string filePath)
        {
            var result = true;
            await Task.Run(() =>
            {
                try
                {
                    var directoryName = Path.GetDirectoryName(filePath);
                    if (Directory.Exists(directoryName) || Directory.CreateDirectory(directoryName).Exists)
                        return;
                    result = false;
                    _logger.LogErrorWithSource("Unable to create directory " + directoryName,
                        "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception while checking/creating directory for " + filePath,
                        "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                }
            });
            return result;
        }

        private string GetDataUpdateResponseFileName(
            RecordChangesData recordChangesData)
        {
            if (recordChangesData?.DataUpdateRequest == null)
            {
                _logger.LogErrorWithSource("DataUpdateRequest must have a value.",
                    "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
                return null;
            }

            var path2 = string.Format("{0}-update-{1}.json", recordChangesData.DataUpdateRequest?.TableName,
                recordChangesData.DataUpdateRequest?.RequestId);
            var path1 = (string)null;
            var fileType = recordChangesData.DataUpdateRequest.FileType;
            if (fileType.HasValue)
            {
                var typeMappings = FilePaths.TypeMappings;
                fileType = recordChangesData.DataUpdateRequest.FileType;
                var key = (int)fileType.Value;
                ref var local = ref path1;
                if (typeMappings.TryGetValue((FileTypeEnum)key, out local))
                    goto label_5;
            }

            path1 = DefaultDataFolder;
            label_5:
            path1 = Path.Combine(path1, recordChangesData.DataUpdateRequest.TableName.ToString());
            return Path.Combine(path1, path2);
        }

        private class RecordChangesData
        {
            public GetRecordChangesResponse GetRecordChangesResponse { get; } = new GetRecordChangesResponse();

            public DataUpdateResponse DataUpdateResponse { get; set; } = new DataUpdateResponse();

            public DataUpdateResponse CurrentDataUpdateResponse { get; set; }

            public IoTCommandResponse<ObjectResult> IoTCommandResponse { get; set; }

            public DataUpdateRequest DataUpdateRequest { get; set; }

            public bool AllPagesLoadedOrAborted { get; set; }
        }
    }
}