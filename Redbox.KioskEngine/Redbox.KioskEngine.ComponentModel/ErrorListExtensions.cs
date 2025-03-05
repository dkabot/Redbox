namespace Redbox.KioskEngine.ComponentModel
{
  public static class ErrorListExtensions
  {
    public static bool IsNotNullOrEmpty(this ErrorList list) => list != null && list.Count > 0;

    public static bool IsNullOrEmpty(this ErrorList list) => list == null || list.Count == 0;
  }
}
