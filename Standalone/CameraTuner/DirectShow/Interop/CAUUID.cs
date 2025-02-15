using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop
{
    [ComVisible(false)]
    internal struct CAUUID
    {
        public int cElems;
        public IntPtr pElems;

        public Guid[] ToGuidArray()
        {
            var guidArray = new Guid[cElems];
            for (var index = 0; index < cElems; ++index)
            {
                var ptr = new IntPtr(pElems.ToInt64() + index * Marshal.SizeOf(typeof(Guid)));
                guidArray[index] = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
            }

            return guidArray;
        }
    }
}