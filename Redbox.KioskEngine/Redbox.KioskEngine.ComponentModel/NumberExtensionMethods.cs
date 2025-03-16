namespace Redbox.KioskEngine.ComponentModel
{
    public static class NumberExtensionMethods
    {
        public static string ToCurrencyString(this decimal amount, bool useCentSign = false)
        {
            return useCentSign && amount < 1M ? (int)(amount * 100M) + "¢" : string.Format("{0:C2}", amount);
        }
    }
}