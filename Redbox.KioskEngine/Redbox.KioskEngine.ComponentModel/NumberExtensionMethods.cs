using System;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class NumberExtensionMethods
  {
    public static string ToCurrencyString(this Decimal amount, bool useCentSign = false)
    {
      return useCentSign && amount < 1M ? ((int) (amount * 100M)).ToString() + "¢" : string.Format("{0:C2}", (object) amount);
    }
  }
}
