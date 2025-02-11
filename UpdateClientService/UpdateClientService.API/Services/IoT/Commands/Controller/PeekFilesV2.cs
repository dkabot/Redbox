using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class PeekFilesV2 : ICommandIoTController
    {
        private readonly IIoTCommandClient _iotCommandClient;
        private readonly IKioskFilesService _kioskFilesService;
        private readonly IMqttProxy _mqttRepo;
        private readonly IStoreService _store;

        public PeekFilesV2(
            IKioskFilesService kioskFilesService,
            IMqttProxy mqttRepo,
            IIoTCommandClient iotCommandClient,
            IStoreService store)
        {
            _kioskFilesService = kioskFilesService;
            _mqttRepo = mqttRepo;
            _iotCommandClient = iotCommandClient;
            _store = store;
        }

        public CommandEnum CommandEnum => CommandEnum.PeekFiles;

        public int Version => 2;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            var mqttResponse =
                await _kioskFilesService.PeekRequestedFilesAsync(
                    JsonConvert.DeserializeObject<KioskFilePeekRequest>(ioTCommand.Payload.ToJson()));
            var num = await _mqttRepo.PublishIoTCommandAsync(
                "redbox/updateservice-instance/" + ioTCommand.SourceId + "/request", new IoTCommandModel
                {
                    RequestId = ioTCommand.RequestId,
                    Command = CommandEnum,
                    Version = Version,
                    MessageType = MessageTypeEnum.Response,
                    SourceId = _store.KioskId.ToString(),
                    Payload = mqttResponse.ToJson(),
                    LogRequest = false
                })
                ? 1
                : 0;
        }
    }
}