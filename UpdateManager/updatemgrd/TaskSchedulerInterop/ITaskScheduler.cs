using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("148BD527-A2AB-11CE-B11F-00AA00530503")]
    internal interface ITaskScheduler
    {
        void SetTargetComputer([MarshalAs(UnmanagedType.LPWStr), In] string Computer);

        void GetTargetComputer(out IntPtr Computer);

        void Enum([MarshalAs(UnmanagedType.Interface)] out IEnumWorkItems EnumWorkItems);

        void Activate([MarshalAs(UnmanagedType.LPWStr), In] string Name, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object obj);

        void Delete([MarshalAs(UnmanagedType.LPWStr), In] string Name);

        void NewWorkItem([MarshalAs(UnmanagedType.LPWStr), In] string TaskName, [In] ref Guid rclsid, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object obj);

        void AddWorkItem([MarshalAs(UnmanagedType.LPWStr), In] string TaskName, [MarshalAs(UnmanagedType.Interface), In] ITask WorkItem);

        void IsOfType([MarshalAs(UnmanagedType.LPWStr), In] string TaskName, [In] ref Guid riid);
    }
}
