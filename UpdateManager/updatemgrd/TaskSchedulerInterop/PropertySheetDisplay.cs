using System.Runtime.InteropServices;

namespace TaskSchedulerInterop
{
    internal class PropertySheetDisplay
    {
        [DllImport("comctl32.dll")]
        public static extern int PropertySheet([MarshalAs(UnmanagedType.Struct), In] ref PropSheetHeader psh);
    }
}
