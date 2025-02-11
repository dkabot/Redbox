using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("148BD52B-A2AB-11CE-B11F-00AA00530503")]
    internal interface ITaskTrigger
    {
        void SetTrigger([MarshalAs(UnmanagedType.Struct), In, Out] ref TaskTrigger Trigger);

        void GetTrigger([MarshalAs(UnmanagedType.Struct), In, Out] ref TaskTrigger Trigger);

        void GetTriggerString(out IntPtr TriggerString);
    }
}
