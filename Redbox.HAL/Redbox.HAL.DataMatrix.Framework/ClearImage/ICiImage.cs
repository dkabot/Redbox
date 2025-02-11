using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("F2BCF189-0B27-11D4-B5F5-9CC767000000")]
    [TypeLibType(4160)]
    [ComVisible(true)]
    [ComImport]
    public interface ICiImage
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Create([In] int Width, [In] int Height);

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Close();

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Open([MarshalAs(UnmanagedType.BStr)] [In] string FileName, [In] int PageNumber = 1);

        [DispId(4)]
        CiRect Zone
        {
            [DispId(4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SaveAs([MarshalAs(UnmanagedType.BStr)] [In] string FileName, [In] EFileFormat Format = EFileFormat.ciEXT);

        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Invert();

        [DispId(8)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Clear();

        [DispId(9)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FlipVert();

        [DispId(10)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FlipHorz();

        [DispId(11)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RotateRight();

        [DispId(12)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RotateLeft();

        [DispId(13)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Flip();

        [DispId(14)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Append([MarshalAs(UnmanagedType.BStr)] [In] string FileName,
            [In] EFileFormat Format = EFileFormat.ciTIFF_G4);

        [DispId(16)]
        int Width
        {
            [DispId(16)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(17)]
        int Height
        {
            [DispId(17)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(33)]
        EScaleType pScaleType
        {
            [TypeLibFunc(1089)]
            [DispId(33)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(33)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(34)]
        int pScaleThreshold
        {
            [DispId(34)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(34)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(35)]
        [TypeLibFunc(1089)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ScaleImage([In] double ScaleX, [In] double ScaleY);

        [DispId(18)]
        int HorzDpi
        {
            [DispId(18)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
            [DispId(18)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(19)]
        int VertDpi
        {
            [DispId(19)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(19)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(20)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Copy([MarshalAs(UnmanagedType.Interface)] CiImage ImageFrom);

        [DispId(21)]
        EBoolean IsModified
        {
            [DispId(21)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(22)]
        string FileName
        {
            [DispId(22)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        [DispId(23)]
        int PageNumber
        {
            [DispId(23)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(24)]
        int PageCount
        {
            [DispId(24)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(25)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Save();

        [DispId(29)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CopyToClipboard();

        [DispId(30)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int SaveToBitmap();

        [DispId(31)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int SaveToDIB();

        [DispId(32)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Struct)]
        object SaveToMemory();

        [DispId(26)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OpenFromClipboard();

        [DispId(27)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OpenFromBitmap([In] int hBitmap);

        [DispId(28)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void LoadFromMemory([MarshalAs(UnmanagedType.Struct)] [In] object pData);

        [DispId(37)]
        int pScaleBmpBrightness
        {
            [DispId(37)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1089)]
            [DispId(37)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(38)]
        int pScaleBmpContrast
        {
            [TypeLibFunc(1089)]
            [DispId(38)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(38)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(36)]
        EScaleBmpType pScaleBmpType
        {
            [DispId(36)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(36)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(40)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int ScaleToDIB([In] double ScaleX, [In] double ScaleY);

        [DispId(41)]
        EFileFormat Format
        {
            [DispId(41)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(42)]
        int Handle
        {
            [DispId(42)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(43)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiImage CreateZoneRect([MarshalAs(UnmanagedType.Interface)] [In] CiRect Rect);

        [DispId(45)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiImage CreateZone([In] int left = 0, [In] int int_0 = 0, [In] int right = 0, [In] int bottom = 0);

        [DispId(44)]
        EBoolean IsZone
        {
            [DispId(44)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(46)]
        CiImage Parent
        {
            [DispId(46)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(47)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        CiImage Duplicate();

        [DispId(49)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        FLogFlags get_LogFlags([In] ELogType LogType);

        [TypeLibFunc(1088)]
        [DispId(49)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_LogFlags([In] ELogType LogType, [In] FLogFlags value);

        [DispId(50)]
        string LogSignature
        {
            [DispId(50)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
            [TypeLibFunc(1088)]
            [DispId(50)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: MarshalAs(UnmanagedType.BStr)]
            [param: In]
            set;
        }

        [DispId(51)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Crop([In] int left, [In] int int_0, [In] int right, [In] int bottom);

        [DispId(52)]
        int LineBytes
        {
            [DispId(52)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(53)]
        int BitsPerPixel
        {
            [DispId(53)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(54)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CreateBpp([In] int Width, [In] int Height, [In] int BitsPerPixel);

        [DispId(55)]
        int Buffer
        {
            [TypeLibFunc(1088)]
            [DispId(55)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(57)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        EBoolean IsBitonal();

        [DispId(58)]
        object CiCx
        {
            [DispId(58)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.IDispatch)]
            get;
        }

        [DispId(59)]
        double FileSize
        {
            [DispId(59)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(60)]
        int JpegQuality
        {
            [DispId(60)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(60)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(61)]
        EBoolean IsValid
        {
            [DispId(61)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [TypeLibFunc(1088)]
        [DispId(62)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int get_cx2l([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(62)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_cx2l([In] int int_0, [In] int value);

        [DispId(63)]
        CiPdf Pdf
        {
            [DispId(63)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(64)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ToBitonal();

        [DispId(65)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ToGrayscale();

        [DispId(66)]
        EComprColor pComprColor
        {
            [DispId(66)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(66)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(67)]
        EComprBitonal pComprBitonal
        {
            [DispId(67)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(67)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [TypeLibFunc(1088)]
        [DispId(68)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.BStr)]
        string get_Info([MarshalAs(UnmanagedType.BStr)] [In] string Type);

        [DispId(68)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_Info([MarshalAs(UnmanagedType.BStr)] [In] string Type,
            [MarshalAs(UnmanagedType.BStr)] [In] string value);
    }
}