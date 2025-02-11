using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EScaleType
    {
        ciScaleMergeLineGaps = 1,
        ciScaleMergeLinePixels = 2,
        ciScaleSkipLines = 3,
        ciScaleThreshold = 101 // 0x00000065
    }
}