using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.CustomMarshalers;

namespace ClearImage
{
    [Guid("41F7F4D4-9FC1-46C6-92E5-2A3457CE3D5E")]
    [TypeLibType(4160)]
    [ComVisible(true)]
    [ComImport]
    public interface ICiBarcodes : IEnumerable
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Add([MarshalAs(UnmanagedType.Interface)] [In] CiBarcode pVal);

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcode Remove([In] int int_0);

        [DispId(1610743810)]
        int Count
        {
            [DispId(1610743810)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(0)]
        CiBarcode this[[In] int int_0]
        {
            [DispId(0)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(-4)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        new IEnumerator GetEnumerator();
    }
}