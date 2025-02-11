using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("BDDB0244-0CFD-11D4-B5F8-B89D57000000")]
    [CoClass(typeof(CiBarcodeProClass))]
    [ComVisible(true)]
    [ComImport]
    public interface CiBarcodePro : ICiBarcodePro
    {
    }
}