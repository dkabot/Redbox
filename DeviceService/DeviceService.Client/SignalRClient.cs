using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeviceService.Client
{
    public class SignalRClient
    {
        public delegate void OnCardReaderStateEvent(CardReaderStateEvent cardReaderStateEvent);

        public delegate void OnCheckActivationResponseEvent(
            CheckActivationResponseEvent checkActivationResponseEvent);

        public delegate void OnCheckDeviceStatusResponseEvent(
            CheckDeviceStatusResponseEvent checkDeviceStatusResponseEvent);

        public delegate void OnDeviceServiceShutDownResponseEvent(
            DeviceServiceShutDownResponseEvent deviceServiceShutDownResponseEvent);

        public delegate void OnDeviceServiceShutDownStartingEvent(
            DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent);

        public delegate void OnEvent(BaseEvent baseEvent);

        public delegate void OnGetCardInsertedStatusResponseEvent(
            GetCardInsertedStatusResponseEvent getCardInsertedStatusResponseEvent);

        public delegate void OnGetUnitHealthResponseEvent(
            GetUnitHealthResponseEvent getUnitHealthResponseEvent);

        public delegate void OnIsConnectedResponseEvent(
            IsConnectedResponseEvent isConnectedResponseEvent);

        public delegate void OnSupportsEMVResponseEvent(
            SupportsEMVResponseEvent supportsEMVResponseEvent);

        public delegate void OnValidateVersionResponseEvent(
            ValidateVersionResponseEvent validateVersionResponseEvent);

        private HubConnection _hubConnection;

        public string Url { get; private set; }

        public bool IsConnectedToDeviceService =>
            _hubConnection != null && _hubConnection.State == HubConnectionState.Connected;

        public event LogHandler OnLog;

        public event OnConnected OnConnectedHandler;

        public event OnDisconnected OnDisconnectedHandler;

        public event OnEvent OnCardReaderConnectedHandler;

        public event OnEvent OnCardReaderDisconnectedHandler;

        public event OnIsConnectedResponseEvent OnIsConnectedResponseEventHandler;

        public event OnGetUnitHealthResponseEvent OnGetUnitHealthResponseEventHandler;

        public event Action<BaseResponseEvent> OnCardReadResponseEventHandler;

        public event Action<SimpleResponseEvent> OnReadConfigurationEventHandler;

        public event Action<SimpleResponseEvent> OnWriteConfigurationEventHandler;

        public event Action<SimpleResponseEvent> OnCardRemovedEventHandler;

        public event Action<SimpleResponseEvent> OnDeviceServiceCanShutDownEventHandler;

        public event Action<BaseResponseEvent> OnCardReadEvent;

        public event Action<CancelCommandResponseEvent> OnCancelCommandResponseEventHandler;

        public event Action<RebootCardReaderResponseEvent> OnRebootCardReaderResponseEventHandler;

        public event Action<ReportAuthorizationResultResponseEvent> OnReportAuthorizationResultResponseEventHandler;

        public event OnValidateVersionResponseEvent OnValidateVersionResponseEventHandler;

        public event OnCheckActivationResponseEvent OnCheckActivationResponseEventHandler;

        public event OnCheckDeviceStatusResponseEvent OnCheckDeviceStatusResponseEventHandler;

        public event OnGetCardInsertedStatusResponseEvent OnGetCardInsertedStatusResponseEventHandler;

        public event OnDeviceServiceShutDownResponseEvent OnDeviceServiceShutDownResponseEventHandler;

        public event OnDeviceServiceShutDownStartingEvent OnDeviceServiceShutDownStartingEventHandler;

        public event OnSupportsEMVResponseEvent OnSupportsEMVResponseEventHandler;

        public event OnEvent OnDeviceTamperedEventHandler;

        public event OnCardReaderStateEvent OnCardReaderStateEventHandler;

        public async Task Connect(string url, int? connectionTimeout)
        {
            Url = url;
            _hubConnection = new HubConnectionBuilder().WithUrl(Url).Build();
            if (_hubConnection != null)
            {
                AddMessageHandlers(_hubConnection);
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;
                    var timedout = false;
                    var task1 = Task.Run(() =>
                    {
                        var num1 = connectionTimeout ?? 5000;
                        int num2;
                        for (num2 = 0; !cancellationToken.IsCancellationRequested && num2 < num1; num2 += 1000)
                            Thread.Sleep(1000);
                        if (num2 >= num1 && !cancellationToken.IsCancellationRequested)
                            timedout = true;
                        cancellationTokenSource.Cancel();
                    }, cancellationToken);
                    try
                    {
                        _hubConnection.StartAsync(cancellationToken).ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                                timedout = true;
                            cancellationTokenSource.Cancel();
                        });
                        await task1;
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (_hubConnection.State == HubConnectionState.Disconnected)
                            Log("DeviceServiceClient SignalRClient connection attempt timed out. ");
                    }

                    if (_hubConnection.State == HubConnectionState.Connected)
                    {
                        var connectedHandler = OnConnectedHandler;
                        if (connectedHandler == null)
                            ;
                        else
                            connectedHandler();
                    }
                    else if (timedout)
                    {
                        Log("DeviceServiceClient SignalRClient connection attempt timed out. ");
                    }
                    else
                    {
                        Log("DeviceServiceClient SignalRClient connection failed. ");
                    }
                }
                catch (Exception ex)
                {
                    Log(string.Format("Exception in DeviceServiceClient SignalRClient.Connect: {0}", ex));
                }
            }
            else
            {
                Log("Unable to create DeviceServiceClient SignalR HubConnection.");
            }
        }

        public bool Disconnect()
        {
            _hubConnection.StopAsync();
            return _hubConnection.State == HubConnectionState.Disconnected;
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
            hubConnection.On("IsConnectedResponseEvent", (Action<IsConnectedResponseEvent>)(isConnectedResponseEvent =>
            {
                var responseEventHandler = OnIsConnectedResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(isConnectedResponseEvent);
            }));
            hubConnection.On("GetUnitHealthResponseEvent",
                (Action<GetUnitHealthResponseEvent>)(getUnitHealthResponseEvent =>
                {
                    var responseEventHandler = OnGetUnitHealthResponseEventHandler;
                    if (responseEventHandler == null)
                        return;
                    responseEventHandler(getUnitHealthResponseEvent);
                }));
            hubConnection.On("EMVCardReadResponseEvent", (Action<EMVCardReadResponseEvent>)(readCardResponseEvent =>
            {
                var responseEventHandler = OnCardReadResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(readCardResponseEvent);
            }));
            hubConnection.On("EncryptedCardReadResponseEvent",
                (Action<EncryptedCardReadResponseEvent>)(readCardResponseEvent =>
                {
                    var responseEventHandler = OnCardReadResponseEventHandler;
                    if (responseEventHandler == null)
                        return;
                    responseEventHandler(readCardResponseEvent);
                }));
            hubConnection.On("UnencryptedCardReadResponseEvent",
                (Action<UnencryptedCardReadResponseEvent>)(readCardResponseEvent =>
                {
                    var responseEventHandler = OnCardReadResponseEventHandler;
                    if (responseEventHandler == null)
                        return;
                    responseEventHandler(readCardResponseEvent);
                }));
            hubConnection.On("ReadConfiguration", (Action<SimpleResponseEvent>)(response =>
            {
                var configurationEventHandler = OnReadConfigurationEventHandler;
                if (configurationEventHandler == null)
                    return;
                configurationEventHandler(response);
            }));
            hubConnection.On("WriteConfiguration", (Action<SimpleResponseEvent>)(response =>
            {
                var configurationEventHandler = OnWriteConfigurationEventHandler;
                if (configurationEventHandler == null)
                    return;
                configurationEventHandler(response);
            }));
            hubConnection.On("CardRemovedResponseEvent", (Action<SimpleResponseEvent>)(response =>
            {
                var removedEventHandler = OnCardRemovedEventHandler;
                if (removedEventHandler == null)
                    return;
                removedEventHandler(response);
            }));
            hubConnection.On("CardProcessingStartedResponseEvent", (Action<SimpleResponseEvent>)(response =>
            {
                var onCardReadEvent = OnCardReadEvent;
                if (onCardReadEvent == null)
                    return;
                onCardReadEvent(response);
            }));
            hubConnection.On("DeviceServiceCanShutDownEvent", (Action<SimpleResponseEvent>)(response =>
            {
                var downEventHandler = OnDeviceServiceCanShutDownEventHandler;
                if (downEventHandler == null)
                    return;
                downEventHandler(response);
            }));
            hubConnection.On("CardReaderConnectedEvent", (Action<CardReaderConnectedEvent>)(response =>
            {
                var connectedHandler = OnCardReaderConnectedHandler;
                if (connectedHandler == null)
                    return;
                connectedHandler(response);
            }));
            hubConnection.On("CardReaderDisconnectedEvent", (Action<CardReaderDisconnectedEvent>)(response =>
            {
                var disconnectedHandler = OnCardReaderDisconnectedHandler;
                if (disconnectedHandler == null)
                    return;
                disconnectedHandler(response);
            }));
            hubConnection.On("CancelCommandResponseEvent", (Action<CancelCommandResponseEvent>)(response =>
            {
                var responseEventHandler = OnCancelCommandResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(response);
            }));
            hubConnection.On("RebootCardReaderResponseEvent", (Action<RebootCardReaderResponseEvent>)(response =>
            {
                var responseEventHandler = OnRebootCardReaderResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(response);
            }));
            hubConnection.On("ReportAuthorizationResultResponseEvent",
                (Action<ReportAuthorizationResultResponseEvent>)(response =>
                {
                    var responseEventHandler = OnReportAuthorizationResultResponseEventHandler;
                    if (responseEventHandler == null)
                        return;
                    responseEventHandler(response);
                }));
            hubConnection.On("ValidateVersionReponseEvent", (Action<ValidateVersionResponseEvent>)(response =>
            {
                var responseEventHandler = OnValidateVersionResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(response);
            }));
            hubConnection.On("CheckActivationResponseEvent", (Action<CheckActivationResponseEvent>)(response =>
            {
                var responseEventHandler = OnCheckActivationResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(response);
            }));
            hubConnection.On("CheckDeviceStatusResponseEvent", (Action<CheckDeviceStatusResponseEvent>)(response =>
            {
                var responseEventHandler = OnCheckDeviceStatusResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(response);
            }));
            hubConnection.On("GetCardInsertedStatusResponseEvent",
                (Action<GetCardInsertedStatusResponseEvent>)(response =>
                {
                    var responseEventHandler = OnGetCardInsertedStatusResponseEventHandler;
                    if (responseEventHandler == null)
                        return;
                    responseEventHandler(response);
                }));
            hubConnection.On("DeviceServiceShutDownResponseEvent",
                (Action<DeviceServiceShutDownResponseEvent>)(response =>
                {
                    var responseEventHandler = OnDeviceServiceShutDownResponseEventHandler;
                    if (responseEventHandler == null)
                        return;
                    responseEventHandler(response);
                }));
            hubConnection.On("DeviceServiceShutDownStartingEvent",
                (Action<DeviceServiceShutDownStartingEvent>)(response =>
                {
                    var startingEventHandler = OnDeviceServiceShutDownStartingEventHandler;
                    if (startingEventHandler == null)
                        return;
                    startingEventHandler(response);
                }));
            hubConnection.On("SupportsEMVResponseEvent", (Action<SupportsEMVResponseEvent>)(response =>
            {
                Log("SignalRClient invoking OnSupportsEMVResponseEventHandler");
                var responseEventHandler = OnSupportsEMVResponseEventHandler;
                if (responseEventHandler == null)
                    return;
                responseEventHandler(response);
            }));
            hubConnection.On("DeviceTamperedEvent", (Action<DeviceTamperedEvent>)(response =>
            {
                var tamperedEventHandler = OnDeviceTamperedEventHandler;
                if (tamperedEventHandler == null)
                    return;
                tamperedEventHandler(response);
            }));
            hubConnection.On("CardReaderStateEvent", (Action<CardReaderStateEvent>)(response =>
            {
                var stateEventHandler = OnCardReaderStateEventHandler;
                if (stateEventHandler == null)
                    return;
                stateEventHandler(response);
            }));
        }

        private Task HubConnection_Closed(Exception arg)
        {
            var disconnectedHandler = OnDisconnectedHandler;
            if (disconnectedHandler != null)
                disconnectedHandler(arg);
            return null;
        }

        public void SendCommand(BaseCommandRequest baseCommandRequest)
        {
            _hubConnection.SendAsync(baseCommandRequest.SignalRCommand, baseCommandRequest);
        }
    }
}