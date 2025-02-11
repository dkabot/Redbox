using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Certificate;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API.Services.IoT
{
    public class MqttProxy : IMqttProxy
    {
        private const int IoTCoreBrokerPort = 8883;
        private readonly AppSettings _appSettings;
        private readonly ICertificateService _certService;
        private readonly IIotCommandDispatch _commandDispatch;
        private readonly IMqttClient _iotClient;
        private readonly IIoTStatisticsService _ioTStatisticsService;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger<MqttProxy> _logger;
        private readonly IStoreService _store;
        private IMqttClientOptions _clientOptions;
        private int _inConnect;
        private bool _isRegistered;
        private bool _isSubscribed;
        private bool _isTimeSynced;

        public MqttProxy(
            ILogger<MqttProxy> logger,
            ICertificateService certService,
            IIotCommandDispatch commandDispatch,
            IStoreService store,
            IIoTStatisticsService ioTStatisticsService,
            IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _certService = certService;
            _commandDispatch = commandDispatch;
            _store = store;
            _appSettings = appSettings.Value;
            _ioTStatisticsService = ioTStatisticsService;
            _iotClient = new MqttFactory().CreateMqttClient();
            _iotClient.UseApplicationMessageReceivedHandler(async e => await MessageReceivedHandler(e));
            _iotClient.UseConnectedHandler(async e => await ConnectedHandler(e));
            _iotClient.UseDisconnectedHandler(async e => await DisconnectedHandler(e));
            _logger.LogInfoWithSource("MqttProxy instantiated", ".ctor",
                "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
        }

        private IEnumerable<string> Topics =>
            new List<string>
            {
                string.Format("redbox/kiosk/{0}/command", _store.KioskId),
                "redbox/kiosk/market/" + _store.Market + "/command",
                "redbox/kiosk/all/command"
            };

        private bool IsMqttConnected
        {
            get
            {
                var iotClient = _iotClient;
                return iotClient != null && iotClient.IsConnected;
            }
        }

        public async Task<bool> PublishIoTCommandAsync(string topicName, IoTCommandModel request)
        {
            return await PublishMessageAsync(topicName, request.ToJson(),
                (request != null ? request.LogRequest : null) ?? true, request.QualityOfServiceLevel);
        }

        public async Task<bool> PublishAwsRuleActionAsync(
            string ruleName,
            string payload,
            bool logMessage = true,
            QualityOfServiceLevel qualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce)
        {
            return await PublishMessageAsync(ruleName, payload, logMessage, qualityOfServiceLevel);
        }

        public async Task<bool> CheckConnectionAsync()
        {
            bool flag;
            if (Interlocked.CompareExchange(ref _inConnect, 1, 0) == 1)
            {
                _logger.LogError("Already in CheckConnectionAsync, exiting");
                flag = false;
            }
            else
            {
                try
                {
                    var flag2 = await Connect();
                    flag = flag2;
                }
                finally
                {
                    _inConnect = 0;
                }
            }

            return flag;
        }

        public async Task Disconnect()
        {
            try
            {
                var iotClient = _iotClient;
                await (iotClient != null ? iotClient.DisconnectAsync() : null);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex,
                    "Exception occured while attempting to disconnect from MqttClient, this can happen if the connection lost already.",
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            }

            Clear();
        }

        private async Task ConnectedHandler(MqttClientConnectedEventArgs e)
        {
            _logger.LogInfoWithSource("Connected Result: " + e.ConnectResult.ToJson(),
                "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            if (e.ConnectResult.ResultCode != MqttClientConnectResultCode.Success)
                return;
            var num = await _ioTStatisticsService.RecordConnectionSuccess() ? 1 : 0;
        }

        private async Task DisconnectedHandler(MqttClientDisconnectedEventArgs e)
        {
            if (e.Exception == null)
                _logger.LogInfoWithSource(string.Format("Received DisconnectedHandler Event Reason: {0}", e.Reason),
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            else
                _logger.LogErrorWithSource(e.Exception,
                    string.Format("Received DisconnectedHandler Event Reason: {0}", e.Reason),
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            var num = await _ioTStatisticsService.RecordDisconnection() ? 1 : 0;
        }

        private async Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = e?.ApplicationMessage?.Payload;
            if (payload == null)
            {
                _logger.LogInfoWithSource(
                    string.Format("Message received Topic = {0}, Payload Length = 0, QoS = {1}, Retain = {2}",
                        e.ApplicationMessage?.Topic, e?.ApplicationMessage?.QualityOfServiceLevel,
                        e?.ApplicationMessage?.Retain), "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            }
            else
            {
                var str = Encoding.UTF8.GetString(payload);
                _logger.LogInfoWithSource(
                    string.Format("Message received Topic = {0}, Payload Length = {1}, QoS = {2}, Retain = {3}",
                        e?.ApplicationMessage?.Topic, str != null ? str.Length : 0,
                        e?.ApplicationMessage?.QualityOfServiceLevel, e?.ApplicationMessage?.Retain),
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            }

            await Task.Run(async () => await _commandDispatch.Execute(payload, e?.ApplicationMessage?.Topic));
        }

        private async Task<bool> PublishMessageAsync(
            string topicName,
            string message,
            bool logMessage = true,
            QualityOfServiceLevel qualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce)
        {
            var flag = false;
            if (!IsMqttConnected)
            {
                _logger.LogInfoWithSource("Not connected. Cannot publish to topic: " + topicName,
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                return false;
            }

            string str1;
            if (!logMessage)
            {
                var str2 = message;
                str1 = string.Format("Message length: {0}", str2 != null ? str2.Length : 0);
            }
            else
            {
                str1 = "Message : " + message;
            }

            var str3 = str1;
            _logger.LogInfoWithSource("Publishing message to topic: " + topicName + " ." + str3,
                "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            try
            {
                var applicationMessage = new MqttApplicationMessageBuilder().WithTopic(topicName).WithPayload(message)
                    .WithQualityOfServiceLevel(qualityOfServiceLevel.ToMqttQOS()).WithRetainFlag(false).Build();
                using (var cancellationToken = new CancellationTokenSource(5000))
                {
                    var timer = Stopwatch.StartNew();
                    var clientPublishResult =
                        await _iotClient.PublishAsync(applicationMessage, cancellationToken.Token);
                    timer.Stop();
                    if (clientPublishResult.ReasonCode == MqttClientPublishReasonCode.Success)
                    {
                        flag = true;
                        _logger.LogInfoWithSource(
                            string.Format(
                                "Publish successfully for Packet Identifier: {0}, Topic: {1}, QoS Level {2}, Duration: {3}",
                                clientPublishResult.PacketIdentifier, topicName, qualityOfServiceLevel,
                                timer.ElapsedMilliseconds),
                            "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                    }
                    else
                    {
                        _logger.LogErrorWithSource(
                            string.Format(
                                "Publish failed for ReasonCode: {0} - {1}, Topic: {2}, QoS Level {3}, Duration: {4}",
                                clientPublishResult.ReasonCode, clientPublishResult.ReasonString, topicName,
                                qualityOfServiceLevel, timer.ElapsedMilliseconds),
                            "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                        flag = false;
                    }

                    timer = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception while publishing message to topic " + topicName,
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                flag = false;
            }

            return flag;
        }

        private async Task<bool> SubscribeTopics(IEnumerable<string> topics)
        {
            _logger.LogInfoWithSource("Attempting to subscribe to topics",
                "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            _isSubscribed = false;
            bool isSubscribed;
            if ((topics != null && topics.Count() == 0) || !IsMqttConnected)
            {
                isSubscribed = false;
            }
            else
            {
                _logger.LogInfoWithSource("calling _iotClient.Subscribe for topics: " + topics.ToJson(),
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                var builder = new MqttClientSubscribeOptionsBuilder();
                topics.ToList().ForEach(delegate(string t)
                {
                    builder.WithTopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce);
                });
                try
                {
                    using (var cancellationToken = new CancellationTokenSource(5000))
                    {
                        var mqttClientSubscribeResult =
                            await _iotClient.SubscribeAsync(builder.Build(), cancellationToken.Token);
                        var mqttClientSubscribeResult2 = mqttClientSubscribeResult;
                        mqttClientSubscribeResult2.Items.ForEach(delegate(MqttClientSubscribeResultItem item)
                        {
                            _logger.LogInformation(string.Format("Subscribe Topic: {0} Result: {1}",
                                item.TopicFilter.Topic, item.ResultCode));
                        });
                        _isSubscribed = mqttClientSubscribeResult2.Items.All(item =>
                            item.ResultCode <= MqttClientSubscribeResultCode.GrantedQoS2);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogErrorWithSource(ex, "Exception occurred",
                        "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                }

                if (_isSubscribed)
                    _logger.LogInfoWithSource("Successfully subscribed to Topics: " + topics.ToJson(),
                        "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                else
                    _logger.LogErrorWithSource("Failed to subscription to Topics: " + topics.ToJson(),
                        "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                isSubscribed = _isSubscribed;
            }

            return isSubscribed;
        }

        private async Task<bool> Connect()
        {
            try
            {
                if (!IsMqttConnected)
                    await ConnectToMqtt();
                if (IsMqttConnected && !_isSubscribed)
                {
                    var num = await SubscribeTopics(Topics) ? 1 : 0;
                }

                if (IsMqttConnected && _isSubscribed && !_isRegistered)
                    Task.WaitAll(RegisterUCSVersion(), SyncTimestampAndTimezone());
                return IsMqttConnected && _isSubscribed && _isRegistered;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception occured while attempting to connect MqttClient.",
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                return false;
            }
        }

        private void Clear()
        {
            _isSubscribed = false;
            _isRegistered = false;
        }

        private async Task<bool> Initialize()
        {
            try
            {
                if (_clientOptions != null)
                    return true;
                var certificateAsync = await _certService.GetCertificateAsync();
                if (certificateAsync == null)
                {
                    _logger.LogErrorWithSource("No cert, aborting connection attempt",
                        "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                    return false;
                }

                _clientOptions = new MqttClientOptionsBuilder().WithTcpServer(_appSettings?.IoTBrokerEndpoint, 8883)
                    .WithKeepAlivePeriod(new TimeSpan(0, 0, 0, 300)).WithTls(new MqttClientOptionsBuilderTlsParameters
                    {
                        Certificates = new List<X509Certificate>
                        {
                            certificateAsync.RootCa,
                            certificateAsync.DeviceCertPfx
                        },
                        SslProtocol = SslProtocols.Tls12,
                        UseTls = true,
                        AllowUntrustedCertificates = false
                    }).WithProtocolVersion(MqttProtocolVersion.V311).WithClientId(_store.KioskId.ToString())
                    .WithCommunicationTimeout(TimeSpan.FromSeconds(60.0)).WithCleanSession().Build();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception during Initialization",
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                return false;
            }
        }

        private async Task ConnectToMqtt()
        {
            _logger.LogInfoWithSource(string.Format("Attempting to connect KioskId: {0} to mqtt", _store.KioskId),
                "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            var num1 = await _ioTStatisticsService.RecordConnectionAttempt() ? 1 : 0;
            try
            {
                Clear();
                if (!await Initialize())
                {
                    _logger.LogErrorWithSource("Initialization failed",
                        "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                    return;
                }

                using (var cancellationToken = new CancellationTokenSource(20000))
                {
                    _logger.LogInfoWithSource(
                        string.Format("MqttClient ConnectAsync completed with KioskId: {0}, and ResultCode: {1}",
                            _store.KioskId,
                            (await _iotClient.ConnectAsync(_clientOptions, cancellationToken.Token)).ResultCode),
                        "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "Exception occurred in ConnectToMqtt",
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
                var num2 = await _ioTStatisticsService.RecordConnectionException(ex?.Message +
                    ex?.InnerException?.Message)
                    ? 1
                    : 0;
                var num3 = await _certService.Validate() ? 1 : 0;
                _clientOptions = null;
            }
        }

        private async Task<bool> RegisterUCSVersion()
        {
            _isRegistered = false;
            _logger.LogInfoWithSource("Attempting to register ucs",
                "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            var message = new UCSRegisterMessage
            {
                KioskId = _store.KioskId,
                Version = "2.0",
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
            _isRegistered = await PublishMessageAsync("$aws/rules/kioskucsregister", message.ToJson());
            if (_isRegistered)
                _logger.LogInfoWithSource("Register UCS version " + message.Version + " succeeded",
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            else
                _logger.LogErrorWithSource("Register UCS version " + message.Version + " failed",
                    "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
            return _isRegistered;
        }

        private async Task SyncTimestampAndTimezone()
        {
            if (_isTimeSynced)
                return;
            var num = await PublishIoTCommandAsync("$aws/rules/kioskrestcall", new IoTCommandModel
            {
                RequestId = Guid.NewGuid().ToString(),
                Command = CommandEnum.SyncTimestampAndTimezone,
                MessageType = MessageTypeEnum.Request,
                Version = 1,
                SourceId = _store?.KioskId.ToString(),
                LogRequest = true,
                QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce
            })
                ? 1
                : 0;
            _isTimeSynced = true;
        }
    }
}