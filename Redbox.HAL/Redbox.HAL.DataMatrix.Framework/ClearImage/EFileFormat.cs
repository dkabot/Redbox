using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EFileFormat
    {
        ciEXT = 1,
        ciBMP = 3,
        ciPCX = 4,
        ciTIFF = 5,
        ciTIFF_G3_1D = 6,
        ciTIFF_G4 = 7,
        ciTIFF_NEOL = 8,
        ciTIFF_JPEG = 20, // 0x00000014
        ciTIFF_LZW = 21, // 0x00000015
        cifPDF = 22, // 0x00000016
        cifICL = 23, // 0x00000017
        ciGIF = 102, // 0x00000066
        ciJPG = 103, // 0x00000067
        ciPNG = 104, // 0x00000068
        ciICO = 106, // 0x0000006A
        ciTGA = 108, // 0x0000006C
        ciWBMP = 110, // 0x0000006E
        ciJBG = 113, // 0x00000071
        ciJP2 = 114, // 0x00000072
        ciJPC = 115, // 0x00000073
        ciPGX = 116, // 0x00000074
        ciPNM = 117 // 0x00000075
    }
}