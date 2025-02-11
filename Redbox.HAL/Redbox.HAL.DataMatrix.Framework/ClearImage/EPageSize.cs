using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(true)]
    public enum EPageSize
    {
        epgsCustom = 4,
        epgsOriginal = 5,
        epgsLetter = 10, // 0x0000000A
        epgsLegal = 11, // 0x0000000B
        epgsLedger = 12, // 0x0000000C
        epgsA4 = 20, // 0x00000014
        [TypeLibVar(1216)] epgsImage = 40, // 0x00000028
        [TypeLibVar(1216)] epgsNearest = 41, // 0x00000029
        [TypeLibVar(1216)] epgsNearestStd = 42, // 0x0000002A
        [TypeLibVar(1216)] epgsNearestMetric = 43 // 0x0000002B
    }
}