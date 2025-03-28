using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Redbox.Rental.Model.KioskClientService.Configuration;
using Redbox.Rental.Model.ShoppingCart;
using System;
using System.Collections.Generic;
using System.IO;

namespace Redbox.KioskEngine.Environment
{
  public class DeviceServiceClientHelperSimulator : IDeviceServiceClientHelper
  {
    private OnDeviceServiceCanShutDown _onDeviceServiceCanShutDownHandler;
    private ReadCardJob _readCardJob;
    private Version _deviceServiceVersion;
    private bool _statusChecked;
    private string _fileDir;
    private DeviceServiceHelperSimulatorModel _model;

    public static DeviceServiceClientHelperSimulator Instance
    {
      get => Singleton<DeviceServiceClientHelperSimulator>.Instance;
    }

    public bool ConnectToDeviceService() => this.IsConnectedToDeviceService;

    public void RegisterOnDeviceServiceCanShutDownHandler(
      OnDeviceServiceCanShutDown onDeviceServiceCanShutDown)
    {
      this._onDeviceServiceCanShutDownHandler = onDeviceServiceCanShutDown;
    }

    public void PollForCardReaderConnectionAndValidVersion()
    {
    }

    public bool ProcessKey(char keyChar)
    {
      if (this._readCardJob == null || !this._readCardJob.IsExecuting || !this._model.StoredCardData.ContainsKey(keyChar.ToString()))
        return false;
      Action<BaseResponseEvent> cardEventHandler = this._readCardJob.ReadCardEventHandler;
      SimpleResponseEvent simpleResponseEvent = new SimpleResponseEvent((BaseCommandRequest) null, "CardProcessingStartedResponseEvent");
      simpleResponseEvent.RequestId = this._readCardJob.RequestId;
      cardEventHandler((BaseResponseEvent) simpleResponseEvent);
      CardData cardData = this._model.StoredCardData[keyChar.ToString()];
      CardSourceType cardSourceType = this.ParseCardSourceType(cardData.CardSource);
      BaseResponseEvent baseResponseEvent;
      switch (cardSourceType)
      {
        case CardSourceType.Swipe:
        case CardSourceType.MSDContactless:
          baseResponseEvent = this.CreateSwipeResponse(cardData, cardSourceType, this._readCardJob);
          break;
        case CardSourceType.EMVContact:
        case CardSourceType.QuickChip:
        case CardSourceType.EMVContactless:
        case CardSourceType.Mobile:
        case CardSourceType.VASOnly:
          baseResponseEvent = this.CreateEMVResponse(cardData, cardSourceType);
          break;
        default:
          return false;
      }
      this._readCardJob.ReadCardResponseHandler(baseResponseEvent);
      return true;
    }

    private BaseResponseEvent CreateEMVResponse(CardData cardData, CardSourceType sourceType)
    {
      EMVCardReadModel emvCardReadModel = new EMVCardReadModel();
      if (sourceType != CardSourceType.VASOnly)
        emvCardReadModel = (EMVCardReadModel) this.ParseCardData(cardData, sourceType);
      emvCardReadModel.VasData = cardData.VasData;
      EMVCardReadResponseEvent emvResponse = new EMVCardReadResponseEvent((BaseCommandRequest) null);
      emvResponse.Data = emvCardReadModel;
      emvResponse.EventName = "EMVCardReadResponseEvent";
      emvResponse.Success = true;
      emvResponse.RequestId = this._readCardJob.RequestId;
      return (BaseResponseEvent) emvResponse;
    }

