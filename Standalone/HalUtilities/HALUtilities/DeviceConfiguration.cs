namespace HALUtilities
{
    public class DeviceConfiguration
    {
        public readonly int DeviceHeight;
        public readonly int DevicePixelWidth;

        internal DeviceConfiguration(int width, int height)
        {
            DevicePixelWidth = width;
            DeviceHeight = height;
        }
    }
}