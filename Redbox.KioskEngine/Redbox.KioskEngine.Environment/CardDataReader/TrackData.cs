using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public abstract class TrackData : ITrackData
  {
    private CardSourceType _cardSourceType;
    private ErrorList _errors;
    private string _firstSix;
    private string _lastFour;
    private string _lastName;
    private Redbox.Core.CardType? _cardType;
    private string _firstName;
    private string _expiryYear;
    private string _expiryMonth;
    protected string _cardHashId;
    private string _reservationHashId;
    private bool _cardHasChip;
    private bool _isInTechnicalFallback;
    private bool _nextCardReadIsInTechnicalFallback;
    private FallbackType? _lastFallbackReason;
    private FallbackStatusAction _fallbackStatusAction;
    private readonly bool _chipEnabled;
    private readonly bool _contactlessEnabled;
    protected bool _isDataLocked;

    public TrackData()
    {
      this.Errors = new ErrorList();
      IDeviceServiceClientHelper service1 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
      if ((service1 != null ? (service1.SupportsEMV ? 1 : 0) : 0) == 0)
        return;
      IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
      this._chipEnabled = service2.KioskSession.DeviceService.ChipEnabled;
      this._contactlessEnabled = service2.KioskSession.DeviceService.ContactlessEnabled;
    }

    public virtual bool HasValidData()
    {
      if (this.ReadStatus.HasValue)
      {
        ResponseStatus? readStatus = this.ReadStatus;
        ResponseStatus responseStatus = ResponseStatus.Success;
        if (!(readStatus.GetValueOrDefault() == responseStatus & readStatus.HasValue))
          goto label_6;
      }
      if (this.Errors != null && !this.Errors.ContainsError())
        return this.HasPay || this.HasVas;
label_6:
      return false;
    }

    public ErrorList Errors
    {
      get => this._errors;
      set
      {
        if (this._isDataLocked)
          return;
        this._errors = value;
      }
    }

    public string FirstSix
    {
      get => this._firstSix;
      set
      {
        if (this._isDataLocked)
          return;
        this._firstSix = value;
        if (!(this is EncryptedTrackData) || string.IsNullOrEmpty(this._firstSix) || string.IsNullOrEmpty(this._lastFour))
          return;
        this._cardHashId = CardHelper.GenerateReservationCardId(this._firstSix, this._lastFour);
      }
    }

    public string LastFour
    {
      get => this._lastFour;
      set
      {
        if (this._isDataLocked)
          return;
        this._lastFour = value;
        if (!(this is EncryptedTrackData) || string.IsNullOrEmpty(this._firstSix) || string.IsNullOrEmpty(this._lastFour))
          return;
        this._cardHashId = CardHelper.GenerateReservationCardId(this._firstSix, this._lastFour);
      }
    }

    public string LastName
    {
      get => this._lastName;
      set
      {
        if (this._isDataLocked)
          return;
        this._lastName = value;
      }
    }

    public Redbox.Core.CardType? CardType
    {
      get => this._cardType;
      set
      {
        if (this._isDataLocked)
          return;
        this._cardType = value;
      }
    }

    public string FirstName
    {
      get => this._firstName;
      set
      {
        if (this._isDataLocked)
          return;
        this._firstName = value;
      }
    }

    public string ExpiryYear
    {
      get => this._expiryYear;
      set
      {
        if (this._isDataLocked)
          return;
        this._expiryYear = value;
      }
    }

    public string ExpiryMonth
    {
      get => this._expiryMonth;
      set
      {
        if (this._isDataLocked)
          return;
        this._expiryMonth = value;
      }
    }

    public string CardHashId => this._cardHashId;

    public string ReservationHashId
    {
      get
      {
        if (this._reservationHashId == null && !string.IsNullOrEmpty(this._firstSix) && !string.IsNullOrEmpty(this._lastFour))
          this._reservationHashId = CardHelper.GenerateReservationCardId(this._firstSix, this._lastFour);
        return this._reservationHashId;
      }
    }

    public void LockData() => this._isDataLocked = true;

    public CardSourceType CardSourceType
    {
      get => this._cardSourceType;
      set
      {
        if (this._isDataLocked)
          return;
        this._cardSourceType = value;
      }
    }

    public FallbackStatusAction FallbackStatusAction
    {
      get => this._fallbackStatusAction;
      set
      {
        if (this._isDataLocked)
          return;
        this._fallbackStatusAction = value;
      }
    }

    public FallbackType? FallbackReason { get; set; }

    public FallbackType? LastFallbackReason
    {
      get => this._lastFallbackReason;
      set
      {
        if (this._isDataLocked)
          return;
        this._lastFallbackReason = value;
      }
    }

    public bool IsInTechnicalFallback
    {
      get => this._isInTechnicalFallback;
      set
      {
        if (this._isDataLocked)
          return;
        this._isInTechnicalFallback = value;
      }
    }

    public bool NextCardReadIsInTechnicalFallback
    {
      get => this._nextCardReadIsInTechnicalFallback;
      set
      {
        if (this._isDataLocked)
          return;
        this._nextCardReadIsInTechnicalFallback = value;
      }
    }

    public bool CardHasChip
    {
      get => this._cardHasChip;
      set
      {
        if (this._isDataLocked)
          return;
        this._cardHasChip = value;
      }
    }

    public ResponseStatus? ReadStatus { get; set; }

    public bool ChipEnabledAndSupportsEmv => this._chipEnabled;

    public bool ContactlessEnabledAndSupportsEmv => this._contactlessEnabled;

    public bool EmvEnabled
    {
      get => this.ChipEnabledAndSupportsEmv || this.ContactlessEnabledAndSupportsEmv;
    }

    public string VasIdentifier { get; set; }

    public bool HasVas => !string.IsNullOrEmpty(this.VasIdentifier);

    public bool HasPay
    {
      get => !string.IsNullOrEmpty(this.FirstSix) && !string.IsNullOrEmpty(this.LastFour);
    }

    public WalletType WalletType { get; set; }
  }
}
