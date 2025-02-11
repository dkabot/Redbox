using System;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeviceService.Client.Core
{
    public class SignalRClient
    {
        private HubConnection _hubConnection;
        private string _url;

        public bool IsConnectedToDeviceService =>
            _hubConnection != null && _hubConnection.State == HubConnectionState.Connected;

        public event LogHandler OnLog;

        public event OnConnected OnConnectedHandler;

        public event OnDisconnected OnDisconnectedHandler;

        public async void Connect(string url)
        {
            _url = url;
            _hubConnection = new HubConnectionBuilder().WithUrl(_url).Build();
            if (_hubConnection != null)
            {
                AddMessageHandlers(_hubConnection);
                try
                {
                    await _hubConnection.StartAsync();
                    if (_hubConnection.State != HubConnectionState.Connected)
                        return;
                    var connectedHandler = OnConnectedHandler;
                    if (connectedHandler == null)
                        return;
                    connectedHandler();
                }
                catch (Exception ex)
                {
                    Log(string.Format("Exception in SignalRClient.Connect: {0}", ex));
                }
            }
            else
            {
                Log("Unable to create SignalR HubConnection.");
            }
        }

        public void SendSimpleEvent(SimpleEvent simpleEvent)
        {
            _hubConnection.SendAsync(nameof(SendSimpleEvent), simpleEvent);
        }

        public void SendDeviceServiceShutDownStartingEvent(
            DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent)
        {
            _hubConnection.SendAsync("SendShutDownStartingEvent", deviceServiceShutDownStartingEvent);
        }

        public void SendCardReaderStateEvent(CardReaderStateEvent cardReaderStateEvent)
        {
            _hubConnection.SendAsync(nameof(SendCardReaderStateEvent), cardReaderStateEvent);
        }

        public void SendDeviceTamperedEvent(DeviceTamperedEvent deviceTamperedEvent)
        {
            _hubConnection.SendAsync(nameof(SendDeviceTamperedEvent), deviceTamperedEvent);
        }

        public void SetCommandQueueState(
            SetCommandQueueStateCommand setCommandQueueStateCommand)
        {
            _hubConnection.SendAsync(nameof(SetCommandQueueState), setCommandQueueStateCommand);
        }

        private void Log(string logText)
        {
            var onLog = OnLog;
            if (onLog == null)
                return;
            onLog(logText);
        }

        private void AddMessageHandlers(HubConnection hubConnection)
        {
            if (hubConnection == null)
                return;
            hubConnection.Closed += HubConnection_Closed;
        }

        private Task HubConnection_Closed(Exception arg)
        {
            var disconnectedHandler = OnDisconnectedHandler;
            if (disconnectedHandler != null)
                disconnectedHandler(arg);
            return null;
        }
    }
}