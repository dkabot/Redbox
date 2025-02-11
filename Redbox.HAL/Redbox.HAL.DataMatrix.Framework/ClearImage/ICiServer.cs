using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("A34FC1A7-C73F-4706-886E-C4A33E37C6E5")]
    [TypeLibType(4160)]
    [ComVisible(true)]
    [ComImport]
    public interface ICiServer
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiImage CreateImage();

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcodePro CreateBarcodePro();

        [DispId(10)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OpenExt([In] int hModule, [In] int MasterId, [In] int pParam);

        [DispId(11)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiRect CreateRect([In] int left = 0, [In] int int_0 = 0, [In] int right = 0, [In] int bottom = 0);

        [DispId(12)]
        int VerMajor
        {
            [DispId(12)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(13)]
        int VerMinor
        {
            [DispId(13)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(14)]
        int VerRelease
        {
            [DispId(14)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiRepair CreateRepair();

        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiTools CreateTools();

        [DispId(8)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcodeBasic CreateBarcodeBasic();

        [DispId(9)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiPdf417 CreatePdf417();

        [DispId(15)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiDataMatrix CreateDataMatrix();

        [DispId(16)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.BStr)]
        string get_Info([In] EInfoType Type, [In] int nParam = 0);

        [DispId(17)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiAdvColor CreateAdvColor();

        [DispId(18)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcode CreateBarcode();

        [DispId(19)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiBarcodes CreateBarcodes();

        [DispId(20)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int get_DevMode([In] int bGlobalDevMode = 0);

        [DispId(20)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_DevMode([In] int bGlobalDevMode, [In] int value);

        [TypeLibFunc(1088)]
        [DispId(21)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.BStr)]
        string get_Var([MarshalAs(UnmanagedType.BStr)] [In] string Name);

        [TypeLibFunc(1088)]
        [DispId(21)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_Var([MarshalAs(UnmanagedType.BStr)] [In] string Name,
            [MarshalAs(UnmanagedType.BStr)] [In] string value);

        [TypeLibFunc(1088)]
        [DispId(22)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int uncompress([MarshalAs(UnmanagedType.Struct)] [In] object DataIn, [In] [Out] ref EBarcodeEncoding Encoding,
            [MarshalAs(UnmanagedType.Struct)] out object DataOut);

        [DispId(23)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiQR CreateQR();
    }
}