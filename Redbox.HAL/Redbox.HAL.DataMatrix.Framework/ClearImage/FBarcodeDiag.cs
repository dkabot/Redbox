using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum FBarcodeDiag
    {
        cibRawData = 64, // 0x00000040
        cibNoValidation = 512, // 0x00000200
        cibSymbologyIdentifier = 8192 // 0x00002000
    }
}