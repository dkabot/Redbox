using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [ComSourceInterfaces("ClearImage._ICiImageEvents\0")]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("F2BCF18A-0B27-11D4-B5F5-9CC767000000")]
    [ComVisible(true)]
    [ComImport]
    public class CiImageClass : ICiImage, CiImage, _ICiImageEvents_Event
    {
        public virtual extern event _ICiImageEvents_LogEventHandler Log;

        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Create([In] int Width, [In] int Height);

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Close();

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Open([MarshalAs(UnmanagedType.BStr)] [In] string FileName, [In] int PageNumber = 1);

        [DispId(4)]
        public virtual extern CiRect Zone
        {
            [DispId(4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void SaveAs([MarshalAs(UnmanagedType.BStr)] [In] string FileName,
            [In] EFileFormat Format = EFileFormat.ciEXT);

        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Invert();

        [DispId(8)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Clear();

        [DispId(9)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void FlipVert();

        [DispId(10)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void FlipHorz();

        [DispId(11)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void RotateRight();

        [DispId(12)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void RotateLeft();

        [DispId(13)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Flip();

        [DispId(14)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Append([MarshalAs(UnmanagedType.BStr)] [In] string FileName,
            [In] EFileFormat Format = EFileFormat.ciTIFF_G4);

        [DispId(16)]
        public virtual extern int Width
        {
            [DispId(16)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(17)]
        public virtual extern int Height
        {
            [DispId(17)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(33)]
        public virtual extern EScaleType pScaleType
        {
            [DispId(33)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1089)]
            [DispId(33)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(34)]
        public virtual extern int pScaleThreshold
        {
            [DispId(34)]
            [TypeLibFunc(1089)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1089)]
            [DispId(34)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [TypeLibFunc(1089)]
        [DispId(35)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ScaleImage([In] double ScaleX, [In] double ScaleY);

        [DispId(18)]
        public virtual extern int HorzDpi
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
        public virtual extern int VertDpi
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
        public virtual extern void Copy([MarshalAs(UnmanagedType.Interface)] CiImage ImageFrom);

        [DispId(21)]
        public virtual extern EBoolean IsModified
        {
            [DispId(21)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(22)]
        public virtual extern string FileName
        {
            [DispId(22)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        [DispId(23)]
        public virtual extern int PageNumber
        {
            [DispId(23)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(24)]
        public virtual extern int PageCount
        {
            [DispId(24)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(25)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Save();

        [DispId(29)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void CopyToClipboard();

        [DispId(30)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int SaveToBitmap();

        [DispId(31)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int SaveToDIB();

        [DispId(32)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Struct)]
        public virtual extern object SaveToMemory();

        [DispId(26)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void OpenFromClipboard();

        [DispId(27)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void OpenFromBitmap([In] int hBitmap);

        [DispId(28)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void LoadFromMemory([MarshalAs(UnmanagedType.Struct)] [In] object pData);

        [DispId(37)]
        public virtual extern int pScaleBmpBrightness
        {
            [TypeLibFunc(1089)]
            [DispId(37)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1089)]
            [DispId(37)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(38)]
        public virtual extern int pScaleBmpContrast
        {
            [TypeLibFunc(1089)]
            [DispId(38)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1089)]
            [DispId(38)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(36)]
        public virtual extern EScaleBmpType pScaleBmpType
        {
            [DispId(36)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [TypeLibFunc(1088)]
            [DispId(36)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [TypeLibFunc(1088)]
        [DispId(40)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int ScaleToDIB([In] double ScaleX, [In] double ScaleY);

        [DispId(41)]
        public virtual extern EFileFormat Format
        {
            [DispId(41)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(42)]
        public virtual extern int Handle
        {
            [DispId(42)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(43)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiImage CreateZoneRect([MarshalAs(UnmanagedType.Interface)] [In] CiRect Rect);

        [DispId(45)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiImage CreateZone([In] int left = 0, [In] int int_0 = 0, [In] int right = 0,
            [In] int bottom = 0);

        [DispId(44)]
        public virtual extern EBoolean IsZone
        {
            [DispId(44)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(46)]
        public virtual extern CiImage Parent
        {
            [DispId(46)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(47)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiImage Duplicate();

        [TypeLibFunc(1088)]
        [DispId(49)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern FLogFlags get_LogFlags([In] ELogType LogType);

        [TypeLibFunc(1088)]
        [DispId(49)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_LogFlags([In] ELogType LogType, [In] FLogFlags value);

        [DispId(50)]
        public virtual extern string LogSignature
        {
            [TypeLibFunc(1088)]
            [DispId(50)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
            [DispId(50)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: MarshalAs(UnmanagedType.BStr)]
            [param: In]
            set;
        }

        [DispId(51)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Crop([In] int left, [In] int int_0, [In] int right, [In] int bottom);

        [DispId(52)]
        public virtual extern int LineBytes
        {
            [DispId(52)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(53)]
        public virtual extern int BitsPerPixel
        {
            [DispId(53)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(54)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void CreateBpp([In] int Width, [In] int Height, [In] int BitsPerPixel);

        [DispId(55)]
        public virtual extern int Buffer
        {
            [TypeLibFunc(1088)]
            [DispId(55)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [TypeLibFunc(1088)]
        [DispId(57)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern EBoolean IsBitonal();

        [DispId(58)]
        public virtual extern object CiCx
        {
            [TypeLibFunc(1088)]
            [DispId(58)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.IDispatch)]
            get;
        }

        [DispId(59)]
        public virtual extern double FileSize
        {
            [DispId(59)]
            [TypeLibFunc(1088)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(60)]
        public virtual extern int JpegQuality
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
        public virtual extern EBoolean IsValid
        {
            [DispId(61)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(62)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int get_cx2l([In] int int_0);

        [DispId(62)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx2l([In] int int_0, [In] int value);

        [DispId(63)]
        public virtual extern CiPdf Pdf
        {
            [DispId(63)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(64)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ToBitonal();

        [DispId(65)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ToGrayscale();

        [DispId(66)]
        public virtual extern EComprColor pComprColor
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
        public virtual extern EComprBitonal pComprBitonal
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
        public virtual extern string get_Info([MarshalAs(UnmanagedType.BStr)] [In] string Type);

        [TypeLibFunc(1088)]
        [DispId(68)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_Info([MarshalAs(UnmanagedType.BStr)] [In] string Type,
            [MarshalAs(UnmanagedType.BStr)] [In] string value);
    }
}