    private BaseResponseEvent CreateSwipeResponse(
      CardData cardData,
      CardSourceType sourceType,
      ReadCardJob readCardJob)
    {
      Base87CardReadModel cardData1 = this.ParseCardData(cardData, sourceType);
      if (cardData1 != null && cardData1.EncryptedFlag)
      {
        EncryptedCardReadResponseEvent swipeResponse = new EncryptedCardReadResponseEvent((BaseCommandRequest) null);
        swipeResponse.Data = (EncryptedCardReadModel) cardData1;
        swipeResponse.Success = cardData1 != null;
        swipeResponse.EventName = "EncryptedCardReadResponseEvent";
        swipeResponse.RequestId = readCardJob != null ? readCardJob.RequestId : new Guid();
        return (BaseResponseEvent) swipeResponse;
      }
      UnencryptedCardReadResponseEvent swipeResponse1 = new UnencryptedCardReadResponseEvent((BaseCommandRequest) null);
      swipeResponse1.Data = (UnencryptedCardReadModel) cardData1;
      swipeResponse1.Success = cardData1 != null;
      swipeResponse1.EventName = "UnencryptedCardReadResponseEvent";
      swipeResponse1.RequestId = readCardJob != null ? readCardJob.RequestId : new Guid();
      return (BaseResponseEvent) swipeResponse1;
    }

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
      this._readCardJob = new ReadCardJob(new CardReadRequest()
      {
        InputType = deviceInputType,
        VasMode = vasMode,
        Amount = new Decimal?(num),
        Timeout = commandTimeout + 5000,
        ExcludeCardBrandBySource = cardBrandSources,
        SessionId = new Guid?(currentSession.Id)
      }, readCardResponseHandler, readCardEventHandler, commandTimeout);
      this._readCardJob.Execute();
      LogHelper.Instance.Log(string.Format("Starting DeviceServiceClient.ReadCard with id {0}", (object) this._readCardJob?.RequestId));
      return (IReadCardJob) this._readCardJob;
    }

    public void CancelCommand(Guid id)
    {
      LogHelper.Instance.Log(string.Format("Cancelling DeviceService command with id {0}", (object) id));
      this._readCardJob = (ReadCardJob) null;
    }

    public void ReportAuthorizeResult(bool authorizeSuccessful)
    {
      LogHelper.Instance.Log(string.Format("Reporting Authorize Result of {0} to DeviceService", (object) authorizeSuccessful));
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
      dictionary.Add(CardBrandAndSource.DiscoverChip, deviceService.AmexInsertChipEnabled);
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
      if (cardReaderConnectionAlreadyVerified || this.IsCardReaderConnected)
        flag = true;
      if (!flag)
        return;
      this.RaiseValidDeviceServiceCardReader();
    }

    private void RaiseValidDeviceServiceCardReader()
    {
      this.CheckDeviceActivation();
      ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseValidDeviceServiceCardReader();
    }

    public void CheckDeviceActivation()
    {
      LogHelper.Instance.Log("CheckDeviceActivation simulated.");
    }

    public void CheckDeviceStatus()
    {
      if (this._statusChecked)
      {
        LogHelper.Instance.Log("CheckDeviceStatus: Status already checked. Skipping.");
      }
      else
      {
        ServiceLocator.Instance.GetService<IConfiguration>();
        if (this.IsConnectedToDeviceService)
        {
          LogHelper.Instance.Log("CheckDeviceStatus: simulating CheckDeviceStatus command.");
          this._statusChecked = true;
        }
        else
          LogHelper.Instance.Log("CheckDeviceStatus: Device Service not connected.");
      }
    }

