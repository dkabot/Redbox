using System;
using System.Reflection;
using System.Threading;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.Client.Core
{
    public class DeviceServiceClient : IDeviceServiceClientCore
    {
        private readonly int _defaultCommandTimeout = 180000;
        private readonly IHttpService _httpService;
        private readonly object _lockConnectionObject = new object();
        private readonly SignalRClient _signalRClient;
        private AutoResetEvent _connectionAutoResetEvent;
        private string _url;

        public DeviceServiceClient(IHttpService httpService)
        {
            _signalRClient = new SignalRClient();
            _signalRClient.OnLog += logText => Log(logText);
            _signalRClient.OnConnectedHandler += SignalRConnected;
            _signalRClient.OnDisconnectedHandler += _signalRClient_OnDisconnectedHandler;
            _httpService = httpService;
        }

        public event LogHandler OnLog;

        public event OnConnected OnConnectedHandler;

        public event OnDisconnected OnDisconnectedHandler;

        public bool IsConnectedToDeviceService => _signalRClient != null && _signalRClient.IsConnectedToDeviceService;

        public bool ConnectToDeviceService(string url)
        {
            return ConnectToDeviceService(url, _defaultCommandTimeout);
        }

        public bool ConnectToDeviceService(string url, int connectionTimeout)
        {
            lock (_lockConnectionObject)
            {
                _connectionAutoResetEvent = new AutoResetEvent(false);
                StartConnectionToDeviceService(url);
                _connectionAutoResetEvent.WaitOne(connectionTimeout);
                return IsConnectedToDeviceService;
            }
        }

        public void StartConnectionToDeviceService(string url)
        {
            _url = url;
            Log("Begin Connecting to DeviceService at url: " + _url);
            _signalRClient.Connect(_url);
        }

        public void SendCardReaderConnectedEvent()
        {
            SendSimpleEvent(new CardReaderConnectedEvent());
        }

        public void SendCardReaderStateEvent(CardReaderState cardReaderState)
        {
            _signalRClient.SendCardReaderStateEvent(new CardReaderStateEvent
            {
                CardReaderState = cardReaderState
            });
        }

        public void SendCardReaderDisconnectedEvent()
        {
            SendSimpleEvent(new CardReaderDisconnectedEvent());
        }

        public void SendDeviceServiceCanShutDownEvent()
        {
            SendSimpleEvent(new DeviceServiceCanShutDownEvent());
        }

        public void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason)
        {
            _signalRClient.SendDeviceServiceShutDownStartingEvent(new DeviceServiceShutDownStartingEvent
            {
                ShutDownReason = shutDownReason
            });
        }

        public void SendDeviceTamperedEvent()
        {
            SendSimpleEvent(new DeviceTamperedEvent());
        }

        public void SetCommandQueueState(bool commandQueuePaused)
        {
            _signalRClient.SetCommandQueueState(
                new SetCommandQueueStateCommand(Assembly.GetExecutingAssembly().GetName().Version)
                {
                    CommandQueuePaused = commandQueuePaused
                });
        }

        private void SendSimpleEvent(SimpleEvent simpleEvent)
        {
            LogEvent(simpleEvent);
            _signalRClient.SendSimpleEvent(simpleEvent);
        }

        private void LogEvent(BaseEvent baseEvent)
        {
            Log(">>> " + baseEvent.EventName + ": " + JsonConvert.SerializeObject(baseEvent));
        }

        private void Log(string logText)
        {
            var onLog = OnLog;
            if (onLog == null)
                return;
            onLog(logText);
        }

        private void SignalRConnected()
        {
            Log("DeviceService connected.");
            var connectedHandler = OnConnectedHandler;
            if (connectedHandler != null)
                connectedHandler();
            if (_connectionAutoResetEvent == null)
                return;
            _connectionAutoResetEvent.Set();
        }

        private void _signalRClient_OnDisconnectedHandler(Exception exception)
        {
            Log(string.Format("DeviceService disconnected. {0}", exception));
            var disconnectedHandler = OnDisconnectedHandler;
            if (disconnectedHandler == null)
                return;
            disconnectedHandler(exception);
        }
    }
}