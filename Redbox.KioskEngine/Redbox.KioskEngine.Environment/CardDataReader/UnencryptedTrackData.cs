using Redbox.KioskEngine.ComponentModel.TrackData;
using System.Security;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class UnencryptedTrackData : Redbox.KioskEngine.Environment.CardDataReader.TrackData, IUnencryptedTrackData, ITrackData
  {
    private SecureString _track1;
    private SecureString _track2;
    private SecureString _encryptedTrack2;
    private SecureString _accountNumber;
    private string _encryptedAccountNumber;
    private int _encryptionCerificateKeyId;

    public SecureString Track1
    {
      get => this._track1;
      set
      {
        if (this._isDataLocked)
          return;
        this._track1 = value;
      }
    }

    public SecureString Track2
    {
      get => this._track2;
      set
      {
        if (this._isDataLocked)
          return;
        this._track2 = value;
      }
    }

    public SecureString EncryptedTrack2
    {
      get => this._encryptedTrack2;
      set
      {
        if (this._isDataLocked)
          return;
        this._encryptedTrack2 = value;
      }
    }

    public SecureString AccountNumber
    {
      get => this._accountNumber;
      set
      {
        if (this._isDataLocked)
          return;
        this._accountNumber = value;
        this._cardHashId = this._accountNumber != null ? CardHelper.GenerateCardId(this._accountNumber) : (string) null;
      }
    }

    public string EncryptedAccountNumber
    {
      get => this._encryptedAccountNumber;
      set
      {
        if (this._isDataLocked)
          return;
        this._encryptedAccountNumber = value;
      }
    }

    public int EncryptionCerificateKeyId
    {
      get => this._encryptionCerificateKeyId;
      set
      {
        if (this._isDataLocked)
          return;
        this._encryptionCerificateKeyId = value;
      }
    }
  }
}
