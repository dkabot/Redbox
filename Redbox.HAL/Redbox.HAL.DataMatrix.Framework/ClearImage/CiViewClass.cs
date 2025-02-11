using System.Runtime.InteropServices;

namespace ClearImage
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("BCE12DAC-09D5-4E64-AC70-9C7994B5832C")]
    [ComVisible(true)]
    [ComImport]
    public class CiViewClass : ICiView, CiView
    {
    }
}