using DeviceService.ComponentModel;
using System;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel.TrackData
{
  public static class ITrackDataExtension
  {
    public static bool CardSourceTypeIsEMVContactOrQuickChip(this ITrackData td)
    {
      if (td == null)
        return false;
      return td.CardSourceType == CardSourceType.EMVContact || td.CardSourceType == CardSourceType.QuickChip;
    }

    public static bool CardSourceTypeIsEMVContactlessOrMobileOrMSDContactless(this ITrackData td)
    {
      if (td == null)
        return false;
      return td.CardSourceType == CardSourceType.EMVContactless || td.CardSourceType == CardSourceType.Mobile || td.CardSourceType == CardSourceType.MSDContactless;
    }

    public static bool CardSourceTypeIsMobileAndHasPayAndNotVas(this ITrackData td)
    {
      return td != null && td.CardSourceType == CardSourceType.Mobile && td.HasPay && !td.HasVas;
    }

    public static bool WalletTypeIsGoogleAndHasPayAndHasVas(this ITrackData td)
    {
      return td != null && td.WalletType == WalletType.Google && td.HasPay && td.HasVas;
    }

    public static bool WalletTypeIsGoogleAndNotVas(this ITrackData td)
    {
      return td != null && td.WalletType == WalletType.Google && !td.HasVas;
    }

    public static bool HasErrors(this ITrackData td)
    {
      return td != null && td.Errors != null && td.Errors.Count > 0;
    }

    public static bool HasSwipedCardIsChipEnabledError(this ITrackData td)
    {
      return td.HasErrors() && td.Errors.ContainsCode("T020");
    }

    public static bool HasReservationMobileDeviceError(this ITrackData td)
    {
      return td.HasErrors() && td.Errors.ContainsCode("T022");
    }

    public static bool HasTamperedError(this ITrackData td)
    {
      return td.HasErrors() && td.Errors.Any<Redbox.KioskEngine.ComponentModel.Error>((Func<Redbox.KioskEngine.ComponentModel.Error, bool>) (e => string.Equals(e.Code, "TAMPERED", StringComparison.CurrentCultureIgnoreCase)));
    }

    private static class ErrorCodes
    {
      public const string SwipedCardIsChipEnabled = "T020";
      public const string ReservationMobileDevice = "T022";
    }
  }
}
