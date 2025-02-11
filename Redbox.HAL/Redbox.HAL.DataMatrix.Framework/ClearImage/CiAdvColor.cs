using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("8CBBAECA-9716-40CA-B8F6-0E9FF213522A")]
    [CoClass(typeof(CiAdvColorClass))]
    [ComVisible(true)]
    [ComImport]
    public interface CiAdvColor : ICiAdvColor
    {
    }
}