using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.FileUpdate;
using DeviceService.ComponentModel.KDS;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;
using RBA_SDK_ComponentModel;
using Timer = System.Timers.Timer;

namespace DeviceService.Domain
{
    public class IUC285Proxy : IIUC285Proxy, ITestIUC285Proxy, IFileUpdater
    {
        private const string RKI_VAS_KEY_VALID_PROD = "KCV_VKBPK=713F7E";
        private const string RKI_VAS_KEY_VALID_QA_PREFIX = "KCV_";
        private const string RKI_VAS_KEY_INVALID_PROD = "KCV_VKBPK=483320";
        private const string RKI_VAS_KEY_INTERMEDIATE = "KCV_VKBPK=D668B7";
        private const string _vendorId = "0B00";
        private const string _productId = "0057";
        private const string PASS_TYPE_IDENTIFIER_QA = "pass.com.vibes.RedboxQAEnvironment";
        private const string PASS_TYPE_IDENTIFIER_PROD = "pass.com.vibes.redbox";
        private const string PASS_TYPE_IDENTIFIER_TEST = "pass.com.redbox.testing";
        private const string PASS_TYPE_IDENTIFIER_INGENICO = "pass.com.ingenico.us.vas.test";
        private const string PASS_TYPE_IDENTIFIER_APPLE_RedBox_Testing = "pass.com.redbox.kiosk";
        private const string PASS_TYPE_GOOGLE_MERCHANT_ID_INGENICO = "70779451";
        private const string PASS_TYPE_GOOGLE_MERCHANT_ID_VIBES_QA = "90833929";
        private const string PASS_TYPE_GOOGLE_MERCHANT_ID_VIBES_PROD = "44813557";
        private const int P62_REQ_FILE_DATA_MAX_SIZE = 4064;
        private static ILogger<IUC285Proxy> _logger;
        private static IRBA_API _rba_api;
        private static int _attemptingReconnect;
        private static int _continuousResultTimeouts;
        private static readonly TimeSpan REBOOT_THRESHOLD = TimeSpan.FromMinutes(3.0);
        private static string _keys;
        private static int _connectInProgress;
        private static readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mreInsertCard = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mre3302TransactionPrepareResponse = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mre3303AuthorizationRequest = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mre3305AuthorizationConfirmation = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mre3304AuthorizationResponse = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mre09CardRemoved = new ManualResetEvent(true);
        private static readonly ManualResetEvent _mre166VasResponse = new ManualResetEvent(false);
        private static readonly ManualResetEvent _mre17MerchantDataWriteResponse = new ManualResetEvent(false);
        private static readonly AutoResetEvent _areOfflineResponse = new AutoResetEvent(false);
        private static bool _authSuccessful;

        private static readonly List<string> _ingenicoTags = new List<string>
        {
            "1000",
            "1001",
            "1002",
            "1003",
            "1004",
            "1005",
            "1006",
            "1007",
            "1008",
            "1009",
            "100A",
            "100B",
            "100C",
            "100D",
            "100E",
            "100F",
            "1010",
            "1011",
            "1012",
            "1013",
            "1014",
            "1015",
            "1016",
            "1017",
            "1018",
            "1019",
            "101A",
            "101B",
            "101C",
            "101D",
            "101E",
            "101F",
            "1020",
            "FF1D",
            "FF1E",
            "FF1F",
            "FF20",
            "FF21",
            "9000",
            "9001"
        };

        private static readonly object _getKeysLock = new object();
        private readonly AutoResetEvent _areWaitForReconnect = new AutoResetEvent(false);

        private readonly IEnumerable<ConfigDefault> _configDefaults = new List<ConfigDefault>
        {
            new ConfigDefault
            {
                Group = "0007",
                Index = "0003",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0019",
                Index = "0001",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0019",
                Index = "0002",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0019",
                Index = "0003",
                Value = "3"
            },
            new ConfigDefault
            {
                Group = "0008",
                Index = "0001",
                Value = "9"
            },
            new ConfigDefault
            {
                Group = "0003",
                Index = "0010",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0008",
                Index = "0003",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0013",
                Index = "0014",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0019",
                Index = "0021",
                Value = "1"
            },
            new ConfigDefault
            {
                Group = "0008",
                Index = "0017",
                Value = "86400"
            },
            new ConfigDefault
            {
                Group = "0007",
                Index = "0014",
                Value = "25"
            },
            new ConfigDefault
            {
                Group = "0007",
                Index = "0001",
                Value = "0"
            },
            new ConfigDefault
            {
                Group = "0007",
                Index = "0039",
                Value = "0"
            },
            new ConfigDefault
            {
                Group = "0007",
                Index = "0022",
                Value = "0"
            }
        };

        private readonly IOptionsMonitor<DeviceServiceConfig> _deviceConfig;
        private readonly Timer _healthTimer;
        private readonly IIUC285Notifier _iuc285Notifier;
        private readonly KioskData _kioskData;
        private readonly Timer _reconnectTimer;
        private readonly IApplicationSettings _settings;
        private readonly IKioskDataServiceClient _statusClient;

        private readonly List<string> VasBuilds = new List<string>
        {
            "23kA",
            "23kG",
            "23kK"
        };

        private int _connectAttempts;
        private RebootStatus _currentRebootStatus = new RebootStatus();
        private DateTime? _deviceRebootTime;
        private bool? _isConnected;
        private bool _isInitialized;
        private bool _isVasInitialized;
        private Task _rebootTask;
        private RebootType _rebootType;
        private UnitDataModel _unitData;
        public CardSourceType CardSource;

        public IUC285Proxy(
            IRBA_API rba_api,
            ILogger<IUC285Proxy> logger,
            IIUC285Notifier iuc285Notifier,
            IKioskDataServiceClient statusClient,
            IApplicationSettings settings,
            IOptionsMonitor<DeviceServiceConfig> deviceConfig)
        {
            _rba_api = rba_api;
            _logger = logger;
            _iuc285Notifier = iuc285Notifier;
            _statusClient = statusClient;
            _settings = settings;
            _deviceConfig = deviceConfig;
            if (_settings != null)
                _kioskData =
                    DataFileHelper.GetFileData<KioskData>(settings.DataFilePath, "KioskData.json", LogException);
            _reconnectTimer = new Timer
            {
                Interval = 10000.0,
                AutoReset = true,
                Enabled = false
            };
            _reconnectTimer.Elapsed += OnReconnect;
            _healthTimer = new Timer
            {
                Interval = 900000.0,
                AutoReset = true,
                Enabled = false
            };
            _healthTimer.Elapsed += OnHealthTimer;
            OnConnectedHandler += OnStartup;
            OnDisconnectedHandler += OnDisconnect;
            _deviceConfig.OnChange(OnConfigChange);
        }

        public Version EMVClessVersion { get; private set; }

        public Version EMVContactVersion { get; private set; }

        public ConnectionState ConnectionState { get; set; }

        public ErrorState ErrorState { get; set; }

        private ReadCardContext ReadCardContextData { get; set; }

        private bool IsProdEnvironment =>
            Registry.LocalMachine.OpenSubKey("Software\\Redbox\\REDS\\Kiosk Engine\\Store")?.GetValue("Environment")
                ?.ToString() == "Production";

        private DateTime DeviceRebootTime
        {
            get
            {
                if (!_deviceRebootTime.HasValue)
                {
                    int result;
                    if (int.TryParse(GetVariable_29("500"), out result))
                        _deviceRebootTime = DateTime.Now.AddSeconds(result);
                    if (!_deviceRebootTime.HasValue)
                        return DateTime.MaxValue;
                }

                return _deviceRebootTime.Value;
            }
        }

        public void PrepareToUpdateFiles()
        {
            _iuc285Notifier?.SetCommandQueueState(true);
        }

        public void CompleteFileUpdates()
        {
            _iuc285Notifier?.SetCommandQueueState(false);
        }

        public async Task<bool> WriteStream(
            Stream stream,
            string fileName,
            bool rebootAfterWrite = false,
            bool waitForReconnect = false,
            int reconnectTimeout = 180000)
        {
            _areWaitForReconnect.Reset();
            var flag = await WriteStreamInner(stream, fileName, rebootAfterWrite);
            if (flag & rebootAfterWrite & waitForReconnect && !_areWaitForReconnect.WaitOne(reconnectTimeout))
                return false;
            var kioskData = _kioskData;
            if (kioskData != null)
            {
                var kioskId = kioskData.KioskId;
            }

            return flag;
        }

        public int GetFileUpdateRevisionNumber()
        {
            var updateRevisionNumber = -1;
            if (IsConnected)
            {
                string Data;
                ReadConfig("14", "10", out Data, false);
                int result;
                if (int.TryParse(Data, out result))
                    updateRevisionNumber = result;
            }

            return updateRevisionNumber;
        }

        public bool SetFileUpdateRevisionNumber(string revisionNumber)
        {
            var flag = false;
            if (IsConnected)
                flag = WriteConfig("14", "10", revisionNumber);
            return flag;
        }

        public DateTime? GetExpectedRebootTime()
        {
            var configRebootTime = GetConfigRebootTime();
            var deviceRebootTime = DeviceRebootTime;
            if (configRebootTime == new DateTime())
                return deviceRebootTime;
            return deviceRebootTime == new DateTime() ? configRebootTime :
                DateTime.Compare(configRebootTime, deviceRebootTime) < 0 ? configRebootTime : deviceRebootTime;
        }

        public async Task PostDeviceStatus()
        {
            var kioskData = _kioskData;
            int num;
            if (kioskData == null)
            {
                num = 0;
            }
            else
            {
                var kioskId = kioskData.KioskId;
                num = 1;
            }

            DeviceStatus deviceStatus1;
            if (num != 0)
                deviceStatus1 = await CheckDeviceStatus(_kioskData.KioskId);
            else
                deviceStatus1 = null;
            var deviceStatus2 = deviceStatus1;
            if (deviceStatus2 == null)
                return;
            _statusClient?.PostDeviceStatus(deviceStatus2);
        }

        public CardReaderState GetCardReaderState()
        {
            var cardReaderState = new CardReaderState();
            cardReaderState.SupportsEMV = SupportsEMV;
            cardReaderState.IsConnected = IsConnected;
            var unitData = UnitData;
            cardReaderState.IsTampered = unitData != null && unitData.IsTampered;
            cardReaderState.SupportsVas = SupportsVas;
            cardReaderState.ConnectionState = ConnectionState;
            cardReaderState.Version = Assembly.GetExecutingAssembly().GetName().Version;
            cardReaderState.ErrorState = ErrorState;
            return cardReaderState;
        }

        public UnitDataModel UnitData
        {
            get
            {
                if (!IsConnected)
                    return null;
                if (_unitData == null)
                    _unitData = GetUnitData();
                return _unitData;
            }
            set => _unitData = value;
        }

        public event OnConnected OnConnectedHandler;

        public event OnDisconnected OnDisconnectedHandler;

        public bool IsConnected
        {
            get => _isConnected.GetValueOrDefault();
            set
            {
                _keys = null;
                _isVasInitialized = false;
                var isConnected = _isConnected;
                var flag = value;
                if (!((isConnected.GetValueOrDefault() == flag) & isConnected.HasValue))
                {
                    _isConnected = value;
                    Log(string.Format("IsConnected: {0}", value));
                    if (value)
                    {
                        ConnectionState = ConnectionState.Connected;
                        _reconnectTimer.Stop();
                        var connectedHandler = OnConnectedHandler;
                        if (connectedHandler == null)
                            return;
                        connectedHandler();
                    }
                    else
                    {
                        ConnectionState = ConnectionState.Disconnected;
                        ErrorState = ErrorState.None;
                        _reconnectTimer.Start();
                        var disconnectedHandler = OnDisconnectedHandler;
                        if (disconnectedHandler == null)
                            return;
                        disconnectedHandler(null);
                    }
                }
                else if (!value && !_reconnectTimer.Enabled)
                {
                    _reconnectTimer.Start();
                }
                else
                {
                    if (!value || !_reconnectTimer.Enabled)
                        return;
                    _reconnectTimer.Stop();
                }
            }
        }

        public bool SupportsEMV
        {
            get
            {
                var supportsEmv = false;
                Version version;
                if (TryGetCurrentTgzVersion(out version))
                    supportsEmv = version != null && (version.Major != 0 || version.Minor != 0);
                return supportsEmv;
            }
        }

        public bool SupportsVas
        {
            get
            {
                if (!IsConnected)
                    return false;
                var deviceConfig = _deviceConfig;
                return (deviceConfig != null ? !deviceConfig.CurrentValue.EnableVasCommands ? 1 : 0 : 0) == 0 &&
                       (_unitData?.ApplicationVersion == null ? 0 :
                           VasBuilds.Contains(_unitData.ApplicationVersion) ? 1 : 0) != 0 && HasValidVasRKI &&
                       _isInitialized;
            }
        }

        public bool HasValidVasRKI => HasRKIKey(IsProdEnvironment ? "KCV_VKBPK=713F7E" : "KCV_");

        public bool HasInvalidVasRKI => IsProdEnvironment && HasRKIKey("KCV_VKBPK=483320");

        public bool HasIntermediateVasRepairKey => IsProdEnvironment && HasRKIKey("KCV_VKBPK=D668B7");

