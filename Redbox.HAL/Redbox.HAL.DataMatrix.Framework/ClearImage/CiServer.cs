using System.Runtime.InteropServices;

namespace ClearImage
{
    [CoClass(typeof(CiServerClass))]
    [Guid("A34FC1A7-C73F-4706-886E-C4A33E37C6E5")]
    [ComVisible(true)]
    [ComImport]
    public interface CiServer : ICiServer
    {
    }
}