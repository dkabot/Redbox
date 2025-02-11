using Redbox.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Redbox.UpdateManager.Remoting
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal class ShellHelper
    {
        private const int TOKEN_DUPLICATE = 2;
        private const int TOKEN_QUERY = 8;
        private const int TOKEN_IMPERSONATE = 4;
        private const uint GRANTED_ALL = 268435456;
        private const uint CREATE_NEW_CONSOLE = 16;

        public static int StartProcessAsShellUser(string processName)
        {
            return ShellHelper.StartProcessAsShellUser(processName, (string)null);
        }

        public static int StartProcessAsShellUser(string processName, string args)
        {
            Process[] processesByName = Process.GetProcessesByName("KioskShell");
            if (processesByName.Length == 0 || processesByName.Length > 1)
                return -1;
            IntPtr zero = IntPtr.Zero;
            if (ShellHelper.OpenProcessToken(processesByName[0].Handle, 14, ref zero) != 0)
            {
                try
                {
                    IntPtr num = ShellHelper.DuplicateToken(zero);
                    if (num == IntPtr.Zero)
                    {
                        LogHelper.Instance.Log("Unable to to duplicate process token with DuplicateToken.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                        return -1;
                    }
                    try
                    {
                        string directoryName = Path.GetDirectoryName(processName);
                        string fileName = Path.GetFileName(processName);
                        string lpCommandLine = string.IsNullOrEmpty(args) ? (string)null : string.Format("{0} {1}", (object)processName, (object)args);
                        LogHelper.Instance.Log("starting from path: '{0}' with file: '{1}' in directory: '{2}' with command line: '{3}'", (object)processName, (object)fileName, (object)directoryName, (object)lpCommandLine);
                        ShellHelper.STARTUPINFO lpStartupInfo = new ShellHelper.STARTUPINFO();
                        lpStartupInfo.cb = Marshal.SizeOf<ShellHelper.STARTUPINFO>(lpStartupInfo);
                        lpStartupInfo.lpDesktop = "winsta0\\default";
                        ShellHelper.PROCESS_INFORMATION lpProcessInformation;
                        if (ShellHelper.CreateProcessAsUser(num, processName, lpCommandLine, IntPtr.Zero, IntPtr.Zero, false, 16U, IntPtr.Zero, directoryName, ref lpStartupInfo, out lpProcessInformation))
                            return lpProcessInformation.dwProcessId;
                        LogHelper.Instance.Log("Error Calling CreateProcessWithTokenW", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("Error in Function StartProcessAsShellUser.", ex);
                    }
                    finally
                    {
                        ShellHelper.CloseHandle(num);
                    }
                }
                finally
                {
                    ShellHelper.CloseHandle(zero);
                }
            }
            else
                LogHelper.Instance.Log("Unable to acquire process token via OpenProcessToken.", (Exception)new Win32Exception(Marshal.GetLastWin32Error()));
            return -1;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("advapi32", SetLastError = true)]
        private static extern int OpenProcessToken(
          IntPtr processHandle,
          int desiredAccess,
          ref IntPtr tokenHandle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CreateProcessAsUser(
          IntPtr hToken,
          string lpApplicationName,
          string lpCommandLine,
          IntPtr lpProcessAttributes,
          IntPtr lpThreadAttributes,
          bool bInheritHandles,
          uint dwCreationFlags,
          IntPtr lpEnvironment,
          string lpCurrentDirectory,
          ref ShellHelper.STARTUPINFO lpStartupInfo,
          out ShellHelper.PROCESS_INFORMATION lpProcessInformation);

        private static IntPtr DuplicateToken(IntPtr sourceToken)
        {
            IntPtr phNewToken;
            return !ShellHelper.DuplicateTokenEx(sourceToken, 268435456U, IntPtr.Zero, ShellHelper.SECURITY_IMPERSONATION_LEVEL.Impersonation, ShellHelper.TOKEN_TYPE.Primary, out phNewToken) ? IntPtr.Zero : phNewToken;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DuplicateTokenEx(
          IntPtr hExistingToken,
          uint dwDesiredAccess,
          IntPtr lpTokenAttributes,
          ShellHelper.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
          ShellHelper.TOKEN_TYPE TokenType,
          out IntPtr phNewToken);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            Anonymous,
            Identifcation,
            Impersonation,
            Delegation,
        }

        private enum TOKEN_TYPE
        {
            Primary = 1,
            Impersonation = 2,
        }

        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
    }
}
