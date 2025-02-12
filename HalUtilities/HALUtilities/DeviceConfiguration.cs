namespace HALUtilities
{
  public class DeviceConfiguration
  {
    public readonly int DevicePixelWidth;
    public readonly int DeviceHeight;

    internal DeviceConfiguration(int width, int height)
    {
      this.DevicePixelWidth = width;
      this.DeviceHeight = height;
    }
  }
}
