using DeviceService.Client;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Redbox.KioskEngine.Environment
{
  public class DeviceServiceClientHelper : IDeviceServiceClientHelper
  {
    private OnDeviceServiceCanShutDown _onDeviceServiceCanShutDownHandler;
    private int _pollingInterval;
    private readonly object _pollingLock = new object();
    private Timer _pollingTimer;
    private IReadCardJob _readCardJob;
    private IDeviceServiceClient _deviceServiceClient;
    private CardReaderState _cardReaderState;
    private bool _statusChecked;

    public static DeviceServiceClientHelper Instance
    {
      get => Singleton<DeviceServiceClientHelper>.Instance;
    }

    public bool ConnectToDeviceService()
    {
      bool result = this.IsConnectedToDeviceService;
      if (!result)
      {
        if (this._deviceServiceClient == null)
          this.CreateDeviceServiceClient();
        if (this._deviceServiceClient != null)
        {
          string url = ServiceLocator.Instance.GetService<IConfiguration>()?.Lifetime.DeviceService.DeviceServiceClientUrl;
          Task.Run((Action) (() => result = this._deviceServiceClient.ConnectToDeviceService(url, 5000))).Wait();
          LogHelper.Instance.Log(string.Format("DeviceServiceClientHelper connected: {0}", (object) result));
        }
        else
          LogHelper.Instance.Log("Unable to create DeviceServiceClient.");
      }
      return result;
    }

    public void RegisterOnDeviceServiceCanShutDownHandler(
      OnDeviceServiceCanShutDown onDeviceServiceCanShutDown)
    {
      this._onDeviceServiceCanShutDownHandler = onDeviceServiceCanShutDown;
    }

    public void PollForCardReaderConnectionAndValidVersion()
    {
      lock (this._pollingLock)
      {
        if (this._pollingTimer == null)
        {
          this._pollingTimer = new Timer()
          {
            AutoReset = false
          };
          this._pollingTimer.Elapsed += new ElapsedEventHandler(this.PollingTimerElapsed);
        }
        if (this._pollingTimer.Enabled)
          return;
        this.ResetPollingInterval();
        this._pollingTimer.Interval = (double) this.GetPollingInterval();
        LogHelper.Instance.Log("Starting DeviceServiceClientHelpder polling timer");
        this._pollingTimer.Enabled = true;
      }
    }

    private void PollingTimerElapsed(object sender, ElapsedEventArgs e)
    {
      lock (this._pollingLock)
      {
        bool flag1 = this._deviceServiceClient.IsCardReaderConnected();
        bool flag2 = false;
        ValidateVersionModel validateVersionModel = (ValidateVersionModel) null;
        if (flag1)
        {
          validateVersionModel = this._deviceServiceClient.ValidateVersion();
          flag2 = validateVersionModel != null && validateVersionModel.IsCompatible;
        }
        if (flag1 & flag2)
        {
          LogHelper.Instance.Log("DeviceServiceClientHelper polling succeeded.");
          this.RaiseValidDeviceServiceCardReader();
        }
        else
        {
          if (!flag1)
            LogHelper.Instance.Log("DeviceServiceClientHelper polling failed.  card reader is not connected");
          else if (!flag2)
            LogHelper.Instance.Log(string.Format("DeviceServiceClientHelper polling failed.  device service version {0} is not compatible with device service client version {1}", (object) validateVersionModel?.DeviceServiceVersion, (object) this._deviceServiceClient?.AssemblyVersion));
          int pollingInterval = this.GetPollingInterval();
          LogHelper.Instance.Log(string.Format("Delay DeviceServiceClientHelper polling attempt {0}", (object) TimeSpan.FromMilliseconds((double) pollingInterval)));
          this._pollingTimer.Interval = (double) pollingInterval;
          this._pollingTimer.Enabled = true;
          this.PollForCardReaderConnectionAndValidVersion();
        }
      }
    }

    private int GetPollingInterval()
    {
      if (this._pollingInterval == 0)
        this._pollingInterval = 5000;
      else
        this._pollingInterval *= 2;
      int num = 300000;
      if (this._pollingInterval > num)
        this._pollingInterval = num;
      return this._pollingInterval;
    }

    private void ResetPollingInterval() => this._pollingInterval = 0;

    public IReadCardJob StartCardRead(
      DeviceInputType deviceInputType,
      Action<BaseResponseEvent> readCardResponseHandler,
      Action<BaseResponseEvent> readCardEventHandler,
      int commandTimeout,
      VasMode vasMode)
    {
      IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
      IShoppingSession currentSession = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetCurrentSession();
      IEnumerable<CardBrandAndSource> cardBrandSources = this.GetDeactivatedCardBrandSources();
      Decimal num = Math.Round(service?.CurrentRentalShoppingCart?.Totals.GrandTotal ?? 0, 2);
      this._readCardJob = this._deviceServiceClient.CreateReadCardJob(new CardReadRequest()
      {
        InputType = deviceInputType,
        VasMode = vasMode,
        Amount = new Decimal?(num),
        Timeout = commandTimeout + 5000,
        ExcludeCardBrandBySource = cardBrandSources,
        SessionId = new Guid?(currentSession.Id),
        AppleVasUrl = (string) null,
        GoogleVasUrl = (string) null
      }, readCardResponseHandler, readCardEventHandler, new int?(commandTimeout));
      LogHelper.Instance.Log(string.Format("Starting DeviceServiceClient.ReadCard with id {0}", (object) this._readCardJob?.RequestId));
      this._readCardJob.Execute();
      return this._readCardJob;
    }

    public void CancelCommand(Guid id)
    {
      LogHelper.Instance.Log(string.Format("Cancelling DeviceService command with id {0}", (object) id));
      this._deviceServiceClient?.CancelCommand(id, (Action<BaseResponseEvent>) null);
    }

    public void ReportAuthorizeResult(bool authorizeSuccessful)
    {
      LogHelper.Instance.Log(string.Format("Reporting Authorize Result of {0} to DeviceService", (object) authorizeSuccessful));
      this._deviceServiceClient.ReportAuthorizeResult(authorizeSuccessful, (Action<BaseResponseEvent>) (baseResponseEvent => LogHelper.Instance.Log(string.Format("DeviceServhice has acknowledged receipt of authorize result for request {0}", (object) baseResponseEvent?.RequestId))));
    }

    private void CreateDeviceServiceClient()
    {
      LogHelper.Instance.Log("Creating Device Service Client");
      Task.Run((Action) (() =>
      {
        if (this._deviceServiceClient != null)
          return;
        this._deviceServiceClient = (IDeviceServiceClient) new DeviceServiceClient()
        {
          AutoReconnect = true
        };
        this._deviceServiceClient.OnLog += new LogHandler(this.AddLogText);
        this._deviceServiceClient.OnConnectedHandler += new OnConnected(this.DeviceServiceClient_OnConnectedHandler);
        this._deviceServiceClient.OnDisconnectedHandler += new OnDisconnected(this.DeviceServiceClient_OnDisconnectedHandler);
        this._deviceServiceClient.OnCardReaderConnectedHandler += new OnConnected(this.DeviceServiceClient_OnCardReaderConnectedHandler);
        this._deviceServiceClient.OnCardReaderDisconnectedHandler += new OnDisconnected(this.DeviceServiceClient_OnCardReaderDisconnectedHandler);
        this._deviceServiceClient.OnDeviceServiceCanShutDownHandler += new OnDeviceServiceCanShutDown(this.DeviceServiceClient_OnDeviceServiceCanShutDownHandler);
        this._deviceServiceClient.OnDeviceServiceShutDownInfoChangeHandler += new OnDeviceServiceShutDownInfoChange(this.DeviceServiceClient_OnDeviceServiceShutDownInfoChangeHandler);
        this._deviceServiceClient.OnDeviceTamperedEventHandler += new Action(this.DeviceServiceClient_OnDeviceTamperedEventHandler);
        this._deviceServiceClient.OnCardReaderStateHandler += new OnCardReaderState(this.DeviceServiceClient_OnCardReaderStateHandler);
      }))?.Wait();
      LogHelper.Instance.Log(string.Format("Device Service Client created: {0}", (object) (this._deviceServiceClient != null)));
    }

    private void DeviceServiceClient_OnCardReaderStateHandler(CardReaderState cardReaderState)
    {
      this._cardReaderState = cardReaderState;
    }

    private void DeviceServiceClient_OnDeviceServiceShutDownInfoChangeHandler(
      IDeviceServiceShutDownInfo deviceServiceShutDownInfo)
    {
      LogHelper.Instance.Log(string.Format("DeviceService ShutDownInfo Changed: ShutDown Reason: {0}  ShutDown Time: {1}", (object) deviceServiceShutDownInfo?.ShutDownReason, (object) deviceServiceShutDownInfo?.ShutDownTime));
    }

    private void DeviceServiceClient_OnDeviceTamperedEventHandler()
    {
      LogHelper.Instance.Log("DeviceService Tampered Event received; put Kiosk into Maintenance Mode...");
      if (!this.IsCardReaderTampered)
      {
        this.IsCardReaderTampered = true;
        this.SendTamperedCardReaderKioskAlertMessage();
      }
      ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseTamperedCardReader();
    }

    private bool DeviceServiceClient_OnDeviceServiceCanShutDownHandler()
    {
      bool flag = true;
      if (this._onDeviceServiceCanShutDownHandler != null)
        flag = this._onDeviceServiceCanShutDownHandler();
      return flag;
    }

    private void DeviceServiceClient_OnCardReaderDisconnectedHandler(Exception exception)
    {
      this._cardReaderState = (CardReaderState) null;
      this.ResetPollingInterval();
      this.RaiseInvalidDeviceServiceCardReader();
    }

    private void DeviceServiceClient_OnCardReaderConnectedHandler()
    {
      this.CheckIfCardReaderIsConnectedAndValid(true);
    }

    private IEnumerable<CardBrandAndSource> GetDeactivatedCardBrandSources()
    {
      List<CardBrandAndSource> cardBrandSources = new List<CardBrandAndSource>();
      Dictionary<CardBrandAndSource, bool> dictionary = new Dictionary<CardBrandAndSource, bool>();
      DeviceServiceConfiguration deviceService = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskSession?.DeviceService;
      dictionary.Add(CardBrandAndSource.VisaTap, deviceService.ContactlessVisaEnabled);
      dictionary.Add(CardBrandAndSource.AmexTap, deviceService.ContactlessAmexEnabled);
      dictionary.Add(CardBrandAndSource.MasterCardTap, deviceService.ContactlessMasterCardEnabled);
      dictionary.Add(CardBrandAndSource.DiscoverTap, deviceService.ContactlessDiscoverEnabled);
      dictionary.Add(CardBrandAndSource.VisaChip, deviceService.VisaInsertChipEnabled);
      dictionary.Add(CardBrandAndSource.AmexChip, deviceService.AmexInsertChipEnabled);
      dictionary.Add(CardBrandAndSource.MasterCardChip, deviceService.MasterCardInsertChipEnabled);
      dictionary.Add(CardBrandAndSource.DiscoverChip, deviceService.DiscoverInsertChipEnabled);
      foreach (KeyValuePair<CardBrandAndSource, bool> keyValuePair in dictionary)
      {
        if (!keyValuePair.Value)
          cardBrandSources.Add(keyValuePair.Key);
      }
      return (IEnumerable<CardBrandAndSource>) cardBrandSources;
    }

    private void CheckIfCardReaderIsConnectedAndValid(bool cardReaderConnectionAlreadyVerified)
    {
      bool flag = false;
      if (!cardReaderConnectionAlreadyVerified)
      {
        IDeviceServiceClient deviceServiceClient = this._deviceServiceClient;
        if ((deviceServiceClient != null ? (deviceServiceClient.IsCardReaderConnected() ? 1 : 0) : 0) == 0)
          goto label_4;
      }
      ValidateVersionModel validateVersionModel = this._deviceServiceClient?.ValidateVersion();
      if ((validateVersionModel != null ? (validateVersionModel.IsCompatible ? 1 : 0) : 0) != 0)
        flag = true;
label_4:
      if (flag)
        this.RaiseValidDeviceServiceCardReader();
      else
        this.PollForCardReaderConnectionAndValidVersion();
    }

    private void DeviceServiceClient_OnDisconnectedHandler(Exception exception)
    {
      this._cardReaderState = (CardReaderState) null;
      this.ResetPollingInterval();
      this.RaiseInvalidDeviceServiceCardReader();
    }

    private void DeviceServiceClient_OnConnectedHandler()
    {
      this.AddLogText("DeviceService is connected.");
      this.CheckIfCardReaderIsConnectedAndValid(false);
    }

    private void RaiseInvalidDeviceServiceCardReader()
    {
      ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().RaiseInvalidDeviceServiceCardReader();
    }

    private void RaiseValidDeviceServiceCardReader()
    {
      this.CheckDeviceActivation();
      ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseValidDeviceServiceCardReader();
    }

    public void CheckDeviceActivation()
    {
      DeviceServiceConfiguration deviceService = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskSession?.DeviceService;
      if (deviceService.BluefinAutoActivate)
      {
        if (this.IsCardReaderConnected)
        {
          LogHelper.Instance.Log("CheckDeviceActivation: Sending CheckActivation command to Device Service Client.");
          IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
          if (service != null)
            this._deviceServiceClient.CheckActivation(new BluefinActivationRequest()
            {
              KioskId = service.KioskId,
              BluefinServiceUrl = deviceService.BluefinServiceUrl,
              ApiKey = deviceService.BluefinServiceApiKey,
              Timeout = deviceService.BluefinServiceTimeout
            });
          else
            LogHelper.Instance.Log("CheckDeviceActivation: StoreManager not initialized.");
        }
        else
          LogHelper.Instance.Log("CheckDeviceActivation: card reader not connected.");
      }
      else
        LogHelper.Instance.Log("CheckDeviceActivation: BluefinAutoActivate is disabled.");
    }

    public void CheckDeviceStatus()
    {
      if (this._statusChecked)
      {
        LogHelper.Instance.Log("CheckDeviceStatus: Status already checked. Skipping.");
      }
      else
      {
        IConfiguration service1 = ServiceLocator.Instance.GetService<IConfiguration>();
        if (this.IsConnectedToDeviceService)
        {
          LogHelper.Instance.Log("CheckDeviceStatus: Sending CheckDeviceStatus command to Device Service Client.");
          IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
          if (service2 != null)
          {
            this._deviceServiceClient.CheckDeviceStatus(new DeviceStatusRequest()
            {
              KioskId = service2.KioskId,
              ApiUrl = service1.KioskDataServicesUrl,
              ApiKey = service1.KioskDataServicesKey
            });
            this._statusChecked = true;
          }
          else
            LogHelper.Instance.Log("CheckDeviceStatus: StoreManager not initialized.");
        }
        else
          LogHelper.Instance.Log("CheckDeviceStatus: Device Service not connected.");
      }
    }

    public bool IsConnectedToDeviceService
    {
      get
      {
        return this._deviceServiceClient != null && this._deviceServiceClient.IsConnectedToDeviceService;
      }
    }

    public bool IsCardReaderConnected
    {
      get => this.IsConnectedToDeviceService && this._deviceServiceClient.IsCardReaderConnected();
    }

    public bool IsCardReaderTampered { get; set; }

    public void AddLogText(string logline) => LogHelper.Instance.Log(logline, LogEntryType.Info);

    public ValidateVersionModel ValidateVersion()
    {
      if (!this.IsConnectedToDeviceService)
        return (ValidateVersionModel) null;
      return this._deviceServiceClient?.ValidateVersion();
    }

    public bool SupportsEMV
    {
      get
      {
        CardReaderState cardReaderState = this._cardReaderState;
        return cardReaderState != null && cardReaderState.SupportsEMV;
      }
    }

    public bool SupportsVas
    {
      get
      {
        CardReaderState cardReaderState = this._cardReaderState;
        return cardReaderState != null && cardReaderState.SupportsVas;
      }
    }

    public void SendTamperedCardReaderKioskAlertMessage()
    {
      try
      {
        IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
        long kioskId = service != null ? service.KioskId : 0L;
        ServiceLocator.Instance.GetService<IHealthServices>().SendAlert(kioskId.ToString(), "ApplicationCrash", "mm:TamperedCardReaderError, 0099CDRR", string.Format("<h2>Kiosk Engine Card Reader Tampered Error</h2><p>Card Reader Tampering detected on Kiosk: {0}!<br><br>Placing Kiosk into Maintenance Mode.<br>{1}</p>", (object) kioskId, (object) "0099CDRR"), DateTime.Now, (RemoteServiceCallback) null, skipQueue: true);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceClientHelper.SendTamperdCardReaderKioskAlertMessage!", ex);
      }
    }

    public Version DeviceServiceVersion => this._cardReaderState?.Version;

    public Version DeviceServiceClientVersion => this._deviceServiceClient?.AssemblyVersion;

    public void GetCardInsertedStatus(
      Action<GetCardInsertedStatusResponseEvent> completeCallback)
    {
      this._deviceServiceClient?.GetCardInsertedStatus((Action<BaseResponseEvent>) (baseResponseEventCompleteCallback =>
      {
        GetCardInsertedStatusResponseEvent statusResponseEvent = baseResponseEventCompleteCallback as GetCardInsertedStatusResponseEvent;
        Action<GetCardInsertedStatusResponseEvent> action = completeCallback;
        if (action == null)
          return;
        action(statusResponseEvent);
      }));
    }

    public IDeviceServiceShutDownInfo DeviceServiceShutDownInfo
    {
      get => this._deviceServiceClient?.DeviceServiceShutDownInfo;
    }

    public bool ShutDownDeviceService(bool forceShutdown, ShutDownReason shutDownReason)
    {
      IDeviceServiceClient deviceServiceClient = this._deviceServiceClient;
      return deviceServiceClient != null && deviceServiceClient.ShutDownDeviceService(forceShutdown, shutDownReason);
    }

    private DeviceServiceClientHelper()
    {
    }
  }
}
