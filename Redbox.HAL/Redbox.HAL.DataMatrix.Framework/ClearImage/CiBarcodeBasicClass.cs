using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("12806B0A-7754-4297-AE05-631C2A1E928D")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ComImport]
    public class CiBarcodeBasicClass : ICiBarcodeBasic, CiBarcodeBasic
    {
        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcode FirstBarcode();

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcode NextBarcode();

        [DispId(5)]
        public virtual extern CiImage Image
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

        [DispId(7)]
        public virtual extern FBarcodeType Type
        {
            [DispId(7)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(7)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(8)]
        public virtual extern EBoolean AutoDetect1D
        {
            [DispId(8)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(8)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(9)]
        public virtual extern EBoolean ValidateOptChecksum
        {
            [DispId(9)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(9)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(10)]
        public virtual extern CiBarcodes Barcodes
        {
            [DispId(10)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(11)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int Find([In] int MaxBarcodes = 0);
    }
}