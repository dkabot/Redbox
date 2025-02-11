using System;
using System.Runtime.InteropServices;

namespace TaskSchedulerInterop
{
    [Guid("148BD524-A2AB-11CE-B11F-00AA00530503")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITask
    {
        void CreateTrigger(out ushort NewTriggerIndex, [MarshalAs(UnmanagedType.Interface)] out ITaskTrigger Trigger);

        void DeleteTrigger([In] ushort TriggerIndex);

        void GetTriggerCount(out ushort Count);

        void GetTrigger([In] ushort TriggerIndex, [MarshalAs(UnmanagedType.Interface)] out ITaskTrigger Trigger);

        void GetTriggerString([In] ushort TriggerIndex, out IntPtr TriggerString);

        void GetRunTimes(
          [MarshalAs(UnmanagedType.Struct), In] ref SystemTime Begin,
          [MarshalAs(UnmanagedType.Struct), In] ref SystemTime End,
          ref ushort Count,
          out IntPtr TaskTimes);

        void GetNextRunTime([MarshalAs(UnmanagedType.Struct), In, Out] ref SystemTime NextRun);

        void SetIdleWait([In] ushort IdleMinutes, [In] ushort DeadlineMinutes);

        void GetIdleWait(out ushort IdleMinutes, out ushort DeadlineMinutes);

        void Run();

        void Terminate();

        void EditWorkItem([In] uint hParent, [In] uint dwReserved);

        void GetMostRecentRunTime([MarshalAs(UnmanagedType.Struct), In, Out] ref SystemTime LastRun);

        void GetStatus([MarshalAs(UnmanagedType.Error)] out int Status);

        void GetExitCode(out uint ExitCode);

        void SetComment([MarshalAs(UnmanagedType.LPWStr), In] string Comment);

        void GetComment(out IntPtr Comment);

        void SetCreator([MarshalAs(UnmanagedType.LPWStr), In] string Creator);

        void GetCreator(out IntPtr Creator);

        void SetWorkItemData([In] ushort DataLen, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1), In] byte[] Data);

        void GetWorkItemData(out ushort DataLen, out IntPtr Data);

        void SetErrorRetryCount([In] ushort RetryCount);

        void GetErrorRetryCount(out ushort RetryCount);

        void SetErrorRetryInterval([In] ushort RetryInterval);

        void GetErrorRetryInterval(out ushort RetryInterval);

        void SetFlags([In] uint Flags);

        void GetFlags(out uint Flags);

        void SetAccountInformation([MarshalAs(UnmanagedType.LPWStr), In] string AccountName, [In] IntPtr Password);

        void GetAccountInformation(out IntPtr AccountName);

        void SetApplicationName([MarshalAs(UnmanagedType.LPWStr), In] string ApplicationName);

        void GetApplicationName(out IntPtr ApplicationName);

        void SetParameters([MarshalAs(UnmanagedType.LPWStr), In] string Parameters);

        void GetParameters(out IntPtr Parameters);

        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr), In] string WorkingDirectory);

        void GetWorkingDirectory(out IntPtr WorkingDirectory);

        void SetPriority([In] uint Priority);

        void GetPriority(out uint Priority);

        void SetTaskFlags([In] uint Flags);

        void GetTaskFlags(out uint Flags);

        void SetMaxRunTime([In] uint MaxRunTimeMS);

        void GetMaxRunTime(out uint MaxRunTimeMS);
    }
}
