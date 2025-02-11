using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("A7A8BF7F-8CB8-49DC-8A76-D0A1A145189C")]
    [ComVisible(true)]
    [ComImport]
    public class CiLineClass : ICiLine, CiLine
    {
        [DispId(35)]
        public virtual extern ELineDirection Direction
        {
            [DispId(35)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(36)]
        public virtual extern double Thickness
        {
            [DispId(36)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(37)]
        public virtual extern double Angle
        {
            [DispId(37)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(38)]
        public virtual extern CiRect Rect
        {
            [DispId(38)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(39)]
        public virtual extern CiPoint Start
        {
            [DispId(39)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(40)]
        public virtual extern CiPoint End
        {
            [DispId(40)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }
    }
}