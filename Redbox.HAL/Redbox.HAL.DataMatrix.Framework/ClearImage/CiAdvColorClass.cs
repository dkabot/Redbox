using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("3022D35D-0127-4C24-B1F0-1C66A831807E")]
    [ComVisible(true)]
    [ComImport]
    public class CiAdvColorClass : ICiAdvColor, CiAdvColor
    {
        [DispId(1)]
        public virtual extern CiImage Image
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
        public virtual extern void ConvertToBitonal(
            [In] EBiTonalConversion mode = EBiTonalConversion.ciBtcStandard,
            [In] int Par0 = 0,
            [In] int par1 = 0,
            [In] int par2 = 0);

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ConvertToGrayscale(
            [In] EGrayscaleConversion mode = EGrayscaleConversion.ciGscStandard,
            [In] int Par0 = 0,
            [In] int par1 = 0,
            [In] int par2 = 0);

        [TypeLibFunc(1088)]
        [DispId(4)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void ScaleToDpi([In] EScaleBmpType mode = EScaleBmpType.ciScaleBestQuality,
            [In] int Par0 = 0);

        [DispId(5)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int get_cx2l([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx2l([In] int int_0, [In] int value);

        [DispId(6)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern double get_cx3d([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx3d([In] int int_0, [In] double value);
    }
}