using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Analytics;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeviceService.WebApi.Services
{
    public class CardReaderHub : Hub
    {
        private static readonly object _syncObject = new object();

        private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokenSources =
            new ConcurrentDictionary<Guid, CancellationTokenSource>();

        private static int _configureOccurred;
        private static QueuedCommand ActiveCommand = null;
        private static Task _processCommandQueue;
        private static Dictionary<string, Action<QueuedCommand>> _commandProcessors;
        private static ConcurrentQueue<QueuedCommand> _commandQueue = new ConcurrentQueue<QueuedCommand>();
        private static bool _commandQueuePaused;
        private readonly IActivationService _activationService;
        private readonly IAnalyticsService _analytics;
        private readonly IApplicationControl _applicationControl;
        private readonly IDeviceStatusService _deviceStatusService;
        private readonly ILogger<CardReaderHub> _logger;
        private readonly IIUC285Proxy _proxy;

        public CardReaderHub(
            IIUC285Proxy proxy,
            ILogger<CardReaderHub> logger,
            IActivationService activationService,
            IDeviceStatusService deviceStatusService,
            IApplicationControl applicationControl,
            IAnalyticsService analytics)
        {
            _proxy = proxy;
            _logger = logger;
            _activationService = activationService;
            _deviceStatusService = deviceStatusService;
            _applicationControl = applicationControl;
            _analytics = analytics;
            ConfigureCommandProcessors();
        }

        public static bool IsProcessingCommands => _processCommandQueue != null && !_processCommandQueue.IsCompleted;

        private bool CommandQueuePaused
        {
            get => _commandQueuePaused;
            set
            {
                if (value == _commandQueuePaused)
                    return;
                _commandQueuePaused = value;
                var str = _commandQueuePaused ? "paused" : "restarted";
                var logger = _logger;
                if (logger != null)
                    logger.LogInformation("Command Queue " + str);
                if (_commandQueuePaused)
                    return;
                StartProcessingQueuedCommands();
            }
        }

        public void SendMessage(string message)
        {
            Clients.All.SendAsync("ReceiveMessage", "username", message);
        }

        public void SendSimpleEvent(SimpleEvent simpleEvent)
        {
            if (simpleEvent == null)
                return;
            switch (simpleEvent.EventName)
            {
                case "CardReaderConnectedEvent":
                    Clients.All.SendAsync("CardReaderConnectedEvent", simpleEvent);
                    break;
                case "CardReaderDisconnectedEvent":
                    Clients.All.SendAsync("CardReaderDisconnectedEvent", simpleEvent);
                    break;
                case "CardRemovedResponseEvent":
                    Clients.All.SendAsync("CardRemovedResponseEvent", simpleEvent);
                    break;
                case "DeviceServiceCanShutDownEvent":
                    Clients.All.SendAsync("DeviceServiceCanShutDownEvent", simpleEvent);
                    break;
                default:
                    Clients.All.SendAsync(simpleEvent.EventName, simpleEvent);
                    break;
            }
        }

        private void AddQueuedCommand(QueuedCommand queuedCommand)
        {
            if (queuedCommand?.Command == null)
                return;
            queuedCommand.ConnectionId = Context.ConnectionId;
            lock (_syncObject)
            {
                if (queuedCommand.Command.IsQueuedCommand && queuedCommand.Command.CommandName != "IsConnected" &&
                    queuedCommand.Command.CommandName != "SupportsEMV")
                {
                    _commandQueue.Enqueue(queuedCommand);
                }
                else
                {
                    Action<QueuedCommand> action;
                    if (_commandProcessors.TryGetValue(queuedCommand.Command.CommandName, out action))
                        if (action != null)
                            action(queuedCommand);
                }
            }

            var logger = _logger;
            if (logger != null)
                logger.LogInformation(string.Format(
                    "Added command {0} to Queued commands with request id: {1} with {2} commands in queue.",
                    queuedCommand.Command.CommandName, queuedCommand.Command.RequestId, _commandQueue.Count));
            StartProcessingQueuedCommands();
        }

        private void StartProcessingQueuedCommands()
        {
            if (_processCommandQueue == null || _processCommandQueue.IsCompleted)
            {
                _processCommandQueue = Task.Run(() => ProcessQueuedCommands());
            }
            else
            {
                var logger = _logger;
                if (logger == null)
                    return;
                logger.LogInformation("ProcessQueuedCommands task already running");
            }
        }

        private void ConfigureCommandProcessors()
        {
            if (_commandProcessors != null || Interlocked.CompareExchange(ref _configureOccurred, 1, 0) != 0)
                return;
            _commandProcessors = new Dictionary<string, Action<QueuedCommand>>();
            _commandProcessors["IsConnected"] = ProcessIsConnectedCommand;
            _commandProcessors["GetUnitHealth"] = ProcessGetUnitHealthCommand;
            _commandProcessors["RebootCardReader"] = ProcessRebootCardReaderCommand;
            _commandProcessors["ReadConfiguration"] = ProcessReadConfigurationCommand;
            _commandProcessors["WriteConfiguration"] = ProcessWriteConfigurationCommand;
            _commandProcessors["ReadCard"] = ProcessReadCardCommand;
            _commandProcessors["ValidateVersion"] = ProcessValidateVersionCommand;
            _commandProcessors["CheckActivation"] = ProcessCheckActivationCommand;
            _commandProcessors["CheckDeviceStatus"] = ProcessCheckDeviceStatusCommand;
            _commandProcessors["GetCardInsertedStatus"] = ProcessGetCardInsertedStatusCommand;
            _commandProcessors["SupportsEMV"] = ProcessSupportsEMVCommand;
        }

        private void ProcessQueuedCommands()
        {
            ActiveCommand = null;
            var num = 0;
            lock (_syncObject)
            {
                if (!_commandQueuePaused)
                {
                    num = _commandQueue.Count;
                    if (num > 0)
                        _commandQueue.TryDequeue(out ActiveCommand);
                }
            }

            while (num > 0 && ActiveCommand != null)
            {
                var logger1 = _logger;
                if (logger1 != null)
                    logger1.LogInformation(string.Format("Begining processing for queued command {0} with ID {1}",
                        ActiveCommand?.Command?.CommandName, ActiveCommand?.Command.RequestId));
                Action<QueuedCommand> action;
                if (_commandProcessors.TryGetValue(ActiveCommand?.Command?.CommandName, out action))
                {
                    _proxy.StopHealthTimer();
                    action(ActiveCommand);
                }
                else
                {
                    var logger2 = _logger;
                    if (logger2 != null)
                        logger2.LogError("ProcessQueuedCommands - error.  No handler for command " +
                                         ActiveCommand?.Command?.CommandName);
                }

                var logger3 = _logger;
                if (logger3 != null)
                    logger3.LogInformation(string.Format("Finished processing for queued command {0} with ID {1}",
                        ActiveCommand?.Command?.CommandName, ActiveCommand?.Command.RequestId));
                lock (_syncObject)
                {
                    if (!_commandQueuePaused)
                    {
                        num = _commandQueue.Count;
                        if (num > 0)
                        {
                            var stringList = new List<string>();
                            foreach (var command in _commandQueue)
                                stringList.Add(command?.Command?.CommandName);
                            var str = string.Join(", ", stringList.ToArray());
                            var logger4 = _logger;
                            if (logger4 != null)
                                logger4.LogInformation(string.Format("Queued Command count: {0}   Commands: {1}", num,
                                    str));
                            _commandQueue.TryDequeue(out ActiveCommand);
                        }
                    }
                }
            }

            ActiveCommand = null;
            var str1 = _commandQueuePaused
                ? string.Format("paused.  Command queue count = {0}", _commandQueue.Count)
                : "empty";
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("Command queue is " + str1 + ".");
            _proxy.StartHealthTimer();
        }

        private void ProcessIsConnectedCommand(QueuedCommand queuedCommand)
        {
            var proxy = _proxy;
            var flag = proxy != null && proxy.IsConnected;
            var connectedResponseEvent1 = new IsConnectedResponseEvent(queuedCommand.Command);
            connectedResponseEvent1.IsConnected = flag;
            connectedResponseEvent1.Success = true;
            var connectedResponseEvent2 = connectedResponseEvent1;
            LogEvent(">>> IsConnectedResponseEvent", connectedResponseEvent2);
            queuedCommand.Caller.SendAsync(connectedResponseEvent2.EventName, connectedResponseEvent2);
        }

        private void ProcessGetUnitHealthCommand(QueuedCommand queuedCommand)
        {
            var unitHealth = _proxy?.GetUnitHealth();
            var healthResponseEvent1 = new GetUnitHealthResponseEvent(queuedCommand.Command);
            healthResponseEvent1.UnitHealthModel = unitHealth;
            healthResponseEvent1.Success = unitHealth != null;
            var healthResponseEvent2 = healthResponseEvent1;
            LogEvent(">>> GetUnitHealthResponseEvent", healthResponseEvent2);
            queuedCommand.Caller.SendAsync(healthResponseEvent2.EventName, healthResponseEvent2);
        }

        private void ProcessRebootCardReaderCommand(QueuedCommand queuedCommand)
        {
            var flag = _proxy.Reboot();
            var readerResponseEvent1 = new RebootCardReaderResponseEvent(queuedCommand.Command);
            readerResponseEvent1.Success = flag;
            var readerResponseEvent2 = readerResponseEvent1;
            LogEvent(">>> Reboot Card Reader", readerResponseEvent2);
            queuedCommand.Caller.SendAsync(readerResponseEvent2.EventName, readerResponseEvent2);
        }

        private void ProcessReadConfigurationCommand(QueuedCommand queuedCommand)
        {
            var configCommand = queuedCommand.Command as ConfigCommand;
            string Data;
            var task = Task.Run(() =>
                !_proxy.ReadConfig(configCommand.GroupNumber, configCommand.IndexNumber, out Data) ? null : Data);
            task.Wait();
            var simpleResponseEvent = new SimpleResponseEvent(queuedCommand.Command, "ReadConfiguration")
            {
                Data = task?.Result
            };
            LogEvent(">>> ReadConfiguration", simpleResponseEvent);
            queuedCommand.Caller.SendAsync("ReadConfiguration", simpleResponseEvent);
        }

        private void ProcessWriteConfigurationCommand(QueuedCommand queuedCommand)
        {
            var configCommand = queuedCommand.Command as ConfigCommand;
            var task = Task.Run(() =>
                _proxy.WriteConfig(configCommand.GroupNumber, configCommand.IndexNumber, configCommand.Value));
            task.Wait();
            var simpleResponseEvent = new SimpleResponseEvent(queuedCommand.Command, "WriteConfiguration")
            {
                Data = task?.Result.ToString()
            };
            LogEvent(">>> WriteConfiguration", simpleResponseEvent);
            queuedCommand.Caller.SendAsync("WriteConfiguration", simpleResponseEvent);
        }

        private void ProcessValidateVersionCommand(QueuedCommand queuedCommand)
        {
            if (!(queuedCommand?.Command is ValidateVersionCommand command))
                return;
            var flag = Domain.DeviceService.IsClientVersionCompatible(command.DeviceServiceClientVersion);
            var versionResponseEvent1 = new ValidateVersionResponseEvent(command);
            versionResponseEvent1.ValidateVersionModel = new ValidateVersionModel
            {
                IsCompatible = flag,
                DeviceServiceVersion = Domain.DeviceService.AssemblyVersion
            };
            versionResponseEvent1.Success = true;
            var versionResponseEvent2 = versionResponseEvent1;
            LogEvent(">>> validateVersionResponseEvent", versionResponseEvent2);
            queuedCommand.Caller.SendAsync("ValidateVersionReponseEvent", versionResponseEvent2);
        }

        private void ProcessGetCardInsertedStatusCommand(QueuedCommand queuedCommand)
        {
            if (queuedCommand == null)
                return;
            var insertedStatus = _proxy.CheckIfCardInserted();
            var statusResponseEvent1 = new GetCardInsertedStatusResponseEvent(queuedCommand.Command);
            statusResponseEvent1.CardInsertedStatus = insertedStatus;
            statusResponseEvent1.Success = true;
            var statusResponseEvent2 = statusResponseEvent1;
            LogEvent(">>> GetCardInsertedStatusResponseEvent", statusResponseEvent2);
            queuedCommand.Caller.SendAsync("GetCardInsertedStatusResponseEvent", statusResponseEvent2);
        }

        private void ProcessSupportsEMVCommand(QueuedCommand queuedCommand)
        {
            if (queuedCommand == null)
                return;
            var supportsEmv = _proxy.SupportsEMV;
            var emvResponseEvent1 = new SupportsEMVResponseEvent(queuedCommand.Command);
            emvResponseEvent1.Success = true;
            emvResponseEvent1.SuportsEMV = supportsEmv;
            var emvResponseEvent2 = emvResponseEvent1;
            LogEvent(">>> SupportsEMVResponseEvent", emvResponseEvent2);
            queuedCommand.Caller.SendAsync("SupportsEMVResponseEvent", emvResponseEvent2);
        }

        private void ProcessCheckActivationCommand(QueuedCommand queuedCommand)
        {
            if (!(queuedCommand?.Command is CheckActivationCommand command))
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogDebug(string.Format("CheckActivation called for Kiosk ID {0}", command.Request.KioskId));
            var result = _activationService.CheckAndActivate(command.Request).Result;
            var activationResponseEvent1 = new CheckActivationResponseEvent(command);
            activationResponseEvent1.Success = result;
            var activationResponseEvent2 = activationResponseEvent1;
            LogEvent(">>> checkActivationResponseEvent", activationResponseEvent2);
            queuedCommand.Caller.SendAsync("CheckActivationResponseEvent", activationResponseEvent2);
        }

        private async void ProcessCheckDeviceStatusCommand(QueuedCommand queuedCommand)
        {
            if (!(queuedCommand.Command is CheckDeviceStatusCommand command))
                return;
            var responseEvent = new CheckDeviceStatusResponseEvent(command);
            try
            {
                StandardResponse standardResponse;
                if (command.Request != null)
                    standardResponse = await _deviceStatusService.PostDeviceStatus(command.Request);
                else
                    standardResponse = await _deviceStatusService.PostDeviceStatus();
                responseEvent.Success = standardResponse.Success && standardResponse.StatusCode == 200;
            }
            catch (Exception ex)
            {
                var logger = _logger;
                if (logger != null)
                    logger.LogError(ex, "ProcessCheckDeviceStatusCommand Error: " + ex.Message);
                responseEvent.Success = false;
            }

            await queuedCommand.Caller.SendAsync(responseEvent.EventName, responseEvent);
            responseEvent = null;
        }

        private async void ProcessReadCardCommand(QueuedCommand queuedCommand)
        {
            var command = queuedCommand?.Command as ReadCardCommand;
            if (command == null)
                return;
            var logger1 = _logger;
            if (logger1 != null)
                logger1.LogInformation("<<<CardReaderHub.ExecuteBaseCommand " + JsonConvert.SerializeObject(command));
            if (queuedCommand.Cancelled)
            {
                var logger2 = _logger;
                if (logger2 == null)
                    return;
                logger2.LogInformation(string.Format(
                    "Queued ReadCardCommand not run because it was Canceled.  RequestId: {0}", command.RequestId));
            }
            else
            {
                var cancellationTokenSource = new CancellationTokenSource();
                _cancellationTokenSources[command.RequestId] = cancellationTokenSource;
                var cancellationToken = cancellationTokenSource.Token;
                var num = await _proxy.ReadCard(command.Request, cancellationToken, cardResult =>
                {
                    var caller = queuedCommand.Caller;
                    if (caller != null)
                    {
                        BaseResponseEvent responseEvent;
                        switch (cardResult)
                        {
                            case EMVCardReadModel emvCardReadModel2:
                                responseEvent = new EMVCardReadResponseEvent(command)
                                {
                                    Data = emvCardReadModel2
                                };
                                ProcessCardReadModel(responseEvent, cancellationToken);
                                break;
                            case EncryptedCardReadModel encryptedCardReadModel2:
                                responseEvent = new EncryptedCardReadResponseEvent(command)
                                {
                                    Data = encryptedCardReadModel2
                                };
                                ProcessCardReadModel(responseEvent, cancellationToken);
                                break;
                            default:
                                responseEvent = new UnencryptedCardReadResponseEvent(command)
                                {
                                    Data = cardResult as UnencryptedCardReadModel
                                };
                                ProcessCardReadModel(responseEvent, cancellationToken);
                                break;
                        }

                        caller.SendAsync(responseEvent.EventName, responseEvent);
                    }
                    else
                    {
                        _logger.LogInformation(string.Format("Unable to find active read card client proxy for {0}.",
                            command?.RequestId));
                    }
                }, (eventName, eventData) =>
                {
                    var caller = queuedCommand.Caller;
                    if (caller != null)
                        switch (eventName)
                        {
                            case "CardRemovedResponseEvent":
                                caller.SendAsync("CardRemovedResponseEvent", new CardRemoveResponseEvent(command));
                                break;
                            default:
                                caller.SendAsync(eventName, new SimpleResponseEvent(command, eventName, eventData));
                                break;
                        }
                    else
                        _logger.LogInformation(string.Format("Unable to find remove card client proxy for {0}.",
                            command?.RequestId));
                })
                    ? 1
                    : 0;
                _cancellationTokenSources.TryRemove(command.RequestId, out var _);
            }
        }

        public void ExecuteBaseCommand(BaseCommandRequest command)
        {
            if (command == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(
                    "<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
            AddQueuedCommand(new QueuedCommand
            {
                Command = command,
                Caller = Clients.Caller
            });
        }

        public void ExecuteDeviceServiceShutDownCommand(DeviceServiceShutDownCommand command)
        {
            if (command == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(
                    "<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
            var clientProxy = Clients?.Caller;
            if (_applicationControl == null)
                return;
            Task.Run(() =>
            {
                var flag = _applicationControl.ShutDown(command.ForceShutDown, command.Reason);
                var downResponseEvent = new DeviceServiceShutDownResponseEvent(command)
                {
                    Success = flag
                };
                LogEvent(">>> DeviceServiceShutDownResponseEvent", downResponseEvent);
                var iclientProxy = clientProxy;
                if (iclientProxy == null)
                    return;
                iclientProxy.SendAsync("DeviceServiceShutDownResponseEvent", downResponseEvent);
            });
        }

        public void ExecuteDeviceServiceCanShutDownCommand(DeviceServiceCanShutDownCommand command)
        {
            if (command == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(
                    "<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
            if (command == null || _applicationControl == null)
                return;
            _applicationControl.SetCanShutDownClientResponse(command.CanShutDown);
        }

        public void SetCommandQueueState(SetCommandQueueStateCommand command)
        {
            if (command == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(
                    "<<< " + command?.CommandName + " Command " + JsonConvert.SerializeObject(command));
            CommandQueuePaused = command.CommandQueuePaused;
        }

        public void ExecuteReadCardCommand(ReadCardCommand command)
        {
            if (command == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("<<< ReadCardCommand " + JsonConvert.SerializeObject(command));
            AddQueuedCommand(new QueuedCommand
            {
                Command = command,
                Caller = Clients.Caller
            });
        }

        private void ProcessCardReadModel(
            BaseResponseEvent responseEvent,
            CancellationToken cancellationToken)
        {
            var readResponseEvent1 = responseEvent as ICardReadResponseEvent;
            if (cancellationToken.IsCancellationRequested)
            {
                responseEvent.Success = false;
                if (readResponseEvent1 != null)
                {
                    if (readResponseEvent1.GetBase87CardReadModel() == null &&
                        responseEvent is UnencryptedCardReadResponseEvent readResponseEvent2)
                        readResponseEvent2.Data = new UnencryptedCardReadModel();
                    readResponseEvent1.GetBase87CardReadModel().Status = ResponseStatus.Cancelled;
                }
            }

            var baseResponseEvent = responseEvent;
            int num;
            if (readResponseEvent1 == null)
            {
                num = 0;
            }
            else
            {
                var status = readResponseEvent1.GetBase87CardReadModel()?.Status;
                var responseStatus = ResponseStatus.Success;
                num = (status.GetValueOrDefault() == responseStatus) & status.HasValue ? 1 : 0;
            }

            baseResponseEvent.Success = num != 0;
            LogEvent(string.Format("CardReaderHub.ExecuteBaseCommand {0}", responseEvent?.GetType()), responseEvent);
        }

        public void ExecuteCancelCommand(CancelCommandRequest cancelCommandRequest)
        {
            if (cancelCommandRequest == null)
                return;
            var logger1 = _logger;
            if (logger1 != null)
                logger1.LogInformation("CardReaderHub.ExecuteCancelCommand " +
                                       JsonConvert.SerializeObject(cancelCommandRequest));
            var flag = false;
            CancellationTokenSource cancellationTokenSource;
            _cancellationTokenSources.TryGetValue(cancelCommandRequest.CommandToCancelRequestId,
                out cancellationTokenSource);
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                _proxy.ReadCancel();
                flag = true;
            }
            else
            {
                lock (_syncObject)
                {
                    foreach (var command in _commandQueue)
                    {
                        var requestId = command?.Command.RequestId;
                        var nullable1 = cancelCommandRequest?.CommandToCancelRequestId;
                        if ((requestId.HasValue == nullable1.HasValue
                                ? requestId.HasValue
                                    ? requestId.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0
                                    : 1
                                : 0) != 0)
                        {
                            command.Cancelled = true;
                            var logger2 = _logger;
                            if (logger2 != null)
                            {
                                Guid? nullable2;
                                if (command == null)
                                {
                                    nullable1 = new Guid?();
                                    nullable2 = nullable1;
                                }
                                else
                                {
                                    nullable2 = command.Command.RequestId;
                                }

                                logger2.LogInformation(string.Format("Canceled queued command {0}", nullable2));
                            }

                            flag = true;
                        }
                    }
                }
            }

            var commandResponseEvent1 = new CancelCommandResponseEvent(cancelCommandRequest);
            commandResponseEvent1.Success = flag;
            var commandResponseEvent2 = commandResponseEvent1;
            LogEvent("CardReaderHub.ExecuteCancelCommand cancelCommandResponseEvent", commandResponseEvent2);
            Clients.Caller.SendAsync("CancelCommandResponseEvent", commandResponseEvent2);
        }

        public void SendShutDownStartingEvent(
            DeviceServiceShutDownStartingEvent deviceServiceShutDownStartingEvent)
        {
            if (deviceServiceShutDownStartingEvent == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(">>> SendShutDownStartingEvent Event " +
                                      JsonConvert.SerializeObject(deviceServiceShutDownStartingEvent));
            Clients.All.SendAsync("DeviceServiceShutDownStartingEvent", deviceServiceShutDownStartingEvent);
        }

        public void SendCardReaderStateEvent(CardReaderStateEvent cardReaderStateEvent)
        {
            if (cardReaderStateEvent == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(">>> SendCardReaderStateEvent " +
                                      JsonConvert.SerializeObject(cardReaderStateEvent));
            Clients.All.SendAsync("CardReaderStateEvent", cardReaderStateEvent);
        }

        public void ExecuteValidateVersionCommand(ValidateVersionCommand validateVersionCommand)
        {
            if (validateVersionCommand == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("<<< ValidateVersion Command " +
                                      JsonConvert.SerializeObject(validateVersionCommand));
            AddQueuedCommand(new QueuedCommand
            {
                Command = validateVersionCommand,
                Caller = Clients.Caller
            });
        }

        public void ExecuteCheckActivationCommand(CheckActivationCommand checkActivaionCommand)
        {
            if (checkActivaionCommand == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation(string.Format("<<< CheckActivation Command {0}", checkActivaionCommand.Scrub()));
            AddQueuedCommand(new QueuedCommand
            {
                Command = checkActivaionCommand,
                Caller = Clients.Caller
            });
        }

        public void ExecuteCheckDeviceStatusCommand(CheckDeviceStatusCommand checkDeviceStatusCommand)
        {
            if (checkDeviceStatusCommand == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("<<< CheckDeviceStatus Command " +
                                      JsonConvert.SerializeObject(checkDeviceStatusCommand.Scrub()));
            AddQueuedCommand(new QueuedCommand
            {
                Command = checkDeviceStatusCommand,
                Caller = Clients.Caller
            });
        }

        public void ExecuteReportAuthorizeResultCommand(
            ReportAuthorizeResultCommand reportAuthorizeResultCommand)
        {
            if (reportAuthorizeResultCommand == null)
                return;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("<<< ReportAuthorizeResult Command " +
                                      JsonConvert.SerializeObject(reportAuthorizeResultCommand));
            var success = reportAuthorizeResultCommand.Success;
            _proxy.SetAuthorizationResponse(success);
            var resultResponseEvent1 = new ReportAuthorizationResultResponseEvent(reportAuthorizeResultCommand);
            resultResponseEvent1.Success = success;
            var resultResponseEvent2 = resultResponseEvent1;
            LogEvent(">>> ReportAuthorizeResultResponseEvent", resultResponseEvent2);
            Clients.Caller.SendAsync("ReportAuthorizationResultResponseEvent", resultResponseEvent2);
        }

        private void LogEvent(string eventContext, BaseEvent baseEvent)
        {
            if (baseEvent == null)
                return;
            var logText = (string)null;
            var str = !(baseEvent is EMVCardReadResponseEvent responseEvent1)
                ? !(baseEvent is EncryptedCardReadResponseEvent responseEvent2)
                    ? !(baseEvent is UnencryptedCardReadResponseEvent responseEvent3)
                        ? JsonConvert.SerializeObject(baseEvent)
                        : ObfuscateUnencryptedCardReadResponseEvent(responseEvent3, logText)
                    : ObfuscateEncryptedCardReadResponseEvent(responseEvent2, logText)
                : ObfuscateEMVCardReadResponseEvent(responseEvent1, logText);
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogInformation(eventContext + " " + str);
        }

        private string ObfuscateEMVCardReadResponseEvent(
            EMVCardReadResponseEvent responseEvent,
            string logText)
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
            EncryptedCardReadResponseEvent responseEvent,
            string logText)
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
            UnencryptedCardReadResponseEvent responseEvent,
            string logText)
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
                var str = JsonConvert.SerializeObject(responseEvent, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                responseEventLogText = JsonConvert.SerializeObject(ObfuscateJsonAction(str));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception in CardReaderHub.CreateResponseEventLogText while deserializing a CardReadResponseEvent");
            }

            return responseEventLogText;
        }

        public virtual Task OnConnectedAsync()
        {
            _logger.LogInformation("CardReaderHub.OnConnectedAsync event");
            _analytics?.ClientConnectedToHub();
            SendMessage("Hello");
            if (_proxy?.UnitData?.IsTampered ?? false)
                SendSimpleEvent(new DeviceTamperedEvent());
            var cardReaderState = _proxy?.GetCardReaderState();
            if (cardReaderState != null)
                SendCardReaderStateEvent(new CardReaderStateEvent
                {
                    CardReaderState = cardReaderState
                });
            return base.OnConnectedAsync();
        }

        private void ClearCommands()
        {
            CommandQueuePaused = true;
            var concurrentQueue = new ConcurrentQueue<QueuedCommand>();
            foreach (var command in _commandQueue)
                if (command.ConnectionId != Context.ConnectionId)
                {
                    concurrentQueue.Enqueue(command);
                }
                else
                {
                    CancellationTokenSource cancellationTokenSource;
                    _cancellationTokenSources.TryRemove(command.Command.RequestId, out cancellationTokenSource);
                    cancellationTokenSource?.Cancel();
                }

            _commandQueue = concurrentQueue;
            if (ActiveCommand?.ConnectionId == Context.ConnectionId)
            {
                _cancellationTokenSources[ActiveCommand.Command.RequestId].Cancel();
                ActiveCommand.Cancelled = true;
                if (ActiveCommand.Command.SignalRCommand == "ExecuteReadCardCommand")
                    _proxy.ReadCancel();
            }

            CommandQueuePaused = false;
        }

        public virtual Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("CardReaderHub.OnDisconnectedAsync event");
            _analytics?.ClientDisconnectedFromHub();
            if (exception != null)
                _logger.LogError(exception, "CardReaderHub.OnDisconnectedAsync - Unhandled exception.");
            ClearCommands();
            return base.OnDisconnectedAsync(exception);
        }

        private class QueuedCommand
        {
            public BaseCommandRequest Command { get; set; }

            public string ConnectionId { get; set; }

            public IClientProxy Caller { get; set; }

            public bool Cancelled { get; set; }
        }
    }
}