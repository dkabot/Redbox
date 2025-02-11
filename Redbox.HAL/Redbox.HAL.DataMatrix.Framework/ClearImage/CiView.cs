using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("98AFD5D0-A728-498D-B1DE-0D811D719793")]
    [CoClass(typeof(CiViewClass))]
    [ComVisible(true)]
    [ComImport]
    public interface CiView : ICiView
    {
    }
}