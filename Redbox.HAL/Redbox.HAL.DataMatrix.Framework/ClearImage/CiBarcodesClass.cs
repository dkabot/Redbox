using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.CustomMarshalers;

namespace ClearImage
{
    [Guid("DAB01618-F817-4662-ADBA-46EE1C94921A")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ComImport]
    public class CiBarcodesClass : ICiBarcodes, CiBarcodes, IEnumerable
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Add([MarshalAs(UnmanagedType.Interface)] [In] CiBarcode pVal);

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern CiBarcode Remove([In] int int_0);

        [DispId(1610743810)]
        public virtual extern int Count
        {
            [DispId(1610743810)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        [DispId(0)]
        public virtual extern CiBarcode this[[In] int int_0]
        {
            [DispId(0)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(-4)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        public virtual extern IEnumerator GetEnumerator();
    }
}