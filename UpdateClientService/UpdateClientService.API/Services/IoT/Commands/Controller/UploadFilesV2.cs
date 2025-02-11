using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller
{
    public class UploadFilesV2 : ICommandIoTController
    {
        private readonly IKioskFilesService _kioskFilesService;
        private readonly IMqttProxy _mqttRepo;
        private readonly IStoreService _store;

        public UploadFilesV2(
            IKioskFilesService kioskFilesService,
            IMqttProxy mqttRepo,
            IStoreService store)
        {
            _kioskFilesService = kioskFilesService;
            _mqttRepo = mqttRepo;
            _store = store;
        }

        public CommandEnum CommandEnum => CommandEnum.UploadFiles;

        public int Version => 2;

        public async Task Execute(IoTCommandModel ioTCommand)
        {
            var mqttResponse =
                await _kioskFilesService.UploadFilesAsync(
                    JsonConvert.DeserializeObject<KioskUploadFileRequest>(ioTCommand.Payload.ToJson()));
            var num = await _mqttRepo.PublishIoTCommandAsync(
                "redbox/updateservice-instance/" + ioTCommand.SourceId + "/request", new IoTCommandModel
                {
                    RequestId = ioTCommand.RequestId,
                    Command = CommandEnum,
                    Version = Version,
                    SourceId = _store.KioskId.ToString(),
                    MessageType = MessageTypeEnum.Response,
                    Payload = mqttResponse.ToJson()
                })
                ? 1
                : 0;
        }
    }
}