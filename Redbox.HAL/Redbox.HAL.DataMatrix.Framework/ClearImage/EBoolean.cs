using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EBoolean
    {
        ciFalse = 0,
        ciTrue = 65535 // 0x0000FFFF
    }
}