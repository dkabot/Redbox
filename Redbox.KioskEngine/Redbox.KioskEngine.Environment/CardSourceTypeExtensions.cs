using DeviceService.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public static class CardSourceTypeExtensions
  {
    public static bool IsEmvContactlessOrQuickChip(this CardSourceType type)
    {
      return CardSourceType.EMVContact == type || CardSourceType.QuickChip == type;
    }
  }
}
