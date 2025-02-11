using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("4BBE294D-3C2A-4698-8B5F-84B1D3A6F621")]
    [ComVisible(true)]
    [ComImport]
    public class CiObjectClass : ICiObject, CiObject
    {
        [DispId(3)]
        public virtual extern int Pixels
        {
            [DispId(3)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(4)]
        public virtual extern int Intervals
        {
            [DispId(4)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(5)]
        public virtual extern CiRect Rect
        {
            [DispId(5)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [TypeLibFunc(1088)]
        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern double get_cx3d([In] int int_0);

        [TypeLibFunc(1088)]
        [DispId(6)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx3d([In] int int_0, [In] double value);

        [TypeLibFunc(1088)]
        [DispId(7)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern int get_cx2l([In] int int_0);

        [DispId(7)]
        [TypeLibFunc(1088)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void set_cx2l([In] int int_0, [In] int value);
    }
}