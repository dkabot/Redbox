using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [TypeLibType(4160)]
    [Guid("B8E5EE38-BB0D-4561-B8D3-26C18C8817EE")]
    [ComVisible(true)]
    [ComImport]
    public interface ICiLine
    {
        [DispId(35)]
        ELineDirection Direction
        {
            [DispId(35)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(36)]
        double Thickness
        {
            [DispId(36)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(37)]
        double Angle
        {
            [DispId(37)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(38)]
        CiRect Rect
        {
            [DispId(38)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(39)]
        CiPoint Start
        {
            [DispId(39)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(40)]
        CiPoint End
        {
            [DispId(40)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }
    }
}