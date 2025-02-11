using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class DeployFileSet : ICommandIoTController
    {
        private readonly ILogger<DeployFileSet> _logger;
        private readonly IFileSetService _service;

        public DeployFileSet(ILogger<DeployFileSet> logger, IFileSetService service)
        {
            _logger = logger;
            _service = service;
        }

        public CommandEnum CommandEnum => CommandEnum.DeployFileSet;

        public int Version => 1;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            try
            {
                var errorList = new List<Error>();
                var changeSetResponse = await _service.ProcessChangeSet(
                    JsonConvert.DeserializeObject<ClientFileSetRevisionChangeSet>(ioTCommand.Payload.ToJson()));
            }
            catch (Exception ex)
            {
                var logger = _logger;
                var exception = ex;
                var ioTcommandModel = ioTCommand;
                var str = "Exception while executing command " +
                          (ioTcommandModel != null ? ioTcommandModel.ToJson() : null);
                _logger.LogErrorWithSource(exception, str,
                    "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/DeployFileSet.cs");
            }
        }
    }
}