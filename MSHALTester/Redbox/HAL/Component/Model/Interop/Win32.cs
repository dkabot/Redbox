using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Component.Model.Interop;

public static class Win32
{
    [Flags]
    public enum AccessFlags : uint
    {
        NONE = 0,
        GENERIC_READ = 2147483648, // 0x80000000
        GENERIC_WRITE = 1073741824, // 0x40000000
        GENERIC_READ_WRITE = GENERIC_WRITE | GENERIC_READ, // 0xC0000000
        GENERIC_EXECUTE = 536870912, // 0x20000000
        GENERIC_ALL = 268435456 // 0x10000000
    }

    [Flags]
    public enum ShareFlags : uint
    {
        NONE = 0,
        FILE_SHARE_READ = 1,
        FILE_SHARE_WRITE = 2,
        FILE_SHARE_READ_WRITE = FILE_SHARE_WRITE | FILE_SHARE_READ, // 0x00000003
        FILE_SHARE_DELETE = 4
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern SafeFileHandle CreateFile(
        string pipeName,
        AccessFlags dwDesiredAccess,
        ShareFlags dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplate);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        int nNumberOfBytesToWrite,
        out int lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        int nNumberOfBytesToRead,
        out int lpNumberOfBytesRead,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FlushFileBuffers(SafeFileHandle handle);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool InitializeSecurityDescriptor(
        ref SecurityDescriptor sd,
        uint dwRevision);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool SetSecurityDescriptorDacl(
        ref SecurityDescriptor sd,
        bool daclPresent,
        IntPtr dacl,
        bool daclDefaulted);

    public static class ErrorCodes
    {
        public const int ErrorSuccess = 0;
        public const int ErrorBrokenPipe = 109;
        public const int ErrorPipeBusy = 231;
        public const int ErrorNoData = 232;
        public const int ErrorPipeNotConnected = 233;
        public const int ErrorMoreData = 234;
        public const int ErrorPipeConnected = 535;
        public const int ErrorPipeListening = 536;
    }

    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public int lpSecurityDescriptor;
        public int bInheritHandle;
    }

    public struct SecurityDescriptor
    {
        public byte revision;
        public byte size;
        public short control;
        public IntPtr owner;
        public IntPtr group;
        public IntPtr sacl;
        public IntPtr dacl;
    }

    public struct SecurityAttributes
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }
}