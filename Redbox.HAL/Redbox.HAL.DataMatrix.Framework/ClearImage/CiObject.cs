using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("59A0E32D-5050-47F5-A21B-F00397A21FCC")]
    [CoClass(typeof(CiObjectClass))]
    [ComVisible(true)]
    [ComImport]
    public interface CiObject : ICiObject
    {
    }
}