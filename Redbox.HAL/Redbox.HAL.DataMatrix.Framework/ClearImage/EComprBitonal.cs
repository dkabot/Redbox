using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EComprBitonal
    {
        citbAUTO = 0,
        citbNONE = 1,
        citbCCITTFAX3 = 3,
        citbCCITTFAX4 = 4,
        citbORIGINAL = 500 // 0x000001F4
    }
}