    public bool IsConnectedToDeviceService
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<bool>("Remote Services\\Device Service Simulator", nameof (IsConnectedToDeviceService), false);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue<bool>("Remote Services\\Device Service Simulator", nameof (IsConnectedToDeviceService), value);
      }
    }

    public bool IsCardReaderConnected
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<bool>("Remote Services\\Device Service Simulator", nameof (IsCardReaderConnected), false);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue<bool>("Remote Services\\Device Service Simulator", nameof (IsCardReaderConnected), value);
      }
    }

    public bool IsCardReaderTampered
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<bool>("Remote Services\\Device Service Simulator", nameof (IsCardReaderTampered), false);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue<bool>("Remote Services\\Device Service Simulator", nameof (IsCardReaderTampered), value);
      }
    }

    public bool UseSimulatedCardReader
    {
      get
      {
        return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<bool>("Remote Services\\Device Service Simulator", nameof (UseSimulatedCardReader), false);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue<bool>("Remote Services\\Device Service Simulator", nameof (UseSimulatedCardReader), value);
      }
    }

    public void AddLogText(string logline) => LogHelper.Instance.Log(logline, LogEntryType.Info);

    public ValidateVersionModel ValidateVersion()
    {
      ValidateVersionModel validateVersionModel = new ValidateVersionModel()
      {
        DeviceServiceVersion = new Version("0.0.0.0"),
        IsCompatible = true
      };
      this._deviceServiceVersion = validateVersionModel.DeviceServiceVersion;
      return validateVersionModel;
    }

    public bool SupportsEMV => true;

    public bool SupportsVas => true;

    public void SendTamperedCardReaderKioskAlertMessage()
    {
      try
      {
        IStoreManager service1 = ServiceLocator.Instance.GetService<IStoreManager>();
        long kioskId = service1 != null ? service1.KioskId : 0L;
        IHealthServices service2 = ServiceLocator.Instance.GetService<IHealthServices>();
        IStoreManager service3 = ServiceLocator.Instance.GetService<IStoreManager>();
        string storeNumber = (service3 != null ? service3.KioskId : 0L).ToString();
        string message = string.Format("<h2>Kiosk Engine Card Reader Tampered Error</h2><p>Card Reader Tampering detected on Kiosk: {0}!<br><br>Placing Kiosk into Maintenance Mode.<br>{1}</p>", (object) kioskId, (object) "0099CDRR");
        DateTime now = DateTime.Now;
        service2.SendAlert(storeNumber, "ApplicationCrash", "mm:TamperedCardReaderError, 0099CDRR", message, now, (RemoteServiceCallback) null, skipQueue: true);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceClientHelper.SendTamperdCardReaderKioskAlertMessage!", ex);
      }
    }

    public Version DeviceServiceVersion => this._deviceServiceVersion;

    public Version DeviceServiceClientVersion => this._deviceServiceVersion;

    public void GetCardInsertedStatus(
      Action<GetCardInsertedStatusResponseEvent> completeCallback)
    {
      GetCardInsertedStatusResponseEvent statusResponseEvent1 = new GetCardInsertedStatusResponseEvent((BaseCommandRequest) null);
      statusResponseEvent1.CardInsertedStatus = this._readCardJob.IsCardInserted ? InsertedStatus.Inserted : InsertedStatus.Removed;
      statusResponseEvent1.RequestId = this._readCardJob.RequestId;
      GetCardInsertedStatusResponseEvent statusResponseEvent2 = statusResponseEvent1;
      if (completeCallback == null)
        return;
      completeCallback(statusResponseEvent2);
    }

    public IDeviceServiceShutDownInfo DeviceServiceShutDownInfo
    {
      get => (IDeviceServiceShutDownInfo) new Redbox.KioskEngine.Environment.DeviceServiceShutDownInfo();
    }

    public bool ShutDownDeviceService(bool forceShutdown, ShutDownReason shutDownReason) => true;

    private DeviceServiceClientHelperSimulator()
    {
      this._fileDir = ServiceLocator.Instance.GetService<IEngineApplication>().DataPath;
      this.ReadConfigFile();
    }

    private void ReadConfigFile()
    {
      string path = Path.Combine(this._fileDir, "KioskEngineSimulatedCardReaderData.json");
      if (!File.Exists(path))
        return;
      this._model = JsonConvert.DeserializeObject<DeviceServiceHelperSimulatorModel>(File.ReadAllText(path));
    }

    private CardSourceType ParseCardSourceType(string sourceType)
    {
      CardSourceType cardSourceType;
      if (sourceType != null)
      {
        switch (sourceType.Length)
        {
          case 1:
            switch (sourceType[0])
            {
              case '1':
              case 'M':
                cardSourceType = CardSourceType.Swipe;
                goto label_13;
              case '2':
              case 'S':
                cardSourceType = CardSourceType.EMVContact;
                goto label_13;
              case '3':
              case 'Q':
                cardSourceType = CardSourceType.QuickChip;
                goto label_13;
              case '4':
              case 'C':
                cardSourceType = CardSourceType.MSDContactless;
                goto label_13;
              case '5':
              case 'E':
                cardSourceType = CardSourceType.EMVContactless;
                goto label_13;
              case '6':
              case 'm':
                cardSourceType = CardSourceType.Mobile;
                goto label_13;
              case '7':
              case 'c':
                cardSourceType = CardSourceType.VASOnly;
                goto label_13;
            }
            break;
          case 2:
            if (sourceType == "cq")
            {
              cardSourceType = CardSourceType.VASOnly;
              goto label_13;
            }
            else
              break;
        }
      }
      cardSourceType = CardSourceType.Unknown;
label_13:
      return cardSourceType;
    }

    private Base87CardReadModel ParseCardData(CardData cardData, CardSourceType sourceType)
    {
      if (cardData == null)
      {
        LogHelper.Instance.Log("ParseCardData -> cardData contains no data.");
        return (Base87CardReadModel) null;
      }
      try
      {
        if (cardData.EncryptedFlag)
        {
          if (sourceType == CardSourceType.Swipe)
          {
            LogHelper.Instance.Log("Creating Encrypted swipe model");
            EncryptedCardReadModel cardData1 = new EncryptedCardReadModel();
            cardData1.FirstSix = cardData.FirstSix;
            cardData1.LastFour = cardData.LastFour;
            cardData1.PANLength = cardData.PANLength;
            cardData1.Mod10CheckFlag = cardData.Mod10CheckFlag;
            cardData1.Expiry = cardData.Expiry;
            cardData1.ServiceCode = cardData.ServiceCode;
            cardData1.LanguageCode = cardData.LanguageCode;
            cardData1.EncryptedFlag = cardData.EncryptedFlag;
            cardData1.Name = cardData.Name;
            cardData1.NameLength = cardData.NameLength;
            cardData1.EncFormat = cardData.EncFormat;
            cardData1.KSN = cardData.KSN;
            cardData1.ICEncDataLength = cardData.ICEncDataLength;
            cardData1.ICEncData = cardData.ICEncData;
            cardData1.AESPANLength = cardData.AESPANLength;
            cardData1.AESPAN = cardData.AESPAN;
            cardData1.LSEncDataLength = cardData.LSEncDataLength;
            cardData1.LSEncData = cardData.LSEncData;
            cardData1.ExtLangCode = cardData.ExtLangCode;
            cardData1.Status = ResponseStatus.Success;
            cardData1.MfgSerialNumber = cardData.MfgSerialNumber;
            cardData1.InjectedSerialNumber = "Simulated Injected Serial Number";
            cardData1.CardSource = sourceType;
            cardData1.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
            return (Base87CardReadModel) cardData1;
          }
          LogHelper.Instance.Log("Creating ENV model");
          EMVCardReadModel cardData2 = new EMVCardReadModel();
          cardData2.FirstSix = cardData.FirstSix;
          cardData2.LastFour = cardData.LastFour;
          cardData2.PANLength = cardData.PANLength;
          cardData2.Mod10CheckFlag = cardData.Mod10CheckFlag;
          cardData2.Expiry = cardData.Expiry;
          cardData2.ServiceCode = cardData.ServiceCode;
          cardData2.LanguageCode = cardData.LanguageCode;
          cardData2.EncryptedFlag = cardData.EncryptedFlag;
          cardData2.Name = cardData.Name;
          cardData2.NameLength = cardData.NameLength;
          cardData2.EncFormat = cardData.EncFormat;
          cardData2.KSN = cardData.KSN;
          cardData2.ICEncDataLength = cardData.ICEncDataLength;
          cardData2.ICEncData = cardData.ICEncData;
          cardData2.AESPANLength = cardData.AESPANLength;
          cardData2.AESPAN = cardData.AESPAN;
          cardData2.LSEncDataLength = cardData.LSEncDataLength;
          cardData2.LSEncData = cardData.LSEncData;
          cardData2.ExtLangCode = cardData.ExtLangCode;
          cardData2.Tags = (IDictionary<string, string>) cardData.Tags;
          cardData2.Status = ResponseStatus.Success;
          cardData2.MfgSerialNumber = cardData.MfgSerialNumber;
          cardData2.InjectedSerialNumber = "Simulated Injected Serial Number";
          cardData2.CardSource = sourceType;
          cardData2.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
          return (Base87CardReadModel) cardData2;
        }
        UnencryptedCardReadModel cardData3 = new UnencryptedCardReadModel();
        cardData3.FirstSix = cardData.FirstSix;
        cardData3.LastFour = cardData.LastFour;
        cardData3.PANLength = cardData.PANLength;
        cardData3.Mod10CheckFlag = cardData.Mod10CheckFlag;
        cardData3.Expiry = cardData.Expiry;
        cardData3.ServiceCode = cardData.ServiceCode;
        cardData3.LanguageCode = cardData.LanguageCode;
        cardData3.EncryptedFlag = cardData.EncryptedFlag;
        cardData3.Name = cardData.Name;
        cardData3.Track1 = cardData.Track1;
        cardData3.Track2 = cardData.Track2;
        cardData3.Status = ResponseStatus.Success;
        cardData3.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
        return (Base87CardReadModel) cardData3;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.LogException("Parse Card Data", ex);
      }
      return (Base87CardReadModel) null;
    }
  }
}