        public UnitHealthModel GetUnitHealth()
        {
            var num = (int)_rba_api.SetParam(PARAMETER_ID.P08_REQ_REQUEST_TYPE, "0");
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M08_HEALTH_STAT);
            HandleAndLogResult(result, "08 Health Stat");
            if (result != ERROR_ID.RESULT_SUCCESS)
                return null;
            var unitHealth = new UnitHealthModel
            {
                MSRSwipeCount = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_MSR_SWIPES)
            };
            Log("P08_RES_COUNT_MSR_SWIPES: " + unitHealth.MSRSwipeCount);
            unitHealth.BadTrack1Reads = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_BAD_TRACK1_READS);
            Log("P08_RES_COUNT_BAD_TRACK1_READS: " + unitHealth.BadTrack1Reads);
            unitHealth.BadTrack2Reads = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_BAD_TRACK2_READS);
            Log("P08_RES_COUNT_BAD_TRACK2_READS: " + unitHealth.BadTrack2Reads);
            unitHealth.BadTrack3Reads = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_BAD_TRACK3_READS);
            Log("P08_RES_COUNT_BAD_TRACK3_READS: " + unitHealth.BadTrack3Reads);
            unitHealth.RebootCount = _rba_api.GetParam(PARAMETER_ID.P08_RES_COUNT_REBOOT);
            Log("P08_RES_COUNT_REBOOT: " + unitHealth.RebootCount);
            unitHealth.DeviceName = _rba_api.GetParam(PARAMETER_ID.P08_RES_DEVICE_NAME);
            Log("P08_RES_DEVICE_NAME: " + unitHealth.DeviceName);
            unitHealth.SerialNumber = _rba_api.GetParam(PARAMETER_ID.P08_RES_SERIAL_NUMBER);
            Log("P08_RES_SERIAL_NUMBER: " + unitHealth.SerialNumber);
            unitHealth.OSVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_OS_VERSION);
            Log("P08_RES_OS_VERSION: " + unitHealth.OSVersion);
            unitHealth.AppVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_APP_VERSION);
            Log("P08_RES_APP_VERSION: " + unitHealth.AppVersion);
            unitHealth.SecurityLibVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_SECURITY_LIB_VERSION);
            Log("P08_RES_SECURITY_LIB_VERSION: " + unitHealth.SecurityLibVersion);
            unitHealth.TDAVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_TDA_VERSION);
            Log("P08_RES_TDA_VERSION: " + unitHealth.TDAVersion);
            unitHealth.EFTLVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_EFTL_VERSION);
            Log("P08_RES_EFTL_VERSION: " + unitHealth.EFTLVersion);
            unitHealth.EFTPVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_EFTP_VERSION);
            Log("P08_RES_EFTP_VERSION: " + unitHealth.EFTPVersion);
            unitHealth.RamSize = _rba_api.GetParam(PARAMETER_ID.P08_RES_RAM_SIZE);
            Log("P08_RES_RAM_SIZE: " + unitHealth.RamSize);
            unitHealth.FlashSize = _rba_api.GetParam(PARAMETER_ID.P08_RES_FLASH_SIZE);
            Log("P08_RES_FLASH_SIZE: " + unitHealth.FlashSize);
            unitHealth.ManufactureDate = _rba_api.GetParam(PARAMETER_ID.P08_RES_MANUFACTURE_DATE);
            Log("P08_RES_MANUFACTURE_DATE: " + unitHealth.ManufactureDate);
            unitHealth.CPEMType = _rba_api.GetParam(PARAMETER_ID.P08_RES_CPEM_TYPE);
            Log("P08_RES_CPEM_TYPE: " + unitHealth.CPEMType);
            unitHealth.AppName = _rba_api.GetParam(PARAMETER_ID.P08_RES_APP_NAME);
            Log("P08_RES_APP_NAME: " + unitHealth.AppName);
            unitHealth.ManufactureId = _rba_api.GetParam(PARAMETER_ID.P08_RES_MANUFACTURE_ID);
            Log("P08_RES_MANUFACTURE_ID: " + unitHealth.ManufactureId);
            unitHealth.DigitizerVersion = _rba_api.GetParam(PARAMETER_ID.P08_RES_DIGITIZER_VERSION);
            Log("P08_RES_DIGITIZER_VERSION: " + unitHealth.DigitizerVersion);
            unitHealth.ManufacturingSerialNumber = _rba_api.GetParam(PARAMETER_ID.P08_RES_MANUFACTURING_SERIAL_NUMBER);
            Log("P08_RES_MANUFACTURING_SERIAL_NUMBER: " + unitHealth.ManufacturingSerialNumber);
            return unitHealth;
        }

        public async Task<bool> ReadCard(
            ICardReadRequest request,
            CancellationToken token,
            Action<Base87CardReadModel> jobCompletedCallback,
            Action<string, string> eventsCallback)
        {
            Log("ENTER READ CARD");
            return await ReadCardInner(request, token, jobCompletedCallback, eventsCallback);
        }

        public void SetAuthorizationResponse(bool success)
        {
            _authSuccessful = success;
            _mre3304AuthorizationResponse.Set();
        }

        public bool UpdateDeviceConfiguration()
        {
            var flag = false;
            var num = 3;
            foreach (var configDefault in _configDefaults)
            {
                var Data = (string)null;
                flag = false;
                while (!flag && num > 0)
                {
                    flag = ReadConfig(configDefault.Group, configDefault.Index, out Data, false) && Data != null;
                    if (!flag)
                        --num;
                }

                if (flag)
                {
                    if (num > 0)
                    {
                        if (string.Compare(Data, configDefault.Value, true) != 0)
                        {
                            flag = WriteConfig(configDefault.Group, configDefault.Index, configDefault.Value, false);
                            if (!flag)
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (flag)
                flag = UpdateCardConfigurationData();
            var timeSpan1 = DateTime.Now.TimeOfDay;
            SetVariable_28("201", timeSpan1.ToString("hhmmss"));
            var dateTime = DateTime.Now;
            dateTime = dateTime.Date;
            SetVariable_28("202", dateTime.ToString("MMddyy"));
            if (_settings != null)
            {
                var fileData =
                    DataFileHelper.GetFileData<TimeSpan>(_settings.DataFilePath, "RebootTime.json", LogException);
                timeSpan1 = new TimeSpan();
                var timeSpan2 = timeSpan1;
                if (fileData == timeSpan2)
                    SetRebootTime();
            }

            Offline();
            return flag;
        }

        public async Task<bool> WriteFile(string fullPath, bool rebootAfterWrite = false)
        {
            return !string.IsNullOrEmpty(fullPath) && await WriteFileInner(fullPath, rebootAfterWrite);
        }

        public InsertedStatus CheckIfCardInserted()
        {
            var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P33_01_REQ_STATUS, "00");
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P33_01_REQ_EMVH_CURRENT_PACKET_NBR, "0");
            var num4 = (int)_rba_api.SetParam(PARAMETER_ID.P33_01_REQ_EMVH_PACKET_TYPE, "0");
            HandleAndLogResult(_rba_api.ProcessMessage(MESSAGE_ID.M33_01_EMV_STATUS), "3301CardStatus");
            return M3301Status();
        }

        public string GetVariable_29(string varID)
        {
            var num = (int)_rba_api.SetParam(PARAMETER_ID.P29_REQ_VARIABLE_ID, varID);
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M29_GET_VARIABLE);
            var status = _rba_api.GetParam(PARAMETER_ID.P29_RES_STATUS);
            var statusText = GetStatusText(status);
            var variable29 = _rba_api.GetParam(PARAMETER_ID.P29_RES_VARIABLE_DATA);
            HandleAndLogResult(result,
                "29 Get Variable ID: " + varID + ", Data: " + variable29 + " - P29_RES_STATUS: (" + status + ") - " +
                statusText + " -");
            return variable29;
        }

        public bool ReadConfig(string group, string index, out string Data, bool sendOffline = true)
        {
            if (sendOffline)
                Offline();
            var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P61_REQ_GROUP_NUM, group);
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P61_REQ_INDEX_NUM, index);
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M61_CONFIGURATION_READ);
            var statusNum = _rba_api.GetParam(PARAMETER_ID.P61_RES_STATUS);
            Data = _rba_api.GetParam(PARAMETER_ID.P61_RES_DATA_CONFIG_PARAMETER);
            HandleAndLogResult(result,
                "61 Configuration Read Group#: " + group + ", Index#: " + index + ", Data: " + Data +
                " - P61_RES_STATUS: (" + statusNum + ") - " + ConfigStatusText(statusNum) + " -");
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        public bool WriteConfig(string group, string index, string value, bool sendOffline = true)
        {
            if (sendOffline)
                Offline();
            var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P60_REQ_GROUP_NUM, group.PadLeft(4, '0'));
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P60_REQ_INDEX_NUM, index.PadLeft(4, '0'));
            var num4 = (int)_rba_api.SetParam(PARAMETER_ID.P60_REQ_DATA_CONFIG_PARAM, value);
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M60_CONFIGURATION_WRITE);
            var statusNum = _rba_api.GetParam(PARAMETER_ID.P60_RES_STATUS);
            HandleAndLogResult(result,
                "60 Configuration Write Group#: " + group + ", Index#: " + index + ", Data: " + value +
                " - P60_RES_STATUS: (" + statusNum + ") - " + ConfigStatusText(statusNum) + " -");
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        public bool Reboot(RebootType rebootType = RebootType.Manual)
        {
            _rebootType = rebootType;
            return RebootInner();
        }

        public bool Reconnect()
        {
            Log("Reconnecting to Device.");
            if (!IsConnected || Disconnect())
                return Connect();
            Log("Failed To Disconnect");
            return false;
        }

        public bool PingDevice()
        {
            return SendCustomMessage("PING");
        }

        public void ReadCancel()
        {
            if (ReadCardContextData.CanContinue(VasMode.VasAndPay))
                ReadCardContextData.State = ReadCardContextState.Canceled;
            _mre.Set();
            _mreInsertCard.Set();
            _mre09CardRemoved.Set();
            _mre3302TransactionPrepareResponse.Set();
            _mre3303AuthorizationRequest.Set();
            _mre3304AuthorizationResponse.Set();
            _mre3305AuthorizationConfirmation.Set();
            _mre166VasResponse.Set();
        }

        public async Task<DeviceStatus> CheckDeviceStatus(long kioskId)
        {
            var deviceStatus = new DeviceStatus
            {
                Id = kioskId,
                Serial = _unitData?.UnitSerialNumber,
                MfgSerial = _unitData?.ManufacturingSerialNumber,
                RBA = _unitData?.GetApplicationVersionString(),
                Assembly = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                EMVClessVersion = EMVClessVersion?.ToString(),
                EMVContactVersion = EMVContactVersion?.ToString(),
                LocalTime = DateTime.Now,
                RevisionNumber = GetFileUpdateRevisionNumber(),
                ErrorState = ErrorState,
                InjectedKeys = GetKeys()
            };
            deviceStatus.SupportsVas = SupportsVas;
            Version version;
            if (TryGetCurrentTgzVersion(out version))
                deviceStatus.TgzVersion = version?.ToString();
            Log("Status Check Result: " + JsonConvert.SerializeObject(deviceStatus));
            return deviceStatus;
        }

        public void StartHealthTimer()
        {
            Log("Start Health Timer");
            _healthTimer.Start();
        }

        public void StopHealthTimer()
        {
            Log("Stop Health Timer");
            _healthTimer.Stop();
        }

        public bool Connect()
        {
            if (Interlocked.CompareExchange(ref _connectInProgress, 1, 0) == 1)
                return false;
            ConnectionState = ConnectionState.Connecting;
            ErrorState = ErrorState.None;
            ++_connectAttempts;
            var result = ERROR_ID.RESULT_ERROR;
            var comPort = string.Empty;
            try
            {
                if (!_isInitialized && !Initialize())
                {
                    IsConnected = false;
                    ErrorState = ErrorState.FailedToInitializeConnection;
                    return false;
                }

                foreach (var comPortName in ComPortNames())
                {
                    comPort = comPortName;
                    result = _rba_api.Connect(comPort);
                    if (result == ERROR_ID.RESULT_SUCCESS)
                        break;
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ErrorState = ErrorState.Unknown;
                LogException(nameof(Connect), ex);
                return false;
            }
            finally
            {
                if (ErrorState == ErrorState.None)
                {
                    var flag1 = result == ERROR_ID.RESULT_SUCCESS || result == ERROR_ID.RESULT_ERROR_ALREADY_CONNECTED;
                    var flag2 = UpdateDeviceConfiguration();
                    IsConnected = flag1 & flag2;
                    if (IsConnected)
                    {
                        HandleAndLogResult(result, "Connect to " + comPort);
                        _connectAttempts = 0;
                    }
                    else if (flag1 && !flag2)
                    {
                        ErrorState = ErrorState.FailedToUpdateDevice;
                        var unitData = GetUnitData();
                        if ((unitData != null ? unitData.IsTampered ? 1 : 0 : 0) != 0)
                        {
                            ErrorState = ErrorState.Tampered;
                            _iuc285Notifier?.SendDeviceTamperedEvent();
                            IsConnected = true;
                        }

                        HandleAndLogResult(result, "Connect to " + comPort, false);
                    }
                    else
                    {
                        ErrorState = ErrorState.ConnectionError;
                        HandleAndLogResult(result, "Connect to " + comPort, false);
                    }

                    _connectInProgress = 0;
                }
            }

            return IsConnected;
        }

        public Base87CardReadModel ParseOnGuardData(string cardData)
        {
            if (string.IsNullOrWhiteSpace(cardData))
            {
                Log("ParseOnGuardData -> cardData contains no data.");
                return null;
            }

            try
            {
                Log("**********************************************");
                Log("Parsed OnGuard MSD Data");
                Log("**********************************************");
                var startIndex1 = 0;
                var flag1 = false;
                if (cardData.StartsWith('B'))
                {
                    flag1 = true;
                    cardData = cardData.Substring(cardData.LastIndexOf(" ") + 1);
                }

                var str1 = cardData.Substring(startIndex1, 6);
                if (cardData.IndexOf("=") > 20 && str1 != "601056" && !flag1)
                {
                    var startIndex2 = startIndex1 + 6;
                    var str2 = cardData.Substring(startIndex2, 4);
                    var startIndex3 = startIndex2 + 4;
                    int result1;
                    int.TryParse(cardData.Substring(startIndex3, 2), out result1);
                    var startIndex4 = startIndex3 + 2;
                    var str3 = cardData.Substring(startIndex4, 1);
                    var startIndex5 = startIndex4 + 1;
                    var str4 = cardData.Substring(startIndex5, 4);
                    var startIndex6 = startIndex5 + 4;
                    var str5 = cardData.Substring(startIndex6, 3);
                    var startIndex7 = startIndex6 + 3;
                    var str6 = cardData.Substring(startIndex7, 1);
                    var startIndex8 = startIndex7 + 1;
                    int result2;
                    if (!int.TryParse(cardData.Substring(startIndex8, 2), out result2))
                    {
                        Log("ParseOnGuardData -> Could not find valid Name length");
                        return null;
                    }

                    var startIndex9 = startIndex8 + 2;
                    var str7 = cardData.Substring(startIndex9, result2);
                    var startIndex10 = startIndex9 + result2;
                    var str8 = cardData.Substring(startIndex10, 1);
                    var flag2 = str8 == "1";
                    var startIndex11 = startIndex10 + 1;
                    int num;
                    if (str8 == "1")
                    {
                        Log("First 6: " + str1);
                        Log("Last 4: " + str2);
                        Log("PAN Length: " + result1);
                        Log("Mod10 check flag: " + str3);
                        Log("Service Code: " + str5);
                        Log("Language Code: " + str6);
                        Log("Cardholder Name Length: " + result2);
                        Log("Encrypted Flag: " + str8);
                        var str9 = cardData.Substring(startIndex11, 1);
                        Log("Encryption Format: " + str9);
                        var startIndex12 = startIndex11 + 1;
                        var str10 = cardData.Substring(startIndex12, 20);
                        Log("4-digit Extension: " + cardData.Substring(startIndex12 + 20, 4));
                        var startIndex13 = startIndex12 + 24;
                        var int32_1 = Convert.ToInt32(cardData.Substring(startIndex13, 2));
                        Log("IC Encrypted Data Length: " + int32_1);
                        var startIndex14 = startIndex13 + 2;
                        var str11 = cardData.Substring(startIndex14, int32_1);
                        var startIndex15 = startIndex14 + int32_1;
                        var int32_2 = Convert.ToInt32(cardData.Substring(startIndex15, 2));
                        Log("AES Pan Length: " + int32_2);
                        var startIndex16 = startIndex15 + 2;
                        var str12 = cardData.Substring(startIndex16, int32_2);
                        var startIndex17 = startIndex16 + int32_2;
                        var int32_3 = Convert.ToInt32(cardData.Substring(startIndex17, 2));
                        Log("LS Encrypted Data Length: " + int32_3);
                        var startIndex18 = startIndex17 + 2;
                        var str13 = cardData.Substring(startIndex18, int32_3);
                        var startIndex19 = startIndex18 + int32_3;
                        var str14 = cardData.Substring(startIndex19, 1);
                        Log("Extended Language Code: " + str14);
                        num = startIndex19 + 1;
                        Log("**********************************************");
                        var onGuardData = new EncryptedCardReadModel();
                        onGuardData.FirstSix = str1;
                        onGuardData.LastFour = str2;
                        onGuardData.PANLength = result1;
                        onGuardData.Mod10CheckFlag = str3;
                        onGuardData.Expiry = str4;
                        onGuardData.ServiceCode = str5;
                        onGuardData.LanguageCode = str6;
                        onGuardData.EncryptedFlag = flag2;
                        onGuardData.Name = str7;
                        onGuardData.NameLength = result2;
                        onGuardData.EncFormat = str9;
                        onGuardData.KSN = str10;
                        onGuardData.ICEncDataLength = int32_1;
                        onGuardData.ICEncData = str11;
                        onGuardData.AESPANLength = int32_2;
                        onGuardData.AESPAN = str12;
                        onGuardData.LSEncDataLength = int32_3;
                        onGuardData.LSEncData = str13;
                        onGuardData.ExtLangCode = str14;
                        onGuardData.Status = ResponseStatus.Success;
                        onGuardData.MfgSerialNumber = UnitData?.ManufacturingSerialNumber;
                        onGuardData.InjectedSerialNumber = UnitData?.UnitSerialNumber;
                        onGuardData.CardSource = CardSource;
                        onGuardData.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
                        return onGuardData;
                    }

                    if (str8 == "0")
                    {
                        Log("First 6: " + str1);
                        Log("Last 4: " + str2);
                        Log("PAN Length: " + result1);
                        Log("Mod10 check flag: " + str3);
                        Log("Expiry: " + str4);
                        Log("Service Code: " + str5);
                        Log("Language Code: " + str6);
                        Log("Encrypted Flag: " + str8);
                        var int32_4 = Convert.ToInt32(cardData.Substring(startIndex11, 2));
                        var startIndex20 = startIndex11 + 2;
                        Log("Track1 length: " + int32_4);
                        var str15 = (string)null;
                        if (int32_4 > 0)
                        {
                            str15 = cardData.Substring(startIndex20, int32_4);
                            startIndex20 += int32_4;
                        }

                        var int32_5 = Convert.ToInt32(cardData.Substring(startIndex20, 2));
                        var startIndex21 = startIndex20 + 2;
                        Log("Track2 length: " + int32_5);
                        var str16 = (string)null;
                        if (int32_5 > 0)
                        {
                            str16 = cardData.Substring(startIndex21, int32_5);
                            startIndex21 += int32_5;
                        }

                        Log("Extended Language Code: " + cardData.Substring(startIndex21, 1));
                        num = startIndex21 + 1;
                        var onGuardData = new UnencryptedCardReadModel();
                        onGuardData.FirstSix = str1;
                        onGuardData.LastFour = str2;
                        onGuardData.PANLength = result1;
                        onGuardData.Mod10CheckFlag = str3;
                        onGuardData.Expiry = str4;
                        onGuardData.ServiceCode = str5;
                        onGuardData.LanguageCode = str6;
                        onGuardData.EncryptedFlag = flag2;
                        onGuardData.Name = str7;
                        onGuardData.Track1 = str15;
                        onGuardData.Track2 = str16;
                        onGuardData.Status = ResponseStatus.Success;
                        onGuardData.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
                        return onGuardData;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(cardData))
                    {
                        Log("Card data whitelisted using secbin.dat");
                        if (str1 != "601056")
                            return null;
                        Log("Card Bin Matches Gift Card");
                        var num = cardData.IndexOf("=");
                        var onGuardData = new UnencryptedCardReadModel();
                        onGuardData.Track2 = cardData;
                        onGuardData.PANLength = num;
                        onGuardData.CardType = CardType.RedboxGiftCard;
                        onGuardData.FirstSix = str1;
                        onGuardData.EncryptedFlag = false;
                        onGuardData.SwipeMode = SwipeType.Swipe;
                        onGuardData.LastFour = cardData.Substring(num - 4, 4);
                        onGuardData.Expiry = cardData.Substring(num + 1, 4);
                        onGuardData.ServiceCode = cardData.Substring(num + 5, 3);
                        onGuardData.Status = ResponseStatus.Success;
                        onGuardData.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
                        return onGuardData;
                    }

                    Log("Card Data: Empty");
                    Log("**********************************************");
                }
            }
            catch (Exception ex)
            {
                LogException("Parse on Guard Data", ex);
            }

            return null;
        }

        public bool SetAmount(string amount)
        {
            var num = (int)_rba_api.SetParam(PARAMETER_ID.P13_REQ_AMOUNT, amount);
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M13_AMOUNT);
            HandleAndLogResult(result, "13 Set Amount: " + amount);
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private void OnConfigChange(DeviceServiceConfig config, string data)
        {
            if (!IsConnected)
                return;
            _iuc285Notifier?.SendCardReaderStateEvent(GetCardReaderState());
        }

        private void OnStartup()
        {
            _continuousResultTimeouts = 0;
            _deviceRebootTime = new DateTime?();
            UnitData = GetUnitData();
            EMVClessVersion = GetXMLVersion("emvcless.xml");
            EMVContactVersion = GetXMLVersion("emvcontact.xml");
            _rebootTask = StartRebootCountdown();
            _statusClient?.PostRebootStatus(GetRebootStatus());
            _rebootType = RebootType.Disconnect;
            _iuc285Notifier?.SendCardReaderConnectedEvent();
            Log(string.Format("Connected after {0} attempts.", _connectAttempts));
            StartHealthTimer();
            _areWaitForReconnect.Set();
            var unitData = UnitData;
            if ((unitData != null ? unitData.IsTampered ? 1 : 0 : 0) != 0)
                _iuc285Notifier?.SendDeviceTamperedEvent();
            _iuc285Notifier?.SendCardReaderStateEvent(GetCardReaderState());
            InitializeVas();
        }

        private Version GetXMLVersion(string fileName)
        {
            var result = (Version)null;
            try
            {
                using (var stream = RetrieveFile(RetrieveFileDataFormat.Text, Path.Combine("/HOST/", fileName)))
                {
                    if (stream != null)
                    {
                        var streamReader = new StreamReader(stream);
                        var flag = false;
                        while (!flag)
                            if (!streamReader.EndOfStream)
                            {
                                var input = streamReader?.ReadLine()?.Trim();
                                if (!string.IsNullOrEmpty(input) && input.StartsWith("<!--") &&
                                    input.ToUpper().Contains("VERSION") && input.EndsWith("-->"))
                                {
                                    flag = true;
                                    Version.TryParse(new Regex("\\d+(\\.\\d+)+").Match(input)?.Value, out result);
                                }
                            }
                            else
                            {
                                break;
                            }
                    }
                    else
                    {
                        Log("Unable to read file " + fileName + " to extract version.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Error in GetXMLVersion. {0}", ex));
            }

            Log(string.Format("Extracted version: {0} from XML file: {1}", result, fileName));
            return result;
        }

        private void OnReconnect(object s, EventArgs e)
        {
            if (IsConnected || Interlocked.CompareExchange(ref _attemptingReconnect, 1, 0) != 0)
                return;
            Connect();
            _attemptingReconnect = 0;
        }

        private void OnDisconnect(Exception exception)
        {
            var currentRebootStatus = _currentRebootStatus;
            int num;
            if ((currentRebootStatus != null ? currentRebootStatus.DisconnectTime.HasValue ? 1 : 0 : 0) != 0)
            {
                var now1 = DateTime.Now;
                var dateTime1 = _currentRebootStatus.DisconnectTime.Value;
                var dateTime2 = dateTime1.AddDays(1.0) - REBOOT_THRESHOLD;
                if (now1 >= dateTime2)
                {
                    var now2 = DateTime.Now;
                    dateTime1 = _currentRebootStatus.DisconnectTime.Value;
                    var dateTime3 = dateTime1.AddDays(1.0) + REBOOT_THRESHOLD;
                    num = now2 <= dateTime3 ? 1 : 0;
                    goto label_4;
                }
            }

            num = 0;
            label_4:
            var flag = num != 0;
            _rebootType =
                _rebootType == RebootType.ServiceStart || _rebootType == RebootType.Scheduled ||
                _rebootType == RebootType.Manual || _rebootType == RebootType.FileUpdate ? _rebootType :
                flag ? RebootType.Automatic : RebootType.Disconnect;
            _currentRebootStatus.DisconnectTime = DateTime.Now;
            if (ReadCardContextData != null)
            {
                ReadCardContextData.State = ReadCardContextState.Canceled;
                ReadCardContextData.Errors.Add(new Error
                {
                    Code = "READERDISCONNECT",
                    Message = "The Reader was disconnected, automatically cancelling job"
                });
                ReadCancel();
            }

            _iuc285Notifier?.SendCardReaderDisconnectedEvent(exception);
            StopHealthTimer();
            if (ReadCardContextData == null)
                return;
            ReadCardContextData.State = ReadCardContextState.Canceled;
            ReadCardContextData.Errors.Add(new Error
            {
                Code = "READERDISCONNECT",
                Message = "The Reader was disconnected, automatically cancelling job"
            });
            ReadCancel();
        }

        public static void Log(string s)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogInformation(s);
        }

        public static void LogError(string s)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogError(s);
        }

        public void HandleAndLogResult(ERROR_ID result, string resultName = null, bool includeResultLog = true)
        {
            if (includeResultLog)
                Log(string.Format("{0} Result: {1}", resultName, result));
            switch (result)
            {
                case ERROR_ID.RESULT_ERROR_TIMEOUT:
                    ++_continuousResultTimeouts;
                    break;
                case ERROR_ID.RESULT_SUCCESS:
                    _continuousResultTimeouts = 0;
                    break;
                default:
                    _continuousResultTimeouts = 0;
                    break;
            }

            if (_continuousResultTimeouts < 10 || _continuousResultTimeouts % 10 != 0)
                return;
            Log(string.Format("Continuous Timeouts: {0}", _continuousResultTimeouts));
        }

        public static void LogException(string message, Exception e)
        {
            var logger = _logger;
            if (logger == null)
                return;
            logger.LogError(e, "Unhandled Exception in " + message);
        }

        private void OnHealthTimer(object sender, ElapsedEventArgs e)
        {
            Log("OnHealthTimer Elapsed Begin");
            if (PingDevice())
                return;
            Reconnect();
        }

        private bool HasRKIKey(string rki)
        {
            Log("Begin HasRKI " + rki + " in " + _keys);
            if (string.IsNullOrWhiteSpace(_keys))
                GetKeys();
            return !string.IsNullOrEmpty(_keys) && _keys.Contains(rki);
        }

        public bool Initialize()
        {
            ERROR_ID result1;
            try
            {
                _rba_api.logHandler = s =>
                {
                    var deviceConfig = _deviceConfig;
                    if ((deviceConfig != null ? deviceConfig.CurrentValue.EnableIngenicoTraceLogging ? 1 : 0 : 0) == 0)
                        return;
                    Log(s);
                };
                _rba_api.SetDefaultLogLevel(LOG_LEVEL.TRACE);
                var result2 = _rba_api.Initialize();
                HandleAndLogResult(result2, "Initialize RBA API");
                if (result2 != ERROR_ID.RESULT_SUCCESS)
                {
                    IsConnected = false;
                    _isInitialized = false;
                    return false;
                }

                var versionNonStatic = _rba_api.GetVersion_NonStatic();
                _rba_api.pinpadHandler = TerminalMessageHandler;
                Log("Initialized!");
                Log("RBA_SDK Version: " + versionNonStatic);
                var num = (int)_rba_api.SetNotifyRbaDisconnected(() =>
                {
                    Log("**** DISCONNECTED ****");
                    IsConnected = false;
                });
                var timeouts = new SETTINGS_COMM_TIMEOUTS
                {
                    ConnectTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendTimeout = 5000
                };
                result1 = _rba_api.SetCommTimeouts(timeouts);
                HandleAndLogResult(result1, "Set Comm Timeouts");
            }
            catch (Exception ex)
            {
                LogException("Initialize RBA SDK", ex);
                result1 = ERROR_ID.RESULT_ERROR;
            }

            return _isInitialized = result1 == ERROR_ID.RESULT_SUCCESS;
        }

        private UnitDataModel GetUnitData()
        {
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M07_UNIT_DATA);
            HandleAndLogResult(result, "07 Unit Data");
            if (result != ERROR_ID.RESULT_SUCCESS)
                return null;
            var unitData = new UnitDataModel
            {
                Manufacture = _rba_api.GetParam(PARAMETER_ID.P07_RES_MANUFACTURE)
            };
            Log("P07_RES_MANUFACTURE: " + unitData.Manufacture);
            unitData.DeviceType = _rba_api.GetParam(PARAMETER_ID.P07_RES_DEVICE);
            Log("P07_RES_DEVICE: " + unitData.DeviceType);
            unitData.UnitSerialNumber = _rba_api.GetParam(PARAMETER_ID.P07_RES_UNIT_SERIAL_NUMBER);
            Log("P07_RES_UNIT_SERIAL_NUMBER: " + unitData.UnitSerialNumber);
            unitData.RamSize = _rba_api.GetParam(PARAMETER_ID.P07_RES_RAM_SIZE);
            Log("P07_RES_RAM_SIZE: " + unitData.RamSize);
            unitData.FlashSize = _rba_api.GetParam(PARAMETER_ID.P07_RES_FLASH_SIZE);
            Log("P07_RES_FLASH_SIZE: " + unitData.FlashSize);
            unitData.DigitizerVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_DIGITIZER_VERSION);
            Log("P07_RES_DIGITIZER_VERSION: " + unitData.DigitizerVersion);
            unitData.SecurityModuleVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_SECURITY_MODULE_VERSION);
            Log("P07_RES_SECURITY_MODULE_VERSION: " + unitData.SecurityModuleVersion);
            unitData.OSVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_OS_VERSION);
            Log("P07_RES_OS_VERSION: " + unitData.OSVersion);
            unitData.ApplicationVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_APPLICATION_VERSION);
            Log("P07_RES_APPLICATION_VERSION: " + unitData.ApplicationVersion);
            unitData.EFTLVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_EFTL_VERSION);
            Log("P07_RES_EFTL_VERSION: " + unitData.EFTLVersion);
            unitData.EFTPVersion = _rba_api.GetParam(PARAMETER_ID.P07_RES_EFTP_VERSION);
            Log("P07_RES_EFTP_VERSION: " + unitData.EFTPVersion);
            unitData.ManufacturingSerialNumber = _rba_api.GetParam(PARAMETER_ID.P07_RES_MANUFACTURING_SERIAL_NUMBER);
            Log("P07_RES_MANUFACTURING_SERIAL_NUMBER: " + unitData.ManufacturingSerialNumber);
            unitData.DCKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_EMV_DC_KERNEL_TYPE);
            Log("P07_RES_EMV_DC_KERNEL_TYPE: " + unitData.DCKernelType);
            unitData.EMVEngineKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_EMV_ENGINE_KERNEL_TYPE);
            Log("P07_RES_EMV_ENGINE_KERNEL_TYPE: " + unitData.EMVEngineKernelType);
            unitData.CLessDiscoverKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_DISCOVER_KERNEL_TYPE);
            Log("P07_RES_CLESS_DISCOVER_KERNEL_TYPE: " + unitData.CLessDiscoverKernelType);
            unitData.ClessExpresPayV3KernelType =
                _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_EXPRESSPAY_V3_KERNEL_TYPE);
            Log("P07_RES_CLESS_EXPRESSPAY_V3_KERNEL_TYPE: " + unitData.ClessExpresPayV3KernelType);
            unitData.ClessExpresPayV2KernelType =
                _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_EXPRESSPAY_V2_KERNEL_TYPE);
            Log("P07_RES_CLESS_EXPRESSPAY_V2_KERNEL_TYPE: " + unitData.ClessExpresPayV2KernelType);
            unitData.ClessPayPassV3KernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_PAYPASS_V3_KERNEL_TYPE);
            Log("P07_RES_CLESS_PAYPASS_V3_KERNEL_TYPE: " + unitData.ClessPayPassV3KernelType);
            unitData.ClessPayPassV3AppType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_PAYPASS_V3_APP_TYPE);
            Log("P07_RES_CLESS_PAYPASS_V3_APP_TYPE: " + unitData.ClessPayPassV3AppType);
            unitData.ClessVisaPayWaveKernelType =
                _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_VISA_PAYWAVE_KERNEL_TYPE);
            Log("P07_RES_CLESS_VISA_PAYWAVE_KERNEL_TYPE: " + unitData.ClessVisaPayWaveKernelType);
            unitData.ClessInteracKernelType = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_INTERAC_KERNEL_TYPE);
            Log("P07_RES_CLESS_INTERAC_KERNEL_TYPE: " + unitData.ClessInteracKernelType);
            unitData.ClessInterfaceIsSupported = _rba_api.GetParam(PARAMETER_ID.P07_RES_CLESS_INTERFACE_IS_SUPPORTED);
            Log("P07_RES_CLESS_INTERFACE_IS_SUPPORTED: " + unitData.ClessInterfaceIsSupported);
            return unitData;
        }

        private bool IsCancelled(ReadCardContext context)
        {
            if (!context.CancelToken.IsCancellationRequested)
                return false;
            Log("Cancelling...");
            context.Errors.Add(new Error
            {
                Code = "Cancel",
                Message = "Cancelled by client."
            });
            var readCardContext = context;
            var encryptedCardReadModel = new EncryptedCardReadModel(context.Errors);
            encryptedCardReadModel.Status = ResponseStatus.Cancelled;
            encryptedCardReadModel.TimeTaken = context.Watch.Elapsed;
            readCardContext.ResponseDataModel = encryptedCardReadModel;
            context.SendCompletedCallback();
            return true;
        }

        private bool SendCardIsBeingProcessed(ReadCardContext context)
        {
            context.SendCardProcessing();
            return true;
        }

        private void PerformActionAndCheckSuccess<T>(
            Func<ReadCardContext, T> action,
            Predicate<T> checkSuccess,
            ReadCardContext context,
            VasMode vasMode = VasMode.VasAndPay)
        {
            if (typeof(T) == typeof(bool) && checkSuccess == null)
                checkSuccess = input => Convert.ToBoolean(input);
            if ((context != null ? !context.CanContinue(VasMode.VasAndPay) ? 1 : 0 : 1) != 0)
            {
                Log(string.Format("PerformActionAndCheckSuccess on {0} is in state: {1} and can no longer continue.",
                    action?.Method?.Name, context?.State));
            }
            else if (context.Id != ReadCardContextData.Id)
            {
                LogError(string.Format(
                    "PerformActionAndCheckSuccess on {0} for context: {1} does not match ReadcardContextData: {2}",
                    action?.Method?.Name, context.Id, ReadCardContextData?.Id));
                context.UpdateStopStateFor(VasMode.VasAndPay);
                context.Errors.Add(new Error
                {
                    Code = "CNTXTMM",
                    Message = "The Context ID for read does not match what is currently running."
                });
            }
            else
            {
                var unitData = _unitData;
                if ((unitData != null ? unitData.IsTampered ? 1 : 0 : 0) != 0)
                {
                    Log("PerformActionAndCheckSuccess on " + action?.Method?.Name +
                        " is Tampered and can no longer continue.");
                    context.State = ReadCardContextState.Tampered;
                }
                else if (action == null || checkSuccess == null)
                {
                    LogError("PerformActionAndCheckSuccess Action or Check Success Is Null.");
                    context.UpdateStopStateFor(VasMode.VasAndPay);
                }
                else
                {
                    if (IsCancelled(context))
                    {
                        Log("PerformActionAndCheckSuccess on " + action?.Method?.Name +
                            " - context is cancelled and can no longer continue.");
                        context.State = ReadCardContextState.Canceled;
                    }

                    var obj = action(context);
                    if (checkSuccess(obj))
                        return;
                    Log("PerformActionAndCheckSuccess on " + action?.Method?.Name + " did not complete successfully.");
                    context.UpdateStopStateFor(vasMode);
                }
            }
        }

        private async Task<bool> ReadCardInner(
            ICardReadRequest request,
            CancellationToken cancel,
            Action<Base87CardReadModel> jobCompletedCallback,
            Action<string, string> eventsCallback)
        {
            var iuC285Proxy1 = this;
            iuC285Proxy1.InitializeReadCard();
            var iuC285Proxy2 = iuC285Proxy1;
            var readCardContext1 = new ReadCardContext(_logger);
            readCardContext1.Request = request;
            readCardContext1.UserInteractionTimeout =
                iuC285Proxy1._deviceConfig?.CurrentValue.CardReadUserInteractionTimeout;
            readCardContext1.CancelToken = cancel;
            readCardContext1.State = ReadCardContextState.Continue;
            readCardContext1.JobCompletedCallback = jobCompletedCallback;
            readCardContext1.EventsCallback = eventsCallback;
            readCardContext1.UnitData = iuC285Proxy1._unitData;
            var readCardContext2 = readCardContext1;
            iuC285Proxy2.ReadCardContextData = readCardContext1;
            var context = readCardContext2;
            try
            {
                if (context.Request.Timeout < 30000)
                    context.Request.Timeout = 30000;
                if (context.CanContinue(VasMode.VasOnly) && !iuC285Proxy1.SupportsVas)
                {
                    LogError("Vas Is Not Supported. Requests should not be sent for vas data.");
                    context.UpdateStopStateFor(VasMode.VasOnly);
                    if (!context.CanContinue(VasMode.PayOnly))
                        context.Errors.Add(new Error
                        {
                            Code = "VASNOTSUPPORTED",
                            Message = "Vas is not supported or is not enabled."
                        });
                }

                if (context.Request.VasMode == VasMode.PayOnly && context.State == ReadCardContextState.Continue)
                    context.State = ReadCardContextState.PayContinue;
                if (context.Request.VasMode == VasMode.VasOnly && context.State == ReadCardContextState.Continue)
                    context.State = ReadCardContextState.VasContinue;
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.Offline, null, context);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.PrepareVas, null, context);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.SetAmount, null, context);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.CardReadRequestEncrypted, null, context);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.WaitForVasReadStart, null, context,
                    VasMode.VasOnly);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.WaitForCardReadComplete, null, context);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.SendCardIsBeingProcessed, null, context);
                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.GetVasData, null, context, VasMode.VasOnly);
                if (context.IsPayEnabled)
                    switch (context.CardSource)
                    {
                        case CardSourceType.Swipe:
                            Log("Enter Swipe Flow");
                            break;
                        case CardSourceType.EMVContact:
                            Log("Enter EMV Contact Flow");
                            if (context.State.HasFlag(ReadCardContextState.PayContinue))
                            {
                                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.EMVTransInitiation, null,
                                    context, VasMode.PayOnly);
                                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.SetPaymentType_04, null, context,
                                    VasMode.PayOnly);
                                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.SetAmount, null, context,
                                    VasMode.PayOnly);
                                iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.EMVResponse, null, context,
                                    VasMode.PayOnly);
                            }

                            break;
                        case CardSourceType.QuickChip:
                            Log("Enter Quick Chip Flow");
                            if (context.State.HasFlag(ReadCardContextState.PayContinue))
                                if (!_mre3305AuthorizationConfirmation.WaitOne(context.TimeRemainingUntilTimeout))
                                {
                                    context.State = ReadCardContextState.Timeout;
                                    Log("Timeout in Quickchip waiting for data.");
                                }

                            break;
                        case CardSourceType.MSDContactless:
                            Log("Enter MSD Contactless Flow");
                            break;
                        case CardSourceType.EMVContactless:
                        case CardSourceType.Mobile:
                            Log("Enter EMV Contactless Flow");
                            iuC285Proxy1.PerformActionAndCheckSuccess(iuC285Proxy1.EMVResponse, null, context,
                                VasMode.PayOnly);
                            break;
                        case CardSourceType.VASOnly:
                            Log("Vas Only Flow");
                            if (context.State.HasFlag(ReadCardContextState.VasContinue))
                                if (!_mre166VasResponse.WaitOne(context.TimeRemainingUntilTimeout))
                                {
                                    context.State = ReadCardContextState.Timeout;
                                    Log("Timeout Waiting for data.");
                                }

                            break;
                        default:
                            Log(string.Format("Unhandled Source Type Occurred: {0}", iuC285Proxy1.CardSource));
                            context.Errors.Add(new Error
                            {
                                Code = "SourceInvalid",
                                Message = string.Format(
                                    "An invalid or unhandled source type: {0} was received in ReadCardInner.",
                                    iuC285Proxy1.CardSource)
                            });
                            break;
                    }
            }
            catch (TaskCanceledException ex)
            {
                Log("Read Card Cancelled");
            }
            catch (Exception ex)
            {
                LogException("Encrypted Read Card", ex);
                context.Errors.Add(new Error
                {
                    Code = "DS001",
                    Message = "Unhandled Exception in ReadCardInner."
                });
                var readCardContext3 = context;
                var encryptedCardReadModel = new EncryptedCardReadModel();
                encryptedCardReadModel.Status = ResponseStatus.Errored;
                readCardContext3.ResponseDataModel = encryptedCardReadModel;
                context.SendCompletedCallback();
                return false;
            }
            finally
            {
                iuC285Proxy1.Offline();
                Log("END READ CARD");
            }

            context.SendCompletedCallback();
            if (context.TimeRemainingUntilTimeout >= 0 && context.State != ReadCardContextState.Timeout &&
                context.State != ReadCardContextState.Canceled)
            {
                Version version;
                if (iuC285Proxy1.TryGetCurrentTgzVersion(out version))
                    context.CardStats.TgzVersion = version?.ToString();
                if (iuC285Proxy1._kioskData != null && iuC285Proxy1._kioskData.KioskId != 0L)
                    context.CardStats.KioskId = iuC285Proxy1._kioskData.KioskId;
                Task.Run(() => _statusClient?.PostCardStats(context.CardStats));
            }

            return true;
        }

        private bool EMVResponse(ReadCardContext context)
        {
            if (context.State.HasFlag(ReadCardContextState.PayContinue))
            {
                if (_mre3303AuthorizationRequest.WaitOne(context.TimeRemainingUntilTimeout))
                {
                    _authSuccessful = false;
                    Log("GO AUTH.");
                    context.SendCompletedCallback();
                    if (context.EventsCallback == null ||
                        _mre3304AuthorizationResponse.WaitOne(context.TimeRemainingUntilTimeout))
                        SendAuthorizationReponse(context.EventsCallback == null || _authSuccessful);
                    else
                        Log("Failed to receive auth response");
                    Log("GET CONFIRMATION");
                    _mre3305AuthorizationConfirmation.WaitOne(context.TimeRemainingUntilTimeout);
                }
                else
                {
                    Log("Timeout Waiting for EMV Response.");
                    return false;
                }
            }

            return true;
        }

        private void SetRebootTime()
        {
            if (_settings == null)
                return;
            DataFileHelper.TryUpdateFileData(_settings.DataFilePath, "RebootTime.json",
                TimeSpan.FromSeconds(10800 + new Random().Next(0, 3600)));
        }

        public bool UpdateCardConfigurationData()
        {
            var flag1 = true;
            var group = "0011";
            for (var index1 = 1; index1 <= 16; ++index1)
            {
                string Data;
                flag1 = ReadConfig(group, index1.ToString(), out Data, false);
                if (flag1)
                {
                    var list = Data.Split(' ').ToList();
                    if (string.Compare(list[0], "1", true) == 0)
                    {
                        var flag2 = false;
                        var num = 0;
                        for (var index2 = 0; index2 < list.Count; ++index2)
                            if (!string.IsNullOrWhiteSpace(list[index2]))
                            {
                                if ((num == 6 && list[index2] != "0") || (num == 8 && list[index2] != "0"))
                                {
                                    list[index2] = "0";
                                    flag2 = true;
                                }

                                ++num;
                            }

                        var str = string.Join(" ", list);
                        if (flag2)
                        {
                            flag1 = WriteConfig(group, index1.ToString(), str, false);
                            if (!flag1)
                                break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return flag1;
        }

        private Stream RetrieveFile(RetrieveFileDataFormat dataFormat, string filename)
        {
            var stream = (Stream)null;
            Log("Starting RetrieveFile for filename " + filename);
            try
            {
                var str1 = "";
                var errorId = _rba_api.SetParam(PARAMETER_ID.P65_REQ_RECORD_TYPE, "1");
                errorId = _rba_api.SetParam(PARAMETER_ID.P65_REQ_DATA_TYPE, ((int)dataFormat).ToString());
                errorId = _rba_api.SetParam(PARAMETER_ID.P65_REQ_FILE_NAME, filename);
                Log(string.Format("ProcessMessage 65. {0}", _rba_api.ProcessMessage(MESSAGE_ID.M65_RETRIEVE_FILE)));
                var str2 = _rba_api.GetParam(PARAMETER_ID.P65_RES_RESULT);
                Log("P65_RES_RESULT: " + str2);
                switch (str2)
                {
                    case "0":
                        var int32 = Convert.ToInt32(_rba_api.GetParam(PARAMETER_ID.P65_RES_TOTAL_NUMBER_OF_BLOCKS));
                        Log(string.Format("Total number of blocks: {0}", int32));
                        Log("CRC: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_CRC));
                        Log("Data type: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_DATA_TYPE));
                        Log("# of blocks downloaded: " + _rba_api.GetParam(PARAMETER_ID.P65_RES_BLOCK_NUMBER));
                        var str3 = _rba_api.GetParam(PARAMETER_ID.P65_RES_DATA);
                        var str4 = str1 + str3;
                        if (int32 > 1)
                            for (var index = 1; index < int32; ++index)
                            {
                                errorId = _rba_api.SetParam(PARAMETER_ID.P65_REQ_RECORD_TYPE, "0");
                                errorId = _rba_api.SetParam(PARAMETER_ID.P65_REQ_DATA_TYPE,
                                    ((int)dataFormat).ToString());
                                errorId = _rba_api.SetParam(PARAMETER_ID.P65_REQ_FILE_NAME, filename);
                                errorId = _rba_api.ProcessMessage(MESSAGE_ID.M65_RETRIEVE_FILE);
                                if (_rba_api.GetParam(PARAMETER_ID.P65_RES_RESULT) == "0")
                                {
                                    Log("# of blocks downloaded: " +
                                        _rba_api.GetParam(PARAMETER_ID.P65_RES_BLOCK_NUMBER));
                                    var str5 = _rba_api.GetParam(PARAMETER_ID.P65_RES_DATA);
                                    str4 += str5;
                                }
                            }

                        try
                        {
                            stream = new MemoryStream();
                            var streamWriter = new StreamWriter(stream);
                            streamWriter.Write(str4);
                            streamWriter.Flush();
                            stream.Position = 0L;
                            Log("File successfully read into memeory stream " + filename);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log(string.Format("Exception: {0}", ex));
                            break;
                        }
                    case "1":
                        Log("File not found: " + filename);
                        break;
                    case "2":
                        Log("Error while converting original data to Base64 format");
                        break;
                    default:
                        Log("Format change error");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Error in RetrieveFile. {0}", ex));
            }

            Log("Ending RetrieveFile for filename " + filename);
            return stream;
        }

        public bool TryGetCurrentTgzVersion(out Version version)
        {
            version = new Version();
            if (!IsConnected)
                return false;
            string Data;
            ReadConfig("14", "1", out Data, false);
            return Version.TryParse(Data, out version);
        }

        private static List<string> ComPortNames()
        {
            var regex = new Regex("^VID_0B00.PID_0057", RegexOptions.IgnoreCase);
            var source = new List<string>();
            var registryKey1 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
            foreach (var subKeyName1 in registryKey1.GetSubKeyNames())
            {
                var registryKey2 = registryKey1.OpenSubKey(subKeyName1);
                foreach (var subKeyName2 in registryKey2.GetSubKeyNames())
                    if (regex.Match(subKeyName2).Success)
                    {
                        var registryKey3 = registryKey2.OpenSubKey(subKeyName2);
                        foreach (var subKeyName3 in registryKey3.GetSubKeyNames())
                        {
                            var registryKey4 = registryKey3?.OpenSubKey(subKeyName3);
                            var str1 = (string)registryKey4?.GetValue("LocationInformation");
                            var registryKey5 = registryKey4.OpenSubKey("Device Parameters");
                            var str2 = (string)registryKey5?.GetValue("PortName");
                            if (!string.IsNullOrEmpty(str2) && SerialPort.GetPortNames().Contains(str2))
                                source.Add((string)registryKey5.GetValue("PortName"));
                        }
                    }
            }

            if (!source.Any())
                source.Add("COM7");
            return source;
        }

        private static string ByteArrayToString(byte[] data)
        {
            var stringBuilder = new StringBuilder(data.Length * 2);
            foreach (var num in data)
                stringBuilder.AppendFormat("{0:x2}", num);
            return stringBuilder.ToString();
        }

        private string VasMessageResponseText(string response)
        {
            switch (response)
            {
                case "0":
                    return "Command Successful";
                case "1":
                    return "Command Failed";
                case "2":
                    return "FailedToRetrieveData";
                case "4":
                    return "Invalid Format";
                case "10":
                    return "Vas Returned";
                case "11":
                    return "Vas Returned, Payment Pending";
                default:
                    return "Unknown Response";
            }
        }

        private VasResult M17_6SendVasMessage(VasCmd vasCmd, params string[] vasParams)
        {
            var vasResult = new VasResult
            {
                VasCommand = vasCmd
            };
            var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P17_REQ_TYPE, "6");
            var data = ((int)vasCmd).ToString().PadLeft(3, '0');
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P17_REQ_VAS_CMD, data);
            if (vasParams.Length != 0)
                foreach (var vasParam in vasParams)
                {
                    var num4 = (int)_rba_api.AddParam(PARAMETER_ID.P17_REQ_VAS_PARAMETER, vasParam);
                }

            vasResult.CommandResult = _rba_api.ProcessMessage(MESSAGE_ID.M17_MERCHANT_DATA_WRITE);
            if (_rba_api.GetParam(PARAMETER_ID.P17_RES_TYPE) != "6")
                return vasResult;
            var str1 = _rba_api.GetParam(PARAMETER_ID.P17_RES_STATUS);
            vasResult.Status = Enum.TryParse(typeof(VasStatus), str1, out var obj) ? (VasStatus)obj : VasStatus.Unknown;
            vasResult.AdditionalData = _rba_api.GetParam(PARAMETER_ID.P17_RES_IS_ADDITIONAL_DATA);
            var str2 = _rba_api.GetParam(PARAMETER_ID.P17_RES_VAS_RESULT);
            vasResult.VasDataResult = str2;
            var stringList = new List<string>();
            for (var paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA);
                 vasResult.CommandResult == ERROR_ID.RESULT_SUCCESS && paramLen >= 0;
                 paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA))
            {
                var str3 = _rba_api.GetParam(PARAMETER_ID.P17_RES_VAS_DATA);
                if (!string.IsNullOrWhiteSpace(str3))
                    stringList.Add(str3);
            }

            vasResult.Data = stringList;
            HandleAndLogResult(vasResult.CommandResult,
                "M17_6SendVasMessage - VasCommand: " + data + "-(" + vasCmd + ") - Params: " +
                string.Join(",", vasParams) + " - " + vasResult);
            return vasResult;
        }

        private void M09SetAllowedPayment()
        {
            var str1 = _rba_api.GetParam(PARAMETER_ID.P09_RES_CARD_TYPE);
            var str2 = _rba_api.GetParam(PARAMETER_ID.P09_RES_CARD_STATUS);
            if ((str1 == "02" || str1 == "99") && str2 == "I")
            {
                Log("**Card inserted");
                _mre09CardRemoved.Reset();
            }
            else if ((str1 == "02" || str1 == "99") && str2 == "R")
            {
                Log("**Card Removed");
                ReadCardContextData?.SendCardRemoved();
                _mre09CardRemoved.Set();
            }
            else if (str2 == "P")
            {
                Log("**Unknown problem with Card Insertion / Removal");
                _mre09CardRemoved.Set();
            }
            else
            {
                Log("**Unknown Card activity");
                _mre09CardRemoved.Set();
            }
        }

        private void M23CardRead()
        {
            var str1 = _rba_api.GetParam(PARAMETER_ID.P23_RES_EXIT_TYPE);
            Log("P23_RES_EXIT_TYPE: (" + str1 + ") - " + ExitCodeText(str1));
            var str2 = _rba_api.GetParam(PARAMETER_ID.P23_RES_TRACK1);
            Log("P23_RES_TRACK1: " + str2);
            var str3 = _rba_api.GetParam(PARAMETER_ID.P23_RES_TRACK2);
            Log("P23_RES_TRACK2: " + str3);
            var str4 = _rba_api.GetParam(PARAMETER_ID.P23_RES_TRACK3);
            Log("P23_RES_TRACK3: " + str4);
            var sourceType = _rba_api.GetParam(PARAMETER_ID.P23_RES_CARD_SOURCE);
            Log("P23_RES_CARD_SOURCE: " + sourceType);
            CardSource = ReadCardContextData.CardSource = ParseCardSourceType(sourceType);
            ReadCardContextData.ExitType = (CardReadExitType)int.Parse(str1);
            if (str1 == "0")
            {
                switch (CardSource)
                {
                    case CardSourceType.Swipe:
                    case CardSourceType.MSDContactless:
                        GetVariable_29("394");
                        GetVariable_29("000398");
                        ReadCardContextData.ResponseDataModel = new UnencryptedCardReadModel
                        {
                            Track1 = str2,
                            Track2 = str3,
                            Track3 = str4
                        };
                        break;
                }
            }
            else
            {
                CardSource = CardSourceType.Unknown;
                Log("ExitType: " + str1);
            }
        }

        private InsertedStatus M3301Status()
        {
            var str = _rba_api.GetParam(PARAMETER_ID.P33_01_RES_F1_CHIP_CARD);
            Log("P33_01_RES_F1_CHIP_CARD: " + str);
            switch (str)
            {
                case "I":
                    return InsertedStatus.Inserted;
                default:
                    return InsertedStatus.Removed;
            }
        }

        private void EMVDataFromDevice(MESSAGE_ID messageId, PARAMETER_ID parameter)
        {
            try
            {
                var str1 = _rba_api.GetParam(parameter);
                var emvCardReadModel1 = new EMVCardReadModel();
                emvCardReadModel1.CardSource = CardSource;
                var emvCardReadModel2 = emvCardReadModel1;
                var data = new byte[1];
                if (str1 == "E")
                {
                    var str2 = string.Format(
                        "Error when retrieving status when Getting Parameter {0} from EMV Message {1}", parameter,
                        messageId);
                    emvCardReadModel2.AddError("D001", str2);
                    Log(str2);
                    ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
                }

                var parseTags = new ParseTags();
                while (true)
                {
                    var tagParamLen = _rba_api.GetTagParamLen(messageId);
                    if (tagParamLen > 0)
                    {
                        var tagParam = _rba_api.GetTagParam(messageId, out data);
                        var TagDataStr = ByteArrayToString(data);
                        var str3 = tagParam.ToString("X");
                        if (!_ingenicoTags.Contains(str3))
                            emvCardReadModel2.Tags.Add(str3, TagDataStr);
                        parseTags.ParseEMVTags(str3, tagParamLen, TagDataStr, data, CardSource, Log,
                            ReadCardContextData?.Errors);
                    }
                    else
                    {
                        break;
                    }
                }

                if (parseTags.Tag9F27CryptogramDeclined)
                {
                    ReadCardContextData.Errors.Add(new Error
                    {
                        Code = "EMVCardDecline",
                        Message = "Card Declined Transaction."
                    });
                    ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
                }

                if (!string.IsNullOrEmpty(parseTags.ErrorCode))
                {
                    ReadCardContextData.Errors.Add(new Error
                    {
                        Code = parseTags.ErrorCode,
                        Message = "Error During Read."
                    });
                    ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
                }

                emvCardReadModel2.FirstSix = parseTags.First6;
                emvCardReadModel2.LastFour = parseTags.Last4;
                emvCardReadModel2.AID = parseTags.AID;
                emvCardReadModel2.MfgSerialNumber = UnitData.ManufacturingSerialNumber;
                if (ReadCardContextData.CardSource == CardSourceType.EMVContactless && parseTags.IsMobileWallet)
                    ReadCardContextData.CardSource = emvCardReadModel2.CardSource = CardSourceType.Mobile;
                if (ParseOnGuardData(parseTags.FF1FOnGuardCardData) is EncryptedCardReadModel onGuardData)
                {
                    emvCardReadModel2.AESPAN = onGuardData.AESPAN;
                    emvCardReadModel2.AESPANLength = onGuardData.AESPANLength;
                    emvCardReadModel2.EncFormat = onGuardData.EncFormat;
                    emvCardReadModel2.EncryptedFlag = onGuardData.EncryptedFlag;
                    var errors = onGuardData.Errors;
                    if ((errors != null ? errors.Any() ? 1 : 0 : 0) != 0)
                        emvCardReadModel2.AddErrors(onGuardData.Errors);
                    emvCardReadModel2.Expiry = onGuardData.Expiry;
                    emvCardReadModel2.ExtLangCode = onGuardData.ExtLangCode;
                    emvCardReadModel2.FirstSix = onGuardData.FirstSix;
                    emvCardReadModel2.ICEncData = onGuardData.ICEncData;
                    emvCardReadModel2.ICEncDataLength = onGuardData.ICEncDataLength;
                    emvCardReadModel2.InjectedSerialNumber = onGuardData.InjectedSerialNumber;
                    emvCardReadModel2.KSN = onGuardData.KSN;
                    emvCardReadModel2.LanguageCode = onGuardData.LanguageCode;
                    emvCardReadModel2.LastFour = onGuardData.LastFour;
                    emvCardReadModel2.LSEncData = onGuardData.LSEncData;
                    emvCardReadModel2.LSEncDataLength = onGuardData.LSEncDataLength;
                    emvCardReadModel2.MfgSerialNumber = onGuardData.MfgSerialNumber;
                    emvCardReadModel2.Mod10CheckFlag = onGuardData.Mod10CheckFlag;
                    emvCardReadModel2.Name = onGuardData.Name;
                    emvCardReadModel2.NameLength = onGuardData.NameLength;
                    emvCardReadModel2.PANLength = onGuardData.PANLength;
                    emvCardReadModel2.ServiceCode = onGuardData.ServiceCode;
                }

                emvCardReadModel2.Name = parseTags.CardHolderName;
                var emvCardReadModel3 = emvCardReadModel2;
                var cardHolderName = parseTags.CardHolderName;
                var length = cardHolderName != null ? cardHolderName.Length : 0;
                emvCardReadModel3.NameLength = length;
                emvCardReadModel2.FallbackStatusAction = parseTags.Fallback;
                emvCardReadModel2.FallbackReason = parseTags.FallbackReason;
                emvCardReadModel2.ErrorCode = parseTags.ErrorCode;
                if (!string.IsNullOrWhiteSpace(ReadCardContextData.ResponseDataModel?.Name))
                {
                    emvCardReadModel2.Name = ReadCardContextData.ResponseDataModel.Name;
                    emvCardReadModel2.NameLength = ReadCardContextData.ResponseDataModel.NameLength;
                }

                ReadCardContextData.ResponseDataModel = emvCardReadModel2;
                ReadCardContextData.CardBrand = parseTags.CardBrand;
                var num = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            }
            catch (Exception ex)
            {
                LogException(nameof(EMVDataFromDevice), ex);
                ReadCardContextData.Errors.Add(new Error
                {
                    Code = "EMVDataError",
                    Message = "Error Reading EMV Data From Device."
                });
                ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
            }
        }

        private bool M87e2eeCardRead()
        {
            Log("87 E2EE Card Read Message Received");
            try
            {
                var ch = '\u001C';
                var str = _rba_api.GetParam(PARAMETER_ID.P87_RES_EXIT_TYPE);
                int result;
                ReadCardContextData.ExitType = int.TryParse(str, out result)
                    ? (CardReadExitType)result
                    : CardReadExitType.InvalidExitCode;
                Log("P87_RES_EXIT_TYPE: (" + str + ") - " + ExitCodeText(str));
                var sourceType = _rba_api.GetParam(PARAMETER_ID.P87_RES_CARD_SOURCE);
                CardSource = ReadCardContextData.CardSource = ParseCardSourceType(sourceType);
                Log(string.Format("P87_RES_CARD_SOURCE: ({0}) - {1}", sourceType, ReadCardContextData.CardSource));
                if (str != "0")
                    return false;
                if (string.IsNullOrWhiteSpace(sourceType))
                    Log("Could not detect card source, assuming source as MSD");
                else
                    switch (ReadCardContextData.CardSource)
                    {
                        case CardSourceType.Swipe:
                        case CardSourceType.MSDContactless:
                            if (ReadCardContextData.IsVasEnabled && ReadCardContextData.CanContinue(VasMode.VasOnly))
                                ReadCardContextData.UpdateStopStateFor(VasMode.VasOnly);
                            _mre09CardRemoved.Set();
                            break;
                        case CardSourceType.EMVContact:
                        case CardSourceType.QuickChip:
                            Log(string.Format("CardType: EMV Data. - {0}", ReadCardContextData.CardSource));
                            if (ReadCardContextData.IsVasEnabled && ReadCardContextData.CanContinue(VasMode.VasOnly))
                                ReadCardContextData.UpdateStopStateFor(VasMode.VasOnly);
                            _mre09CardRemoved.Reset();
                            return true;
                        case CardSourceType.EMVContactless:
                            Log(string.Format("CardType: EMV Contactless Data. - {0}", ReadCardContextData.CardSource));
                            _mre09CardRemoved.Set();
                            return true;
                        case CardSourceType.VASOnly:
                            _mre09CardRemoved.Set();
                            return true;
                        default:
                            ReadCardContextData.Errors.Add(new Error
                            {
                                Code = "E2EEICS",
                                Message = "Invalid Card Source When Reading Card Read Data."
                            });
                            _mre09CardRemoved.Set();
                            Log(string.Format("Unhandled Card Source Type: {0}", ReadCardContextData.CardSource));
                            ReadCardContextData.Errors.Add(new Error
                            {
                                Code = "UnknownCardSource",
                                Message = string.Format("Unhandled Card Source Type: {0}",
                                    ReadCardContextData.CardSource)
                            });
                            ReadCardContextData.UpdateStopStateFor(VasMode.VasAndPay);
                            return false;
                    }

                var cardData = _rba_api.GetParam(PARAMETER_ID.P87_RES_CARD_DATA);
                if (!string.IsNullOrEmpty(cardData) && cardData.IndexOf(ch) != -1)
                    cardData = cardData.Replace(ch.ToString(), "");
                Log("P87_RES_CARD_DATA: " + cardData);
                if (cardData.Length <= 0)
                {
                    ReadCardContextData.Errors.Add(new Error
                    {
                        Code = "SwipeHasNoData",
                        Message = "Card Swipe has no data."
                    });
                    ReadCardContextData.UpdateStopStateFor(VasMode.PayOnly);
                    return false;
                }

                ReadCardContextData.ResponseDataModel = ParseOnGuardData(cardData);
                return true;
            }
            catch (Exception ex)
            {
                ReadCardContextData.Errors.Add(new Error
                {
                    Code = "E2EEUH",
                    Message = "Unhandled Exception When Reading Card Read Data."
                });
                Log("Exception occurred" + ex);
                ReadCardContextData.Errors.Add(new Error
                {
                    Code = "87ReadCardUnhandled",
                    Message = "Unhandled exception occured in 87 - Read Card Encrypted Response from device."
                });
                ReadCardContextData.UpdateStopStateFor(VasMode.VasAndPay);
            }

            return false;
        }

        private void TerminalMessageHandler(MESSAGE_ID messageId)
        {
            Log(string.Format("Device Message: {0}", messageId));
            switch (messageId)
            {
                case MESSAGE_ID.M00_OFFLINE:
                    _areOfflineResponse.Set();
                    break;
                case MESSAGE_ID.M09_SET_ALLOWED_PAYMENT:
                    M09SetAllowedPayment();
                    break;
                case MESSAGE_ID.M16_CONTACTLESS_MODE:
                    M16ContactlessMode();
                    _mre166VasResponse.Set();
                    break;
                case MESSAGE_ID.M17_MERCHANT_DATA_WRITE:
                    M17_MERCHANT_DATA_WRITE();
                    _mre17MerchantDataWriteResponse.Set();
                    break;
                case MESSAGE_ID.M23_CARD_READ:
                    M23CardRead();
                    _mreInsertCard.Set();
                    break;
                case MESSAGE_ID.M87_E2EE_CARD_READ:
                    _mre166VasResponse.Set();
                    M87e2eeCardRead();
                    _mreInsertCard.Set();
                    _mre166VasResponse.Set();
                    break;
                case MESSAGE_ID.M33_01_EMV_STATUS:
                    var num = (int)M3301Status();
                    break;
                case MESSAGE_ID.M33_02_EMV_TRANSACTION_PREPARATION_RESPONSE:
                    EMVDataFromDevice(messageId, PARAMETER_ID.P33_02_RES_STATUS);
                    _mre3302TransactionPrepareResponse.Set();
                    break;
                case MESSAGE_ID.M33_03_EMV_AUTHORIZATION_REQUEST:
                    EMVDataFromDevice(messageId, PARAMETER_ID.P33_03_REQ_STATUS);
                    _mre3302TransactionPrepareResponse.Set();
                    _mre3303AuthorizationRequest.Set();
                    break;
                case MESSAGE_ID.M33_05_EMV_AUTHORIZATION_CONFIRMATION:
                    EMVDataFromDevice(messageId, PARAMETER_ID.P33_05_RES_STATUS);
                    _mre3302TransactionPrepareResponse.Set();
                    _mre3303AuthorizationRequest.Set();
                    _mre3305AuthorizationConfirmation.Set();
                    break;
                default:
                    Log(string.Format("UNHANDLED MESSAGEID: {0}", messageId));
                    break;
            }
        }

        private void M17_MERCHANT_DATA_WRITE()
        {
            var str1 = _rba_api.GetParam(PARAMETER_ID.P17_RES_STATUS);
            _rba_api.GetParam(PARAMETER_ID.P17_RES_TYPE);
            var paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA);
            _rba_api.GetParamLen(PARAMETER_ID.P17_RES_DATA);
            var values = new List<string>();
            for (; str1 == "0" && paramLen > 0; paramLen = _rba_api.GetParamLen(PARAMETER_ID.P17_RES_VAS_DATA))
            {
                var str2 = _rba_api.GetParam(PARAMETER_ID.P17_RES_VAS_DATA);
                values.Add(str2);
            }

            _rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_CMD_ID_FOR_NOTIFY);
            _rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_CMD_TYPE);
            _rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_DATA);
            _rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_IDX);
            _rba_api.GetParam(PARAMETER_ID.P17_RES_CLESS_CARD_NEED_NOTIFY);
            _rba_api.GetParam(PARAMETER_ID.P17_RES_IS_ADDITIONAL_DATA);
            _rba_api.GetParam(PARAMETER_ID.P17_REQ_CLESS_CARD_CMD_TYPE);
            Log("M17_MerchDataWrite - " + string.Join(",", values));
        }

        private void M16ContactlessMode()
        {
            var readCardContextData = ReadCardContextData;
            if (readCardContextData == null)
            {
                LogError("M16 Response - No Context Available.");
            }
            else if (!readCardContextData.State.HasFlag(ReadCardContextState.VasContinue))
            {
                LogError("M16 Response - Vas Flow is in Disabled State.");
            }
            else
            {
                if (_rba_api.GetParam(PARAMETER_ID.P16_RES_TYPE) != "6")
                    Log("Not vas.");
                var str1 = _rba_api.GetParam(PARAMETER_ID.P16_RES_DATA);
                var str2 = _rba_api.GetParam(PARAMETER_ID.P16_RES_IS_ADDITIONAL_DATA);
                var str3 = _rba_api.GetParam(PARAMETER_ID.P16_RES_STATUS);
                var vasMessageStatus1 = Enum.TryParse(typeof(VasMessageStatus), str1, out var obj)
                    ? (VasMessageStatus)obj
                    : VasMessageStatus.VAS_ERROR_UNKNOWN;
                var vasMessageStatus2 = Enum.IsDefined(typeof(VasMessageStatus), vasMessageStatus1)
                    ? vasMessageStatus1
                    : VasMessageStatus.VAS_ERROR_UNKNOWN;
                if ((str3 != "0" && str3 != "6") || (vasMessageStatus2 != VasMessageStatus.VAS_STATUS_OK &&
                                                     vasMessageStatus2 != VasMessageStatus.VAS_STATUS_CANCELLED &&
                                                     vasMessageStatus2 !=
                                                     VasMessageStatus.VAS_STATUS_COMPLETE_TAP &&
                                                     vasMessageStatus2 !=
                                                     VasMessageStatus.VAS_STATUS_EXPECT_PAYMENT &&
                                                     vasMessageStatus2 != VasMessageStatus
                                                         .VAS_ERROR_MOBILE_DATA_NOT_AVAILABLE))
                {
                    ReadCardContextData.UpdateStopStateFor(VasMode.VasOnly);
                    if (!readCardContextData.IsPayEnabled)
                        readCardContextData.Errors.Add(new Error
                        {
                            Code = "VAS166",
                            Message = string.Format("Failed 16 Command with error {0} - {1}", str1, vasMessageStatus2)
                        });
                }

                Log(string.Format("data = {0} - vas message status = {1}, additionalData = {2}, status = {3}", str1,
                    vasMessageStatus2, str2, str3));
            }
        }

        private string ExitCodeText(string exitType)
        {
            string str;
            switch (exitType)
            {
                case "0":
                    str = "Good Read";
                    break;
                case "1":
                    str = "Bad Read";
                    break;
                case "2":
                    str = "Cancelled";
                    break;
                case "3":
                    str = "Button Pressed";
                    break;
                case "4":
                    str = "Cless Card Floor Limit Exceeded";
                    break;
                case "5":
                    str = "Max Cless FLoor Limit Exceeded";
                    break;
                case "6":
                    str = "Invalid Prompt";
                    break;
                case "7":
                    str = "Encryption Failed";
                    break;
                case "8":
                    str = "Bad Key card";
                    break;
                case "9":
                    str = "Bad Format of 23";
                    break;
                case "A":
                    str = "Amount was not set and the contactless reader was not enabled";
                    break;
                case "R":
                    str = "At least one specified reader is disabled";
                    break;
                default:
                    str = "Not performed or unknown exit type";
                    break;
            }

            return str;
        }

        private string ExitCodeText(CardReadExitType exitType)
        {
            string str;
            switch (exitType)
            {
                case CardReadExitType.GoodRead:
                    str = "Good Read";
                    break;
                case CardReadExitType.BadRead:
                    str = "Bad Read";
                    break;
                case CardReadExitType.Cancelled:
                    str = "Cancelled";
                    break;
                case CardReadExitType.ButtonPressed:
                    str = "Button Pressed";
                    break;
                case CardReadExitType.ClessCardFloorLimitExceeded:
                    str = "Cless Card Floor Limit Exceeded";
                    break;
                case CardReadExitType.MaxClessFloorLimitExceeded:
                    str = "Max Cless FLoor Limit Exceeded";
                    break;
                case CardReadExitType.InvalidPrompt:
                    str = "Invalid Prompt";
                    break;
                case CardReadExitType.EncryptionFailed:
                    str = "Encryption Failed";
                    break;
                case CardReadExitType.BadKeyCard:
                    str = "Bad Key card";
                    break;
                case CardReadExitType.BadFormatOf23:
                    str = "Bad Format of 23";
                    break;
                case CardReadExitType.AmountWasNotSetAndContactlessReaderWasNotEnabled:
                    str = "Amount was not set and the contactless reader was not enabled";
                    break;
                case CardReadExitType.AtLeastOneSpecifiedReaderIsDisabled:
                    str = "At least one specified reader is disabled";
                    break;
                default:
                    str = "Not performed or unknown exit type";
                    break;
            }

            return str;
        }

        private CardSourceType ParseCardSourceType(string sourceType)
        {
            CardSourceType cardSourceType;
            switch (sourceType)
            {
                case "C":
                    cardSourceType = CardSourceType.MSDContactless;
                    break;
                case "E":
                    cardSourceType = CardSourceType.EMVContactless;
                    break;
                case "M":
                    cardSourceType = CardSourceType.Swipe;
                    break;
                case "Q":
                    cardSourceType = CardSourceType.QuickChip;
                    break;
                case "S":
                    cardSourceType = CardSourceType.EMVContact;
                    break;
                case "c":
                    cardSourceType = CardSourceType.VASOnly;
                    break;
                case "cq":
                    cardSourceType = CardSourceType.VASOnly;
                    break;
                case "m":
                    cardSourceType = CardSourceType.Mobile;
                    break;
                default:
                    cardSourceType = CardSourceType.Unknown;
                    break;
            }

            return cardSourceType;
        }

        private bool Offline(ReadCardContext context)
        {
            return Offline();
        }

        private bool Offline(int attempts = 3)
        {
            var result = ERROR_ID.RESULT_ERROR;
            var source = new List<ERROR_ID>();
            while (result != ERROR_ID.RESULT_SUCCESS && attempts-- > 0)
            {
                result = _rba_api.ProcessMessage(MESSAGE_ID.M00_OFFLINE);
                HandleAndLogResult(result, "00 Offline");
                source.Add(result);
                var millisecondsTimeout = 5250;
                if (result == ERROR_ID.RESULT_SUCCESS && !_areOfflineResponse.WaitOne(millisecondsTimeout))
                {
                    Log(string.Format("Offline Not received in wait of {0}", millisecondsTimeout));
                    return false;
                }
            }

            if (source.All(x => x == ERROR_ID.RESULT_ERROR_TIMEOUT))
                Reconnect();
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private bool SetAmount(ReadCardContext context)
        {
            var nullable1 = context?.Request?.Amount;
            string amount;
            if (!(nullable1.GetValueOrDefault() > 0M))
            {
                amount = "100";
            }
            else
            {
                decimal? nullable2;
                if (context == null)
                {
                    nullable1 = new decimal?();
                    nullable2 = nullable1;
                }
                else
                {
                    nullable2 = context.Request.Amount;
                }

                nullable1 = nullable2;
                amount = (nullable1.Value * 100M).ToString("0");
            }

            if (SetAmount(amount))
                return true;
            context.Errors.Add(new Error
            {
                Code = "RBA001",
                Message = "Set Amount Message Failed."
            });
            context.ResponseDataModel = context.CardSource == CardSourceType.EMVContact
                ? new EMVCardReadModel()
                : (Base87CardReadModel)new EncryptedCardReadModel();
            context.ResponseDataModel.Status = ResponseStatus.Errored;
            return false;
        }

        private bool WaitForVasData(ReadCardContext context)
        {
            if (context.IsVasEnabled)
            {
                PerformActionAndCheckSuccess(GetVasData, null, context, VasMode.VasOnly);
                if (!context.CanContinue(VasMode.VasAndPay))
                {
                    context.Errors.Add(new Error
                    {
                        Code = "VASDATA001",
                        Message = "Error When Attempting To Get Vas Data."
                    });
                    if (!context.IsPayEnabled)
                    {
                        var readCardContext = context;
                        var emvCardReadModel = new EMVCardReadModel();
                        emvCardReadModel.Status = ResponseStatus.Errored;
                        readCardContext.ResponseDataModel = emvCardReadModel;
                    }

                    return false;
                }
            }

            return true;
        }

        private bool WaitForCardReadComplete(ReadCardContext context)
        {
            if (!_mreInsertCard.WaitOne(context.TimeRemainingUntilTimeout))
            {
                Log("No Device Interaction.");
                context.Errors.Add(new Error
                {
                    Code = "TimeOut",
                    Message = "No User interaction."
                });
                var readCardContext = context;
                var encryptedCardReadModel = new EncryptedCardReadModel();
                encryptedCardReadModel.Status = ResponseStatus.Errored;
                readCardContext.ResponseDataModel = encryptedCardReadModel;
                return false;
            }

            if (context.ExitType != CardReadExitType.GoodRead)
            {
                context.Errors.Add(new Error
                {
                    Code = "BadExitType",
                    Message = string.Format("({0}) - {1}", context.ExitType, ExitCodeText(context.ExitType))
                });
                Log("Exit Type Not Good Read.");
            }

            if (context.IsPayEnabled &&
                (((context.CardSource == CardSourceType.MSDContactless || context.CardSource == CardSourceType.Swipe) &&
                  context.ResponseDataModel != null) || context.CardSource == CardSourceType.QuickChip ||
                 context.CardSource == CardSourceType.EMVContact ||
                 context.CardSource == CardSourceType.EMVContactless || context.CardSource == CardSourceType.Mobile))
                return true;
            return context.CardSource == CardSourceType.VASOnly && context.IsVasEnabled;
        }

        private bool WaitForVasReadStart(ReadCardContext context)
        {
            if (context.IsVasEnabled && context.CanContinue(VasMode.VasOnly) &&
                !_mre166VasResponse.WaitOne(context.TimeRemainingUntilTimeout))
            {
                Log("No Device Interaction.");
                context.State = ReadCardContextState.Timeout;
                context.Errors.Add(new Error
                {
                    Code = "TimeOut",
                    Message = "No User interaction."
                });
                var readCardContext = context;
                var encryptedCardReadModel = new EncryptedCardReadModel();
                encryptedCardReadModel.Status = ResponseStatus.Errored;
                readCardContext.ResponseDataModel = encryptedCardReadModel;
                return false;
            }

            if (!context.CanContinue(VasMode.VasOnly))
            {
                Log("No Longer in Vas Flow.");
                return false;
            }

            context.SendCardProcessing();
            return true;
        }

        private bool CardReadRequestEncrypted(ReadCardContext context)
        {
            _mreInsertCard.Reset();
            if (CardReadRequestEncrypted(context.Request.InputType))
                return true;
            Log("Encrypted Read Failed.");
            context.Errors.Add(new Error
            {
                Code = "RBA001",
                Message = "87 Read Card Encrypted Message Failed."
            });
            var readCardContext = context;
            var encryptedCardReadModel = new EncryptedCardReadModel();
            encryptedCardReadModel.Status = ResponseStatus.Errored;
            readCardContext.ResponseDataModel = encryptedCardReadModel;
            context.SendCompletedCallback();
            return false;
        }

        private bool CardReadRequestEncrypted(DeviceInputType? inputs = DeviceInputType.M)
        {
            if (!inputs.HasValue)
                inputs = DeviceInputType.M;
            var data1 = "COD.K3Z";
            if (!string.IsNullOrEmpty(data1))
            {
                var num1 = (int)_rba_api.SetParam(PARAMETER_ID.P87_REQ_FORM_NAME, data1);
            }

            var description = inputs.GetDescription();
            if (!string.IsNullOrEmpty(description))
            {
                var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P87_REQ_PROMPT_INDEX, description);
            }

            var data2 = inputs.ToString();
            if (!string.IsNullOrEmpty(data2))
            {
                var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P87_REQ_ENABLE_DEVICES, data2);
            }

            var result = _rba_api.ProcessMessage(MESSAGE_ID.M87_E2EE_CARD_READ);
            HandleAndLogResult(result, "87 OnGuard Card Read");
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private bool CardReadRequestUnencrypted(DeviceInputType? inputs = DeviceInputType.M)
        {
            if (!inputs.HasValue)
                inputs = DeviceInputType.M;
            var data1 = "COD.K3Z";
            if (!string.IsNullOrEmpty(data1))
            {
                var num1 = (int)_rba_api.SetParam(PARAMETER_ID.P23_REQ_FORM_NAME, data1);
            }

            var description = inputs.GetDescription();
            if (!string.IsNullOrEmpty(description))
            {
                var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P23_REQ_PROMPT_INDEX, description);
            }

            var data2 = inputs.ToString();
            if (!string.IsNullOrEmpty(data2))
            {
                var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P23_REQ_ENABLE_DEVICES, data2);
            }

            var result = _rba_api.ProcessMessage(MESSAGE_ID.M23_CARD_READ);
            HandleAndLogResult(result, "23 Card Read");
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private string VasModeToIngenicoVasMode(VasMode mode)
        {
            var ingenicoVasMode = (string)null;
            switch (mode)
            {
                case VasMode.PayOnly:
                    ingenicoVasMode = "2";
                    break;
                case VasMode.VasOnly:
                    ingenicoVasMode = "0";
                    break;
                case VasMode.VasAndPay:
                    ingenicoVasMode = "1";
                    break;
            }

            return ingenicoVasMode;
        }

        private bool EnableVas(bool enable = true)
        {
            _isVasInitialized = enable;
            return M17_6SendVasMessage(VasCmd.SetEnable, enable ? "1" : "0").Success;
        }

        private bool AddAppleVas(string identifier, ReadCardContext context)
        {
            if (context?.Request.AppleVasUrl == null)
            {
                var empty = string.Empty;
            }

            return M17_6SendVasMessage(VasCmd.AddAppleMerchantId, identifier, "tbd", "0100000000", "1").Success;
        }

        private bool AddGoogleVas(string identifier, ReadCardContext context)
        {
            return M17_6SendVasMessage(VasCmd.AddGoogleMerchantId, identifier, "PLACE03", "TRMNL01", "Google Store",
                "en", "1", "0").Success;
        }

        private void InitializeVas()
        {
            if (!SupportsVas)
                return;
            EnableVas();
            AddAppleVas("pass.com.apple.wallet.vas.prodtest", null);
            AddAppleVas("pass.com.ingenico.us.vas.test", null);
            AddAppleVas("pass.com.redbox.kiosk", null);
            AddGoogleVas("70779451", null);
        }

        private bool PrepareVas(ReadCardContext context)
        {
            if (!SupportsVas)
            {
                if (_isVasInitialized)
                    EnableVas(false);
                return true;
            }

            if (!_isVasInitialized && !EnableVas())
                return false;
            var ingenicoVasMode = VasModeToIngenicoVasMode(context?.Request?.VasMode ?? default);
            if (context != null && context.IsVasEnabled)
            {
                M17_6SendVasMessage(VasCmd.SetMode, ingenicoVasMode);
                M17_6SendVasMessage(VasCmd.ClearAppleMerchantList);
                AddAppleVas(IsProdEnvironment ? "pass.com.vibes.redbox" : "pass.com.vibes.RedboxQAEnvironment",
                    context);
                var vasResult1 = M17_6SendVasMessage(VasCmd.GetAppleMerchantCount);
                AddGoogleVas(IsProdEnvironment ? "44813557" : "70779451", context);
                var vasResult2 = M17_6SendVasMessage(VasCmd.GetGoogleMerchantCount);
                Log("Apple: " + vasResult1.VasDataResult + " - Google: " + vasResult2.VasDataResult);
            }
            else
            {
                M17_6SendVasMessage(VasCmd.SetMode, ingenicoVasMode);
            }

            return true;
        }

        private bool GetVasData(ReadCardContext context)
        {
            if (!context.CanContinue(VasMode.VasOnly))
                return true;
            var vasResult = M17_6SendVasMessage(VasCmd.GetData);
            context.VasData = vasResult.GetVasData(context);
            if (context.IsPayEnabled)
                return true;
            return vasResult.Success && !string.IsNullOrWhiteSpace(context.VasData);
        }

        private void SetVariable_28(string varID, string varData)
        {
            var num1 = (int)_rba_api.SetParam(PARAMETER_ID.P28_REQ_RESPONSE_TYPE, "1");
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P28_REQ_VARIABLE_ID, varID);
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P28_REQ_VARIABLE_DATA, varData);
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M28_SET_VARIABLE);
            var status = _rba_api.GetParam(PARAMETER_ID.P28_RES_STATUS);
            var statusText = GetStatusText(status);
            HandleAndLogResult(result,
                "28 Set Variable ID: " + varID + ", Data: " + varData + " - P28_RES_STATUS: " + status + " - " +
                statusText + " -");
        }

        private static string GetStatusText(string status)
        {
            var statusText = string.Empty;
            switch (status)
            {
                case "2":
                    statusText = "Success";
                    break;
                case "3":
                    statusText = "Error";
                    break;
                case "4":
                    statusText = "Insufficient Memory";
                    break;
                case "5":
                    statusText = "Invalid Id";
                    break;
                case "6":
                    statusText = "No Data";
                    break;
            }

            return statusText;
        }

        private bool EMVTransInitiation(ReadCardContext context)
        {
            if (!EMVTransInitiation() || !_mre3302TransactionPrepareResponse.WaitOne(20000))
            {
                Log("Failed EMV Transaction Initiation or Timed out waiting for response.");
                context.Errors.Add(new Error
                {
                    Code = "RBA001",
                    Message = "EMV Initiate Message Failed."
                });
                var readCardContext = context;
                var emvCardReadModel = new EMVCardReadModel();
                emvCardReadModel.Status = ResponseStatus.Errored;
                readCardContext.ResponseDataModel = emvCardReadModel;
                context.SendCompletedCallback();
                return false;
            }

            if (!_mre3305AuthorizationConfirmation.WaitOne(0))
                return true;
            Log("Bad EMV Read Or Error.");
            context.Errors.Add(new Error
            {
                Code = "EMV001",
                Message = "EMV Data Issue."
            });
            context.SendCompletedCallback();
            return false;
        }

        private bool EMVTransInitiation()
        {
            var num1 = (int)_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_STATUS, "00");
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_EMVH_CURRENT_PACKET_NBR, "0");
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_EMVH_PACKET_TYPE, "0");
            var num4 = (int)_rba_api.SetParam(PARAMETER_ID.P33_00_REQ_RESEND_TIMER, "5000");
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M33_00_EMV_TRANSACTION_INITIATION);
            HandleAndLogResult(result, "33.00 EMV Transaction Initiation");
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private bool SetPaymentType_04(ReadCardContext context)
        {
            if (SetPaymentType_04("B", "00"))
                return true;
            context.Errors.Add(new Error
            {
                Code = "RBA001",
                Message = "Set Payment Message Failed."
            });
            var readCardContext = context;
            var emvCardReadModel = new EMVCardReadModel();
            emvCardReadModel.Status = ResponseStatus.Errored;
            readCardContext.ResponseDataModel = emvCardReadModel;
            context.SendCompletedCallback();
            return false;
        }

        private bool SetPaymentType_04(string paymentType, string amount)
        {
            var num1 = (int)_rba_api.SetParam(PARAMETER_ID.P04_REQ_FORCE_PAYMENT_TYPE, "0");
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P04_REQ_PAYMENT_TYPE, paymentType);
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P04_REQ_AMOUNT, amount);
            var errorId = _rba_api.ProcessMessage(MESSAGE_ID.M04_SET_PAYMENT_TYPE);
            Log(string.Format("ProcessMessage 04. {0}", errorId));
            var str1 = _rba_api.GetParam(PARAMETER_ID.P04_RES_FORCE_PAYMENT_TYPE);
            var str2 = string.Empty;
            switch (str1)
            {
                case "0":
                    str2 = "Success";
                    break;
                case "1":
                    str2 = "Failed";
                    break;
                case "2":
                    str2 = "Changed Customer Selection";
                    break;
            }

            Log("P04_RES_FORCE_PAYMENT_TYPE: (" + str1 + ") - " + str2);
            var str3 = _rba_api.GetParam(PARAMETER_ID.P04_RES_PAYMENT_TYPE);
            var str4 = "Unknown";
            switch (str3)
            {
                case "A":
                    str4 = "Debit";
                    break;
                case "B":
                    str4 = "Credit";
                    break;
                case "C":
                    str4 = "EBT Cash";
                    break;
                case "D":
                    str4 = "ET Food Stamps";
                    break;
                case "E":
                    str4 = "Store Charge";
                    break;
                case "F":
                    str4 = "Loyalty";
                    break;
                case "G":
                    str4 = "PayPal";
                    break;
            }

            Log("P04_RES_PAYMENT_TYPE: (" + str3 + ") - " + str4);
            Log("P04_RES_AMOUNT: " + _rba_api.GetParam(PARAMETER_ID.P04_RES_AMOUNT));
            return errorId == ERROR_ID.RESULT_SUCCESS;
        }

        private void SendAuthorizationReponse(bool success = true)
        {
            var num1 = (int)_rba_api.SetParam(PARAMETER_ID.P33_04_RES_STATUS, "00");
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P33_04_RES_EMVH_CURRENT_PACKET_NBR, "0");
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P33_04_RES_EMVH_PACKET_TYPE, "0");
            var num4 = (int)_rba_api.AddTagParam(MESSAGE_ID.M33_04_EMV_AUTHORIZATION_RESPONSE, 138,
                success ? "00" : "05");
            HandleAndLogResult(_rba_api.ProcessMessage(MESSAGE_ID.M33_04_EMV_AUTHORIZATION_RESPONSE),
                string.Format("33.04 EMV Auth Response: {0}", success));
        }

        private static string ConfigStatusText(string statusNum)
        {
            var str = string.Empty;
            switch (statusNum)
            {
                case "1":
                    str = "Unknown Error";
                    break;
                case "2":
                    str = "Success";
                    break;
                case "3":
                    str = "Error, One Or More Parameters Have An Invalid Id";
                    break;
                case "4":
                    str = "Error, One Or More Paramaters Were Not Updated";
                    break;
                case "5":
                    str = "Message Rejected, Wrong Message Format";
                    break;
                case "9":
                    str = "Message Rejected, Cannot Be Executed";
                    break;
            }

            return str;
        }

        private bool RebootInner()
        {
            var result = _rba_api.ProcessMessage(MESSAGE_ID.M97_REBOOT);
            HandleAndLogResult(result, "97 Reboot");
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private static EncodedData GetEncodedData(Stream ist)
        {
            var encodedData = new EncodedData();
            var byteList = new List<byte>(4074);
            var maxValue = byte.MaxValue;
            var num1 = -1;
            do
            {
                byte num2;
                try
                {
                    var num3 = ist.ReadByte();
                    if (num3 == num1)
                    {
                        encodedData.Result = EncodedResult.EndOfFile;
                        encodedData.EncData = byteList.ToArray();
                        return encodedData;
                    }

                    num2 = (byte)(num3 & byte.MaxValue);
                    ++encodedData.DataLength;
                }
                catch (IOException ex)
                {
                    encodedData.Result = EncodedResult.Error;
                    return encodedData;
                }

                if (num2 >= 0 && num2 <= 31)
                {
                    var num4 = (byte)(num2 + 32U);
                    byteList.Add(maxValue);
                    byteList.Add(num4);
                }
                else if (num2 >= 32 && num2 <= 254)
                {
                    byteList.Add(num2);
                }
                else if (num2 == byte.MaxValue)
                {
                    byteList.Add(maxValue);
                    byteList.Add(maxValue);
                }
            } while (byteList.Count < 4063);

            encodedData.EncData = byteList.ToArray();
            return encodedData;
        }

        private bool WriteData(
            EncodedData data,
            string fileName,
            WriteFileRecordType recordType,
            int segmentNumber,
            int retryOnTimeout = 3)
        {
            var result = ERROR_ID.RESULT_ERROR;
            var unpackFlag = UnpackFlag.WriteFileDirectly;
            while (result != ERROR_ID.RESULT_SUCCESS && retryOnTimeout-- > 0)
            {
                var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
                var data1 = ((int)recordType).ToString();
                var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_RECORD_TYPE, data1);
                var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_ENCODING_FORMAT, "8");
                var data2 = string.Format("{0:D2}", segmentNumber);
                var num4 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_SEGMENT_NBR, data2);
                var data3 = ((int)unpackFlag).ToString();
                var num5 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_UNPACK_FLAG, data3);
                var num6 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_FAST_DOWNLOAD, "1");
                if (recordType == WriteFileRecordType.NewFileFull ||
                    recordType == WriteFileRecordType.NewFileInitialMulti)
                {
                    var num7 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_FILE_NAME, fileName);
                }

                var num8 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_FILE_DATA, data.EncData);
                var num9 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_LAST_MESSAGE_TIMEOUT_SEC, "180");
                result = _rba_api.ProcessMessage(MESSAGE_ID.M62_FILE_WRITE);
                HandleAndLogResult(result,
                    string.Format(
                        "62 FileWrite: {0} - Record Type: {1} - Segment: {2} - Length: {3} - Result Status: {4}",
                        fileName, recordType, segmentNumber, data.DataLength,
                        _rba_api.GetParam(PARAMETER_ID.P62_RES_STATUS)));
            }

            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private WriteFileRecordType GetWriteFileRecordType(bool firstFile, bool lastFile)
        {
            if (firstFile & lastFile)
                return WriteFileRecordType.NewFileFull;
            if (firstFile)
                return WriteFileRecordType.NewFileInitialMulti;
            return lastFile ? WriteFileRecordType.LastRecord : WriteFileRecordType.Continuation;
        }

        private static Stream ReduceRkiStream(StreamReader sr, string serialNumber)
        {
            string str;
            while (sr != null && !sr.EndOfStream && !string.IsNullOrWhiteSpace(str = sr?.ReadLine()))
                if (str.StartsWith(serialNumber))
                    return new MemoryStream(Encoding.UTF8.GetBytes(str));
            return null;
        }

        private async Task<bool> WriteStreamInner(
            Stream stream,
            string fileName,
            bool rebootAfterWrite)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Length > 15)
            {
                LogError(string.Format("Must have a filename with 1-15 characters.  FileName: {0} has {1} characters.",
                    fileName, fileName.Length));
                return false;
            }

            var str1 = fileName;
            var stream1 = stream;
            var length1 = stream1 != null ? stream1.Length : 0L;
            var local1 = rebootAfterWrite;
            Log(string.Format("Write Steam File: {0} - Length: {1} bytes - reboot: {2}", str1, length1,
                local1));
            if (!stream.CanRead)
            {
                LogError("WriteFileInner Can't Read Stream for " + fileName);
                return false;
            }

            if (Path.GetExtension(fileName).ToUpper() == ".RKI")
            {
                stream = ReduceRkiStream(new StreamReader(stream), UnitData?.UnitSerialNumber);
                if (stream == null)
                    return true;
            }

            var flag1 = true;
            var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            var firstFile = true;
            var flag2 = true;
            var segmentNumber = 1;
            long num2 = 0;
            while (flag2)
            {
                var encodedData = GetEncodedData(stream);
                num2 += encodedData.DataLength;
                switch (encodedData.Result)
                {
                    case EncodedResult.Success:
                        var writeFileRecordType1 = GetWriteFileRecordType(firstFile, false);
                        if (!WriteData(encodedData, fileName, writeFileRecordType1, segmentNumber))
                        {
                            LogError("WriteData Failed.");
                            return false;
                        }

                        break;
                    case EncodedResult.EndOfFile:
                        flag2 = false;
                        var writeFileRecordType2 = GetWriteFileRecordType(firstFile, true);
                        if (!WriteData(encodedData, fileName, writeFileRecordType2, segmentNumber))
                        {
                            LogError("WriteData Failed.");
                            return false;
                        }

                        break;
                    case EncodedResult.Error:
                        Log("Aborting File Transfer of " + fileName + ".  Error During GetEncodedData.");
                        WriteData(encodedData, fileName, WriteFileRecordType.AbortRequest, segmentNumber);
                        return false;
                }

                var local2 = num2;
                var stream2 = stream;
                var length2 = stream2 != null ? stream2.Length : 0L;
                Log(string.Format("{0} of {1} bytes Written.", local2, length2));
                firstFile = false;
                ++segmentNumber;
                if (segmentNumber % 100 == 0)
                    segmentNumber = 1;
            }

            if (rebootAfterWrite)
                _rebootType = RebootType.FileUpdate;
            var flag3 = RebootInner();
            var str2 = !rebootAfterWrite ? string.Empty : string.Format("- Reboot Succesful: {0}", flag3);
            Log(string.Format("File {0} Transfered Successfully: {1} {2}", fileName, flag1, str2));
            return flag1 & flag3;
        }

        private async Task<bool> WriteFileInner(string fullPath, bool rebootAfterWrite)
        {
            Log(string.Format("Write File: {0}, reboot: {1}", fullPath, rebootAfterWrite));
            var fileName = Path.GetFileName(fullPath);
            if (Path.GetExtension(fullPath).ToUpper() == ".OGZ")
                rebootAfterWrite = false;
            var num1 = (int)_rba_api.ResetParam(PARAMETER_ID.P_ALL_PARAMS);
            var num2 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_RECORD_TYPE, "0");
            var num3 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_ENCODING_FORMAT, "8");
            var num4 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_UNPACK_FLAG, string.Format("{0}", 1));
            var num5 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_FAST_DOWNLOAD, "1");
            var num6 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_OS_FILE_NAME, fullPath);
            var num7 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_FILE_NAME, fileName);
            var num8 = (int)_rba_api.SetParam(PARAMETER_ID.P62_REQ_LAST_MESSAGE_TIMEOUT_SEC, "180");
            var result = _rba_api.ProcessMessage(MESSAGE_ID.FILE_WRITE);
            HandleAndLogResult(result, "62 FileWrite");
            Log("P62_RES_STATUS " + _rba_api.GetParam(PARAMETER_ID.P62_RES_STATUS));
            Log("P62_RES_FILE_LENGTH " + _rba_api.GetParam(PARAMETER_ID.P62_RES_FILE_LENGTH));
            var flag = true;
            if ((result == ERROR_ID.RESULT_SUCCESS) & rebootAfterWrite)
                flag = RebootInner();
            return (result == ERROR_ID.RESULT_SUCCESS) & flag;
        }

        public bool Disconnect()
        {
            ERROR_ID errorId;
            try
            {
                errorId = _rba_api.Disconnect();
            }
            catch (Exception ex)
            {
                LogException("Discconnect", ex);
                return false;
            }

            return errorId == ERROR_ID.RESULT_SUCCESS;
        }

        private bool SendCustomMessage(string message, bool waitForResponse = false)
        {
            var result = _rba_api.SendCustomMessage(message, waitForResponse);
            HandleAndLogResult(result, "SendCustomMessage " + message);
            return result == ERROR_ID.RESULT_SUCCESS;
        }

        private void InitializeReadCard()
        {
            Log("Initialize Read Card");
            _mre.Reset();
            _mreInsertCard.Reset();
            _mre09CardRemoved.Reset();
            _mre3302TransactionPrepareResponse.Reset();
            _mre3303AuthorizationRequest.Reset();
            _mre3304AuthorizationResponse.Reset();
            _mre3305AuthorizationConfirmation.Reset();
            _mre166VasResponse.Reset();
            CardSource = CardSourceType.Unknown;
        }

        private RebootStatus GetRebootStatus()
        {
            var disconnectTime = _currentRebootStatus?.DisconnectTime;
            var rebootStatus = new RebootStatus();
            rebootStatus.ExpectedTime = GetExpectedRebootTime();
            rebootStatus.DeviceTime = GetDeviceTime();
            rebootStatus.Id = _unitData?.UnitSerialNumber;
            rebootStatus.DisconnectTime = disconnectTime;
            rebootStatus.Type = _rebootType;
            var kioskData = _kioskData;
            rebootStatus.KioskId = kioskData != null ? kioskData.KioskId : 0L;
            _currentRebootStatus = rebootStatus;
            var logger = _logger;
            if (logger != null)
                logger.LogInformation("Reboot Status: " + JsonConvert.SerializeObject(_currentRebootStatus));
            return _currentRebootStatus;
        }

        private DateTime? GetDeviceTime()
        {
            var s = "";
            try
            {
                s = GetVariable_29("202") + GetVariable_29("201");
                return DateTime.ParseExact(s, "MMddyyHHmmss", CultureInfo.InvariantCulture);
            }
            catch
            {
                var logger = _logger;
                if (logger != null)
                    logger.LogInformation("GetDeviceTime: Could not parse date from deviceTimeString - '" + s + "'");
                return new DateTime?();
            }
        }

        private DateTime GetConfigRebootTime()
        {
            if (_settings == null)
                return DateTime.Today;
            var configRebootTime = DateTime.Today +
                                   DataFileHelper.GetFileData<TimeSpan>(_settings.DataFilePath, "RebootTime.json",
                                       LogException);
            var now = DateTime.Now;
            if (configRebootTime < now - REBOOT_THRESHOLD)
                configRebootTime = configRebootTime.AddDays(1.0);
            return configRebootTime;
        }

        private async Task StartRebootCountdown()
        {
            if (_rebootTask != null && !_rebootTask.IsCompleted)
                return;
            await Task.Delay((int)(GetConfigRebootTime() - DateTime.Now).TotalMilliseconds);
            Reboot(RebootType.Scheduled);
        }

        private string GetKeys()
        {
            Log("Begin GetKeys");
            lock (_getKeysLock)
            {
                if (!IsConnected || _unitData.IsTampered)
                    return null;
                if (string.IsNullOrWhiteSpace(_keys))
                    _keys = GetVariable_29("810");
                if (string.IsNullOrWhiteSpace(_keys))
                    return null;
            }

            return string.Join("|", _keys.Split("\n", StringSplitOptions.RemoveEmptyEntries));
        }

        public bool UpdateFile(string fileName, Stream steam, bool rebootRequired)
        {
            throw new NotImplementedException();
        }

        public class Constants
        {
            public const string EMVClessFileName = "emvcless.xml";
            public const string EMVContactFileName = "emvcontact.xml";
            public const string DeviceFilesPath = "/HOST/";
        }

        private enum RetrieveFileDataFormat
        {
            Text,
            Base64
        }

        private class ConfigDefault
        {
            public string Group { get; set; }

            public string Index { get; set; }

            public string Value { get; set; }
        }

        private class EncodedData
        {
            public byte[] EncData { get; set; }

            public int DataLength { get; set; }

            public EncodedResult Result { get; set; }
        }

        private enum EncodedResult
        {
            Success,
            EndOfFile,
            Error
        }

        private enum WriteFileRecordType
        {
            NewFileFull = 0,
            NewFileInitialMulti = 1,
            Continuation = 2,
            LastRecord = 3,
            SetRKIVersion = 4,
            AbortRequest = 9
        }

        private enum UnpackFlag
        {
            UnpackAndVerifyAfterSending,
            WriteFileDirectly
        }
    }
}