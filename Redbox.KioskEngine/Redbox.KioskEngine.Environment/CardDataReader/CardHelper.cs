using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public static class CardHelper
  {
    public const string RedboxGiftCardBIN = "601056";
    public const string RedboxGiftCardName = "REDBOX";

    public static string GenerateCardId(SecureString cardNumber)
    {
      SHA256Managed shA256Managed = new SHA256Managed();
      string base64String = Convert.ToBase64String(shA256Managed.ComputeHash(shA256Managed.ComputeHash(Encoding.UTF8.GetBytes(CardHelper.ExtractFromSecure(cardNumber)))));
      shA256Managed.Clear();
      shA256Managed.Dispose();
      return base64String;
    }

    public static string GenerateReservationCardId(string firstSix, string lastFour)
    {
      return CardHelper.GenerateHash(firstSix) + CardHelper.GenerateHash(lastFour);
    }

    public static string GenerateHash(string sourcString)
    {
      SHA256Managed shA256Managed = new SHA256Managed();
      string base64String = Convert.ToBase64String(shA256Managed.ComputeHash(shA256Managed.ComputeHash(Encoding.UTF8.GetBytes(sourcString))));
      shA256Managed.Clear();
      shA256Managed.Dispose();
      return base64String;
    }

    public static SecureString CopyToSecure(string value)
    {
      SecureString secure = new SecureString();
      foreach (char c in value.ToCharArray())
        secure.AppendChar(c);
      return secure;
    }

    public static string ExtractFromSecure(SecureString value)
    {
      try
      {
        return Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(value));
      }
      catch
      {
        return string.Empty;
      }
    }

    public static string EncryptCreditCardFromXmlKey(SecureString cardNumber, string keyXml)
    {
      if (cardNumber.Length == 0)
        return (string) null;
      RSACryptoServiceProvider cryptoServiceProvider = (RSACryptoServiceProvider) null;
      try
      {
        cryptoServiceProvider = new RSACryptoServiceProvider();
        cryptoServiceProvider.FromXmlString(keyXml);
        return Convert.ToBase64String(cryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes(CardHelper.ExtractFromSecure(cardNumber)), false));
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in CardHHelper.EncryptCreditCardFromXmlKey.", ex);
      }
      finally
      {
        if (cryptoServiceProvider != null)
        {
          cryptoServiceProvider.Clear();
          cryptoServiceProvider.Dispose();
        }
      }
      return (string) null;
    }

    public static string EncryptCreditCardFromCertificate(
      SecureString cardNumber,
      X509Certificate2 certificate)
    {
      if (cardNumber.Length == 0)
        return (string) null;
      RSACryptoServiceProvider cryptoServiceProvider = (RSACryptoServiceProvider) null;
      try
      {
        cryptoServiceProvider = (RSACryptoServiceProvider) certificate.PublicKey.Key;
        return Convert.ToBase64String(cryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes(CardHelper.ExtractFromSecure(cardNumber)), false));
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in CardHHelper.EncryptCreditCardFromCertificate.", ex);
      }
      finally
      {
        cryptoServiceProvider?.Clear();
      }
      return (string) null;
    }

    public static void CheckForTrackDataErrors(ITrackData trackData)
    {
      if (trackData == null || !trackData.HasPay)
        return;
      if (trackData.FirstSix == null || trackData.LastFour == null)
        CardHelper.AddErrorMessageIfNotDuplicate(trackData, CardHelper.ErrorCodes.NewInvalidPANError());
      if (!trackData.CardType.HasValue)
        CardHelper.AddErrorMessageIfNotDuplicate(trackData, CardHelper.ErrorCodes.NewInvalidInvalidBinRangeError());
      CardType? cardType1 = trackData.CardType;
      CardType cardType2 = CardType.RedboxGiftCard;
      if (!(cardType1.GetValueOrDefault() == cardType2 & cardType1.HasValue))
      {
        if (!CardHelper.IsInNumericRange(trackData.ExpiryMonth, 1, new int?(12)))
          CardHelper.AddErrorMessageIfNotDuplicate(trackData, CardHelper.ErrorCodes.NewInvalidInvalidExpiryMonthError());
        if (!CardHelper.IsNumeric(trackData.ExpiryYear))
          CardHelper.AddErrorMessageIfNotDuplicate(trackData, CardHelper.ErrorCodes.NewInvalidInvalidExpiryYearError());
      }
      if (trackData is IUnencryptedTrackData unencryptedTrackData)
      {
        if (unencryptedTrackData.AccountNumber == null || unencryptedTrackData.AccountNumber.Length < 13 || unencryptedTrackData.AccountNumber.Length > 16)
        {
          unencryptedTrackData.AccountNumber.Clear();
          CardHelper.AddErrorMessageIfNotDuplicate((ITrackData) unencryptedTrackData, CardHelper.ErrorCodes.NewInvalidPANError());
        }
        CardType? cardType3 = unencryptedTrackData.CardType;
        CardType cardType4 = CardType.RedboxGiftCard;
        if (cardType3.GetValueOrDefault() == cardType4 & cardType3.HasValue)
        {
          if (CardHelper.ExtractFromSecure(unencryptedTrackData.Track2).Split('=')[1].Length < 20)
            CardHelper.AddErrorMessageIfNotDuplicate((ITrackData) unencryptedTrackData, CardHelper.ErrorCodes.NewInvalidInvalidFirstDataAccountNumberError());
        }
      }
      if (!(trackData is IEncryptedTrackData encryptedTrackData))
        return;
      CardType? cardType5 = encryptedTrackData.CardType;
      CardType cardType6 = CardType.RedboxGiftCard;
      if (cardType5.GetValueOrDefault() == cardType6 & cardType5.HasValue && encryptedTrackData.EncryptedFlag)
        CardHelper.AddErrorMessageIfNotDuplicate((ITrackData) encryptedTrackData, CardHelper.ErrorCodes.NewInvalidInvalidRedboxCardNameError());
      int result;
      if (string.IsNullOrEmpty(encryptedTrackData.Expiry) || encryptedTrackData.Expiry.Length != 4 || !int.TryParse(encryptedTrackData.Expiry, out result) || result <= 0)
        CardHelper.AddErrorMessageIfNotDuplicate((ITrackData) encryptedTrackData, CardHelper.ErrorCodes.NewInvalidInvalidExpiryYearError());
      if (encryptedTrackData.PANLength >= 13 && encryptedTrackData.PANLength <= 19)
        return;
      CardHelper.AddErrorMessageIfNotDuplicate((ITrackData) encryptedTrackData, CardHelper.ErrorCodes.NewInvalidPANError());
    }

    private static void AddErrorMessageIfNotDuplicate(ITrackData trackData, Redbox.KioskEngine.ComponentModel.Error error)
    {
      if (error == null || trackData?.Errors == null || trackData.Errors.ContainsCode(error.Code))
        return;
      trackData.Errors.Add(error);
    }

    private static bool IsInNumericRange(string data, int start, int? end)
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

    public static void AssignRedboxGiftCardAccountNumber(ITrackData trackData)
    {
      CardType? cardType1 = trackData.CardType;
      CardType cardType2 = CardType.RedboxGiftCard;
      if (!(cardType1.GetValueOrDefault() == cardType2 & cardType1.HasValue) || !(trackData is IUnencryptedTrackData unencryptedTrackData))
        return;
      string[] strArray = CardHelper.ExtractFromSecure(unencryptedTrackData.Track2).Split('=');
      LogHelper.Instance.Log("Using First Data account number parsing.");
      if (strArray != null && strArray.Length >= 1 && strArray[0] != null && strArray[0].Length >= 15 && strArray[1] != null && strArray[1].Length >= 20)
        unencryptedTrackData.AccountNumber = CardHelper.CopyToSecure(strArray[1].Substring(12, 1) + strArray[1].Substring(14, 2) + strArray[0].Substring(6, 9) + strArray[1].Substring(16, 4));
      else
        LogHelper.Instance.Log("Unable to parse First Data account number");
    }

    public static void AssignCardType(ITrackData trackData, string cardNumber)
    {
      if (string.IsNullOrEmpty(cardNumber) || trackData == null)
        return;
      if (cardNumber.Length >= 6 && cardNumber.Substring(0, 6) == "601056")
        trackData.CardType = new CardType?(CardType.RedboxGiftCard);
      else
        trackData.CardType = CardTypeHelper.GetCardType(cardNumber);
    }

    public static IUnencryptedTrackData Parse(SecureString value)
    {
      IUnencryptedTrackData unencryptedTrackData = (IUnencryptedTrackData) new UnencryptedTrackData();
      ErrorList errors = unencryptedTrackData.Errors;
      try
      {
        string[] strArray1 = CardHelper.ExtractFromSecure(value).Split(';');
        if (strArray1.Length == 0)
        {
          errors.Add(CardHelper.ErrorCodes.NewInvalidTracksError());
          return unencryptedTrackData;
        }
        if (strArray1.Length > 1)
        {
          unencryptedTrackData.Track1 = CardHelper.CopyToSecure(strArray1[0]);
          unencryptedTrackData.Track2 = CardHelper.CopyToSecure(strArray1[1]);
          CardHelper.ParseNameData(unencryptedTrackData);
        }
        else if (strArray1.Length != 0)
        {
          unencryptedTrackData.Track1.Clear();
          unencryptedTrackData.Track2 = CardHelper.CopyToSecure(strArray1[0]);
        }
        string[] strArray2 = CardHelper.ExtractFromSecure(unencryptedTrackData.Track2).Split('=');
        unencryptedTrackData.AccountNumber = CardHelper.CopyToSecure(strArray2[0]);
        CardHelper.AssignCardType((ITrackData) unencryptedTrackData, CardHelper.ExtractFromSecure(unencryptedTrackData.AccountNumber));
        CardType? cardType1 = unencryptedTrackData.CardType;
        CardType cardType2 = CardType.RedboxGiftCard;
        if (cardType1.GetValueOrDefault() == cardType2 & cardType1.HasValue)
        {
          if (unencryptedTrackData.FirstName?.ToUpper() != "REDBOX" && unencryptedTrackData.LastName?.ToUpper() != "REDBOX")
          {
            errors.Add(CardHelper.ErrorCodes.NewInvalidInvalidRedboxCardNameError());
            return unencryptedTrackData;
          }
          CardHelper.AssignRedboxGiftCardAccountNumber((ITrackData) unencryptedTrackData);
        }
        CardType? cardType3 = unencryptedTrackData.CardType;
        CardType cardType4 = CardType.RedboxGiftCard;
        if (!(cardType3.GetValueOrDefault() == cardType4 & cardType3.HasValue) && !LuhnHelper.IsCreditCardValid(CardHelper.ExtractFromSecure(unencryptedTrackData.AccountNumber)))
        {
          errors.Add(CardHelper.ErrorCodes.NewInvalidFailedLuhnValidationError());
          return unencryptedTrackData;
        }
        unencryptedTrackData.FirstSix = CardHelper.ExtractFromSecure(unencryptedTrackData.AccountNumber).Substring(0, 6);
        unencryptedTrackData.LastFour = CardHelper.ExtractFromSecure(unencryptedTrackData.AccountNumber).Substring(unencryptedTrackData.AccountNumber.Length - 4);
        unencryptedTrackData.ExpiryYear = strArray2[1].Substring(0, 2);
        unencryptedTrackData.ExpiryMonth = strArray2[1].Substring(2, 2);
        if (unencryptedTrackData.Track2.Length > 1)
          unencryptedTrackData.Track2 = CardHelper.CopyToSecure(CardHelper.ExtractFromSecure(unencryptedTrackData.Track2).Substring(0, CardHelper.ExtractFromSecure(unencryptedTrackData.Track2).IndexOf("?")));
        CardHelper.CheckForTrackDataErrors((ITrackData) unencryptedTrackData);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in CardHelper.Parse.", ex);
        errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("T999", "An unhandled exception was raised in CardHelper.Parse.", ex));
      }
      return unencryptedTrackData;
    }

    private static void ParseNameData(IUnencryptedTrackData unencryptedTrackData)
    {
      CardHelper.ParseNameData(CardHelper.ExtractFromSecure(unencryptedTrackData.Track1), (ITrackData) unencryptedTrackData);
    }

    public static void ParseNameData(string nameData, ITrackData trackData)
    {
      Regex regex = new Regex("[a-z][^0-9][a-z]*", RegexOptions.IgnoreCase);
      if (string.IsNullOrEmpty(nameData) || trackData == null)
        return;
      MatchCollection matchCollection = regex.Matches(nameData);
      if (matchCollection.Count > 1)
      {
        trackData.FirstName = matchCollection[0].Groups[0].Value;
        trackData.LastName = matchCollection[1].Groups[0].Value;
      }
      else
      {
        if (matchCollection.Count <= 0)
          return;
        trackData.FirstName = matchCollection[0].Groups[0].Value;
      }
    }

    private static bool IsNumeric(string stringToCheck)
    {
      return !string.IsNullOrEmpty(stringToCheck) && new Regex("^[0-9]+$").IsMatch(stringToCheck);
    }

    public static class ErrorCodes
    {
      public const string InvalidPAN = "T001";
      public const string FailedLuhnValidation = "T002";
      public const string InvalidBinRange = "T003";
      public const string InvalidExpiryYear = "T004";
      public const string InvalidExpiryMonth = "T005";
      public const string InvalidFirstDataAccountNumber = "T006";
      public const string InvalidRedboxCardName = "T007";
      public const string InvalidTracks = "T008";
      public const string SwipedCardIsChipEnabled = "T020";
      public const string AmexCannotFallback = "T021";
      public const string ReservationMobileDevice = "T022";

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidPANError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T001", "Invalid PAN provided in track data stream.", "A valid PAN is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidFailedLuhnValidationError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T002", "PAN failed Luhn validation.", "A valid PAN is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidBinRangeError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T003", "Invalid PAN provided in track data stream; unknown bin range.", "A valid PAN is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidExpiryYearError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T004", "Invalid expiry year in track data stream.", "A valid exiry year is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidExpiryMonthError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T005", "Invalid expiry month provided in track data stream.", "A valid expiry month is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidFirstDataAccountNumberError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T006", "Track data will fail First Data account number parsing.", "Valid track data is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidInvalidRedboxCardNameError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T007", "Invalid Redbox Card name provided in track data.", "Valid Redbox Card name is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewInvalidTracksError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T008", "No valid tracks in data stream.", "Valid track data is required.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewSwipedCardIsChipEnabledError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T020", "Swiped Card is chip enabled. Chip needs to be inserted", "Insert chip instead of swiping card.");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewAmexCannotFallbackError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T021", "Amex Cannot Fallback", "Amex Cannot Fallback");
      }

      public static Redbox.KioskEngine.ComponentModel.Error NewReservationMobileDeviceError()
      {
        return Redbox.KioskEngine.ComponentModel.Error.NewError("T022", "Cannot use a mobile wallet for a reservation", "Cannot use a mobile wallet for a reservation");
      }
    }
  }
}
