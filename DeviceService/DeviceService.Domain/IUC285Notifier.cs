using System;
using DeviceService.Client.Core;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;

namespace DeviceService.Domain
{
    public class IUC285Notifier : IIUC285Notifier
    {
        private readonly IDeviceServiceClientCore _deviceServiceClient;
        private readonly ILogger<IUC285Notifier> _logger;
        private readonly IApplicationSettings _settings;
        private readonly string _url;

        public IUC285Notifier(
            ILogger<IUC285Notifier> logger,
            IHttpService httpService,
            IApplicationSettings settings)
        {
            _settings = settings;
            _logger = logger;
            _deviceServiceClient = new DeviceServiceClient(httpService);
            _deviceServiceClient.OnLog += _deviceServiceClient_OnLog;
            _url = GetDeviceServiceClientUrl();
            Log("Configuring IUC285Notifier with DeviceServiceClientUrl: " + _url);
        }

        public void SendCardReaderConnectedEvent()
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SendCardReaderConnectedEvent();
        }

        public void SendCardReaderStateEvent(CardReaderState cardReaderState)
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SendCardReaderStateEvent(cardReaderState);
        }

        public void SendCardReaderDisconnectedEvent(Exception exception)
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SendCardReaderDisconnectedEvent();
        }

        public void SendDeviceServiceCanShutDownEvent()
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SendDeviceServiceCanShutDownEvent();
        }

        public void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason)
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SendDeviceServiceShutDownStartingEvent(shutDownReason);
        }

        public void SetCommandQueueState(bool commandQueuePaused)
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SetCommandQueueState(commandQueuePaused);
        }

        public void SendDeviceTamperedEvent()
        {
            if (!ConnectToDeviceServiceClient())
                return;
            _deviceServiceClient.SendDeviceTamperedEvent();
        }

        private void _deviceServiceClient_OnLog(string logText)
        {
            Log(logText);
        }

        private Uri GetDeviceServiceUri()
        {
            return new Uri(_settings.DeviceServiceUrl);
        }

        private string GetDeviceServiceClientUrl()
        {
            var serviceClientUrl = (string)null;
            try
            {
                var uriBuilder = new UriBuilder(GetDeviceServiceUri());
                uriBuilder.Path += _settings.DeviceServiceClientPath;
                serviceClientUrl = uriBuilder.ToString();
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    logger.LogInformation(
                        string.Format("Startup.GetDeviceServiceUrl: Unable to create DeviceServiceClientUrl.  {0}",
                            ex));
            }

            return serviceClientUrl;
        }

        private void Log(string s)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogInformation(s);
        }

        private bool ConnectToDeviceServiceClient()
        {
            var deviceServiceClient = false;
            if (_deviceServiceClient != null)
            {
                deviceServiceClient = _deviceServiceClient.IsConnectedToDeviceService;
                if (!deviceServiceClient)
                    deviceServiceClient = _deviceServiceClient.ConnectToDeviceService(_url, 5000);
            }

            if (!deviceServiceClient)
                Log("IUC285Notifier: device service client is null or not connected.  event not sent");
            return deviceServiceClient;
        }
    }
}