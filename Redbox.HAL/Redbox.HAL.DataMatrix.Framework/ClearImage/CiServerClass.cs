using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("D836E300-A317-4C7E-BE61-D650CE242589")]
    [ClassInterface(ClassInterfaceType.None)]
    [TypeLibType(2)]
    [ComVisible(true)]
    [ComImport]
    public class CiServerClass : ICiServer, CiServer
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiImage CreateImage();

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcodePro CreateBarcodePro();

        [DispId(10)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void OpenExt([In] int hModule, [In] int MasterId, [In] int pParam);

        [DispId(11)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiRect CreateRect([In] int left = 0, [In] int int_0 = 0, [In] int right = 0,
            [In] int bottom = 0);

        [DispId(12)]
        public virtual extern int VerMajor
        {
            [DispId(12)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(13)]
        public virtual extern int VerMinor
        {
            [DispId(13)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(14)]
        public virtual extern int VerRelease
        {
            [DispId(14)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiRepair CreateRepair();

        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiTools CreateTools();

        [DispId(8)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcodeBasic CreateBarcodeBasic();

        [DispId(9)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiPdf417 CreatePdf417();

        [DispId(15)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiDataMatrix CreateDataMatrix();

        [DispId(16)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public virtual extern string get_Info([In] EInfoType Type, [In] int nParam = 0);

        [DispId(17)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiAdvColor CreateAdvColor();

        [TypeLibFunc(1088)]
        [DispId(18)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcode CreateBarcode();

        [TypeLibFunc(1088)]
        [DispId(19)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcodes CreateBarcodes();

        [DispId(20)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int get_DevMode([In] int bGlobalDevMode = 0);

        [DispId(20)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_DevMode([In] int bGlobalDevMode, [In] int value);

        [DispId(21)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public virtual extern string get_Var([MarshalAs(UnmanagedType.BStr)] [In] string Name);

        [TypeLibFunc(1088)]
        [DispId(21)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_Var([MarshalAs(UnmanagedType.BStr)] [In] string Name,
            [MarshalAs(UnmanagedType.BStr)] [In] string value);

        [DispId(22)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int uncompress(
            [MarshalAs(UnmanagedType.Struct)] [In] object DataIn,
            [In] [Out] ref EBarcodeEncoding Encoding,
            [MarshalAs(UnmanagedType.Struct)] out object DataOut);

        [DispId(23)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiQR CreateQR();
    }
}