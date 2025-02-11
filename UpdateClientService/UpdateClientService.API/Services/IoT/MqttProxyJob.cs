using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.IoT
{
    public class MqttProxyJob : IInvocable
    {
        private readonly IMqttProxy _mqttProxy;
        private ILogger<MqttProxyJob> _logger;

        public MqttProxyJob(ILogger<MqttProxyJob> logger, IMqttProxy mqttProxy)
        {
            _logger = logger;
            _mqttProxy = mqttProxy;
        }

        public async Task Invoke()
        {
            var num = await _mqttProxy.CheckConnectionAsync() ? 1 : 0;
        }
    }
}