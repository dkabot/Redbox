using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("F2BCF18B-0B27-11D4-B5F5-9CC767000000")]
    [TypeLibType(4096)]
    [InterfaceType(2)]
    [ComVisible(true)]
    [ComImport]
    public interface _ICiImageEvents
    {
        [TypeLibFunc(1088)]
        [DispId(1)]
        [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall,
            MethodCodeType = MethodCodeType.Runtime)]
        void Log([MarshalAs(UnmanagedType.BStr)] [In] string LogSignature,
            [MarshalAs(UnmanagedType.BStr)] [In] string LogRecord,
            [MarshalAs(UnmanagedType.BStr)] [In] string RsltRecord);
    }
}