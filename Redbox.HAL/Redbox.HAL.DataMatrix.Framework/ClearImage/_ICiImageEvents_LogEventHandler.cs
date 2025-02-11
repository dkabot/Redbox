using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComVisible(false)]
    public delegate void _ICiImageEvents_LogEventHandler(
        [MarshalAs(UnmanagedType.BStr)] [In] string LogSignature,
        [MarshalAs(UnmanagedType.BStr)] [In] string LogRecord,
        [MarshalAs(UnmanagedType.BStr)] [In] string RsltRecord);
}