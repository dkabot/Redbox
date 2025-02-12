using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.Client.Jobs;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace DeviceService.Client
{
    public class DeviceServiceClient : IDeviceServiceClient
    {
        private readonly ConcurrentDictionary<Guid, CommandData> _activeCommands =
            new ConcurrentDictionary<Guid, CommandData>();

        private readonly object _lockConnectionObject = new object();
        private readonly object _lockSendCommand = new object();
        private Timer _asyncCommandTimeoutTimer;
        private AutoResetEvent _connectionAutoResetEvent;
        private Task _connectionTask;
        private int _connectionTimeout;
        private IDeviceServiceShutDownInfo _deviceServiceShutDownInfo;
        private DateTime? _lastCardReaderDisconnect;
        private int _reconnectionAttemptInterval;
        private SignalRClient _signalRClient;

        public DeviceServiceClient()
        {
            InitializeSignalRClient();
        }

        public string Url { get; private set; }

        public IDeviceServiceShutDownInfo DeviceServiceShutDownInfo
        {
            get => _deviceServiceShutDownInfo;
            private set
            {
                _deviceServiceShutDownInfo = value;
                var infoChangeHandler = OnDeviceServiceShutDownInfoChangeHandler;
                if (infoChangeHandler == null)
                    return;
                infoChangeHandler(_deviceServiceShutDownInfo);
            }
        }

        public event LogHandler OnLog;

        public event OnConnected OnConnectedHandler;

        public event OnDisconnected OnDisconnectedHandler;

        public event OnConnected OnCardReaderConnectedHandler;

        public event OnDisconnected OnCardReaderDisconnectedHandler;

        public event OnDeviceServiceCanShutDown OnDeviceServiceCanShutDownHandler;

        public event OnCardReaderState OnCardReaderStateHandler;

        public event OnDeviceServiceShutDownInfoChange OnDeviceServiceShutDownInfoChangeHandler;

        public event Action OnDeviceTamperedEventHandler;

        public bool IsConnectedToDeviceService => _signalRClient != null && _signalRClient.IsConnectedToDeviceService;

        public int DefaultCommandTimeout { get; set; } = 180000;

        public bool ConnectToDeviceService(string url)
        {
            return ConnectToDeviceService(url, DefaultCommandTimeout);
        }

        public bool ConnectToDeviceService(string url, int connectionTimeout)
        {
            lock (_lockConnectionObject)
            {
                _connectionAutoResetEvent = new AutoResetEvent(false);
                StartConnectionToDeviceService(url, connectionTimeout);
                _connectionAutoResetEvent.WaitOne(connectionTimeout);
                return IsConnectedToDeviceService;
            }
        }

        public bool AutoReconnect { get; set; }

        public void StartConnectionToDeviceService(string url, int? connectionTimeout)
        {
            Url = url;
            _connectionTimeout = connectionTimeout ??
                                 (_connectionTimeout != 0 ? _connectionTimeout : DefaultCommandTimeout);
            if (_connectionTask == null || _connectionTask.IsCompleted)
            {
                Log(string.Format("Begin Connecting to DeviceService at url: {0}   timeout: {1}", Url,
                    _connectionTimeout));
                _connectionTask = _signalRClient.Connect(Url, _connectionTimeout);
                _connectionTask.ContinueWith(task =>
                {
                    if (IsConnectedToDeviceService || !AutoReconnect)
                        return;
                    AttemptReconnection();
                });
            }
            else
            {
                Log("DeviceService Connect attempt rejected.  Only one connection attempt allowed at a time");
            }
        }

        public bool DisconnectFromDeviceService()
        {
            Log("Begin disconnect from DeviceService.");
            return _signalRClient != null && _signalRClient.Disconnect();
        }

        public bool RebootCardReader()
        {
            return RunCommand(new BaseCommandRequest(nameof(RebootCardReader), AssemblyVersion)) is SimpleResponseEvent
                simpleResponseEvent && simpleResponseEvent.Success;
        }

        public bool ShutDownDeviceService(bool forceShutDown, ShutDownReason shutDownReason)
        {
            return RunCommand(new DeviceServiceShutDownCommand(AssemblyVersion)
            {
                ForceShutDown = forceShutDown,
                Reason = shutDownReason
            }) is DeviceServiceShutDownResponseEvent downResponseEvent && downResponseEvent.Success;
        }

        public bool IsCardReaderConnected()
        {
            return RunCommand(new BaseCommandRequest("IsConnected", AssemblyVersion)) is IsConnectedResponseEvent
                connectedResponseEvent && connectedResponseEvent.IsConnected;
        }

        public void ReadCard(
            Guid requestId,
            CardReadRequest request,
            Action<BaseResponseEvent> completeCallback,
            Action<BaseResponseEvent> eventCallback,
            int? timeout = null)
        {
            RunAsyncReadCardCommand(requestId, request, timeout.HasValue ? timeout.Value : DefaultCommandTimeout,
                completeCallback, eventCallback);
        }

        public IReadCardJob CreateReadCardJob(
            CardReadRequest request,
            Action<BaseResponseEvent> readCardJobCompleteCallback,
            Action<BaseResponseEvent> jobEventCallback,
            int? timeout = null)
        {
            return new ReadCardJob(this, request, readCardJobCompleteCallback, jobEventCallback, timeout);
        }

        public void ReportAuthorizeResult(bool success, Action<BaseResponseEvent> completeCallback)
        {
            RunAsyncCommand(new ReportAuthorizeResultCommand(AssemblyVersion)
            {
                Success = success
            }, completeCallback, null);
        }

        public UnitHealthModel GetUnitHealth()
        {
            return !(RunCommand(new BaseCommandRequest(nameof(GetUnitHealth), AssemblyVersion)) is
                GetUnitHealthResponseEvent healthResponseEvent)
                ? null
                : healthResponseEvent.UnitHealthModel;
        }

        public bool GetCardInsertedStatus(Action<BaseResponseEvent> completedCallback)
        {
            return RunAsyncCommand(new BaseCommandRequest(nameof(GetCardInsertedStatus), AssemblyVersion),
                completedCallback, null);
        }

        public bool CancelCommand(
            Guid commandToCancelRequestId,
            Action<BaseResponseEvent> cancelCompleteCallback)
        {
            var cancelCommandRequest = new CancelCommandRequest(AssemblyVersion);
            cancelCommandRequest.CommandToCancelRequestId = commandToCancelRequestId;
            cancelCommandRequest.RequestId = Guid.NewGuid();
            return RunAsyncCommand(cancelCommandRequest, cancelCompleteCallback, null);
        }

        public Version AssemblyVersion => GetType().Assembly.GetName().Version;

        public ValidateVersionModel ValidateVersion()
        {
            var validateVersionModel = new ValidateVersionModel
            {
                IsCompatible = false,
                DeviceServiceVersion = new Version(0, 0)
            };

            var assemblyVersion = AssemblyVersion;

            if (assemblyVersion != null)
            {
                var response = RunCommand(new ValidateVersionCommand(assemblyVersion));

                if (response is ValidateVersionResponseEvent versionResponseEvent)
                    validateVersionModel = versionResponseEvent.ValidateVersionModel;
            }

            return validateVersionModel;
        }

        [Obsolete("Supports is deprecated, please get this value from the ONCardReaderState event.")]
        public bool SupportsEMV()
        {
            var flag = false;
            if (RunCommand(new BaseCommandRequest(nameof(SupportsEMV), AssemblyVersion)) is SupportsEMVResponseEvent
                emvResponseEvent)
                flag = emvResponseEvent.SuportsEMV;
            return flag;
        }

        public bool CheckActivation(BluefinActivationRequest request)
        {
            if (request.KioskId > 0L)
                if (RunCommand(new CheckActivationCommand(AssemblyVersion)
                    {
                        Request = request
                    }) is CheckActivationResponseEvent activationResponseEvent)
                    return activationResponseEvent.Success;
            return false;
        }

        public bool CheckDeviceStatus(DeviceStatusRequest request)
        {
            if (request.KioskId > 0L)
                if (RunCommand(new CheckDeviceStatusCommand(AssemblyVersion)
                    {
                        Request = request
                    }) is CheckDeviceStatusResponseEvent statusResponseEvent)
                    return statusResponseEvent.Success;
            return false;
        }

        private void InitializeSignalRClient()
        {
            _signalRClient = new SignalRClient();
            _signalRClient.OnLog += logText => Log(logText);
            _signalRClient.OnConnectedHandler += SignalRConnected;
            _signalRClient.OnDisconnectedHandler += _signalRClient_OnDisconnectedHandler;
            _signalRClient.OnCardReaderConnectedHandler += _signalRClient_OnCardReaderConnectedHandler;
            _signalRClient.OnCardReaderDisconnectedHandler += _signalRClient_OnCardReaderDisconnectedHandler;
            _signalRClient.OnIsConnectedResponseEventHandler += _signalRClient_OnIsConnectedResponseEventHandler;
            _signalRClient.OnGetUnitHealthResponseEventHandler += _signalRClient_OnGetUnitHealthResponseEventHandler;
            _signalRClient.OnCardReadResponseEventHandler += _signalRClient_OnCardReadResponseEventHandler;
            _signalRClient.OnReadConfigurationEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnCardRemovedEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnCardReadEvent += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnWriteConfigurationEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnCancelCommandResponseEventHandler += _signalRClient_OnCancelCommandResponseEventHandler;
            _signalRClient.OnRebootCardReaderResponseEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnReportAuthorizationResultResponseEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnValidateVersionResponseEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnCheckActivationResponseEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnCheckDeviceStatusResponseEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnGetCardInsertedStatusResponseEventHandler += HandleAsyncCommandResponseEvent;
            _signalRClient.OnDeviceServiceShutDownResponseEventHandler +=
                _signalRClient_OnDeviceServiceShutDownResponseEventHandler;
            _signalRClient.OnDeviceServiceCanShutDownEventHandler +=
                _signalRClient_OnDeviceServiceCanShutDownEventHandler;
            _signalRClient.OnDeviceServiceShutDownStartingEventHandler +=
                _signalRClient_OnDeviceServiceShutDownStartingEventHandler;
            _signalRClient.OnSupportsEMVResponseEventHandler += _signalRClient_OnGeneralEventHandler;
            _signalRClient.OnDeviceTamperedEventHandler += _signalRClient_OnDeviceTamperedHandler;
            _signalRClient.OnCardReaderStateEventHandler += _signalRClient_OnCardReaderStateEventHandler;
        }

        private void _signalRClient_OnCardReaderStateEventHandler(
            CardReaderStateEvent cardReaderStateEvent)
        {
            HandleEvent(cardReaderStateEvent, () => Task.Run(() =>
            {
                var readerStateHandler = OnCardReaderStateHandler;
                if (readerStateHandler == null)
                    return;
                readerStateHandler(cardReaderStateEvent?.CardReaderState);
            }));
        }

        private void _signalRClient_OnDeviceServiceShutDownStartingEventHandler(
            DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent)
        {
            HandleEvent(deviceServiceShutDownStartingEvent, () =>
            {
                if (deviceServiceShutDownStartingEvent == null)
                    return;
                Task.Run((Action)(() => DeviceServiceShutDownInfo = new DeviceServiceShutDownInformation
                {
                    ShutDownReason = deviceServiceShutDownStartingEvent.ShutDownReason,
                    ShutDownTime = DateTime.Now
                }));
            });
        }

        private void _signalRClient_OnDeviceServiceCanShutDownEventHandler(SimpleResponseEvent baseEvent)
        {
            HandleEvent(baseEvent, () => Task.Run(() =>
            {
                var canShutDownHandler = OnDeviceServiceCanShutDownHandler;
                var flag = canShutDownHandler == null || canShutDownHandler();
                SendCommand(new CommandData
                {
                    CommandRequest = new DeviceServiceCanShutDownCommand(AssemblyVersion)
                    {
                        CanShutDown = flag
                    }
                });
            }));
        }

        private void _signalRClient_OnDeviceServiceShutDownResponseEventHandler(
            DeviceServiceShutDownResponseEvent deviceServiceShutDownResponseEvent)
        {
            HandleResponseEvent(deviceServiceShutDownResponseEvent);
        }

        private void _signalRClient_OnGeneralEventHandler(BaseResponseEvent baseResponseEvent)
        {
            Task.Factory.StartNew(() => HandleResponseEvent(baseResponseEvent, commandData =>
            {
                if (baseResponseEvent?.EventName == null)
                    return;
                var eventName = baseResponseEvent.EventName;
                if ((!(eventName == "CardRemovedResponseEvent") &&
                     !(eventName == "CardProcessingStartedResponseEvent")) || commandData == null)
                    return;
                var eventCallback = commandData.EventCallback;
                if (eventCallback == null)
                    return;
                eventCallback(baseResponseEvent);
            }), TaskCreationOptions.LongRunning);
        }

        private void _signalRClient_OnCancelCommandResponseEventHandler(
            CancelCommandResponseEvent cancelCommandResponseEvent)
        {
            HandleAsyncCommandResponseEvent(cancelCommandResponseEvent);
        }

        private void _signalRClient_OnCardReaderDisconnectedHandler(BaseEvent baseEvent)
        {
            HandleEvent(baseEvent, () =>
            {
                Task.Run(() =>
                {
                    var disconnectedHandler = OnCardReaderDisconnectedHandler;
                    if (disconnectedHandler == null)
                        return;
                    disconnectedHandler(null);
                });
                _lastCardReaderDisconnect = DateTime.Now;
            });
        }

        private void _signalRClient_OnCardReaderConnectedHandler(BaseEvent baseEvent)
        {
            HandleEvent(baseEvent, () =>
            {
                if (_lastCardReaderDisconnect.HasValue)
                    Log(string.Format("<<< Duration of card reader disconnection: {0}",
                        DateTime.Now.Subtract(_lastCardReaderDisconnect.Value)));
                Task.Run(() =>
                {
                    var connectedHandler = OnCardReaderConnectedHandler;
                    if (connectedHandler == null)
                        return;
                    connectedHandler();
                });
            });
        }

        private void _signalRClient_OnDeviceTamperedHandler(BaseEvent baseEvent)
        {
            HandleEvent(baseEvent, () => Task.Run(() =>
            {
                var tamperedEventHandler = OnDeviceTamperedEventHandler;
                if (tamperedEventHandler == null)
                    return;
                tamperedEventHandler();
            }));
        }

        private void HandleEvent(BaseEvent baseEvent, Action action = null)
        {
            LogEvent(baseEvent);
            if (action == null)
                return;
            action();
        }

        private void _signalRClient_OnCardReadResponseEventHandler(BaseResponseEvent crre)
        {
            HandleAsyncCommandResponseEvent(crre);
        }

        public string ReadConfig(string group, string index)
        {
            return !(RunCommand(new ConfigCommand("ReadConfiguration", AssemblyVersion)) is SimpleResponseEvent
                simpleResponseEvent)
                ? null
                : simpleResponseEvent.Data;
        }

        public string WriteConfig(string group, string index, string value)
        {
            return !(RunCommand(new ConfigCommand("ReadConfiguration", AssemblyVersion)) is SimpleResponseEvent
                simpleResponseEvent)
                ? null
                : simpleResponseEvent.Data;
        }

        private void Log(string logText)
        {
            var onLog = OnLog;
            if (onLog == null)
                return;
            onLog(logText);
        }

        private void _signalRClient_OnDisconnectedHandler(Exception exception)
        {
            _lastCardReaderDisconnect = new DateTime?();
            Log(string.Format("DeviceService disconnected. {0}", exception));
            if (AutoReconnect)
                AttemptReconnection();
            Task.Run(() =>
            {
                var disconnectedHandler = OnDisconnectedHandler;
                if (disconnectedHandler == null)
                    return;
                disconnectedHandler(exception);
            });
        }

        private void AttemptReconnection()
        {
            Task.Run(async () =>
            {
                var reconnectionInterval = GetReconnectionInterval();
                Log(string.Format("Delay DeviceServiceClient reconnection attempt {0}",
                    TimeSpan.FromMilliseconds(reconnectionInterval)));
                await Task.Delay(reconnectionInterval);
                StartConnectionToDeviceService(Url, new int?());
            });
        }

        private void ResetReconnectionInterval()
        {
            _reconnectionAttemptInterval = 0;
        }

        private int GetReconnectionInterval()
        {
            if (_reconnectionAttemptInterval == 0)
                _reconnectionAttemptInterval = 5000;
            else
                _reconnectionAttemptInterval *= 2;
            var num = 300000;
            if (_reconnectionAttemptInterval > num)
                _reconnectionAttemptInterval = num;
            return _reconnectionAttemptInterval;
        }

        private void _signalRClient_OnGetUnitHealthResponseEventHandler(
            GetUnitHealthResponseEvent getUnitHealthResponseEvent)
        {
            LogResponseEvent(getUnitHealthResponseEvent);
            CommandData commandData;
            if (_activeCommands.TryGetValue(getUnitHealthResponseEvent.RequestId, out commandData))
            {
                commandData.ResponseEvent = getUnitHealthResponseEvent;
                commandData?.AutoResetEvent.Set();
            }
            else
            {
                Log("Response Event Handler error - unable to find active command: " +
                    JsonConvert.SerializeObject(getUnitHealthResponseEvent));
            }
        }

        private void HandleResponseEvent(
            BaseResponseEvent baseResponseEvent,
            Action<CommandData> action = null)
        {
            CommandData commandData;
            if (_activeCommands.TryGetValue(baseResponseEvent.RequestId, out commandData))
            {
                LogResponseEvent(baseResponseEvent);
                commandData.ResponseEvent = baseResponseEvent;
                if (commandData.AutoResetEvent != null && commandData != null)
                    commandData.AutoResetEvent.Set();
                if (action == null)
                    return;
                action(commandData);
            }
            else
            {
                Log("Response Event Handler error - unable to find active command: " +
                    JsonConvert.SerializeObject(baseResponseEvent));
            }
        }

        private void HandleAsyncCommandResponseEvent(BaseResponseEvent baseResponseEvent)
        {
            HandleResponseEvent(baseResponseEvent, commandData =>
            {
                _activeCommands.TryRemove(commandData.CommandRequest.RequestId, out var _);
                Task.Run(() =>
                {
                    var completeCallback = commandData.CommandCompleteCallback;
                    if (completeCallback == null)
                        return;
                    completeCallback(baseResponseEvent);
                });
            });
        }

        private void _signalRClient_OnIsConnectedResponseEventHandler(
            IsConnectedResponseEvent isConnectedResponseEvent)
        {
            HandleResponseEvent(isConnectedResponseEvent);
        }

        private void LogCommand(BaseCommandRequest baseCommandRequest)
        {
            Log(string.Format(">>> {0}: {1}", baseCommandRequest.CommandName, baseCommandRequest.Scrub()));
        }

        private void LogCommandTimeout(BaseCommandRequest baseCommandRequest)
        {
            Log(string.Format("*** Timeout for {0}: {1}", baseCommandRequest.CommandName, baseCommandRequest.Scrub()));
        }

        private void LogResponseEvent(BaseResponseEvent responseEvent)
        {
            string str;
            switch (responseEvent)
            {
                case EMVCardReadResponseEvent responseEvent1:
                    str = ObfuscateEMVCardReadResponseEvent(responseEvent1);
                    break;
                case EncryptedCardReadResponseEvent responseEvent2:
                    str = ObfuscateEncryptedCardReadResponseEvent(responseEvent2);
                    break;
                case UnencryptedCardReadResponseEvent responseEvent3:
                    str = ObfuscateUnencryptedCardReadResponseEvent(responseEvent3);
                    break;
                default:
                    str = JsonConvert.SerializeObject(responseEvent);
                    break;
            }

            Log("<<< " + responseEvent.EventName + ": " + str);
        }

        private void LogEvent(BaseEvent baseEvent)
        {
            Log("<<< " + baseEvent?.EventName + ": " + JsonConvert.SerializeObject(baseEvent));
        }

        private string ObfuscateEMVCardReadResponseEvent(EMVCardReadResponseEvent responseEvent)
        {
            return CreateResponseEventLogText(responseEvent, json =>
            {
                var readResponseEvent = JsonConvert.DeserializeObject<EMVCardReadResponseEvent>(json);
                if (readResponseEvent == null)
                    return readResponseEvent;
                var data = readResponseEvent.Data;
                if (data == null)
                    return readResponseEvent;
                data.ObfuscateSensitiveData();
                return readResponseEvent;
            });
        }

        private string ObfuscateEncryptedCardReadResponseEvent(
            EncryptedCardReadResponseEvent responseEvent)
        {
            return CreateResponseEventLogText(responseEvent, json =>
            {
                var readResponseEvent = JsonConvert.DeserializeObject<EncryptedCardReadResponseEvent>(json);
                if (readResponseEvent == null)
                    return readResponseEvent;
                var data = readResponseEvent.Data;
                if (data == null)
                    return readResponseEvent;
                data.ObfuscateSensitiveData();
                return readResponseEvent;
            });
        }

        private string ObfuscateUnencryptedCardReadResponseEvent(
            UnencryptedCardReadResponseEvent responseEvent)
        {
            return CreateResponseEventLogText(responseEvent, json =>
            {
                var readResponseEvent = JsonConvert.DeserializeObject<UnencryptedCardReadResponseEvent>(json);
                if (readResponseEvent == null)
                    return readResponseEvent;
                var data = readResponseEvent.Data;
                if (data == null)
                    return readResponseEvent;
                data.ObfuscateSensitiveData();
                return readResponseEvent;
            });
        }

        private string CreateResponseEventLogText(
            BaseEvent responseEvent,
            Func<string, object> ObfuscateJsonAction)
        {
            var responseEventLogText = (string)null;
            try
            {
                var str = JsonConvert.SerializeObject(responseEvent);
                responseEventLogText = JsonConvert.SerializeObject(ObfuscateJsonAction(str));
            }
            catch (Exception ex)
            {
                Log(string.Format(
                    "Unhandled exception in DeviceServiceClient.CreateResponseEventLogText while deserializing a CardReadResponseEvent: {0}",
                    ex));
            }

            return responseEventLogText;
        }

        private void SignalRConnected()
        {
            ResetReconnectionInterval();
            _lastCardReaderDisconnect = new DateTime?();
            DeviceServiceShutDownInfo = null;
            Log("DeviceService connected.");
            Task.Run(() =>
            {
                var connectedHandler = OnConnectedHandler;
                if (connectedHandler == null)
                    return;
                connectedHandler();
            });
            if (_connectionAutoResetEvent == null)
                return;
            _connectionAutoResetEvent.Set();
        }

        private bool RunAsyncReadCardCommand(
            Guid? requestId,
            CardReadRequest request,
            int timeout,
            Action<BaseResponseEvent> commandCompleteCallback = null,
            Action<BaseResponseEvent> eventCallback = null)
        {
            var readCardCommand = new ReadCardCommand(AssemblyVersion);
            readCardCommand.RequestId = requestId ?? Guid.NewGuid();
            readCardCommand.Request = request;
            readCardCommand.CommandTimeout = timeout;
            return RunAsyncCommand(readCardCommand, commandCompleteCallback, eventCallback);
        }

        private BaseResponseEvent RunCommand(BaseCommandRequest baseCommandRequest)
        {
            var commandData = new CommandData
            {
                CommandRequest = baseCommandRequest,
                AutoResetEvent = new AutoResetEvent(false)
            };
            SendCommand(commandData);
            var millisecondsTimeout = baseCommandRequest.CommandTimeout.HasValue
                ? baseCommandRequest.CommandTimeout.Value
                : DefaultCommandTimeout;
            commandData.AutoResetEvent.WaitOne(millisecondsTimeout);
            if (commandData.ResponseEvent == null)
                LogCommandTimeout(commandData.CommandRequest);
            _activeCommands.TryRemove(commandData.CommandRequest.RequestId, out var _);
            return commandData?.ResponseEvent;
        }

        private bool RunAsyncCommand(
            BaseCommandRequest baseCommandRequest,
            Action<BaseResponseEvent> commandCompleteCallback,
            Action<BaseResponseEvent> eventCallback)
        {
            var flag = false;
            if (baseCommandRequest != null && (commandCompleteCallback != null || eventCallback != null))
            {
                var commandData = new CommandData
                {
                    CommandRequest = baseCommandRequest
                };
                if (commandCompleteCallback != null)
                    commandData.CommandCompleteCallback = commandCompleteCallback;
                if (eventCallback != null)
                    commandData.EventCallback = eventCallback;
                SendCommand(commandData);
                StartTimeoutTimer();
                flag = true;
            }

            return flag;
        }

        private void SendCommand(CommandData commandData)
        {
            lock (_lockSendCommand)
            {
                _activeCommands[commandData.CommandRequest.RequestId] = commandData;
                LogCommand(commandData.CommandRequest);
                if (commandData.CommandRequest != null)
                    _signalRClient.SendCommand(commandData.CommandRequest);
                else
                    Log(string.Format("DeviceServiceClient.SendCommand unknown command type {0}",
                        commandData?.CommandRequest?.GetType()));
            }
        }

        private void StartTimeoutTimer()
        {
            if (_asyncCommandTimeoutTimer == null)
            {
                _asyncCommandTimeoutTimer = new Timer
                {
                    Interval = 60000.0,
                    AutoReset = true
                };
                _asyncCommandTimeoutTimer.Elapsed += AsyncCommandTimeoutTimer_Elapsed;
            }

            if (_asyncCommandTimeoutTimer.Enabled)
                return;
            Log("Starting DeviceServiceClient aysnc command timeout timer");
            _asyncCommandTimeoutTimer.Enabled = true;
        }

        private void AsyncCommandTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var num = 0;
                foreach (var activeCommand in _activeCommands)
                {
                    var commandData1 = activeCommand.Value;
                    if (commandData1 != null && commandData1.IsAsync)
                    {
                        if (commandData1.IsTimedOut)
                        {
                            CommandData removedCommandData;
                            if (_activeCommands.TryRemove(commandData1.CommandRequest.RequestId,
                                    out removedCommandData))
                            {
                                var timeoutResponseEvent1 = new TimeoutResponseEvent(removedCommandData.CommandRequest);
                                timeoutResponseEvent1.Success = false;
                                var timeoutResponseEvent = timeoutResponseEvent1;
                                Task.Run(() =>
                                {
                                    var commandData2 = removedCommandData;
                                    if (commandData2 == null)
                                        return;
                                    commandData2.CommandCompleteCallback(timeoutResponseEvent);
                                });
                                CancelCommand(removedCommandData.CommandRequest.RequestId, null);
                                Log("DeviceServiceClient removing timed out aync command " +
                                    JsonConvert.SerializeObject(removedCommandData.CommandRequest));
                            }
                        }
                        else
                        {
                            ++num;
                        }
                    }
                }

                if (num == 0)
                {
                    Log("DeviceServiceClient no active aysnc commands.  Stopping timeout timer");
                    _asyncCommandTimeoutTimer.Enabled = false;
                }
                else
                {
                    Log(string.Format("DeviceServiceClient active aysnc commands: {0}", num));
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Unhandled exception in DeviceServiceClient.AsyncCommandTimeoutTimer_Elapsed. {0}",
                    ex));
            }
        }

        private class DeviceServiceShutDownInformation : IDeviceServiceShutDownInfo
        {
            public DateTime ShutDownTime { get; set; }

            public ShutDownReason ShutDownReason { get; set; }
        }

        private class CommandData
        {
            public Action<BaseResponseEvent> CommandCompleteCallback;
            public Action<BaseResponseEvent> EventCallback;

            public BaseCommandRequest CommandRequest { get; set; }

            public AutoResetEvent AutoResetEvent { get; set; }

            public BaseResponseEvent ResponseEvent { get; set; }

            public bool IsAsync => AutoResetEvent == null;

            public bool IsTimedOut =>
                CommandRequest.CreatedDate.AddMilliseconds(CommandRequest.CommandTimeout ?? 180000) < DateTime.Now;
        }
    }
}