using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop
{
    internal class CoTaskMem
    {
        public static string LPWStrToString(IntPtr lpwstr)
        {
            string stringUni = Marshal.PtrToStringUni(lpwstr);
            Marshal.FreeCoTaskMem(lpwstr);
            return stringUni;
        }
    }
}
