using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("8B79E556-FAD7-4339-8A8F-2C35D5C42C70")]
    [TypeLibType(4160)]
    [ComVisible(true)]
    [ComImport]
    public interface ICiQR
    {
        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcode FirstBarcode();

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcode NextBarcode();

        [DispId(5)]
        CiImage Image
        {
            [DispId(5)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
            [DispId(5)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: MarshalAs(UnmanagedType.Interface)]
            [param: In]
            set;
        }

        [DispId(4)]
        EBarcodeAlgorithm Algorithm
        {
            [DispId(4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(7)]
        FBarcodeDiag DiagFlags
        {
            [DispId(7)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(7)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(9)]
        CiBarcodes Barcodes
        {
            [DispId(9)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(8)]
        FBarcodeDirections Directions
        {
            [DispId(8)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1088)]
            [DispId(8)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(10)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int Find([In] int MaxBarcodes = 0);

        [DispId(11)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object Fx1([In] int par1 = 0, [In] int par2 = 0);

        [DispId(12)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int get_cx2l([In] int int_0);

        [DispId(12)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_cx2l([In] int int_0, [In] int value);

        [DispId(13)]
        EBarcodeEncoding Encodings
        {
            [DispId(13)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(13)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }
    }
}