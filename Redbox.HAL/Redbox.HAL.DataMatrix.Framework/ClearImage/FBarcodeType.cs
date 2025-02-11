using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum FBarcodeType
    {
        cibfInterleaved2of5 = 1,
        cibfCode39 = 2,
        cibfCode128 = 4,
        cibfCodabar = 8,
        cibfUpca = 16, // 0x00000010
        cibfEan8 = 32, // 0x00000020
        cibfCode93 = 64, // 0x00000040
        cibfUpce = 128, // 0x00000080
        cibfEan13 = 256, // 0x00000100
        cibfAddon2 = 512, // 0x00000200
        cibfAddon5 = 1024, // 0x00000400
        cibfIndustrial2of5 = 65536, // 0x00010000
        cibfDatalogic2of5 = 131072, // 0x00020000
        cibfMatrix2of5 = 262144, // 0x00040000
        cibfAirline2of5 = 524288, // 0x00080000
        cibfUcc128 = 1048576, // 0x00100000
        cibfPatch = 2097152, // 0x00200000
        cibfCode32 = 8388608, // 0x00800000
        cibfPostnet = 16777216, // 0x01000000
        cibf4State = 33554432 // 0x02000000
    }
}