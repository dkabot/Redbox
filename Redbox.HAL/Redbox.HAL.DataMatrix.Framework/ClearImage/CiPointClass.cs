using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClearImage
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("A7E1D34A-24DA-4D97-AF2E-CE7E4481E1E2")]
    [ComVisible(true)]
    [ComImport]
    public class CiPointClass : ICiPoint, CiPoint
    {
        [DispId(1)]
        public virtual extern int Int32_0
        {
            [DispId(1)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(1)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }

        [DispId(2)]
        public virtual extern int Int32_1
        {
            [DispId(2)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
            [DispId(2)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [param: In]
            set;
        }
    }
}