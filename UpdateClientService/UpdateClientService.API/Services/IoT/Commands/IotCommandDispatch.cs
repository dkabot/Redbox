using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT.Commands
{
    public class IotCommandDispatch : IIotCommandDispatch
    {
        private readonly ILogger<IoTCommandService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public IotCommandDispatch(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<IoTCommandService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task Execute(byte[] message, string topic)
        {
            _logger.LogInfoWithSource("Execute(" + topic,
                "/sln/src/UpdateClientService.API/Services/IoT/Commands/IotCommandDispatch.cs");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await scope.ServiceProvider.GetService<IIoTCommandService>().Execute(message, topic);
            }
        }
    }
}