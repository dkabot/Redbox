using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class DeviceServiceTrackDataService : IDeviceServiceTrackDataService, ITrackDataService
  {
    private List<DeviceServiceTrackDataService.ReadCardJobEntry> _readCardJobs = new List<DeviceServiceTrackDataService.ReadCardJobEntry>();
    private object _lockObject = new object();
    private IReadCardJob _readCardJob;
    private bool _cardReadAborted;

    public bool IsOlderReader => false;

    public bool IsNewReader => true;

    public void StartCardRead(
      CardReadRequestInfo request,
      Action<IReadCardJob> onReadJobStart,
      Action<Guid> onCardProcessing,
      Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete)
    {
      IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
      if (deviceServiceClientHelper == null)
        return;
      this._cardReadAborted = false;
      Task.Run((Action) (() =>
      {
        try
        {
          Action<BaseResponseEvent, CardReadRequestInfo, Action<CardReadRequestInfo, CardReadCompleteDetails>> cardReadHandler = (Action<BaseResponseEvent, CardReadRequestInfo, Action<CardReadRequestInfo, CardReadCompleteDetails>>) null;
          cardReadHandler = VasMode.VasOnly != request.VasMode ? new Action<BaseResponseEvent, CardReadRequestInfo, Action<CardReadRequestInfo, CardReadCompleteDetails>>(this.CardReadResponseHandler) : new Action<BaseResponseEvent, CardReadRequestInfo, Action<CardReadRequestInfo, CardReadCompleteDetails>>(this.CardReadVasOnlyResponseHandler);
          IReadCardJob readCardJob = deviceServiceClientHelper.StartCardRead(request.InputType, (Action<BaseResponseEvent>) (result =>
          {
            Action<BaseResponseEvent, CardReadRequestInfo, Action<CardReadRequestInfo, CardReadCompleteDetails>> action = cardReadHandler;
            if (action == null)
              return;
            action(result, request, onComplete);
          }), (Action<BaseResponseEvent>) (result => this.CardReadEventHandler(result, onCardProcessing)), request.Timeout, request.VasMode);
          lock (this._lockObject)
          {
            this._readCardJobs.Add(new DeviceServiceTrackDataService.ReadCardJobEntry()
            {
              ReadCardJob = readCardJob,
              SessionId = request.SessionId,
              IsZeroTouchRead = request.IsZeroTouchRead
            });
            LogHelper.Instance.Log(string.Format("Adding ReadCardJobEntry {0}.  list count= {1}", (object) readCardJob.RequestId, (object) this._readCardJobs?.Count));
          }
          Action<IReadCardJob> action1 = onReadJobStart;
          if (action1 == null)
            return;
          action1(readCardJob);
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceTrackDataService.StartCardRead.", ex);
        }
      }));
    }

    public void StopCardRead(CancelReason cancelReason)
    {
      lock (this._lockObject)
      {
        DeviceServiceTrackDataService.ReadCardJobEntry cardReadJobEntry = this.GetActiveCardReadJobEntry();
        if (cardReadJobEntry != null)
          DeviceServiceTrackDataService.CancelReadCardJobEntry(cancelReason, cardReadJobEntry);
        else
          LogHelper.Instance.Log(string.Format("Failed to StopCardRead with cancel reason: {0} because no active card read was available.  Possible that card read has already returned data, please report auth result.", (object) cancelReason));
      }
    }

    private static void CancelReadCardJobEntry(
      CancelReason cancelReason,
      DeviceServiceTrackDataService.ReadCardJobEntry activeCardReadJobEntry)
    {
      ICardReadAttempt commandRequestId = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.CardReadAttemptCollection?.GetByCardReadJobCommandRequestId(activeCardReadJobEntry.ReadCardJob.RequestId);
      if (commandRequestId != null)
      {
        commandRequestId.AttemptedCancel = true;
        commandRequestId.CancelReason = cancelReason;
        LogHelper.Instance.Log(string.Format("Setting CardReadAttempt.AttemptedCancel = true for card read command {0}.  CancelReason = {1}", (object) activeCardReadJobEntry.ReadCardJob.RequestId, (object) cancelReason));
      }
      activeCardReadJobEntry.AttemptedCancel = true;
      Task.Run((Action) (() =>
      {
        try
        {
          activeCardReadJobEntry.ReadCardJob.Cancel((Action<BaseResponseEvent>) (baseResponseEvent =>
          {
            CancelCommandResponseEvent commandResponseEvent = baseResponseEvent as CancelCommandResponseEvent;
            LogHelper.Instance.Log(string.Format("DeviceServiceTrackDataService.StopCardRead - cancel card read success = {0}", commandResponseEvent != null ? (commandResponseEvent.Success ? (object) true : (object) false) : (object) false));
          }));
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceTrackDataService.StopCardRead.", ex);
        }
      }));
    }

    public void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason)
    {
      lock (this._lockObject)
      {
        if (this._readCardJobs == null)
          return;
        List<DeviceServiceTrackDataService.ReadCardJobEntry> readCardJobEntriesToCancelAndRemove = new List<DeviceServiceTrackDataService.ReadCardJobEntry>();
        this._readCardJobs.Where<DeviceServiceTrackDataService.ReadCardJobEntry>((Func<DeviceServiceTrackDataService.ReadCardJobEntry, bool>) (x => x.SessionId == sessionId)).ForEach<DeviceServiceTrackDataService.ReadCardJobEntry>((Action<DeviceServiceTrackDataService.ReadCardJobEntry>) (x => readCardJobEntriesToCancelAndRemove.Add(x)));
        if (readCardJobEntriesToCancelAndRemove.Count <= 0)
          return;
        LogHelper.Instance.Log(string.Format("Removing all Card Read Jobs for session {0}", (object) sessionId));
        foreach (DeviceServiceTrackDataService.ReadCardJobEntry readCardJobEntry in readCardJobEntriesToCancelAndRemove)
        {
          if (!readCardJobEntry.AttemptedCancel)
            DeviceServiceTrackDataService.CancelReadCardJobEntry(cancelReason, readCardJobEntry);
          this.RemoveReadCardJobEntry(readCardJobEntry);
        }
      }
    }

    public bool IsInTechnicalFallback
    {
      get
      {
        bool technicalFallback = false;
        IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
        if ((service != null ? (service.KioskSession.DeviceService.CardReadTechnicalFallbackEnabled ? 1 : 0) : 1) != 0)
        {
          ISession currentSession = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
          technicalFallback = (currentSession != null ? currentSession.CardReadTechnicalFallbackCount : 0) >= (service != null ? service.KioskSession.DeviceService.CardReadTechnicalFallbackCountLimit : 3);
        }
        return technicalFallback;
      }
    }

    public FallbackType? NextFallbackReason { get; set; }

    private bool IsCardReadInProgress => this._readCardJob != null;

    private DeviceServiceTrackDataService.ReadCardJobEntry GetActiveCardReadJobEntry()
    {
      DeviceServiceTrackDataService.ReadCardJobEntry cardReadJobEntry = (DeviceServiceTrackDataService.ReadCardJobEntry) null;
      lock (this._lockObject)
      {
        foreach (DeviceServiceTrackDataService.ReadCardJobEntry readCardJob in this._readCardJobs)
        {
          if (!readCardJob.AttemptedCancel && readCardJob.ReadCardJob != null)
          {
            cardReadJobEntry = readCardJob;
            break;
          }
        }
      }
      return cardReadJobEntry;
    }

    private void CardReadEventHandler(
      BaseResponseEvent baseResponseEvent,
      Action<Guid> OnCardProcessing)
    {
      if (this._cardReadAborted || !this.IsCardReadInProgress || !(baseResponseEvent?.EventName == "CardProcessingStartedResponseEvent") || OnCardProcessing == null)
        return;
      OnCardProcessing(baseResponseEvent.RequestId);
    }

    private DeviceServiceTrackDataService.ReadCardJobEntry RemoveReadCardJobEntry(Guid requestId)
    {
      DeviceServiceTrackDataService.ReadCardJobEntry readCardJobEntry1 = (DeviceServiceTrackDataService.ReadCardJobEntry) null;
      lock (this._lockObject)
      {
        DeviceServiceTrackDataService.ReadCardJobEntry readCardJobEntry2 = this._readCardJobs.FirstOrDefault<DeviceServiceTrackDataService.ReadCardJobEntry>((Func<DeviceServiceTrackDataService.ReadCardJobEntry, bool>) (x => x.ReadCardJob != null && x.ReadCardJob.RequestId == requestId));
        if (readCardJobEntry2 != null)
        {
          readCardJobEntry1 = readCardJobEntry2;
          this.RemoveReadCardJobEntry(readCardJobEntry2);
        }
      }
      if (readCardJobEntry1 == null)
        LogHelper.Instance.Log(string.Format("Unable to find/remove ReadCardJobEntry {0}.  list count= {1}", (object) requestId, (object) this._readCardJobs?.Count));
      return readCardJobEntry1;
    }

    private void RemoveReadCardJobEntry(
      DeviceServiceTrackDataService.ReadCardJobEntry readCardJobEntry)
    {
      lock (this._lockObject)
      {
        if (readCardJobEntry == null)
          return;
        this._readCardJobs.Remove(readCardJobEntry);
        LogHelper.Instance.Log(string.Format("Removing ReadCardJobEntry  RequestId: {0},  SessionId: {1}, IsZeroTouch: {2},  list count= {3}", (object) readCardJobEntry.ReadCardJob.RequestId, (object) readCardJobEntry.SessionId, (object) readCardJobEntry.IsZeroTouchRead, (object) this._readCardJobs?.Count));
      }
    }

    private void CardReadResponseHandler(
      BaseResponseEvent baseResponseEvent,
      CardReadRequestInfo request,
      Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete)
    {
      this._readCardJob = (IReadCardJob) null;
      DeviceServiceTrackDataService.ReadCardJobEntry readEntry = this.RemoveReadCardJobEntry(baseResponseEvent.RequestId);
      bool chipCardRemovedFromReader = true;
      ITrackData fromResponseEvent = this.GetTrackDataFromResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader);
      this.UpdateTechnicalFallbackState(fromResponseEvent);
      this.DeclineChipEnabledCardThatWasSwiped(fromResponseEvent);
      this.DeclineAmexFallback(fromResponseEvent);
      this.DeclineReservationMobileWallet(fromResponseEvent);
      DeviceServiceTrackDataService.UpdateCardReadTechnicalFallbackInfo(fromResponseEvent);
      this.UpdateNextCardReadTechnicalFallbackState(fromResponseEvent);
      CardHelper.CheckForTrackDataErrors(fromResponseEvent);
      DeviceServiceTrackDataService.UpdateCreditCardHealthService(fromResponseEvent, baseResponseEvent, readEntry);
      if (onComplete == null)
        LogHelper.Instance.Log("Unable to Process OnCardReadComplete, no actions are registered.");
      else
        onComplete(request, new CardReadCompleteDetails()
        {
          TrackData = fromResponseEvent,
          VasData = baseResponseEvent is ICardReadResponseEvent readResponseEvent ? readResponseEvent.GetBase87CardReadModel()?.VasData : (string) null,
          CardRemoved = chipCardRemovedFromReader,
          RequestId = baseResponseEvent?.RequestId,
          Success = baseResponseEvent != null && baseResponseEvent.Success
        });
      if (fromResponseEvent == null || fromResponseEvent.HasValidData())
        return;
      ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>().ReportAuthorizeResult(false);
    }

    private void CardReadVasOnlyResponseHandler(
      BaseResponseEvent baseResponseEvent,
      CardReadRequestInfo request,
      Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete)
    {
      this._readCardJob = (IReadCardJob) null;
      bool chipCardRemovedFromReader = false;
      ITrackData fromResponseEvent = this.GetTrackDataFromResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader);
      Base87CardReadModel base87CardReadModel = baseResponseEvent is ICardReadResponseEvent readResponseEvent ? readResponseEvent.GetBase87CardReadModel() : (Base87CardReadModel) null;
      bool flag = baseResponseEvent != null && baseResponseEvent.Success && base87CardReadModel != null && base87CardReadModel.Status == ResponseStatus.Success && base87CardReadModel.HasVasData;
      if (this._cardReadAborted || onComplete == null)
        return;
      onComplete(request, new CardReadCompleteDetails()
      {
        RequestId = new Guid?(baseResponseEvent.RequestId),
        Success = flag,
        TrackData = fromResponseEvent,
        VasData = base87CardReadModel?.VasData,
        CardRemoved = true
      });
    }

    private static void UpdateCreditCardHealthService(
      ITrackData trackData,
      BaseResponseEvent responseEvent,
      DeviceServiceTrackDataService.ReadCardJobEntry readEntry)
    {
      if (DeviceServiceTrackDataService.ResponseEventIsTimedOut(responseEvent) || readEntry != null && readEntry.IsZeroTouchRead || DeviceServiceTrackDataService.GetCardReadAttemptCancelled(responseEvent) || DeviceServiceTrackDataService.GetCardReadResponseCancelledOrBlocked(responseEvent))
      {
        LogHelper.Instance.Log("Card Read Cancelled or Blocked or ZeroTouch or Timed Out, Don't Notify Health Service.");
      }
      else
      {
        ICCReaderHealth service = ServiceLocator.Instance.GetService<ICCReaderHealth>();
        bool dataWasReceived = trackData != null && trackData.HasValidData();
        bool hasError = !dataWasReceived;
        service?.EventOccurred(dataWasReceived, hasError);
      }
    }

    private static bool ResponseEventIsTimedOut(BaseResponseEvent responseEvent)
    {
      return responseEvent is TimeoutResponseEvent;
    }

    private static bool GetCardReadAttemptCancelled(BaseResponseEvent responseEvent)
    {
      bool attemptCancelled = false;
      if (responseEvent != null)
      {
        ICardReadAttempt commandRequestId = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.CardReadAttemptCollection?.GetByCardReadJobCommandRequestId(responseEvent.RequestId);
        attemptCancelled = commandRequestId != null && commandRequestId.AttemptedCancel;
      }
      return attemptCancelled;
    }

    private static bool GetCardReadResponseCancelledOrBlocked(BaseResponseEvent responseEvent)
    {
      Base87CardReadModel base87CardReadModel = responseEvent is ICardReadResponseEvent readResponseEvent ? readResponseEvent.GetBase87CardReadModel() : (Base87CardReadModel) null;
      if (base87CardReadModel == null)
        return false;
      return base87CardReadModel.Status == ResponseStatus.Cancelled || base87CardReadModel.Status == ResponseStatus.Blocked;
    }

    private ITrackData GetTrackDataFromEMVCardReadResponseEvent(
      BaseResponseEvent baseResponseEvent,
      ref bool chipCardRemovedFromReader)
    {
      ITrackData readResponseEvent1 = (ITrackData) null;
      EMVCardReadModel data = baseResponseEvent is EMVCardReadResponseEvent readResponseEvent2 ? readResponseEvent2.Data : (EMVCardReadModel) null;
      if (data != null)
      {
        chipCardRemovedFromReader = data.CardRemoved;
        EMVTrackData emvTrackData = new EMVTrackData();
        this.PopulateEncryptedTrackDataFields((IEncryptedTrackData) emvTrackData, (IEncryptedCardReadModel) data);
        emvTrackData.Tags = (IDictionary<string, string>) new Dictionary<string, string>(data.Tags);
        emvTrackData.AID = data.AID;
        readResponseEvent1 = (ITrackData) emvTrackData;
      }
      return readResponseEvent1;
    }

    private static void UpdateCardReadTechnicalFallbackInfo(ITrackData trackData)
    {
      if (trackData == null)
        return;
      IRentalSessionService service1 = ServiceLocator.Instance.GetService<IRentalSessionService>();
      IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
      ISession currentSession = service1?.GetCurrentSession();
      switch (trackData.FallbackStatusAction)
      {
        case FallbackStatusAction.IncrementFallbackCounter:
          if (currentSession == null)
            break;
          ++currentSession.CardReadTechnicalFallbackCount;
          currentSession.LastFallbackType = (FallbackType?) trackData?.FallbackReason;
          break;
        case FallbackStatusAction.ResetFallbackCounter:
          if (currentSession == null)
            break;
          currentSession.CardReadTechnicalFallbackCount = 0;
          if (trackData.IsInTechnicalFallback)
            break;
          currentSession.LastFallbackType = new FallbackType?();
          break;
        case FallbackStatusAction.ImmediateFallback:
          if (currentSession == null || service2 == null)
            break;
          currentSession.CardReadTechnicalFallbackCount = service2.KioskSession.DeviceService.CardReadTechnicalFallbackCountLimit;
          currentSession.LastFallbackType = (FallbackType?) trackData?.FallbackReason;
          break;
      }
    }

    private void UpdateTechnicalFallbackState(ITrackData trackData)
    {
      if (trackData == null)
        return;
      trackData.IsInTechnicalFallback = this.IsInTechnicalFallback;
    }

    private void UpdateNextCardReadTechnicalFallbackState(ITrackData trackData)
    {
      if (trackData == null)
        return;
      trackData.NextCardReadIsInTechnicalFallback = this.IsInTechnicalFallback;
    }

    private void DeclineAmexFallback(ITrackData trackData)
    {
      if (trackData == null || !trackData.IsInTechnicalFallback || trackData.CardSourceType != CardSourceType.Swipe || !trackData.CardHasChip)
        return;
      Redbox.Core.CardType? cardType1 = trackData.CardType;
      Redbox.Core.CardType cardType2 = Redbox.Core.CardType.AmericanExpress;
      if (!(cardType1.GetValueOrDefault() == cardType2 & cardType1.HasValue))
        return;
      IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
      if (trackData == null || !trackData.ChipEnabledAndSupportsEmv || (service != null ? (service.KioskSession.DeviceService.CardReadTechnicalFallbackEnabled ? 1 : 0) : 1) == 0)
        return;
      trackData.Errors.Add(CardHelper.ErrorCodes.NewAmexCannotFallbackError());
      trackData.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
    }

    private void DeclineChipEnabledCardThatWasSwiped(ITrackData trackData)
    {
      if (trackData == null || trackData.IsInTechnicalFallback || trackData.CardSourceType != CardSourceType.Swipe || !trackData.CardHasChip || trackData == null || !trackData.ChipEnabledAndSupportsEmv)
        return;
      trackData.Errors.Add(CardHelper.ErrorCodes.NewSwipedCardIsChipEnabledError());
    }

    private void DeclineReservationMobileWallet(ITrackData trackData)
    {
      ISession currentSession = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
      if (trackData == null || currentSession == null || currentSession.IsInZeroTouchMode || !currentSession.IsInReservationFlow || trackData.CardSourceType != CardSourceType.Mobile)
        return;
      trackData.Errors.Add(CardHelper.ErrorCodes.NewReservationMobileDeviceError());
    }

    private ITrackData GetTrackDataFromEncryptedCardReadResponseEvent(
      BaseResponseEvent baseResponseEvent)
    {
      ITrackData readResponseEvent1 = (ITrackData) null;
      EncryptedCardReadModel data = baseResponseEvent is EncryptedCardReadResponseEvent readResponseEvent2 ? readResponseEvent2.Data : (EncryptedCardReadModel) null;
      if (data != null)
      {
        readResponseEvent1 = (ITrackData) new EncryptedTrackData();
        this.PopulateEncryptedTrackDataFields(readResponseEvent1 as IEncryptedTrackData, (IEncryptedCardReadModel) data);
      }
      return readResponseEvent1;
    }

    private void PopulateEncryptedTrackDataFields(
      IEncryptedTrackData encryptedTrackData,
      IEncryptedCardReadModel encryptedCardReadModel)
    {
      if (encryptedTrackData != null)
      {
        if (encryptedCardReadModel != null)
        {
          encryptedTrackData.PANLength = encryptedCardReadModel.PANLength;
          encryptedTrackData.ExtLangCode = encryptedCardReadModel.ExtLangCode;
          encryptedTrackData.LSEncData = encryptedCardReadModel.LSEncData;
          encryptedTrackData.LSEncDataLength = encryptedCardReadModel.LSEncDataLength;
          encryptedTrackData.AESPAN = encryptedCardReadModel.AESPAN;
          encryptedTrackData.AESPANLength = encryptedCardReadModel.AESPANLength;
          encryptedTrackData.ICEncData = encryptedCardReadModel.ICEncData;
          encryptedTrackData.ICEncDataLength = encryptedCardReadModel.ICEncDataLength;
          encryptedTrackData.KSN = encryptedCardReadModel.KSN;
          encryptedTrackData.MfgSerialNumber = encryptedCardReadModel.MfgSerialNumber;
          encryptedTrackData.EncFormat = encryptedCardReadModel.EncFormat;
          encryptedTrackData.Name = encryptedCardReadModel.Name;
          encryptedTrackData.NameLength = encryptedCardReadModel.NameLength;
          encryptedTrackData.LanguageCode = encryptedCardReadModel.LanguageCode;
          encryptedTrackData.ServiceCode = encryptedCardReadModel.ServiceCode;
          encryptedTrackData.Expiry = encryptedCardReadModel.Expiry;
          encryptedTrackData.Mod10CheckFlag = encryptedCardReadModel.Mod10CheckFlag;
          encryptedTrackData.EncryptedFlag = encryptedCardReadModel.EncryptedFlag;
          encryptedTrackData.InjectedSerialNumber = encryptedCardReadModel.InjectedSerialNumber;
          this.PopulateBaseCardReadProperties((ITrackData) encryptedTrackData, encryptedCardReadModel as Base87CardReadModel);
        }
        else
          LogHelper.Instance.Log("DeviceServiceTrackDataService.PopulateEncryptedTrackDataFields error: encrypted card read model is null.");
        this.UpdateDeviceSerialNumber(encryptedTrackData.MfgSerialNumber);
      }
      else
        LogHelper.Instance.Log("DeviceServiceTrackDataService.PopulateEncryptedTrackDataFields error: encrypted track data is null.");
    }

    public bool IsInNumericRange(string data, int start, int? end)
    {
      int result;
      if (string.IsNullOrWhiteSpace(data) || !int.TryParse(data, out result) || start > result)
        return false;
      if (end.HasValue)
      {
        int? nullable = end;
        int num = result;
        if (nullable.GetValueOrDefault() < num & nullable.HasValue)
          return false;
      }
      return true;
    }

    private ITrackData GetTrackDataFromResponseEvent(
      BaseResponseEvent baseResponseEvent,
      ref bool chipCardRemovedFromReader)
    {
      return this.GetTrackDataFromEMVCardReadResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader) ?? this.GetTrackDataFromEncryptedCardReadResponseEvent(baseResponseEvent) ?? this.GetTrackDataFromUnencryptedCardReadResponseEvent(baseResponseEvent) ?? this.GetTrackDataFromCancelResponseEvent(baseResponseEvent) ?? this.GetTrackDataFromCardReadUnencryptedResponseEvent(baseResponseEvent);
    }

    private ITrackData GetTrackDataFromCancelResponseEvent(BaseResponseEvent baseResponseEvent)
    {
      ITrackData cancelResponseEvent = (ITrackData) null;
      if (baseResponseEvent is CancelCommandResponseEvent)
      {
        UnencryptedTrackData unencryptedTrackData = new UnencryptedTrackData();
        unencryptedTrackData.ReadStatus = new ResponseStatus?(ResponseStatus.Cancelled);
        cancelResponseEvent = (ITrackData) unencryptedTrackData;
      }
      return cancelResponseEvent;
    }

    private ITrackData GetTrackDataFromUnencryptedCardReadResponseEvent(
        BaseResponseEvent baseResponseEvent)
    {
        ITrackData trackData = null;
        IUnencryptedTrackData unencryptedTrackData = null;
        if (baseResponseEvent is UnencryptedCardReadResponseEvent readResponseEvent)
        {
            UnencryptedCardReadModel data = readResponseEvent.Data;
            if (data != null)
            {
                if (data.FirstSix == "601056" && data.Track1 == null)
                    data.Track1 = "REDBOX";
                trackData = this.GenerateTrackDataFromTrack1AndTrack2(data.Track1, data.Track2);

                if (trackData is IUnencryptedTrackData castedTrackData)
                {
                    unencryptedTrackData = castedTrackData;
                    if (unencryptedTrackData.HasValidData())
                    {
                        unencryptedTrackData.CardSourceType = data.CardSource;
                        ITrackDataEncryptionService service = ServiceLocator.Instance.GetService<ITrackDataEncryptionService>();
                        unencryptedTrackData.EncryptionCerificateKeyId = service.KeyId;
                        unencryptedTrackData.EncryptedAccountNumber = service.GetEncryptedAccountNumber(unencryptedTrackData);
                        unencryptedTrackData.EncryptedTrack2 = service.GetEncryptedTrack2(unencryptedTrackData);
                    }
                }
                unencryptedTrackData?.LockData();
            }
            this.PopulateBaseCardReadProperties(trackData, data);
        }
        return trackData;
    }


        private void PopulateBaseCardReadProperties(
      ITrackData trackData,
      Base87CardReadModel baseCardReadModel)
    {
      if (trackData == null || baseCardReadModel == null)
        return;
      trackData.FallbackStatusAction = baseCardReadModel.FallbackStatusAction;
      trackData.FallbackReason = baseCardReadModel.FallbackReason;
      trackData.CardHasChip = baseCardReadModel.HasChip;
      trackData.WalletType = baseCardReadModel.WalletFormat;
      trackData.CardSourceType = baseCardReadModel.CardSource;
      trackData.FirstSix = baseCardReadModel?.FirstSix;
      trackData.LastFour = baseCardReadModel?.LastFour;
      if (baseCardReadModel != null)
      {
        IEnumerable<DeviceService.ComponentModel.Error> errors = baseCardReadModel.Errors;
        if (errors != null)
          errors.ForEach<DeviceService.ComponentModel.Error>((Action<DeviceService.ComponentModel.Error>) (x => trackData.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DeviceServiceError", x.Code, x.Message))));
      }
      trackData.ReadStatus = new ResponseStatus?(baseCardReadModel.Status);
      if (string.IsNullOrEmpty(trackData.LastName))
        CardHelper.ParseNameData(baseCardReadModel?.Name, trackData);
      Redbox.Core.CardType? cardType1 = trackData.CardType;
      if (!cardType1.HasValue && baseCardReadModel != null && baseCardReadModel.FirstSix != null && baseCardReadModel != null && baseCardReadModel.LastFour != null)
      {
        string str = string.Empty.PadRight(Math.Max(baseCardReadModel.PANLength - 10, 0), 'X');
        string cardNumber = baseCardReadModel.FirstSix + str + baseCardReadModel.LastFour;
        CardHelper.AssignCardType(trackData, cardNumber);
        cardType1 = trackData.CardType;
        Redbox.Core.CardType cardType2 = Redbox.Core.CardType.RedboxGiftCard;
        if (cardType1.GetValueOrDefault() == cardType2 & cardType1.HasValue)
          CardHelper.AssignRedboxGiftCardAccountNumber(trackData);
      }
      if (string.IsNullOrEmpty(trackData.ExpiryYear) || string.IsNullOrEmpty(trackData.ExpiryMonth))
      {
        trackData.ExpiryYear = baseCardReadModel == null || baseCardReadModel.Expiry == null || baseCardReadModel.Expiry.Length != 4 ? (string) null : baseCardReadModel.Expiry.Substring(0, 2);
        trackData.ExpiryMonth = baseCardReadModel == null || baseCardReadModel.Expiry == null || baseCardReadModel.Expiry.Length != 4 ? (string) null : baseCardReadModel.Expiry.Substring(2, 2);
      }
      if (baseCardReadModel.HasVasData)
        trackData.VasIdentifier = baseCardReadModel.VasData;
      this.AddTamperedCardReadErrorsToTrackData(trackData, baseCardReadModel);
    }

    private void AddTamperedCardReadErrorsToTrackData(
      ITrackData trackData,
      Base87CardReadModel baseCardReadModel)
    {
      if (trackData == null || baseCardReadModel == null)
        return;
      trackData.Errors.AddRange((IEnumerable<Redbox.KioskEngine.ComponentModel.Error>) baseCardReadModel.GetTamperedErrors());
    }

    private ITrackData GetTrackDataFromCardReadUnencryptedResponseEvent(
      BaseResponseEvent baseResponseEvent)
    {
      ITrackData unencryptedResponseEvent = (ITrackData) null;
      if (baseResponseEvent is UnencryptedCardReadResponseEvent readResponseEvent && readResponseEvent.Data != null)
        unencryptedResponseEvent = this.GenerateTrackDataFromTrack1AndTrack2(readResponseEvent.Data.Track1, readResponseEvent.Data.Track2);
      return unencryptedResponseEvent;
    }

    private ITrackData GenerateTrackDataFromTrack1AndTrack2(string track1, string track2)
    {
      ITrackData fromTrack1AndTrack2 = (ITrackData) null;
      if (track1 != null || track2 != null)
      {
        SecureString secureString = new SecureString();
        foreach (char c in track1 + ";" + track2 + "?")
          secureString.AppendChar(c);
        fromTrack1AndTrack2 = (ITrackData) CardHelper.Parse(secureString);
        secureString.Dispose();
      }
      return fromTrack1AndTrack2;
    }

    private void UpdateDeviceSerialNumber(string current)
    {
      if (string.IsNullOrWhiteSpace(current))
        return;
      ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<string>("Store", "ReaderSerialNumber", current);
    }

    private class ReadCardJobEntry
    {
      public IReadCardJob ReadCardJob { get; set; }

      public bool AttemptedCancel { get; set; }

      public bool IsZeroTouchRead { get; set; }

      public Guid SessionId { get; set; }
    }
  }
}
