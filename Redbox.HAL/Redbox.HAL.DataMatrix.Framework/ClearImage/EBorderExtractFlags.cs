using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EBorderExtractFlags
    {
        ciBexBorder = 1,
        ciBexBorderDeskew = 2,
        ciBexBorderDeskewCrop = 3,
        ciBexDeskewCrop = 4
    }
}