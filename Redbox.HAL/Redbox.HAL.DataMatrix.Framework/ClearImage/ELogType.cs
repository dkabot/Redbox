using System.Runtime.InteropServices;

namespace ClearImage
{
    [TypeLibType(16)]
    [ComVisible(true)]
    public enum ELogType
    {
        ciLogActions = 1,
        ciLogMeasurements = 2,
        ciLogResults = 3
    }
}