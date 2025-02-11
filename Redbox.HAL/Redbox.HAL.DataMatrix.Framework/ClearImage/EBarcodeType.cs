using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EBarcodeType
    {
        cibInterleaved2of5 = 1,
        cibCode39 = 2,
        cibCode128 = 3,
        cibCodabar = 4,
        cibUpca = 5,
        cibEan8 = 6,
        cibCode93 = 7,
        cibUpce = 8,
        cibEan13 = 9,
        cibAddon2 = 10, // 0x0000000A
        cibAddon5 = 11, // 0x0000000B
        cibCode32 = 12, // 0x0000000C
        cibIndustrial2of5 = 13, // 0x0000000D
        cibDatalogic2of5 = 14, // 0x0000000E
        cibMatrix2of5 = 15, // 0x0000000F
        cibAirline2of5 = 16, // 0x00000010
        cibUcc128 = 17, // 0x00000011
        cibPatch = 18, // 0x00000012
        cibPostnet = 19, // 0x00000013
        cibPlanet = 20, // 0x00000014
        cibDataMatrix = 30, // 0x0000001E
        cibPdf417 = 31, // 0x0000001F
        cib4State = 32, // 0x00000020
        cibUspsIntelligentMail = 33, // 0x00000021
        cibBpoPostcode = 34, // 0x00000022
        cibAustralianPost = 35, // 0x00000023
        cibQR = 36 // 0x00000024
    }
}