using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [Guid("8CBBAECA-9716-40CA-B8F6-0E9FF213522A")]
    [TypeLibType(4160)]
    [ComVisible(true)]
    [ComImport]
    public interface ICiAdvColor
    {
        [DispId(1)]
        CiImage Image
        {
            [DispId(1)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
            [DispId(1)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: MarshalAs(UnmanagedType.Interface)]
            [param: In]
            set;
        }

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ConvertToBitonal([In] EBiTonalConversion mode = EBiTonalConversion.ciBtcStandard, [In] int Par0 = 0,
            [In] int par1 = 0, [In] int par2 = 0);

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ConvertToGrayscale([In] EGrayscaleConversion mode = EGrayscaleConversion.ciGscStandard, [In] int Par0 = 0,
            [In] int par1 = 0, [In] int par2 = 0);

        [DispId(4)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ScaleToDpi([In] EScaleBmpType mode = EScaleBmpType.ciScaleBestQuality, [In] int Par0 = 0);

        [DispId(5)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int get_cx2l([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_cx2l([In] int int_0, [In] int value);

        [DispId(6)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        double get_cx3d([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void set_cx3d([In] int int_0, [In] double value);
    }
}