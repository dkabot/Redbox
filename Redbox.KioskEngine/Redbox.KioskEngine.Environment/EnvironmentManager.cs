using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.KioskEngine.IDE;
using Redbox.USB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Redbox.KioskEngine.Environment
{
  public class EnvironmentManager : IEnvironmentNotificationService
  {
    private CorruptDbCallback m_corruptDbCallback;
    private TamperedCardReaderCallback m_tamperedCardReaderCallback;
    private object _trackDataServiceLockObject = new object();
    private object _validDeviceServiceCardReaderLockObject = new object();
    private readonly IDictionary<string, ConfigurationCallback> m_configurationCallbacks = (IDictionary<string, ConfigurationCallback>) new Dictionary<string, ConfigurationCallback>();
    private Action _cardReaderConnectedCallback;
    private Action<ITrackDataService> _cardReaderDisconnectedCallback;

    public static EnvironmentManager Instance => Singleton<EnvironmentManager>.Instance;

    public void Reset()
    {
      try
      {
        TouchScreenFactory.Instance.Initialize();
        LogHelper.Instance.Log("Reset environment manager:");
        GenericTouchScreen touchScreen = TouchScreenFactory.Instance.GetTouchScreen();
        if (touchScreen != null)
          LogHelper.Instance.Log("...Touch screen state is: {0}.", (object) touchScreen.State);
        else
          LogHelper.Instance.Log("...No touch screen instance available.");
        LogHelper.Instance.Log("...Clear configuration callbacks.");
        this.m_configurationCallbacks.Clear();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.Reset", ex);
      }
    }

    public ErrorList Initialize()
    {
      ErrorList errorList = new ErrorList();
      LogHelper.Instance.Log("Initialize environment manager.");
      ServiceLocator.Instance.AddService(typeof (IEnvironmentNotificationService), (object) EnvironmentManager.Instance);
      this.DetectCardReaderAndRegisterTrackDataService();
      return errorList;
    }

    private ITrackDataService _previousTrackDataService { get; set; }

    public bool PreviousTrackDataServiceWasDeviceServiceTrackDataService
    {
      get => this._previousTrackDataService is IDeviceServiceTrackDataService;
    }

    public bool PreviousTrackDataServiceWasClassicTrackDataService
    {
      get => this._previousTrackDataService is IClassicTrackDataService;
    }

    public void DetectCardReaderAndRegisterTrackDataService()
    {
      LogHelper.Instance.Log("Check for valid credit card reader.");
      if (this.IsDeviceServiceCardReaderAttached)
      {
        LogHelper.Instance.Log("...Valid device service credit card reader found.");
        if (this.IsDeviceServiceClientValid)
        {
          this.SetTrackDataService((ITrackDataService) ServiceLocator.Instance.GetService<IDeviceServiceTrackDataService>());
          this.HasValidCardReader = true;
        }
        else
        {
          LogHelper.Instance.Log("...DeviceServiceClient version is not compatible with DeviceService version.");
          this.PollForCardReaderConnectionAndValidVersion();
        }
      }
      else
      {
        int num = this.IsDeviceServiceClientValid ? 1 : 0;
      }
      if (this.HasValidCardReader)
        return;
      if (CardReaderDetector.IsUSBCardReaderAttached())
      {
        LogHelper.Instance.Log("...Valid credit card reader found.");
        this.HasValidCardReader = true;
        this.SetTrackDataService((ITrackDataService) ServiceLocator.Instance.GetService<IClassicTrackDataService>());
      }
      else if (this.CheckCardReader)
      {
        LogHelper.Instance.Log("...No valid credit card reader found.");
        this.SetTrackDataService((ITrackDataService) null);
        this.HasValidCardReader = false;
      }
      else
      {
        this.HasValidCardReader = true;
        this.SetTrackDataService((ITrackDataService) ServiceLocator.Instance.GetService<IClassicTrackDataService>());
      }
    }

    private void SetTrackDataService(ITrackDataService trackDataService)
    {
      lock (this._trackDataServiceLockObject)
      {
        ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
        if (service != null)
        {
          this._previousTrackDataService = service;
          ServiceLocator.Instance.RemoveService(typeof (ITrackDataService));
        }
        if (trackDataService != null)
          ServiceLocator.Instance.AddService(typeof (ITrackDataService), (object) trackDataService);
        LogHelper.Instance.Log(string.Format("Setting track data service to type {0}", (object) trackDataService?.GetType()));
      }
    }

    private bool IsDeviceServiceCardReaderAttached
    {
      get
      {
        bool cardReaderAttached = false;
        IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
        if (service != null)
        {
          if (!service.IsConnectedToDeviceService && service != null)
            service.ConnectToDeviceService();
          cardReaderAttached = service != null && service.IsCardReaderConnected;
        }
        return cardReaderAttached;
      }
    }

    private bool IsDeviceServiceClientValid
    {
      get
      {
        ValidateVersionModel validateVersionModel = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ValidateVersion();
        return validateVersionModel != null && validateVersionModel.IsCompatible;
      }
    }

    private void PollForCardReaderConnectionAndValidVersion()
    {
      ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.PollForCardReaderConnectionAndValidVersion();
    }

    public void Register(IntPtr handle)
    {
      if (!this.CheckCardReader)
        return;
      try
      {
        UsbNotification.RegisterForUsbEvents(handle);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised by UsbNotification.RegisterForUsbEvents.", ex);
      }
    }

    public void Unregister(IntPtr handle)
    {
      if (!this.CheckCardReader)
        return;
      try
      {
        UsbNotification.UnregisterForUsbEvents(handle);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised by UsbNotification.UnregisterForUsbEvents.", ex);
      }
    }

    public void NotifyConfigurationCallbacks(
      string store,
      string action,
      string path,
      string key,
      string newValue)
    {
      ConfigurationCallback[] array = new ConfigurationCallback[this.m_configurationCallbacks.Count];
      this.m_configurationCallbacks.Values.CopyTo(array, 0);
      foreach (ConfigurationCallback configurationCallback in array)
        CallbackService.Instance.EnqueueCallback((ICallbackEntry) new ConfigurationCallbackEntry()
        {
          Store = store,
          Function = configurationCallback,
          Key = key,
          Path = path,
          Action = action,
          NewValue = newValue
        });
    }

    public void RegisterConfigurationCallback(string name, ConfigurationCallback callback)
    {
      this.m_configurationCallbacks[name] = callback;
    }

    public void UnregisterConfigurationCallback(string name)
    {
      this.m_configurationCallbacks.Remove(name);
    }

    public void RegisterCorruptDbCallback(CorruptDbCallback callback)
    {
      this.m_corruptDbCallback = callback;
    }

    public void RaiseCorruptDb()
    {
      try
      {
        CorruptDbCallback corruptDbCallback = this.m_corruptDbCallback;
        if (corruptDbCallback == null)
          return;
        corruptDbCallback();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseCorruptDb.", ex);
      }
    }

    public void RegisterTamperedCardReaderCallback(TamperedCardReaderCallback callback)
    {
      this.m_tamperedCardReaderCallback = callback;
    }

    public void RaiseTamperedCardReader()
    {
      try
      {
        TamperedCardReaderCallback cardReaderCallback = this.m_tamperedCardReaderCallback;
        if (cardReaderCallback == null)
          return;
        cardReaderCallback();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseTamperedCardReaderCallback.", ex);
      }
    }

    public void RegisterCardReaderConnectedCallback(Action cardReaderConnectedCallback)
    {
      this._cardReaderConnectedCallback = cardReaderConnectedCallback;
    }

    public void RegisterCardReaderDisconnectedCallback(
      Action<ITrackDataService> cardReaderDisconnectedCallback)
    {
      this._cardReaderDisconnectedCallback = cardReaderDisconnectedCallback;
    }

    public EnvironmentNotificationType ProcessClassicCardReaderNotification(ref Message msg)
    {
      if (this.CheckCardReader)
      {
        NotificationEventType notificationEventType = UsbNotification.HandleMessage(ref msg);
        if (this.HasValidCardReader)
        {
          if ((this.IsClassicTrackDataServiceRegistered || !this.IsTrackDataServiceRegistered) && notificationEventType == NotificationEventType.DeviceRemoved && !CardReaderDetector.IsUSBCardReaderAttached())
            return EnvironmentNotificationType.CreditCardReaderInvalid;
        }
        else if ((this.IsClassicTrackDataServiceRegistered || !this.IsTrackDataServiceRegistered) && notificationEventType == NotificationEventType.DeviceArrived && CardReaderDetector.IsUSBCardReaderAttached())
          return EnvironmentNotificationType.CreditCardReaderValid;
      }
      return EnvironmentNotificationType.None;
    }

    public void ToggleTouchScreen() => TouchScreenFactory.Instance.GetTouchScreen()?.Toggle();

    public TouchScreenState TouchScreenStatus()
    {
      GenericTouchScreen touchScreen = TouchScreenFactory.Instance.GetTouchScreen();
      return touchScreen != null ? touchScreen.State : TouchScreenState.Unavailable;
    }

    public string TouchScreenDriver() => TouchScreenFactory.Instance.GetTouchScreen()?.Driver;

    public void LaunchSecureBrowser(
      IDictionary<string, string> approvedUrls,
      NavigateUrlCallback navigateUrlCallback)
    {
      new SecureBrowser(approvedUrls, navigateUrlCallback).Show();
    }

    public void RaiseValidClassicCardReader()
    {
      try
      {
        if (this.IsTrackDataServiceRegistered && !this.IsClassicTrackDataServiceRegistered)
          return;
        this.SetTrackDataService((ITrackDataService) ServiceLocator.Instance.GetService<IClassicTrackDataService>());
        this.HasValidCardReader = true;
        this.HandleConnectedCardReader();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseValidClassicCardReader.", ex);
      }
    }

    public void RaiseValidDeviceServiceCardReader()
    {
      try
      {
        lock (this._validDeviceServiceCardReaderLockObject)
        {
          if (this.IsTrackDataServiceRegistered && !this.IsDeviceServiceTrackDataServiceRegistered)
            return;
          this.SetTrackDataService((ITrackDataService) ServiceLocator.Instance.GetService<IDeviceServiceTrackDataService>());
          this.HasValidCardReader = true;
          this.HandleConnectedCardReader();
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseValidDeviceServiceCardReader.", ex);
      }
    }

    public void RaiseInvalidClassicCardReader()
    {
      try
      {
        if (!this.IsClassicTrackDataServiceRegistered)
          return;
        this.SetTrackDataService((ITrackDataService) null);
        this.HasValidCardReader = false;
        Action<ITrackDataService> disconnectedCallback = this._cardReaderDisconnectedCallback;
        if (disconnectedCallback == null)
          return;
        disconnectedCallback(this._previousTrackDataService);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseInvalidClassicCardReader.", ex);
      }
    }

    public void RaiseInvalidDeviceServiceCardReader()
    {
      try
      {
        if (!this.IsDeviceServiceTrackDataServiceRegistered)
          return;
        this.SetTrackDataService((ITrackDataService) null);
        this.HasValidCardReader = false;
        Action<ITrackDataService> disconnectedCallback = this._cardReaderDisconnectedCallback;
        if (disconnectedCallback == null)
          return;
        disconnectedCallback(this._previousTrackDataService);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseInvalidDeviceServiceCardReader.", ex);
      }
    }

    public bool IsDeviceServiceTrackDataServiceRegistered
    {
      get
      {
        return ServiceLocator.Instance.GetService<ITrackDataService>() is IDeviceServiceTrackDataService;
      }
    }

    public bool IsClassicTrackDataServiceRegistered
    {
      get => ServiceLocator.Instance.GetService<ITrackDataService>() is IClassicTrackDataService;
    }

    public bool IsTrackDataServiceRegistered
    {
      get => ServiceLocator.Instance.GetService<ITrackDataService>() != null;
    }

    private void HandleConnectedCardReader()
    {
      LogHelper.Instance.Log("The card reader was connected, resuming operation.");
      Action connectedCallback = this._cardReaderConnectedCallback;
      if (connectedCallback != null)
        connectedCallback();
      LogHelper.Instance.LogTo("CardReaderLog", "Card Reader Re-Connected");
    }

    public bool HasValidCardReader { get; internal set; }

    public bool CheckCardReader
    {
      get
      {
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        return service != null && service.GetValue<bool>("Core", nameof (CheckCardReader), true);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<bool>("Core", nameof (CheckCardReader), value);
      }
    }

    public int NullKioskId
    {
      get
      {
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        return service == null ? 1 : service.GetValue<int>("Core", nameof (NullKioskId), 1);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<int>("Core", nameof (NullKioskId), value);
      }
    }

    private EnvironmentManager()
    {
    }
  }
}
