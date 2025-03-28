using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Session;
using System;
using System.Security;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class ClassicTrackDataService : IClassicTrackDataService, ITrackDataService
  {
    private Action<CardReadRequestInfo, CardReadCompleteDetails> _onCardReadComplete;
    private CardReadRequestInfo _cardReadRequest;
    private const char TrackEnd = '?';
    private const char TrackStart = '%';
    private const string StartSentinel = ";";
    private bool _allowTrackDataParsing;
    private bool _receivingTrackData;

    public bool IsOlderReader => true;

    public bool IsNewReader => false;

    private void ClearData() => this._trackDataBuffer?.Clear();

    public void StartCardRead(
      CardReadRequestInfo request,
      Action<IReadCardJob> cardReadStartedAction,
      Action<Guid> onCardProcessing,
      Action<CardReadRequestInfo, CardReadCompleteDetails> onCardReadComplete)
    {
      this._cardReadRequest = request;
      this._onCardReadComplete = onCardReadComplete;
      LogHelper.Instance.Log("ClassicTrackDataService start card read");
      this._allowTrackDataParsing = true;
      this.ClearData();
    }

    public void StopCardRead(CancelReason cancelReason)
    {
      LogHelper.Instance.Log("ClassicTrackDataService stop card read");
      if (!this._allowTrackDataParsing)
        return;
      this._allowTrackDataParsing = false;
      ICCReaderHealth service = ServiceLocator.Instance.GetService<ICCReaderHealth>();
      ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
      if (cancelReason != CancelReason.LeavingSwipeView)
        return;
      bool dataWasReceived = trackData != null || trackData != null && trackData.HasValidData();
      bool hasError = !dataWasReceived;
      service?.EventOccurred(dataWasReceived, hasError);
    }

    public void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason)
    {
    }

    public bool AllowTrackDataParsing => this._allowTrackDataParsing;

    public bool ProcessKey(char keyChar, Action onTrackEnd)
    {
      bool flag = false;
      if (this._allowTrackDataParsing)
      {
        if (keyChar == '%')
        {
          this._receivingTrackData = true;
          this._trackDataBuffer = new SecureString();
          LogHelper.Instance.Log("ClassicTrackDataService card read: track start detected");
        }
        if (this._receivingTrackData)
        {
          this._trackDataBuffer?.AppendChar(keyChar);
          if (keyChar == '?' && this.TrackDataBufferContainsStartSentinel())
          {
            LogHelper.Instance.Log("ClassicTrackDataService card read: track end detected");
            this._receivingTrackData = false;
            if (onTrackEnd != null)
              onTrackEnd();
            IUnencryptedTrackData unencryptedTrackData = CardHelper.Parse(this._trackDataBuffer);
            if (unencryptedTrackData != null)
              unencryptedTrackData.CardSourceType = CardSourceType.Unknown;
            if (unencryptedTrackData != null && unencryptedTrackData.HasValidData())
            {
              ITrackDataEncryptionService service = ServiceLocator.Instance.GetService<ITrackDataEncryptionService>();
              unencryptedTrackData.EncryptionCerificateKeyId = service.KeyId;
              unencryptedTrackData.EncryptedAccountNumber = service.GetEncryptedAccountNumber(unencryptedTrackData);
              unencryptedTrackData.EncryptedTrack2 = service.GetEncryptedTrack2(unencryptedTrackData);
            }
            unencryptedTrackData?.LockData();
            Action<CardReadRequestInfo, CardReadCompleteDetails> cardReadComplete = this._onCardReadComplete;
            if (cardReadComplete != null)
              cardReadComplete(this._cardReadRequest, new CardReadCompleteDetails()
              {
                TrackData = (ITrackData) unencryptedTrackData,
                CardRemoved = true,
                Success = unencryptedTrackData != null && unencryptedTrackData.HasValidData()
              });
            this._trackDataBuffer.Clear();
          }
          flag = true;
        }
      }
      return flag;
    }

    private bool TrackDataBufferContainsStartSentinel()
    {
      return this._trackDataBuffer != null && CardHelper.ExtractFromSecure(this._trackDataBuffer).IndexOf(";") > 0;
    }

    private SecureString _trackDataBuffer { get; set; }
  }
}
