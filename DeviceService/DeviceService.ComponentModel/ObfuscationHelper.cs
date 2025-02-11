namespace DeviceService.ComponentModel
{
    public class ObfuscationHelper
    {
        public static string ObfuscateValue(string value)
        {
            return !string.IsNullOrEmpty(value)
                ? string.Format("[Value not logged, length: {0}]", value != null ? value.Length : 0)
                : null;
        }
    }
}